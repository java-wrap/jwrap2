#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ultimate.Json;
using Ultimate.Json.Bson;
using Ultimate.Json.Linq;
using Formatting = Ultimate.Json.Formatting;
namespace Ultimate
{
    public partial class Util
    {
        static Util()
        {
        }
        public static string AssemblyName(Assembly assembly)
        {
            return System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Name;
        }
        public static int FreeTcpPort()
        {
            // https://stackoverflow.com/questions/138043/find-the-next-tcp-port-in-net
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        public static DateTime? AsDateTime(dynamic x)
        {
            if (x == null) return null;
            string fullName = Util.FullName(x);
            if (fullName == "Ultimate.Json.Linq.JValue")
            {
                return ((DateTime)x);
            }
            else if (fullName == "System.DateTime")
            {
                return (System.DateTime)x;
            }
            else if (fullName == "System.String")
            {
                if (((string)x) == "") return null;
                return DateTime.Parse((string)x);
            }
            else
            {
                throw new ArgumentException("x");
            }
        }
        public static string FullName(dynamic x)
        {
            if (x == null) return "null";
            string fullName = ((object)x).GetType().FullName;
            return fullName;
        }
        public static string ToJson(dynamic x, bool indent = false)
        {
            return JsonConvert.SerializeObject(x, indent ? Formatting.Indented : Formatting.None);
        }
        public static dynamic? FromJson(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
        }
        public static T? FromJson<T>(string json, T? fallback = default(T))
        {
            if (String.IsNullOrEmpty(json)) return fallback;
            return JsonConvert.DeserializeObject<T>(json);
        }
    public static byte[] ToBson(dynamic x)
    {
        MemoryStream ms = new MemoryStream();
        using (BsonWriter writer = new BsonWriter(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, x);
        }
        return ms.ToArray();
    }
    public static dynamic? FromBson(byte[] bson)
    {
        if (bson == null) return null;
        MemoryStream ms = new MemoryStream(bson);
        using (BsonReader reader = new BsonReader(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize(reader);
        }
    }
    public static T? FromBson<T>(byte[] bson)
    {
        if (bson == null) return default(T);
        MemoryStream ms = new MemoryStream(bson);
        using (BsonReader reader = new BsonReader(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(reader);
        }
    }
        public static dynamic? FromObject(dynamic x)
        {
            if (x == null) return null;
            var o = (dynamic)JObject.FromObject(new { x = x },
                               new JsonSerializer
                               {
                                   DateParseHandling = DateParseHandling.None
                               });
            return o.x;
        }
        public static T? FromObject<T>(dynamic x)
        {
            dynamic? o = FromObject(x);
            if (o == null) return default(T);
            return (T)(o.ToObject<T>());
        }
        public static string? ToXml(dynamic x)
        {
            if (x == null) return null;
            if (FullName(x) == "System.Xml.Linq.XElement")
            {
                return ((XElement)x).ToString();
            }
            XDocument? doc;
            if (FullName(x) == "System.Xml.Linq.XDocument")
            {
                doc = (XDocument)x;
            }
            else
            {
                string json = ToJson(x);
                doc = JsonConvert.DeserializeXmlNode(json)?.ToXDocument();
                //return "<?>";
            }
            return doc == null ? "null" : doc.ToStringWithDeclaration();
        }
        public static XDocument? FromXml(string xml)
        {
            if (xml == null) return null;
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }
        public static string ToString(dynamic x)
        {
            if ((x as string) != null)
            {
                var s = (string)x;
                return s;
            }
            if (FullName(x) == "Ultimate.Json.Linq.JValue")
            {
                var value = (Ultimate.Json.Linq.JValue)x;
                try
                {
                    x = (DateTime)value;
                }
                catch (Exception)
                {
                }
            }
            if (FullName(x) == "System.Xml.Linq.XDocument" || FullName(x) == "System.Xml.Linq.XElement")
            {
                string xml = ToXml(x);
                return xml;
            }
            else if (FullName(x) == "System.DateTime")
            {
                return x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
            }
            else
            {
                try
                {
                    string json = ToJson(x, true);
                    return json;

                }
                catch (Exception)
                {
                    return x.ToString();
                }
            }
        }

        public static void Print(dynamic x, string? title = null)
        {
            if (title != null) Console.Write(title + ": ");
            Console.WriteLine(Util.ToString(x));
        }
        public static void Log(dynamic x, string? title = null)
        {
            if (title != null) Console.Error.Write(title + ": ");
            Console.Error.WriteLine(Util.ToString(x));
        }
        public static XDocument ParseXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }
        public static string[] ResourceNames(Assembly assembly)
        {
            return assembly.GetManifestResourceNames();
        }
        public static Stream? ResourceAsStream(Assembly assembly, string name)
        {
            Stream? stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            return stream;
        }
        public static string StreamAsText(Stream stream)
        {
            if (stream == null) return "";
            var streamReader = new StreamReader(stream);
            var text = streamReader.ReadToEnd();
            return text;
        }
        public static string ResourceAsText(Assembly assembly, string name)
        {
            Stream stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            return StreamAsText(stream);
        }
        public static byte[] StreamAsBytes(Stream stream)
        {
            if (stream == null) return new byte[] { };
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return bytes;
        }
        public static byte[] ResourceAsBytes(Assembly assembly, string name)
        {
            Stream stream = assembly.GetManifestResourceStream($"{AssemblyName(assembly)}.{name}");
            return StreamAsBytes(stream);
        }
        public static dynamic? StreamAsJson(Stream stream)
        {
            string json = StreamAsText(stream);
            return FromJson(json);
        }
        public static dynamic? ResourceAsJson(Assembly assembly, string name)
        {
            string json = ResourceAsText(assembly, name);
            return FromJson(json);
        }
        public static string FirstPart(string s, params char[] separator)
        {
            string[] split = s.Split(separator);
            if (split.Length == 0) return "";
            return split[0];
        }
        public static string LastPart(string s, params char[] separator)
        {
            string[] split = s.Split(separator);
            if (split.Length == 0) return "";
            return split[split.Length - 1];
        }
        public static string GetRidirectUrl(string url)
        {
            Task<string> task = GetRidirectUrlTask(url);
            task.Wait();
            return task.Result;
        }
        private static async Task<string> GetRidirectUrlTask(string url)
        {
            HttpClient client;
            HttpResponseMessage response;
            try
            {
                client = new HttpClient();
                response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return url;
            }
            string result = response.RequestMessage.RequestUri.ToString();
            response.Dispose();
            return result;
        }
        public static byte[]? ToUtf8Bytes(string? s)
        {
            if (s == null) return null;
            byte[] bytes= System.Text.Encoding.UTF8.GetBytes(s);
            return bytes;
        }
    }
}
