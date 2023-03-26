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
    delegate int run_class_main_proto(string x);

    public static int RunClassMain(string jreRoot, string mainClass, string[] args, string[] classPaths)
    {
        IntPtr hModule = LoadLibraryW(@"C:\Users\Public\root\.repo\base2\java\jwrap-jre\jwrap-jre.dll");
        if (hModule == IntPtr.Zero)
        {
            Console.WriteLine("Failed to load library.");
            return 1;
        }

        IntPtr pFunc = GetProcAddress(hModule, "run_class_main");
        if (pFunc == IntPtr.Zero)
        {
            Console.WriteLine("Failed to get function address.");
            FreeLibrary(hModule);
            return 1;
        }

        run_class_main_proto myFunction = Marshal.GetDelegateForFunctionPointer<run_class_main_proto>(pFunc);

        XElement argsElem = new XElement("args");
        foreach (var arg in args)
        {
            argsElem.Add(new XElement("arg", arg));
        }
        XElement classpathElem = new XElement("classpath");
        foreach (var path in classPaths)
        {
            classpathElem.Add(new XElement("path", path));
        }
        XElement items = new XElement("items");
        items.Add(new XElement("item", "abc"));
        items.Add(new XElement("item", "あああ"));
        items.Add(new XElement("item", "いいい"));
        XElement root = new XElement("root",
            new XElement("jre", jreRoot),
            //new XElement("jvm", jvmDll),
            new XElement("main", mainClass),
            items, argsElem, classpathElem);
        XDocument doc = new XDocument(root);
        
        //string format = "Hello, %s!";
        //IntPtr argList = IntPtr.Zero;
        //int result = myFunction("aaaあああ");
        int result = myFunction(doc.ToString());

        Console.WriteLine("Result: {0}", result);

        //FreeLibrary(hModule);
        return result;
    }
}
