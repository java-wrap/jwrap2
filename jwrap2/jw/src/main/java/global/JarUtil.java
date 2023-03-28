package global;

import java.util.jar.JarFile;
import java.util.jar.Manifest;

public class JarUtil {
	public static String getMainClassName(String jarPath) {
		try (JarFile jarFile = new JarFile(jarPath)) {
			Manifest manifest = jarFile.getManifest();
			String mainClassName = manifest.getMainAttributes().getValue("Main-Class");
			return mainClassName;
		} catch (Exception e) {
			return null;
		}
	}

}
