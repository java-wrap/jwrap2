using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Ultimate;

public static class BsonUtil
{
    public static byte[] EncodeToBytes(JObject x)
    {
        MemoryStream ms = new MemoryStream();
        using (BsonWriter writer = new BsonWriter(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, x);
        }

        return ms.ToArray();
    }

    public static JObject DecodeFromBytes(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                var obj = serializer.Deserialize(reader);
                return (JObject)obj;
            }
        }
    }

    public static void PutToFileEnd(string filePath, JObject x)
    {
        byte[] bytes = EncodeToBytes(x);
        MiscUtil.PutBytesToFileEnd(filePath, bytes);
    }

    public static JObject GetFromFileEnd(string filePath)
    {
        byte[] bytes = MiscUtil.GetBytesFromFileEnd(filePath);
        return DecodeFromBytes(bytes);
    }
}