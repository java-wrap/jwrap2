package global;

import org.bson.BsonBinaryWriter;
import org.bson.BsonDocument;
import org.bson.BsonNull;
import org.bson.BsonSerializationException;
import org.bson.BsonString;
import org.bson.BsonValue;
import org.bson.RawBsonDocument;
import org.bson.codecs.BsonDocumentCodec;
import org.bson.io.BasicOutputBuffer;
import org.bson.json.JsonWriterSettings;

public class BsonUtil {
	public static byte[] EncodeToBytes(BsonDocument doc) {
		BasicOutputBuffer outputBuffer = new BasicOutputBuffer();
		BsonBinaryWriter writer1 = new BsonBinaryWriter(outputBuffer);
		byte[] bsonBytes2 = null;
		try {
			new BsonDocumentCodec().encode(writer1, doc, org.bson.codecs.EncoderContext.builder().build());
			writer1.flush();
			outputBuffer.flush();
			bsonBytes2 = outputBuffer.toByteArray();
			outputBuffer.close();
		} catch (Exception e) {
			throw new BsonSerializationException("Error converting BsonDocument to byte array: " + e.getMessage());
		}
		return bsonBytes2;
	}

	public static BsonDocument DecodeFromBytes(byte[] bytes) {
		RawBsonDocument rawDoc = new RawBsonDocument(bytes);
		BsonDocument doc = rawDoc.toBsonDocument();
		return doc;
	}

	public static String ToJson(BsonDocument doc, boolean indent) {
		return doc.toJson(JsonWriterSettings.builder().indent(indent).build());
	}
	
	public static void Dump(BsonValue val) {
		var doc = new BsonDocument();
		Dump(val, val.getClass().getName());
//		doc.put(";!", new BsonString(val.getClass().getName()));
//		doc.put(";?", val);
//		System.out.println(ToJson(doc, true));
	}

	public static void Dump(BsonValue val, String title) {
		var doc = new BsonDocument();
		doc.put(";!", new BsonString(title + "(" + val.getClass().getName() + ")"));
		doc.put(";?", val);
		System.out.println(ToJson(doc, true));
	}

}
