using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ultimate
{
    public static class Extensions
    {
        public static dynamic? FromJson(this string x)
        {
            if (x == null) return null;
            return Util.FromJson(x);
        }
        public static T? FromJson<T>(this string x, T? fallback = default(T))
        {
            return Util.FromJson<T>(x, fallback);
        }
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            using (TextWriter writer = new Utf8StringWriter())
            {
                doc.Save(writer);
                return writer.ToString();
            }
        }
        internal class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}
