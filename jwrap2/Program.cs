#if !JWRAP_GEN
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using PeterO.Cbor;

namespace jwrap;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            SeparateMain(args);
        }
        catch (Exception e)
        {
            //Console.Error.WriteLine(e.ToString());
            Win32Api.Message(e.ToString(), Path.GetFileName(Application.ExecutablePath));
            Environment.Exit(1);
        }
    }

    private static string GetTimeStampString()
    {
        var now = DateTime.Now;
        string formatted = now.ToString("yyyy-MMdd-HHmmss-fff");
        return formatted;
    }

    private static string UrlToStoreName(string url)
    {
        return Regex.Replace(url, "[^a-zA-z0-9-_.]", "!");
    }

    private static string PrepareJre(string url)
    {
        //string url = $"https://github.com/run-exe/jwrap/releases/download/jre/{name}.zip";
        string name = UrlToStoreName(url);
        //var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); // C:\ProgramData
        var rootPath = $"{profilePath}\\.jwap\\.jre";
        Directory.CreateDirectory(rootPath);
        string timestamp = GetTimeStampString();
        string downloadPath = $"{rootPath}\\{name}+{timestamp}.zip";
        string installPath = $"{rootPath}\\{name}";
        if (!Directory.Exists(installPath))
        {
            DownloadBinaryFromUrl(url, downloadPath);
            ZipFile.ExtractToDirectory(downloadPath, $"{installPath}+{timestamp}");
            Directory.Move($"{installPath}+{timestamp}", installPath);
            File.Delete(downloadPath);
        }

        return installPath;
    }

    private static void SeparateMain(string[] args)
    {
        Misc.Log("SeparateMain(1)");
        string argList = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) argList += " ";
            argList += $"\"{args[i]}\"";
        }

        var tree1 = CborUtil.GetOneFromFileEnd(Application.ExecutablePath);
        CborTree.DumpTree(tree1);
        Misc.Log(CborTree.GetEntry(tree1, "main").ToJSONString());
        Misc.Log(CborTree.GetEntry(tree1, "guid").ToJSONString());
        Misc.Log(CborTree.GetEntry(tree1, "sha").ToJSONString());
        byte[] bootClassData = CborTree.GetEntry(tree1, "boot.class").GetByteString();
        byte[] bootDllData = CborTree.GetEntry(tree1, "boot.dll").GetByteString();
        byte[] jarData = CborTree.GetEntry(tree1, "jar").GetByteString();
        Misc.Log($"jarData={jarData.Length}");
        string guid = CborTree.GetEntry(tree1, "guid").AsString();
        string sha512 = CborTree.GetEntry(tree1, "sha").AsString();
        Misc.Log($"guid={guid}");
        Misc.Log($"sha512={sha512}");
        string mainClass = CborTree.GetEntry(tree1, "main").AsString();
        //var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); // C:\ProgramData
        var rootPath = $"{profilePath}\\.jwap\\.app";
        Directory.CreateDirectory(rootPath);
        //string jarPath = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}.jar";
        string appDir = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}";
        Misc.Log(appDir);
        Misc.Log("SeparateMain(3)");
        if (!Directory.Exists(appDir))
        {
            Misc.Log("SeparateMain(3.1)");
            string timestamp = GetTimeStampString();
            Directory.CreateDirectory($"{appDir}.{timestamp}");
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.class", bootClassData);
            //Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.jar", bootJarData);
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.dll", bootDllData);
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\main.jar", jarData);
            var dlls = CborTree.GetEntry(tree1, "dlls");
            Misc.Log("SeparateMain(3.2)");
            if (dlls.Type == CBORType.Map)
            {
                if (dlls.Entries.Count > 0)
                {
                    Directory.CreateDirectory($"{appDir}.{timestamp}\\dll");
                }

                var entries = dlls.Entries;
                foreach (var dll in entries)
                {
                    Misc.Log("SeparateMain(3.3)");
                    //Misc.Log(dll);
                    Misc.Log(dll.Key.AsString());
                    string dllName = dll.Key.AsString();
                    Misc.Log("SeparateMain(3.3.1)");
                    byte[] dllBinary = dll.Value.GetByteString();
                    Misc.Log("SeparateMain(3.3.2)");
                    Misc.Log($"Writing {dllName}");
                    Misc.WriteBinaryFile($"{appDir}.{timestamp}\\dll\\{dllName}", dllBinary);
                    Misc.Log("SeparateMain(3.3.3)");
                }
            }

            Misc.Log("SeparateMain(3.4)");
            Directory.Move($"{appDir}.{timestamp}", appDir);
            Misc.Log("SeparateMain(3.5)");
        }

        Misc.Log("SeparateMain(4)");
        string jre = PrepareJre(Constants.JRE_URL);
        Misc.Log(jre);

        //JniUtil.RunClassMain(jre, mainClass, args, new string[] { $"{jarPath}\\main.jar" });
        string errorMessage = JniUtil.RunClassMain(jre, mainClass, args, appDir);
        if (errorMessage != "")
        {
            Win32Api.Message(errorMessage, Path.GetFileName(Application.ExecutablePath));
            Environment.Exit(1);
        }
#if false        
        Misc.Log("SeparateMain(2)");
        byte[] buffer = Misc.GetLastUtf8Bytes(Application.ExecutablePath);
        string xml = Encoding.UTF8.GetString(buffer);
        XDocument doc = XDocument.Parse(xml);
        var root = doc.Root;
        Misc.Log(root.XPathSelectElement("./main"));
        Misc.Log(root.XPathSelectElement("./guid"));
        Misc.Log(root.XPathSelectElement("./sha512"));
        byte[] bootClassData = Convert.FromBase64String(root.XPathSelectElement("./boot.class").Value);
        //byte[] bootJarData = Convert.FromBase64String(root.XPathSelectElement("./boot.jar").Value);
        byte[] bootDllData = Convert.FromBase64String(root.XPathSelectElement("./boot.dll").Value);
        byte[] jarData = Convert.FromBase64String(root.XPathSelectElement("./jar").Value);
        Misc.Log($"jarData={jarData.Length}");
        string guid = root.XPathSelectElement("./guid").Value;
        string sha512 = root.XPathSelectElement("./sha").Value;
        Misc.Log($"guid={guid}");
        Misc.Log($"sha512={sha512}");
        string mainClass = root.XPathSelectElement("./main").Value;
        //var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); // C:\ProgramData
        var rootPath = $"{profilePath}\\.jwap\\.app";
        Directory.CreateDirectory(rootPath);
        //string jarPath = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}.jar";
        string appDir = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}";
        Misc.Log(appDir);
        Misc.Log("SeparateMain(3)");
        if (!Directory.Exists(appDir))
        {
            Misc.Log("SeparateMain(3.1)");
            string timestamp = GetTimeStampString();
            Directory.CreateDirectory($"{appDir}.{timestamp}");
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.class", bootClassData);
            //Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.jar", bootJarData);
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\boot.dll", bootDllData);
            Misc.WriteBinaryFile($"{appDir}.{timestamp}\\main.jar", jarData);
            var dlls = root.XPathSelectElements("//dll");
            Misc.Log("SeparateMain(3.2)");
            if (dlls.Count() > 0)
            {
                Directory.CreateDirectory($"{appDir}.{timestamp}\\dll");
            }
            foreach (var dll in dlls)
            {
                Misc.Log("SeparateMain(3.3)");
                //Misc.Log(dll);
                Misc.Log(dll.Elements().Count());
                Misc.Log(dll.XPathSelectElement("./name"));
                string dllName = dll.XPathSelectElement("./name").Value;
                Misc.Log("SeparateMain(3.3.1)");
                byte[] dllBinary = Convert.FromBase64String(dll.XPathSelectElement("./binary").Value);
                Misc.Log("SeparateMain(3.3.2)");
                Misc.Log($"Writing {dllName}");
                Misc.WriteBinaryFile($"{appDir}.{timestamp}\\dll\\{dllName}", dllBinary);
                Misc.Log("SeparateMain(3.3.3)");
            }

            Misc.Log("SeparateMain(3.4)");
            Directory.Move($"{appDir}.{timestamp}", appDir);
            Misc.Log("SeparateMain(3.5)");
        }

        Misc.Log("SeparateMain(4)");
        string jre = PrepareJre(Constants.JRE_URL);
        Misc.Log(jre);

        //JniUtil.RunClassMain(jre, mainClass, args, new string[] { $"{jarPath}\\main.jar" });
        string errorMessage = JniUtil.RunClassMain(jre, mainClass, args, appDir);
        if (errorMessage != "")
        {
            Win32Api.Message(errorMessage, Path.GetFileName(Application.ExecutablePath));
            Environment.Exit(1);
        }
#endif
        return;
    }

    private static void DownloadBinaryFromUrl(string url, string destinationPath)
    {
        WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
        var objResponse = objRequest.GetResponse();
        byte[] buffer = new byte[32768];
        using (Stream input = objResponse.GetResponseStream())
        {
            using (FileStream output = new FileStream(destinationPath, FileMode.CreateNew))
            {
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}
#endif
