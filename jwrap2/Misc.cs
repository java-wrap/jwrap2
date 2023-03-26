using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace jwrap;

public class Misc
{
    public static void Log(object msg)
    {
        if (Constants.DEBUG)
        {
            if (msg == null)
                Console.Error.WriteLine("null");
            else
                Console.Error.WriteLine(msg);
        }
    }

    public static byte[] ReadBinaryFile(string filePath)
    {
        // ファイルを開く
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // バイナリデータを読み取る
            using (BinaryReader br = new BinaryReader(fs))
            {
                // ファイルのサイズを取得
                int fileSize = (int)fs.Length;
                // バイナリデータをbyte[]に読み込む
                byte[] data = br.ReadBytes(fileSize);
                // 読み込んだデータを表示
                //Console.WriteLine(BitConverter.ToString(data));
                return data;
            }
        }
    }

    public static void WriteBinaryFile(string filePath, byte[] data)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(data);
            }
        }
    }

    public static string GetGuidString()
    {
        Guid guid = Guid.NewGuid();
        return guid.ToString();
    }

    public static string GetSha512String(byte[] data)
    {
        //using (SHA512 shaM = new SHA512Managed())
        using (SHA256 shaM = new SHA256Managed())
        {
            byte[] hashBytes = shaM.ComputeHash(data);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }

    public static byte[] GetLastUtf8Bytes(string filePath)
    {
        byte[] fileData = Misc.ReadBinaryFile(filePath);
        long position = fileData.Length;
        while (position > 0)
        {
            position--;
            if (fileData[position] == 0)
            {
                position++;
                break;
            }
        }
        byte[] buffer = new byte[fileData.Length - position];
        Array.Copy(fileData, position, buffer, 0, buffer.Length);
        return buffer;
    }

    public static void PutLastUtf8Bytes(string filePath, byte[] bytes)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
        {
            fs.Seek(fs.Length, SeekOrigin.Begin);
            fs.WriteByte(0);
            fs.Write(bytes, 0, bytes.Length);
        }
    }
}