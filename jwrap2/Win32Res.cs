#if false
namespace jwrap;

using System;
using System.Runtime.InteropServices;

public class Win32Res
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr BeginUpdateResource(string pFileName,
        [MarshalAs(UnmanagedType.Bool)] bool bDeleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool UpdateResource(IntPtr hUpdate, string lpType,
        string lpName, ushort wLanguage, byte[] lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode ,SetLastError = true)]
    static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LockResource(IntPtr hResData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    public static void WriteResourceData(string fileName, string resType, string resName, byte[] data)
    {
        ushort resLang = 0x0409; // English (United States)
        IntPtr hUpdate = BeginUpdateResource(fileName, false);
        if (hUpdate == IntPtr.Zero)
        {
            throw new Exception("Failed to begin update resource.");
        }

        if (!UpdateResource(hUpdate, resType, resName, resLang, data, (uint)data.Length))
        {
            EndUpdateResource(hUpdate, true);
            throw new Exception("Failed to update resource.");
        }

        if (!EndUpdateResource(hUpdate, false))
        {
            throw new Exception("Failed to end update resource.");
        }
    }

    public static byte[] ReadResourceData(string fileName, string resType, string resName)
    {
        IntPtr hModule = LoadLibraryW(fileName);
        if (hModule == IntPtr.Zero)
        {
            throw new Exception("Failed to load library.");
        }

        IntPtr hResInfo = FindResource(hModule, resName, resType);
        if (hResInfo == IntPtr.Zero)
        {
            throw new Exception("Failed to find resource.");
        }

        uint resSize = SizeofResource(hModule, hResInfo);
        if (resSize == 0)
        {
            throw new Exception("Failed to get resource size.");
        }

        IntPtr hResData = LoadResource(hModule, hResInfo);
        if (hResData == IntPtr.Zero)
        {
            throw new Exception("Failed to load resource.");
        }

        IntPtr pResData = LockResource(hResData);
        if (pResData == IntPtr.Zero)
        {
            throw new Exception("Failed to lock resource.");
        }

        byte[] buffer = new byte[resSize];
        Marshal.Copy(pResData, buffer, 0, buffer.Length);
        return buffer;
    }
}
#endif
