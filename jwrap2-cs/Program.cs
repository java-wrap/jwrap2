using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Ultimate;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var e = new
            {
                Name = "Movie Premiere",
                StartDate = new DateTime(2013, 1, 22, 20, 30, 0, DateTimeKind.Utc)
            };

            JObject myStruct = new JObject();
            myStruct["a"] = "xyz";
            myStruct["b"] = new DateTime(2013, 1, 22, 20, 30, 0, DateTimeKind.Utc);
            myStruct["c"] = Encoding.UTF8.GetBytes("abcハロー©");
            myStruct["d"] = 9223372036854775807;

            var cBytes = (byte[])myStruct["c"];
            Console.WriteLine(Encoding.UTF8.GetString(cBytes));

            byte[] bytes1 = BsonUtil.EncodeToBytes(myStruct);
            string data = Convert.ToBase64String(bytes1);
            Console.WriteLine(data);   
            
            File.WriteAllBytes("C:/ProgramData/bson.bin",bytes1);

            JObject obj = BsonUtil.DecodeFromBytes(bytes1);

            BsonUtil.PutToFileEnd("C:/ProgramData/dummy.bin", myStruct);
            JObject obj2 = BsonUtil.GetFromFileEnd("C:/ProgramData/dummy.bin");
            Console.WriteLine(obj2);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}
