#if false
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace jwrap;

public class Win32Api
{
    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    static extern int LoadStringA(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int LoadStringW(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

    public static string LoadString(uint uID)
    {
        int longest = 0;
        int buffLen = 256;
        int length = 0;
        IntPtr hInstance = Marshal.GetHINSTANCE(typeof(Win32Api).Module);
        StringBuilder sb;
        while (true)
        {
            //Print(buffLen, "buffLen");
            sb = new StringBuilder(buffLen); // 読み込んだ文字列を格納するバッファ
            length = LoadStringW(hInstance, uID, sb, sb.Capacity);
            if (length == longest) break;
            longest = length;
            buffLen = buffLen * 2;
        }

        string s = sb.ToString(0, length);
        return s;
    }
}
#endif
