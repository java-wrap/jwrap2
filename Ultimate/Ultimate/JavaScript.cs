#pragma warning disable CS3002
using Jint;
using System.Reflection;
namespace Ultimate
{
    public class JavaScript
    {
        public static Jint.Engine CreateEngine(params Assembly[] list)
        {
            var engine = new Jint.Engine(cfg =>
            {
                cfg.AllowClr(typeof(Ultimate.Util).Assembly);
                for (int i = 0; i < list.Length; i++)
                {
                    cfg.AllowClr(list[i]);
                }
            });
            engine.Execute(@"
var Ultimate = importNamespace('Ultimate');
var print = Ultimate.Util.Print;
var log = Ultimate.Util.Log;
");
            return engine;
        }
    }
}
