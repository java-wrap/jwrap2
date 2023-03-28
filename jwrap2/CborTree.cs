using System;
using PeterO.Cbor;

public class CborTree
{
    public static CBORObject Create()
    {
        return CBORObject.NewMap();
    }

    private static void CreateEntry(CBORObject tree, String entryName, CBORObject content)
    {
        var split = entryName.Split('/');
        var parent = tree;
        for (int i = 0; i < split.Length - 1; i++)
        {
            var x = parent[split[i]];
            if (x != null && x.Type == CBORType.Map)
            {
                parent = x;
                continue;
            }

            x = CBORObject.NewMap();
            parent[split[i]] = x;
            parent = x;
        }

        String last = split[split.Length - 1];
        if (last == "")
        {
            return;
        }

        parent.Set(last, content);
    }

    public static void PutEntry(CBORObject tree, String path, Object x)
    {
        var o = CBORObject.FromObject(x);
        CreateEntry(tree, path, o);
    }

    public static CBORObject GetEntry(CBORObject tree, String path)
    {
        String entryName = path;
        var split = entryName.Split('/');
        var parent = tree;
        for (int i = 0; i < split.Length - 1; i++)
        {
            var x = parent[split[i]];
            if (x == null || x.Type != CBORType.Map)
            {
                return CBORObject.Undefined;
            }

            parent = x;
        }

        String last = split[split.Length - 1];
        if (last == "")
            return parent;
        CBORObject item = parent[last];
        if (item == null)
            return CBORObject.Undefined;
        return item;
    }

    public static void DumpTree(CBORObject tree)
    {
        Console.WriteLine("[TREE]");
        DumpTree_Helper(tree, "");
        Console.WriteLine();
    }

    private static void DumpTree_Helper(CBORObject tree, String path)
    {
        switch (tree.Type)
        {
            case CBORType.Map:
                var entries = tree.Entries;
                foreach (var entry in entries)
                {
                    var key = entry.Key.AsString();
                    String childPath;
                    if (path == "")
                    {
                        childPath = key;
                    }
                    else
                    {
                        childPath = path + "/" + key;
                    }

                    Console.WriteLine("NODE: " + childPath);
                    DumpTree_Helper(entry.Value, childPath);
                }

                break;
            default:
                break;
        }
    }
}