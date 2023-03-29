package global;

import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.CodeSource;
import java.security.ProtectionDomain;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.regex.Pattern;

import org.apache.commons.cli.CommandLine;
import org.apache.commons.cli.CommandLineParser;
import org.apache.commons.cli.DefaultParser;
import org.apache.commons.cli.HelpFormatter;
import org.apache.commons.cli.Option;
import org.apache.commons.cli.Options;
import org.apache.commons.cli.ParseException;
import org.apache.commons.io.IOUtils;
import org.bson.types.ObjectId;

import com.fasterxml.jackson.dataformat.smile.*;
import com.amazon.ion.*;
import com.amazon.ion.system.IonReaderBuilder;
import com.amazon.ion.system.IonSystemBuilder;
import com.amazon.ion.system.IonTextWriterBuilder;
import com.amazon.ion.util.IonValueUtils;
import com.google.common.base.Charsets;
import com.google.common.io.Resources;

import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;
import org.bson.Document;
import org.bson.BsonBinaryReader;
import org.bson.BsonReader;
import org.bson.RawBsonDocument;
import org.bson.BsonDocument;
import org.bson.BsonWriter;
import org.bson.codecs.EncoderContext;
import org.bson.*;
import org.bson.io.*;
import org.bson.json.JsonWriter;
import org.bson.json.JsonWriterSettings;

import java.io.FileInputStream;

import org.bson.BsonDocument;
import org.bson.BsonSerializationException;
import org.bson.BsonWriter;
import java.io.ByteArrayOutputStream;

import org.bson.BsonBinaryWriter;
import org.bson.BsonDocument;
import org.bson.BsonSerializationException;
import org.bson.ByteBufNIO;
import org.bson.codecs.BsonDocumentCodec;
import org.bson.io.BasicOutputBuffer;
import org.bson.BsonArray;
import org.bson.BsonInt32;
import org.bson.BsonString;
import org.bson.BsonArray;
import org.bson.BsonInt32;
import org.bson.BsonString;
import org.bson.json.JsonWriter;
import java.io.StringWriter;

public class Main {
	public static void main(String[] args) throws Exception {
		System.out.println("Hello World!");

        Document bsonDocument = new Document("name", "John").append("age", 30);
        String json = bsonDocument.toJson();
        System.out.println(json);
        var bd = bsonDocument.toBsonDocument();
        BsonArray array = new BsonArray();
        array.add(new BsonInt32(1));
        array.add(new BsonInt32(2));
        array.add(new BsonString("three"));
        bd.put("ary", array);
        System.out.println(bd.toJson(JsonWriterSettings.builder().indent(true).build()));
        System.out.println(BsonUtil.ToJson(bd, true));
        BsonUtil.Dump(bd);

        //		final Document doc = new Document("myKey", "myValue");
//		final String jsonString = doc.toJson();
//		final Document doc2 = Document.parse(jsonString);
//		var bsonDoc = doc.toBsonDocument();
//        StringWriter writer = new StringWriter();
//        JsonWriter jsonWriter = new JsonWriter(writer);
//        jsonWriter.writeStartArray();
//        for (int i = 0; i < array.size(); i++) {
//            jsonWriter.write(array.get(i));
//        }
//        jsonWriter.writeEndArray();
//        String json = writer.toString();
//        System.out.println(json);

		// バイト列のBSONデータ
		byte[] bsonBytes = null;
		try (FileInputStream inputStream = new FileInputStream(new File("C:/ProgramData/bson.bin"));
				ByteArrayOutputStream outputStream = new ByteArrayOutputStream()) {
			byte[] buffer = new byte[256];
			int length;
			while ((length = inputStream.read(buffer)) != -1) {
				outputStream.write(buffer, 0, length);
			}
			bsonBytes = outputStream.toByteArray();
		}
		// RawBsonDocumentを作成
		BsonDocument doc = BsonUtil.DecodeFromBytes(bsonBytes);
		System.out.println(doc);
		var bb = doc.get("c").asBinary();
		var bbBytes = bb.getData();
		System.out.println(new String(bbBytes, Charsets.UTF_8));
		byte[] bsonBytes2 = BsonUtil.EncodeToBytes(doc);
		BsonDocument doc2 = BsonUtil.DecodeFromBytes(bsonBytes2);
		System.out.println(doc2);

		System.exit(0);

		IonSystem sys1 = IonSystemBuilder.standard().build();

		IonList parent = sys1.newEmptyList();
		IonInt child = sys1.newInt(23);
		parent.add(child);
		IonStruct struct = sys1.newEmptyStruct();
		struct.put("f").newInt(3);
		struct.put("bin").newBlob("hello".getBytes(Charsets.UTF_8));
		IonList list = sys1.newEmptyList();
		list.add().newString("demo");
		parent.add(struct);
		parent.add(list);
		String pretty = parent.toPrettyString();
		System.out.println(pretty);
		var loader = sys1.getLoader();
		IonDatagram loaded = loader.load(pretty);
		System.out.println(loaded.toPrettyString());
		System.out.println(loaded.getClass().getName());
		System.out.println(loaded.size());
		System.out.println(loaded.get(0).toPrettyString());
		System.out.println(loaded.toArray()[0].getClass().getName());
		IonList list2 = (IonList) loaded.toArray()[0];
		System.out.println(list2.get(1).toString());
		var loaded2 = loader.load("{f:3}{f:4}");
		System.out.println(loaded2.size());
		System.out.println(loaded2.toPrettyString());
		var binEncoded = loaded2.getBytes();
		var binLoaded = loader.load(binEncoded);
		System.out.println(binLoaded.toPrettyString());

		// IonTextWriterBuilder textWriterBuilder = IonTextWriterBuilder.standard();
		IonTextWriterBuilder textWriterBuilder = IonTextWriterBuilder.pretty();
		ByteArrayOutputStream out = new ByteArrayOutputStream();
		String ionStr = null;
		try (IonWriter writer = textWriterBuilder.build(out)) {
			writer.stepIn(IonType.STRUCT); // step into a struct
			writer.setFieldName("hello"); // set the field name for the next value to be written
			writer.writeString("world"); // write the next value
			writer.setFieldName("my-list"); // set the field name for the next value to be written
			writer.stepIn(IonType.LIST);
			writer.writeInt(123);
			writer.stepOut(); // step out of the struct
			writer.stepOut(); // step out of the struct
			writer.close();
			ionStr = out.toString(Charsets.UTF_8);
			System.out.println(ionStr);
		}

		ByteArrayInputStream in = new ByteArrayInputStream(ionStr.getBytes(Charsets.UTF_8));
		IonReaderBuilder readerBuilder = IonReaderBuilder.standard();
		try (IonReader reader = readerBuilder.build(ionStr)) {
			reader.next(); // position the reader at the first value, a struct
			reader.stepIn(); // step into the struct
			reader.next(); // position the reader at the first value in the struct
			String fieldName = reader.getFieldName(); // retrieve the current value's field name
			String value = reader.stringValue(); // retrieve the current value's String value
			reader.stepOut(); // step out of the struct
			System.out.println(fieldName + " " + value); // prints "hello world"
		}
		System.exit(0);

		System.out.println(Adder.Add2(11, 22));
		System.out.println("abc".startsWith(""));
		String s1 = ResourceUtil.GetString("dummy.txt", Charsets.UTF_8);
		System.out.printf("s1=%s\n", s1);
		byte[] b2 = ResourceUtil.GetBinary("dummy.txt");
		String s2 = new String(b2, "UTF-8");
		System.out.printf("s2=%s\n", s2);
		String s3 = new String(b2, Charsets.UTF_8);
		System.out.printf("s3=%s\n", s3);

		var resTree = CborTree.LoadResourceTree("my_resource");
		CborTree.DumpTree(resTree);
		var dummy = CborTree.GetEntry(resTree, "my_resource/dummy.txt");
		System.out.println(new String(dummy.GetByteString(), Charsets.UTF_8));

		System.exit(0);

//		InputStream inputStream = Main.class.getResourceAsStream("/");
//		BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream));
//		String line;
//		while ((line = reader.readLine()) != null) {
//			System.out.println(line);
//		}
//
//		ClassLoader classLoader = Main.class.getClassLoader();
//		URL resource = classLoader.getResource(".");
//		File folder = new File(resource.getFile());
//		File[] listOfFiles = folder.listFiles();
//		for (File file : listOfFiles) {
//			System.out.println(file.getName());
//			if (file.isFile()) {
//				System.out.println(file.getName());
//			}
//		}

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

	private static List<String> getResourceFiles(String path) throws IOException {
		List<String> filenames = new ArrayList<>();

		URL url = Resources.getResource(path);
		InputStream in = url.openStream();
		try (
				// InputStream in = getResourceAsStream(path);
				BufferedReader br = new BufferedReader(new InputStreamReader(in))) {
			String resource;

			while ((resource = br.readLine()) != null) {
				filenames.add(resource);
			}
		}

		return filenames;
	}

//	private static InputStream getResourceAsStream(String resource) {
//	    final InputStream in
//	            = getContextClassLoader().getResourceAsStream(resource);
//
//	    return in == null ? getClass().getResourceAsStream(resource) : in;
//	}
//
//	private static ClassLoader getContextClassLoader() {
//	    return Thread.currentThread().getContextClassLoader();
//	}	
}
