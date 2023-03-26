using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.XPath;

#if !JWRAP_GEN
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
            Console.Error.WriteLine(e.ToString());
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
        var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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

#if JWRAP_HEAD
    private static void SeparateMain(string[] args)
    {
        Misc.Log("SeparateMain(1)");
        string argList = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) argList += " ";
            argList += $"\"{args[i]}\"";
        }

        Misc.Log("SeparateMain(2)");
        byte[] buffer = Misc.GetLastUtf8Bytes(Application.ExecutablePath);
        //string xml = Encoding.UTF8.GetString(Win32Res.ReadResourceData(Application.ExecutablePath, "JWRAP", "XML"));
        string xml = Encoding.UTF8.GetString(buffer);
        XDocument doc = XDocument.Parse(xml);
        var root = doc.Root;
        Misc.Log(root.XPathSelectElement("./main"));
        Misc.Log(root.XPathSelectElement("./guid"));
        Misc.Log(root.XPathSelectElement("./sha512"));
        byte[] jarData = Convert.FromBase64String(root.XPathSelectElement("./jar").Value);
        Misc.Log($"jarData={jarData.Length}");
        string guid = root.XPathSelectElement("./guid").Value;
        string sha512 = root.XPathSelectElement("./sha512").Value;
        Misc.Log($"guid={guid}");
        Misc.Log($"sha512={sha512}");
        string mainClass = root.XPathSelectElement("./main").Value;
        var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var rootPath = $"{profilePath}\\.jwap\\.jar";
        Directory.CreateDirectory(rootPath);
        //string jarPath = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}.jar";
        string jarPath = $"{rootPath}\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}+{guid}+{sha512}";
        Misc.Log(jarPath);
        Misc.Log("SeparateMain(3)");
        if (!Directory.Exists(jarPath))
        {
            Misc.Log("SeparateMain(3.1)");
            string timestamp = GetTimeStampString();
            Directory.CreateDirectory($"{jarPath}.{timestamp}");
            Misc.WriteBinaryFile($"{jarPath}.{timestamp}\\main.jar", jarData);
            var dlls = root.XPathSelectElements("//dll");
            Misc.Log("SeparateMain(3.2)");
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
                Misc.WriteBinaryFile($"{jarPath}.{timestamp}\\{dllName}", dllBinary);
                Misc.Log("SeparateMain(3.3.3)");
            }

            Misc.Log("SeparateMain(3.4)");
            Directory.Move($"{jarPath}.{timestamp}", jarPath);
            Misc.Log("SeparateMain(3.5)");
        }

        Misc.Log("SeparateMain(4)");
        string jre = PrepareJre(Constants.JRE_URL);
        Misc.Log(jre);

        JniUtil.RunClassMain(jre, mainClass, args, new string[] { $"{jarPath}\\main.jar" });

        return;
        
#if false
        string java = $@"{jre}\bin\java.exe";
        Misc.Log(java);
        Misc.Log(File.Exists(java));
        Misc.Log(mainClass);
        //string jarFile = Regex.Replace(Application.ExecutablePath, "[.][eE][xX][eE]$", ".jar");
        Misc.Log("SeparateMain(5)");
        ProcessStartInfo psi = new ProcessStartInfo(java, $"-cp \"{jarPath}\\main.jar\" {mainClass} {argList}");
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        Process process = new Process();
        process.StartInfo = psi;
        //Process process = new Process();
        //process.StartInfo.FileName = java;
        //process.StartInfo.Arguments = $"-cp \"{jarFile}\" global.Main {argList}";
        //process.StartInfo.UseShellExecute = false;
        process.OutputDataReceived += (sender, e) =>
        {
            Console.WriteLine(e.Data);
            try
            {
                using (StreamWriter writer = File.AppendText(Application.ExecutablePath + ".log"))
                {
                    writer.WriteLine(e.Data);
                }
            }
            catch (Exception /*exception*/)
            {
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            Console.WriteLine(e.Data);
            try
            {
                using (StreamWriter writer = File.AppendText(Application.ExecutablePath + ".log"))
                {
                    writer.WriteLine(e.Data);
                }
            }
            catch (Exception /*exception*/)
            {
            }
        };
        Misc.Log("SeparateMain(6)");
        try
        {
            File.Delete(Application.ExecutablePath + ".log");
        }
        catch (Exception /*e*/)
        {
        }

        Misc.Log("SeparateMain(7)");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        Misc.Log("SeparateMain(8)");
        Environment.Exit(process.ExitCode);
#endif
    }
#else
    private static void SeparateMain(string[] args)
    {
        Misc.Log("(1)");
        string jarPath = new FileInfo(args[0]).FullName;
        jarPath = Convert.ToBase64String(Encoding.UTF8.GetBytes(jarPath));
        ArraySegment<string> arySeg = new ArraySegment<string>(args, 1, args.Length - 1);
        args = arySeg.ToArray();
        string argList = "";
        for (int i = 0; i < args.Length; i++)
        {
            //if (i > 0) argList += " ";
            //argList += $"\"{args[i]}\"";
            if (i > 0) argList += ",";
            argList += Convert.ToBase64String(Encoding.UTF8.GetBytes(args[i]));
        }
        Misc.Log("["+argList+"]");
        string jre = PrepareJre(Constants.JRE_URL);
        string java = $@"{jre}\bin\java.exe";
        string exeDir = Directory.GetParent(Application.ExecutablePath).FullName;
        string bootJar = $"{exeDir}\\jwrap.boot.jar";
        string mainClass = "global.Main";
        mainClass = Convert.ToBase64String(Encoding.UTF8.GetBytes(mainClass));
        
        Misc.Log("(2)");
        ProcessStartInfo psi =
 new ProcessStartInfo(java, $"-cp \"{bootJar}\" jwrap.boot.App --debug {Constants.DEBUG} --jar {jarPath} --main {mainClass} --args \"{argList}\"");
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        Process process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, e) =>
        {
            Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            Console.WriteLine(e.Data);
        };

        Misc.Log("(3)");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        Misc.Log("(4)");
        Environment.Exit(process.ExitCode);
    }
#endif
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