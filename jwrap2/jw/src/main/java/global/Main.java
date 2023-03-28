package global;

import java.io.InputStream;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.CodeSource;
import java.security.ProtectionDomain;

import org.apache.commons.cli.CommandLine;
import org.apache.commons.cli.CommandLineParser;
import org.apache.commons.cli.DefaultParser;
import org.apache.commons.cli.HelpFormatter;
import org.apache.commons.cli.Option;
import org.apache.commons.cli.Options;
import org.apache.commons.cli.ParseException;
import org.apache.commons.io.IOUtils;

import com.google.common.base.Charsets;
import com.google.common.io.Resources;

public class Main {
	public static void main(String[] args) throws Exception {
		System.out.println("Hello World!");
		String s1 = ResourceUtil.GetString("dummy.txt", Charsets.UTF_8);
		System.out.printf("s1=%s\n", s1);
		byte[] b2 = ResourceUtil.GetBinary("dummy.txt");
		String s2 = new String(b2, "UTF-8");
		System.out.printf("s2=%s\n", s2);
		String s3 = new String(b2, Charsets.UTF_8);
		System.out.printf("s3=%s\n", s3);
		// リソースの取得をシンプルに。
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

		Options options = new Options();
		Option input = new Option("i", "input", true, "input file path");
		input.setRequired(true);
		options.addOption(input);

//		options.addOption("m", true, "メールアドレス");
//
//		options.addOption(Option.builder(/*"u"*/) // オプションの名前
//				.longOpt("user").argName("serviceid") // 引数名
//				.hasArg() // 引数をとる。
//				.desc("ユーザー") // 説明
//				.build()); // インスタンスを生成
//
//		options.addOption(Option.builder("a").argName("age").required() // 必須
//				.hasArg().desc("年齢").build());

		options.addOption(Option.builder("m") // オプションの名前
				.longOpt("main").argName("mainclass") // 引数名
				.hasArg(true) // 引数をとる
				.required(false) // 引数をとる
				.desc("メインクラス") // 説明
				.build());

		options.addOption(Option.builder("i") // オプションの名前
				.longOpt("input").argName("inputfile") // 引数名
				.hasArg(true) // 引数をとる
				.required(true) // 引数をとる
				.desc("インプットjarファイル") // 説明
				.build());

		CommandLineParser parser = new DefaultParser();
		HelpFormatter formatter = new HelpFormatter();
		CommandLine cmd;

		try {
			cmd = parser.parse(options, args);
		} catch (ParseException e) {
			System.out.println(e.getMessage());
			formatter.printHelp("jwrap-gen", options);
			System.exit(1);
			return;
		}

		String inputFilePath = cmd.getOptionValue("input");
		String outputFilePath = cmd.getOptionValue("output");

//		String[] otherArgs = cmd.getArgs();
//		if (otherArgs.length > 1) {
//			System.out.println("Too many arguments.");
//			formatter.printHelp("utility-name", options);
//
//			System.exit(1);
//			return;
//		}

		System.out.println("Input file path: " + inputFilePath);
		System.out.println("Output file path: " + outputFilePath);
		System.out.println("Other arguments:");
//		for (int i = 0; i < otherArgs.length; i++) {
//			System.out.println(otherArgs[i]);
//		}

		System.setProperty("jwrap.application.path", "C:\\ProgramData\\.repo\\base2\\cmd\\jwrap-gen.exe");
		System.setProperty("jwrap.application.directory", "C:\\ProgramData\\.repo\\base2\\cmd");
		JwrapTest.main(new String[] {});
	}

	private static String getParentDirPath(String filePath) {
		Path path = Paths.get(filePath);
		Path parentPath = path.getParent();
		return parentPath.toString();
	}

	public static Path getApplicationPath(Class<?> cls) throws URISyntaxException {
		ProtectionDomain pd = cls.getProtectionDomain();
		CodeSource cs = pd.getCodeSource();
		URL location = cs.getLocation();
		URI uri = location.toURI();
		Path path = Paths.get(uri);
		return path;
	}
}
