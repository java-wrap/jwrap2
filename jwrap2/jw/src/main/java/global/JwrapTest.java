package global;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.Base64;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import java.util.zip.ZipOutputStream;

import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;
import com.upokecenter.cbor.JSONOptions;

public class JwrapTest {

	public static void main(String[] args) throws Exception {
		System.out.println("jwrap.application.path=" + System.getProperty("jwrap.application.path"));
		System.out.println("jwrap.application.directory=" + System.getProperty("jwrap.application.directory"));
		String jarPath = "C:\\ProgramData\\.repo\\base2\\cmd\\SwingSet2-all.jar";
		// Path path = Paths.get(jarPath);
		// long fileSize = Files.size(path);
		System.out.println("File size: " + getFileSize(jarPath) + " bytes");
		String destExePath = "C:\\ProgramData\\.repo\\base2\\SwingSet2-all.exe";
		copyFile(jarPath, destExePath);
		System.out.println("File size: " + getFileSize(destExePath) + " bytes");

		System.out.println(JarUtil.getMainClassName(jarPath));
		//var cu = new CborUtil();
		
		var zipData = CborTree.Create();
		CborTree.PutEntry(zipData, "a/b/c/d", 1234);
		System.out.println("a/b/c/d=" + CborTree.GetEntry(zipData, "a/b/c/d").ToJSONString());
		System.out.println("a/b/c/=" +  CborTree.GetEntry(zipData, "a/b/c/").ToJSONString());
		System.out.println(zipData.ToJSONString());
		CborTree.DumpTree(zipData);
		var tree = zipData;
		//var content = zipData.Content;
		System.out.println(tree.ToJSONString());
		var entries = tree.get("a").getEntries();
		for(var entry: entries)
		{
			System.out.println("key="+entry.getKey().ToJSONString());
			System.out.println("val="+entry.getValue().ToJSONString());
		}

		CBORObject list = CBORObject.NewArray();
		list.Add(123);
		list.Add("abc");
		list.Add("xyz".getBytes());

		//var conv = cu.SeparateCborBinaries(list);
		//System.out.println(conv.ToJSONString());

		CBORObject map = CBORObject.NewOrderedMap();
		map.set("a", CBORObject.FromObject(1234));
		map.set("b", CBORObject.FromObject("777".getBytes()));

		//var conv2 = cu.SeparateCborBinaries(map);
		//System.out.println(conv2.ToJSONString());

		System.out.println("(1)");
		CborUtil.PutArrayToFileEnd(destExePath, list);
		System.out.println("(2)");
		var list2 = CborUtil.GetArrayFromFileEnd(destExePath);
		System.out.println("(3)");
		System.out.println(list2.ToJSONString());
		System.out.println("(4)");

		var one = CborUtil.GetOneFromFileEnd(destExePath);
		System.out.println(one.ToJSONString());

		CborUtil.PutOneToFileEnd(destExePath, one);
		var list3 = CborUtil.GetArrayFromFileEnd(destExePath);
		System.out.println(list3.ToJSONString());

		ZipTest();
		// putLastCborArray(destExePath, list);
		// System.out.println("File size: " + getFileSize(destExePath) + " bytes");
		// System.out.printf("cborBytes.length=%d\n", cborBytes.length);
		// var bytes2 = getLastBytes(destExePath);
		// System.out.printf("bytes2.length=%d\n", bytes2.length);
		// var list2 = getLastCborArray(destExePath);
		// System.out.println(list2.size());
		// System.out.println(list2.ToJSONString());
	}

	private static void ZipTest() throws Exception {
		ByteArrayOutputStream baos = new ByteArrayOutputStream();
		ZipOutputStream zos = new ZipOutputStream(baos);

		// Zipファイルに追加するエントリを作成
		ZipEntry entry = new ZipEntry("sample.txt");
		zos.putNextEntry(entry);

		// Zipファイルに書き込むデータを設定
		String data = "This is a sample text.";
		byte[] bytes = data.getBytes();
		entry.setSize(bytes.length);

		// データをZipファイルに書き込む
		zos.write(bytes, 0, bytes.length);
		zos.closeEntry();

		// Zipファイルをクローズ
		zos.close();

		// Zipファイルのイメージをバイト配列として取得
		byte[] zipData = baos.toByteArray();
		System.out.println(zipData.length);

		WriteBinaryToFile("C:/ProgramData/tmp.zip", zipData);
		byte[] zipData2 = ReadBinaryFromFile("C:/ProgramData/tmp.zip");
		
        ByteArrayInputStream bais = new ByteArrayInputStream(zipData);
        ZipInputStream zis = new ZipInputStream(bais);
        //ZipEntry entry;
        while ((entry = zis.getNextEntry()) != null) {
            String name = entry.getName();
            System.out.println(name);
            //System.out.println(entry.getSize()); //-1
            System.out.println(entry.getLastModifiedTime());
            ///*byte[]*/ bytes = new byte[(int) entry.getSize()];
            //zis.read(bytes, 0, bytes.length);
            bytes = zis.readAllBytes();
            System.out.println(bytes.length);
            // ファイルの内容を処理する
            System.out.println(new String(bytes));
        }
        zis.close();

		System.out.println(zipData2.length);
	}

	public static void WriteBinaryToFile(String filePath, byte[] data) throws Exception {
		try (FileOutputStream fos = new FileOutputStream(filePath)) {
			fos.write(data);
		}
	}

	public static byte[] ReadBinaryFromFile(String filePath) throws Exception {
		try (FileInputStream fis = new FileInputStream(filePath)) {
			byte[] data = fis.readAllBytes();
			return data;
		}
	}



	private static long getFileSize(String filePath) throws IOException {
		Path path = Paths.get(filePath);
		return Files.size(path);
	}

	private static void copyFile(String sourcePath, String destPath) throws IOException {
		Files.copy(Paths.get(sourcePath), Paths.get(destPath), StandardCopyOption.REPLACE_EXISTING);
	}

	private static String bytesToBase64String(byte[] bytes) {
		return Base64.getEncoder().encodeToString(bytes);
	}

	/*
	 * private static void putLastBytes(String filePath, byte[] bytes) throws
	 * Exception { byte[] zero = new byte[] { 0 }; byte[] base64Bytes = new byte[0];
	 * try (FileOutputStream fos = new FileOutputStream(filePath, true)) {
	 * fos.write(zero); fos.write("[[DATA]]".getBytes("UTF-8")); fos.write(zero);
	 * String base64 = bytesToBase64String(bytes); System.out.println(base64);
	 * base64Bytes = base64.getBytes("UTF-8"); fos.write(base64Bytes); } }
	 * 
	 * private static byte[] getLastBytes(String filePath) throws Exception {
	 * RandomAccessFile file = new RandomAccessFile(filePath, "r"); long fileLength
	 * = file.length(); long lastZeroBytePosition = fileLength; while
	 * (lastZeroBytePosition > 0) { lastZeroBytePosition--;
	 * file.seek(lastZeroBytePosition); byte b = file.readByte(); if (b == 0) {
	 * break; } } byte[] bytes = new byte[0]; if (lastZeroBytePosition >= 0) {
	 * file.seek(lastZeroBytePosition + 1); bytes = new byte[(int) (fileLength -
	 * lastZeroBytePosition) - 1]; file.readFully(bytes); System.out.println(new
	 * String(bytes)); } file.close(); return Base64.getDecoder().decode(bytes); }
	 * 
	 * private static void putLastCborArray(String filePath, CBORObject list) throws
	 * Exception { if (list.getType() != CBORType.Array) { CBORObject elem = list;
	 * list = CBORObject.NewArray(); list.Add(elem); } byte[] bytes = new byte[0];
	 * try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) { for (int i
	 * = 0; i < list.size(); i++) { CBORObject.Write(list.get(i), stream); } bytes =
	 * stream.toByteArray(); } putLastBytes(filePath, bytes); }
	 * 
	 * private static CBORObject getLastCborArray(String filePath) throws Exception
	 * { byte[] bytes = getLastBytes(filePath); // List<CBORObject> list = new
	 * ArrayList<>(); CBORObject list = CBORObject.NewArray(); try
	 * (ByteArrayInputStream stream = new ByteArrayInputStream(bytes)) { while
	 * (true) { var cbor = CBORObject.Read(stream); list.Add(cbor); if
	 * (stream.available() == 0) break; } } // return list.toArray(new
	 * CBORObject[list.size()]); return list; }
	 */
}
