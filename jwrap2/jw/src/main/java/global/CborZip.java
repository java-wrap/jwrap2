/*
package global;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import java.util.zip.ZipOutputStream;
import java.util.Base64;
import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;

public class CborZip {
	public CBORObject Tree = CBORObject.NewMap();
	// public CBORObject Content = CBORObject.NewArray();

	public static CborZip FromZipBytes(byte[] zipData) throws Exception {
		var result = new CborZip();
		ByteArrayInputStream bais = new ByteArrayInputStream(zipData);
		ZipInputStream zis = new ZipInputStream(bais);
		ZipEntry entry;
		while ((entry = zis.getNextEntry()) != null) {
			String name = entry.getName();
			System.out.println(name);
			byte[] bytes = zis.readAllBytes();
			// CreateZipEntry(result.Tree, result.Content, entry, bytes);
			CreateZipEntry(result, entry, CBORObject.FromObject(bytes));
		}
		zis.close();
		return result;
	}

	private static void CreateZipEntry(CborZip cborZip, ZipEntry entry, CBORObject content) {
		CreateZipEntry(cborZip, entry.getName(), content);
	}

	private static void CreateZipEntry(CborZip cborZip, String entryName, CBORObject content) {
		var split = entryName.split("/");
		var parent = cborZip.Tree;
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
		if (last.startsWith("$")) {
			parent.Set(last, content.ToJSONString());
			return;
		}
		if (content.getType() == CBORType.ByteString) {
			content = CBORObject.FromObject(Base64.getEncoder().encodeToString(content.GetByteString()));
			parent.Set(last, content);
			return;
		}
		parent.Set(last, content.ToJSONString());
	}

	public void PutEntry(String path, Object x) {
		var o = CBORObject.FromObject(x);
		System.out.println("(0)" + o.getType());
		CreateZipEntry(this, path, o);
	}

	public CBORObject GetEntry(String path) {
		String entryName = path;
		var split = entryName.split("/");
		var parent = this.Tree;
		for (int i = 0; i < split.length - 1; i++) {
			var x = parent.get(split[i]);
			System.out.println("x=" + x.ToJSONString());
			if (x == null || x.getType() != CBORType.Map) {
				return CBORObject.Undefined;
			}
			parent = x;
		}
		System.out.println("(1)");
		String last = split[split.length - 1];
		System.out.println("(2)");
		if (last == "")
			return parent;
		System.out.println("(3)");
		CBORObject item = parent.get(last);
		if (item == null)
			return CBORObject.Undefined;
		if (last.startsWith("$")) {
			if (item.getType() == CBORType.ByteString) {
				return CBORObject.FromJSONBytes(item.GetByteString());
			} else if (item.getType() == CBORType.TextString) {
				return CBORObject.FromJSONString(item.AsString());
			} else {
				return item;
			}
		}
		System.out.println("(4)" + item.getType());
		if (item.getType() == CBORType.TextString) {
			System.out.println("(7)");
			return CBORObject.FromJSONString(item.AsString());
		} else {
			System.out.println("(8)");
			return item;
		}
	}

	public byte[] ToZipBytes() throws Exception {
		ByteArrayOutputStream baos = new ByteArrayOutputStream();
		ZipOutputStream zos = new ZipOutputStream(baos);
		ToZipBytes_Helper(zos, this.Tree, "");
		zos.close();
		return baos.toByteArray();
	}

	private void ToZipBytes_Helper(ZipOutputStream zos, CBORObject tree, String path) throws Exception {
		switch (tree.getType()) {
		case Map:
			if (path != "") {
				ZipEntry entry = new ZipEntry(path + "/");
				zos.putNextEntry(entry);
				zos.closeEntry();
			}
			var entries = tree.getEntries();
			for (var entry : entries) {
				var key = entry.getKey().AsString();
				String childPath;
				if (path == "") {
					childPath = key;
				} else {
					childPath = path + "/" + key;
				}
				System.out.println("childPath=" + childPath);
				ToZipBytes_Helper(zos, entry.getValue(), childPath);
			}
			break;
		case ByteString: {
			byte[] bytes = tree.GetByteString();
			ZipEntry entry = new ZipEntry(path);
			zos.putNextEntry(entry);
			zos.write(bytes);
			zos.closeEntry();
			break;
		}
		case TextString: {
			System.out.println(tree.AsString());
			byte[] bytes = Base64.getDecoder().decode(tree.AsString());
			ZipEntry entry = new ZipEntry(path);
			zos.putNextEntry(entry);
			zos.write(bytes);
			zos.closeEntry();
			break;
		}
		default:
			break;
		}
	}
}
*/