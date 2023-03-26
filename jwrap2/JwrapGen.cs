using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

#if JWRAP_GEN
namespace jwrap;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using CommandLine;

class Options
{
    [Option('w', "window", Required = false, HelpText = "Run in Windoe Mode")]
    public bool window { get; set; }

    [Option('m', "main", Required = false, HelpText = "Main Class Name")]
    public string main { get; set; }

    [Value(0, MetaName = "file", Required = true, HelpText = "Jar Path")]
    public string FilePath { get; set; }
}

public class JwrapGen
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            string exeDir = Directory.GetParent(Application.ExecutablePath).FullName;
            Misc.Log(Directory.GetParent(Application.ExecutablePath));
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options =>
                {
                    string bootJarPath = exeDir + $"\\jwrap-boot.jar";
                    string bootDllPath = exeDir + $"\\jwrap-jre.dll";
                    string windowSuffix = options.window ? "w" : "";
                    string headPath = exeDir + $"\\jwrap{windowSuffix}-head.exe";
                    Misc.Log(headPath);
                    Misc.Log(options.FilePath);
                    if (!options.FilePath.EndsWith(".jar"))
                    {
                        throw new Exception($"File is not jar: {options.FilePath}");
                    }

                    if (!File.Exists(options.FilePath))
                    {
                        throw new Exception($"File not exist: {options.FilePath}");
                    }

                    byte[] bootJarData = Misc.ReadBinaryFile(bootJarPath);
                    byte[] bootDllData = Misc.ReadBinaryFile(bootDllPath);
                    byte[] jarData = Misc.ReadBinaryFile(options.FilePath);
                    string exePath = Regex.Replace(options.FilePath, "[.]jar$", ".exe");
                    Misc.Log(exePath);
                    File.Delete(exePath);
                    File.Copy(headPath, exePath);
                    string mainClass = options.main;
                    if (mainClass == null) mainClass = "global.Main";
                    //Win32Res.WriteResourceData(exePath, "JWRAP", "MAIN", Encoding.UTF8.GetBytes(mainClass));
                    XElement root = new XElement("xml",
                        new XElement("main", mainClass),
                        new XElement("guid", Misc.GetGuidString()),
                        new XElement("sha512", Misc.GetSha512String(jarData)),
                        new XElement("boot.jar", Convert.ToBase64String(bootJarData)),
                        new XElement("boot.dll", Convert.ToBase64String(bootDllData)),
                        new XElement("jar", Convert.ToBase64String(jarData))
                    );
                    XElement dlls = new XElement("dlls");
                    string jarDir = Directory.GetParent(options.FilePath).ToString();
                    string[] files = Directory.GetFiles(jarDir, "*.dll"); // ディレクトリ内の".dll"で終わるファイル名の一覧を取得
                    foreach (var file in files)
                    {
                        Misc.Log(file);
                        Misc.Log(Path.GetFileName(file));
                        XElement dll = new XElement("dll",
                            new XElement("name", Path.GetFileName(file)),
                            new XElement("binary", Convert.ToBase64String(Misc.ReadBinaryFile(file)))
                        );
                        dlls.Add(dll);
                    }

                    root.Add(dlls);
                    XDocument doc = new XDocument(root);
                    byte[] docBytes = Encoding.UTF8.GetBytes(doc.ToString());
#if false
                    using (FileStream fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        fs.Seek(fs.Length, SeekOrigin.Begin);
                        fs.WriteByte(0);
                        fs.Write(docBytes, 0, docBytes.Length);
                    }
#endif
                    Misc.PutLastUtf8Bytes(exePath, docBytes);
                    //Win32Res.WriteResourceData(exePath, "JWRAP", "XML", Encoding.UTF8.GetBytes(doc.ToString()));
                });
            //SeparateMain(args);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }
}
#endif