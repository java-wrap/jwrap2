package global;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import java.util.zip.ZipOutputStream;
import java.util.Base64;
import java.util.Collection;
import java.util.regex.Pattern;

import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;
import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;

public class CborTree {
	// public CBORObject Tree = CBORObject.NewMap();
	public static CBORObject Create() {
		return CBORObject.NewMap();
	}

	private static void CreateEntry(CBORObject tree, String entryName, CBORObject content) {
		var split = entryName.split("/");
		var parent = tree;
		for (int i = 0; i < split.length - 1; i++) {
			var x = parent.get(split[i]);
			if (x != null && x.getType() == CBORType.Map) {
				parent = x;
				continue;
			}
			x = CBORObject.NewMap();
			parent.set(split[i], x);
			parent = x;
		}
		String last = split[split.length - 1];
		if (last == "") {
			return;
		}
		parent.Set(last, content);
	}

	public static void PutEntry(CBORObject tree, String path, Object x) {
		var o = CBORObject.FromObject(x);
		CreateEntry(tree, path, o);
	}

	public static CBORObject GetEntry(CBORObject tree, String path) {
		String entryName = path;
		var split = entryName.split("/");
		var parent = tree;
		for (int i = 0; i < split.length - 1; i++) {
			var x = parent.get(split[i]);
			if (x == null || x.getType() != CBORType.Map) {
				return CBORObject.Undefined;
			}
			parent = x;
		}
		String last = split[split.length - 1];
		if (last == "")
			return parent;
		CBORObject item = parent.get(last);
		if (item == null)
			return CBORObject.Undefined;
		return item;
	}

	public static void DumpTree(CBORObject tree) throws Exception {
		System.out.println("[TREE]");
		DumpTree_Helper(tree, "");
		System.out.println();
	}

	private static void DumpTree_Helper(CBORObject tree, String path) throws Exception {
		switch (tree.getType()) {
		case Map:
			var entries = tree.getEntries();
			for (var entry : entries) {
				var key = entry.getKey().AsString();
				String childPath;
				if (path == "") {
					childPath = key;
				} else {
					childPath = path + "/" + key;
				}
				System.out.println("NODE: " + childPath);
				DumpTree_Helper(entry.getValue(), childPath);
			}
			break;
		default:
			break;
		}
	}

	public static CBORObject LoadResourceTree(String path) throws Exception {
		var result = CborTree.Create();
		if (path == null || path == ".") {
			path = "";
		} else if (!path.endsWith("/")) {
			path = path + "/";
		}
		Pattern pattern;
		pattern = Pattern.compile(".*");
		final Collection<String> list = ResourceList.getResources(pattern);
		for (final String name : list) {
			if (name.startsWith(path))
			{
				byte[] bytes = null;
				if (!name.endsWith("/")) {
					bytes = ResourceUtil.GetBinary(name);
				}
				CborTree.PutEntry(result, name, bytes);
			}
		}
		return result;
	}
}
