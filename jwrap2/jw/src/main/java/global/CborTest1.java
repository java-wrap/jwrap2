package global;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.text.MessageFormat;
import java.util.Date;

import com.upokecenter.cbor.CBORObject;
import com.upokecenter.cbor.CBORType;

public class CborTest1 {

	public static void main(String[] args) throws Exception {
		try (FileOutputStream stream = new FileOutputStream("object.cbor")) {
			CBORObject.Write(true, stream);
			CBORObject.Write(422.5, stream);
			CBORObject.Write(9223372036854775807L, stream);
			CBORObject.Write("some string", stream);
			CBORObject.Write(CBORObject.Undefined, stream);
			CBORObject.NewArray().Add(42).WriteTo(stream);
			CBORObject.NewOrderedMap().Add("a", 123).WriteTo(stream);
			CBORObject.Write(new Date(), stream);
		}
		try (FileInputStream stream = new FileInputStream("object.cbor")) {
			while (true) {
				var cbor = CBORObject.Read(stream);
				System.out.printf("[%s]\n", cbor.getType());
				System.out.println(cbor.ToJSONString());
				if (cbor.getType() == CBORType.Map) {
					System.out.printf("value=%s\n", cbor.get("a"));
				}
				if (stream.getChannel().position() == stream.getChannel().size())
					break;
			}
		}
	}

}
