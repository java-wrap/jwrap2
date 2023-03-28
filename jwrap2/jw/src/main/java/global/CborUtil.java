package global;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.RandomAccessFile;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;

public class CborUtil {


	public static void PutArrayToFileEnd(String filePath, CBORObject list) throws Exception {
		try (RandomAccessFile file = new RandomAccessFile(filePath, "rw")) {
			long offset = file.length();
			file.seek(offset);
			byte[] cborBytes;
			try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) {
				CBORObject.Write(list.size(), stream);
				for (int i = 0; i < list.size(); i++) {
					CBORObject.Write(list.get(i), stream);
				}
				cborBytes = stream.toByteArray();
			}
			file.write(cborBytes);
			file.write(new byte[] { 0 });
			String offsetString = "" + offset;
			byte[] offsetBytes = offsetString.getBytes("UTF-8");
			file.write(offsetBytes);
			file.close();
		}
	}

	public static void PutOneToFileEnd(String filePath, CBORObject one) throws Exception {
		try (RandomAccessFile file = new RandomAccessFile(filePath, "rw")) {
			long offset = file.length();
			file.seek(offset);
			byte[] cborBytes;
			try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) {
				CBORObject.Write(1, stream);
				CBORObject.Write(one, stream);
				cborBytes = stream.toByteArray();
			}
			file.write(cborBytes);
			file.write(new byte[] { 0 });
			String offsetString = "" + offset;
			byte[] offsetBytes = offsetString.getBytes("UTF-8");
			file.write(offsetBytes);
			file.close();
		}
	}

	public static CBORObject GetArrayFromFileEnd(String filePath) throws Exception {
		try (RandomAccessFile file = new RandomAccessFile(filePath, "r")) {
			{
				long offset = file.length();
				while (offset > 0) {
					offset--;
					file.seek(offset);
					byte b = file.readByte();
					if (b == 0) {
						offset++;
						file.seek(offset);
						byte[] offsetBytes = new byte[(int) (file.length() - offset)];
						file.readFully(offsetBytes);
						String offsetString = new String(offsetBytes, "UTF-8");
						long start = Long.parseLong(offsetString);
						file.seek(start);
						byte[] bytes = new byte[(int) (file.length() - start)];
						file.readFully(bytes);
						CBORObject list = CBORObject.NewArray();
						try (ByteArrayInputStream stream = new ByteArrayInputStream(bytes)) {
							var count = CBORObject.Read(stream).AsInt32();
							if (count <= 0)
								return CBORObject.Undefined;
							for (int i = 0; i < count; i++) {
								var cbor = CBORObject.Read(stream);
								list.Add(cbor);
							}
						}
						return list;
					}
				}
				return CBORObject.Undefined;
			}
		}
	}

	public static CBORObject GetOneFromFileEnd(String filePath) throws Exception {
		try (RandomAccessFile file = new RandomAccessFile(filePath, "r")) {
			{
				long offset = file.length();
				while (offset > 0) {
					offset--;
					file.seek(offset);
					byte b = file.readByte();
					if (b == 0) {
						offset++;
						file.seek(offset);
						byte[] offsetBytes = new byte[(int) (file.length() - offset)];
						file.readFully(offsetBytes);
						String offsetString = new String(offsetBytes, "UTF-8");
						long start = Long.parseLong(offsetString);
						file.seek(start);
						byte[] bytes = new byte[(int) (file.length() - start)];
						file.readFully(bytes);
						CBORObject list = CBORObject.NewArray();
						try (ByteArrayInputStream stream = new ByteArrayInputStream(bytes)) {
							var count = CBORObject.Read(stream).AsInt32();
							if (count <= 0)
								return CBORObject.Undefined;
							var cbor = CBORObject.Read(stream);
							return cbor;
						}
					}
				}
				return CBORObject.Undefined;
			}
		}
	}

}
