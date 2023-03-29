using System.IO;
using System.Text;

namespace Ultimate;

public static class MiscUtil
{
    public static void PutBytesToFileEnd(string filePath, byte[] bytes)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
        {
            long offset = stream.Length;
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
            string offsetString = $"{offset}";
            byte[] offsetBytes = Encoding.UTF8.GetBytes(offsetString);
            stream.Write(offsetBytes, 0, offsetBytes.Length);
        }
    }

    public static byte[] GetBytesFromFileEnd(string filePath)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            long offset = stream.Length;
            while (offset > 0)
            {
                offset--;
                stream.Seek(offset, SeekOrigin.Begin);
                int b = stream.ReadByte();
                if (b == 0)
                {
                    offset++;
                    stream.Seek(offset, SeekOrigin.Begin);
                    byte[] offsetBytes = new byte[stream.Length - offset];
                    stream.Read(offsetBytes, 0, offsetBytes.Length);
                    string offsetString = Encoding.UTF8.GetString(offsetBytes);
                    long start = long.Parse(offsetString);
                    stream.Seek(start, SeekOrigin.Begin);
                    byte[] result = new byte[(offset - start - 1)];
                    stream.Read(result, 0, result.Length);
                    return result;
                }
            }

            return new byte[0];
        }
    }

}