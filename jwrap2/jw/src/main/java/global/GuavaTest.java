package global;

import java.io.BufferedReader;
import java.io.File;
import java.net.URL;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

import com.google.common.base.CharMatcher;
import com.google.common.base.Charsets;
import com.google.common.base.Joiner;
import com.google.common.base.Joiner.MapJoiner;
import com.google.common.base.MoreObjects;
import com.google.common.base.Objects;
import com.google.common.base.Splitter;
import com.google.common.base.Strings;
import com.google.common.collect.ArrayListMultimap;
import com.google.common.collect.HashBasedTable;
import com.google.common.collect.HashMultiset;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableTable;
import com.google.common.collect.ListMultimap;
import com.google.common.collect.Lists;
import com.google.common.collect.Maps;
import com.google.common.collect.Multiset;
import com.google.common.collect.Sets;
import com.google.common.collect.Table;
import com.google.common.collect.TreeBasedTable;
import com.google.common.io.Files;
import com.google.common.io.Resources;
import com.google.common.primitives.Ints;

public class GuavaTest {

	public static void main(String[] args) throws Exception {
		// ComparisonChain compare/compareToを書くときに、メソッドチェインで書けるので、シンプル。
		// https://qiita.com/disc99/items/c4b98045fc4682cbb620

		/* Lists / Maps / Sets */

		// 冗長なインスタンス化の排除、配列のような柔軟な初期化。
		// Guava → シンプル(ただしJava7からは右辺のジェネリクスは省略できるので、、、)
		List<Object> listGuava = Lists.newArrayList();
		Map<String, Object> mapGuava = Maps.newLinkedHashMap();
		// Guava → 配列ライクな初期化
		List<String> theseElements = Lists.newArrayList("alpha", "beta", "gamma");
		Set<String> theseSet = Sets.newHashSet("alpha", "beta", "gamma");
		// Guava → 定数とか
		final List<String> constList = ImmutableList.of("dog", "cat", "pig");
		final Map<String, String> constMap = ImmutableMap.of("dog", "犬", "cat", "猫");

		/* HashMultiset */

		// setの中に含まれる、単語検索
		Multiset<String> wordsMultiset = HashMultiset.create();
		wordsMultiset.add("word1");
		wordsMultiset.add("word2");
		wordsMultiset.add("word1");
		wordsMultiset.count("word1"); // result 2

		/* Multimap */

		// Guava MultiMap
		ListMultimap<String, String> multimap = ArrayListMultimap.create();
		// keyが含まれていなければ、、というチェック無しで、単にputするだけでCollectionに追加される。
		multimap.put("key", "value");

		/* Table */

		// 2次元配列のような、2つのキーを持つテーブルを作成
		Table<String, String, Integer> table1 = HashBasedTable.create();
		Table<String, String, Integer> table2 = TreeBasedTable.create();
		// Table<String, String, Integer> universityCourseSeatTable =
		// ArrayTable.create(universityRowTable, courseColumnTables);
		Table<String, String, Integer> table3 = ImmutableTable.<String, String, Integer>builder()
				.put("Mumbai", "Chemical", 120).build();
		Integer value = table1.get("key1", "kye2");

		/* Joiner */

		// データを区切り文字で連結して一つの文字列にする。配列もOK。
		String[] animals = new String[] { "dog", "cat", null, "pig" };
		Joiner.on(", ").skipNulls().join(animals); // dog, cat, pig
		Map<String, String> dictionary = new HashMap<String, String>();
		dictionary.put("米", "rice");
		dictionary.put("パン", "bread");
		dictionary.put("うどん", null);
		MapJoiner joiner = Joiner.on(", ").withKeyValueSeparator(":").useForNull("登録なし");
		joiner.join(dictionary); // 米:rice, パン:bread, うどん:登録なし

		/* Splitter / CharMatcher(baseパッケージ) */

		// データを区切り文字で分割して一つのリストにする。
		String telnum = "090 1234 5678"; // もしくは090-1234-5678に対応
		CharMatcher matcher = CharMatcher.whitespace().or(CharMatcher.is('-'));
		Iterable<String> splits = Splitter.on(matcher).split(telnum); // 090,1234,5678という長さ3のリスト

		/* ComparisonChain */

		// compare/compareToを書くときに、メソッドチェインで書けるので、シンプル。
		// 省略

		/* Ints */

		// プリミティブ型の操作ユーティリティ。
		int[] ary = new int[] { 1, 2, 3 };
		List<Integer> list = Ints.asList(ary); // 配列からListへの変換
		int max = Ints.max(ary); // 3
		String joinedStr = Ints.join(" : ", ary); // 1 : 2 : 3

		/* Strings */

		// 文字列操作ユーティリティ。
		String str = null;
		Strings.isNullOrEmpty(str); // nullか空文字?
		Strings.repeat("*", 10); // **********
		Strings.padStart("7", 2, '0'); // 07
		Strings.padStart("12", 2, '0'); // 12
		Strings.padEnd("7", 3, '0'); // 700

		/* Objects */

		// nullを気にせずにオブジェクトの比較
		Objects.equal("a", "a"); // returns true
		Objects.equal(null, "a"); // returns false
		Objects.equal("a", null); // returns false
		Objects.equal(null, null); // returns true

		// 安全なハッシュが簡単に作成
		Object o = new Object();
		Objects.hashCode(o);

		// toStringメソッド作成の簡略化
		/*
		 * String s1 = // Returns "ClassName{x=1}" Objects.MoreObjects(this) .add("x",
		 * 1) //変数名とその値 .toString();
		 */
		String s2 = // or
				// Returns "MyObject{x=1}"
				MoreObjects.toStringHelper("MyObject").add("x", 1).toString();

		/* Resources */

		// リソースの取得をシンプルに。
		URL resourceGuava = Resources.getResource("sample.txt");

		/* Files */

		// BufferedReaderの作成や、ファイル入出力の簡略化。
		BufferedReader reader = Files.newReader(new File("sample.txt"), Charsets.UTF_8);
		// 全行リスト取得
		List<String> lines = Files.readLines(new File("sample.txt"), Charsets.UTF_8);

	}

}
