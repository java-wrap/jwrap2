package global;

import java.io.RandomAccessFile;

public class MiscUtil {
	public static void PutBytesToFileEnd(String filePath, byte[] bytes) throws Exception {
		try (RandomAccessFile file = new RandomAccessFile(filePath, "rw")) {
			long offset = file.length();
			file.seek(offset);
			file.write(bytes);
			file.write(new byte[] { 0 });
			String offsetString = "" + offset;
			byte[] offsetBytes = offsetString.getBytes("UTF-8");
			file.write(offsetBytes);
			file.close();
		}
	}

	public static byte[] GetBytesFromFileEnd(String filePath) throws Exception {
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
						return bytes;
					}
				}
				return new byte[0];
			}
		}
	}
}
