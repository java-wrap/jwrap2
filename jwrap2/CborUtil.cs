using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using PeterO.Cbor;
using System;
using System.IO;
using System.IO.Compression;

public class CborUtil
{
    public static void PutArrayToFileEnd(string filePath, CBORObject list)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
        {
            long offset = stream.Length;
            stream.Seek(offset, SeekOrigin.Begin);
            CBORObject.Write(list.Count, stream);
            for (int i = 0; i < list.Count; i++)
            {
                CBORObject.Write(list[i], stream);
            }

            stream.WriteByte(0);
            string offsetString = $"{offset}";
            byte[] offsetBytes = Encoding.UTF8.GetBytes(offsetString);
            stream.Write(offsetBytes, 0, offsetBytes.Length);
        }
    }

    public static void PutOneToFileEnd(string filePath, CBORObject one)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
        {
            long offset = stream.Length;
            stream.Seek(offset, SeekOrigin.Begin);
            CBORObject.Write(1, stream);
            CBORObject.Write(one, stream);
            stream.WriteByte(0);
            string offsetString = $"{offset}";
            byte[] offsetBytes = Encoding.UTF8.GetBytes(offsetString);
            stream.Write(offsetBytes, 0, offsetBytes.Length);
        }
    }

    public static CBORObject GetArrayFromFileEnd(string filePath)
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
                    var count = CBORObject.Read(stream).AsInt32();
                    CBORObject list = CBORObject.NewArray();
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(CBORObject.Read(stream));
                    }

                    return list;
                }
            }

            return CBORObject.Undefined;
        }
    }

    public static CBORObject GetOneFromFileEnd(string filePath)
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
                    var count = CBORObject.Read(stream).AsInt32();
                    if (count <= 0) return CBORObject.Undefined;
                    CBORObject list = CBORObject.NewArray();
                    return CBORObject.Read(stream);
                }
            }

            return CBORObject.Undefined;
        }
    }

}