package global;

import java.io.InputStream;
import java.net.URL;
import java.nio.charset.Charset;

import org.apache.commons.io.IOUtils;

import com.google.common.base.Charsets;
import com.google.common.io.Resources;

public class ResourceUtil {
	static void Dummy() throws Exception {
		URL resourceGuava = Resources.getResource("dummy.txt");
		InputStream in1 = resourceGuava.openStream();
		InputStream in2 = resourceGuava.openStream();
		try {
			System.out.println(IOUtils.toString(in1, "UTF-8"));
			byte[] bytes = IOUtils.toByteArray(in2);
			System.out.println(bytes.length);
			System.out.println(new String(bytes, "UTF-8"));
		} finally {
			IOUtils.closeQuietly(in1);
			IOUtils.closeQuietly(in2);
		}
	}

	public static String GetString(URL url, Charset cs) throws Exception {
//		InputStream in = url.openStream();
//		try {
//			return IOUtils.toString(in, "UTF-8");
//		} finally {
//			IOUtils.closeQuietly(in);
//		}
		return Resources.toString(url, cs);
	}

	public static String GetString(String name, Charset cs) throws Exception {
//		URL url = Resources.getResource(name);
//		return GetString(url);
		return Resources.toString(Resources.getResource(name), cs);
	}

	public static byte[] GetBinary(URL url) throws Exception {
//		InputStream in = url.openStream();
//		try {
//			return IOUtils.toByteArray(in);
//		} finally {
//			IOUtils.closeQuietly(in);
//		}
		return Resources.toByteArray(url);
	}

	public static byte[] GetBinary(String name) throws Exception {
//		URL url = Resources.getResource(name);
//		return GetBinary(url);
		return Resources.toByteArray(Resources.getResource(name));

	}

}
