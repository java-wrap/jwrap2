```
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Linq;
using Ultimate;
using Ultimate.Json.Linq;
using NUnit.Framework;
using System.Linq;
Console.WriteLine("Version: {0}", Environment.Version.ToString());
Util.Print(DateTime.Now);
{
    var json = Util.ToJson(new[] { 777, 888 });
    Util.Print(json);
    Assert.AreEqual("[777,888]", json);
}
{
    var jsonData = Util.FromObject(new[] { 777, 888 });
    Util.Print(jsonData);
    Assert.AreEqual("[777,888]", Util.ToJson(jsonData));
    Util.Print(jsonData[0]);
    Assert.AreEqual(777, (int)jsonData[0]);
    jsonData = Util.FromObject(jsonData);
    Util.Print(jsonData);
    Assert.AreEqual("[777,888]", Util.ToJson(jsonData));
    jsonData = Util.FromObject(new { a = 1, b = 2 });
    Util.Print(jsonData);
    Assert.AreEqual("{\"a\":1,\"b\":2}", Util.ToJson(jsonData));
    Util.Print(jsonData.a);
    Assert.AreEqual("1", Util.ToJson(jsonData.a));
    jsonData.a = 777;
    Util.Print(jsonData);
    Assert.AreEqual("{\"a\":777,\"b\":2}", Util.ToJson(jsonData));
    Util.Print(Util.FullName(jsonData));
    Assert.AreEqual("Ultimate.Json.Linq.JObject", Util.FullName(jsonData));
    Util.Print(jsonData.c);
    Assert.AreEqual(null, jsonData.c);
    jsonData["c"] = 888;
    Util.Print(jsonData);
    Assert.AreEqual("{\"a\":777,\"b\":2,\"c\":888}", Util.ToJson(jsonData));
}

{
    string json = @"{
  'channel': {
    'title': 'James Newton-King',
    'link': 'http://james.newtonking.com',
    'description': 'James Newton-King\'s blog.',
    'item': [
      {
        'title': 'Json.NET 1.3 + New license + Now on CodePlex',
        'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
        'link': 'http://james.newtonking.com/projects/json-net.aspx',
        'categories': [
          'Json.NET',
          'CodePlex'
        ]
      },
      {
        'title': 'LINQ to JSON beta',
        'description': 'Announcing LINQ to JSON',
        'link': 'http://james.newtonking.com/projects/json-net.aspx',
        'categories': [
          'Json.NET',
          'LINQ'
        ]
      }
    ]
  }
}";
    JObject rss = (JObject)Util.FromJson(json);
    Console.WriteLine(rss);
    JArray categories = (JArray)rss["channel"]["item"][0]["categories"];
    Console.WriteLine(categories);
    Assert.AreEqual("[\"Json.NET\",\"CodePlex\"]", Util.ToJson(categories));
}
{
    var n = Util.FromJson("18446744073709551615");
    Util.Print(n);
    Assert.AreEqual(18446744073709551615, (UInt64)n);
}
{
    dynamic flexible = new ExpandoObject();
    flexible.Int = 3;
    flexible.String = "hi";
    flexible.Deep = new ExpandoObject();
    flexible.Deep.Deeper = 777;
    var dictionary = (IDictionary<string, object>)flexible;
    dictionary.Add("Bool", false);
    Util.Print(flexible);
    Assert.AreEqual("{\"Int\":3,\"String\":\"hi\",\"Deep\":{\"Deeper\":777},\"Bool\":false}", Util.ToJson(flexible));
}
var settings = Util.FromJson("{}");
settings.a = (settings.a != null ? settings.a : Util.FromJson("{}"));
settings.a.b = 123;
settings.a.c = 456;
Util.Print(settings);
Assert.AreEqual("{\"a\":{\"b\":123,\"c\":456}}", Util.ToJson(settings));
Util.Print("quote-\ntest");
var results = JObject.Parse(@"
{ ""a"": 123 /* my commnet */}
", new JsonLoadSettings
{
    CommentHandling = CommentHandling.Load
});
Util.Print(results);
var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
<チーム>
    <メンバー情報 attr=""abc"">
        <名前>佐藤</名前>
        <住所><![CDATA[北海道札幌市]]></住所>
        <年齢>28</年齢>
        <既婚>false</既婚>
        あああ
    </メンバー情報>
    <メンバー情報 attr=""xyz"">
        <名前>山田</名前>
        <住所><![CDATA[東京都北区]]></住所>
        <年齢>30</年齢>
        <既婚>true</既婚>
    </メンバー情報>
</チーム>";
//xmlファイルを指定する
XDocument xml = Util.ParseXml(xmlString);
//メンバー情報のタグ内の情報を取得する
IEnumerable<XElement> infos = from item in xml.Root!.Elements("メンバー情報")
                              select item;
//メンバー情報分ループして、コンソールに表示
foreach (XElement info in infos)
{
    Console.Write(info.Element("名前").Value + @",");
    Console.Write(info.Element("住所").Value + @",");
    Console.Write(info.Element("年齢").Value.FromJson() + @",");
    Console.Write((bool)info.Element("既婚") + @",");
    Console.WriteLine(info.Attribute("attr").Value);
    Util.Print(info, "info");
}
Util.Print(xml.ToStringWithDeclaration());
Util.Print(infos);
Util.Print(xml);
{
    string json = @"{
  'channel': {
    'title': 'James Newton-King',
    'link': { '#text': 'http://james.newtonking.com', '@target': '_blank' },
    'description': { '#cdata-section': 'James Newton-King\'s blog.' },
    'item': [
      {
        'title': 'Json.NET 1.3 + New license + Now on CodePlex',
        'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
        'link': 'http://james.newtonking.com/projects/json-net.aspx',
        'categories': [
          'Json.NET',
          'CodePlex'
        ]
      },
      {
        'title': 'LINQ to JSON beta',
        'description': 'Announcing LINQ to JSON',
        'link': 'http://james.newtonking.com/projects/json-net.aspx',
        'categories': [
          'Json.NET',
          'LINQ'
        ]
      }
    ]
  }
}";
    JObject rss = (JObject)Util.FromJson(json);
    var xml2 = Util.ToXml(rss);
    Util.Print(xml2);
    Util.Print(Util.FromXml(xml2));
}
XElement data = new XElement("メンバー情報",
                             new XAttribute("属性1", true),
                             new XElement("名前", "田中"),
                             new XElement("住所", "大阪府大阪市"),
                             new XElement("年齢", "35"));
XElement list = new XElement("チーム", new XAttribute("属性2", DateTime.Now));
list.Add(data);
XDocument yourResult = new XDocument(list);
Util.Print(yourResult);
Util.Print(Util.FromObject(yourResult));
Util.Print((long)"0".FromJson());
Util.Print((bool)"false".FromJson());
Util.Print((bool)"true".FromJson());
Console.WriteLine(Util.AsDateTime(yourResult.Root.Attribute("属性2").Value));
Util.Print(DateTime.Now);
Util.Print(Util.AsDateTime("2022-10-29T03:34:11.1741296+09:00"));
var z = Util.FromJson("\"2022-10-29T03:34:11.1741296+09:00\"");
var zs = (string)z;
Util.Print(zs);
Util.Print(Util.AsDateTime(z));
var zz1 = Util.FromObject(new { t = DateTime.Now });
Util.Print(zz1);
var zz2 = Util.FromObject(new { a = new[] { DateTime.Now } });
Util.Print(zz2);
var zz3 = Util.FromObject(DateTime.Now);
Util.Print(zz3);
Util.Print((string)zz3);
Util.Print(Util.AsDateTime(zz3));
Util.Print(Util.FullName(zz3));
Util.Print(Util.FullName(null));

Util.Print(Util.FreeTcpPort());
Util.Print(Util.FreeTcpPort());
```
