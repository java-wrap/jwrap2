using System.IO;
using System.Windows.Forms;

namespace jwrap;

using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;

public class JniUtil
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    static extern bool FreeLibrary(IntPtr hModule);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    delegate IntPtr run_class_main_proto(string x);

    public static string RunClassMain(string jreRoot, string mainClass, string[] args, string appDir)
    {
        IntPtr hModule = LoadLibraryW($"{appDir}\\boot.dll");
        if (hModule == IntPtr.Zero)
        {
            return "Failed to load library.";
        }

        IntPtr pFunc = GetProcAddress(hModule, "run_class_main");
        if (pFunc == IntPtr.Zero)
        {
            FreeLibrary(hModule);
            return "Failed to get function address.";
        }

        run_class_main_proto myFunction = Marshal.GetDelegateForFunctionPointer<run_class_main_proto>(pFunc);

        string appPathProp = Application.ExecutablePath;
        string appDirProp = Directory.GetParent(Application.ExecutablePath).FullName;

        var appPathElem = new XElement("prop", appPathProp);
        appPathElem.SetAttributeValue("name", "jwrap.application.path");
        var appDirElem = new XElement("prop", appDirProp);
        appDirElem.SetAttributeValue("name", "jwrap.application.directory");
        XElement propsElem = new XElement("props", appPathElem, appDirElem);

        XElement argsElem = new XElement("args");
        foreach (var arg in args)
        {
            argsElem.Add(new XElement("arg", arg));
        }

#if false
        XElement classpathElem = new XElement("classpath");
        foreach (var path in new string[] { $"{appDir}\\main.jar" })
        {
            classpathElem.Add(new XElement("path", path));
        }
#endif

        XElement items = new XElement("items");
        XElement root = new XElement("root",
            new XElement("jre", jreRoot),
            new XElement("jar", $"{appDir}\\main.jar"),
            new XElement("main", mainClass),
            new XElement("boot.class", $"{appDir}\\boot.class"),
            new XElement("boot.jar", $"{appDir}\\boot.jar"),
            items, argsElem, propsElem);
        XDocument doc = new XDocument(root);

        Console.WriteLine(doc.ToString());
        IntPtr result = myFunction(doc.ToString());
        string str = Marshal.PtrToStringUni(result);

        //FreeLibrary(hModule);
        return str;
    }
}