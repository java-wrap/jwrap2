package global;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Base64;

public class BinaryToBase64 {
	public static String calculate(String filePath) {
		try (FileInputStream fis = new FileInputStream(filePath)) {
			File file = new File(filePath);
			byte[] data = new byte[(int) file.length()];
			fis.read(data);
			String base64 = Base64.getEncoder().encodeToString(data);
			return base64;
		} catch (IOException e) {
			return null;
		}
	}
}
