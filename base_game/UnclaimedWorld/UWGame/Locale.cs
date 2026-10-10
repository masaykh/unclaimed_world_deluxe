using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace UWGame;

/// <summary>
/// The studio's string tables: <c>data/BaseData/Strings/&lt;language&gt;.xml</c>, a list of key and
/// value pairs, keys prefixed by area - "(GUI)" and the English text itself, in the studio's file.
///
/// PORT: the studio built this and never used it. Get had no caller, Init ran only when a Sim
/// started (the main menu was up long before), nothing chose a language, and Get threw for a key
/// missing from English (US).xml. Kastuk, "Translation", 2026-10-09: "Start with adding
/// localisation keys to strings of main menu and in-game menu, options. Then to names and
/// descriptions of items". So:
///
/// - <see cref="Text(string)"/> and <see cref="Text(string, string)"/> take the English with the
///   key, and give the chosen language's value when it has one, the English otherwise. They never
///   throw, and load the table on first use.
/// - The language is PORT -> LANGUAGE (PortSettings.Language), one entry per file in Strings.
/// - English (US).xml is the template for translators, written by tools/build/37-make-strings.sh
///   from the code (every literal passed to Text) and the tables (settings, items). English text
///   always comes from the code, so a stale template cannot change what English players see.
/// </summary>
public partial class Locale
{
	public const string InvariantCulture = "English (US)";

	private static Dictionary<string, string> InvariantStrings;

	private static Dictionary<string, string> CurrentStrings;

	private static string currentCulture;

	public static void Init()
	{
		Load(ref InvariantStrings, InvariantCulture);
		CurrentStrings = null;
		currentCulture = null;
		UseChosenCulture();
	}

	/// <summary>
	/// The studio's lookup, as it was: throws for a key in neither table. Use <see cref="Text(string, string)"/>.
	/// </summary>
	public static string Get(string key)
	{
		if (CurrentStrings != null && CurrentStrings.TryGetValue(ShortKey(key), out var value))
		{
			return value;
		}
		return InvariantStrings[ShortKey(key)];
	}

	/// <summary>How long a key's readable part may be before it is cut and given a hash.</summary>
	public const int ShortKeyLength = 50;

	private static readonly ConcurrentDictionary<string, string> shortKeys = new ConcurrentDictionary<string, string>();

	/// <summary>
	/// PORT: the key a language file uses for the code's key. Kastuk and tripleacoder,
	/// "Translation", 2026-10-09: keys without spaces, letters and digits only, PascalCase, about 50
	/// characters readable, "50+hash12 in case of doubles". The code still builds its keys as
	/// before - "(GUI)" and the English, "(ITEM DESCRIPTION)" and a KeyName - and only the files
	/// use this form:
	///
	/// - the area's words capitalised, then every word of the rest with its first letter raised and
	///   the rest as it is: "(ITEM DESCRIPTION)item:acetylene" is ItemDescriptionItemAcetylene,
	///   "(GUI)FULL SCREEN" GuiFULLSCREEN, "(GUI)Close" GuiClose - case kept, so CLOSE and Close stay two;
	/// - then "_" and the first 12 hex digits of the full key's SHA-1, when the readable part is
	///   longer than <see cref="ShortKeyLength"/> (it is cut there), or when interface text or a count
	///   has anything but words and single spaces - "HEALTH:" and "HEALTH", "Subscore: +" and
	///   "Subscore: -" would otherwise be one key.
	///
	/// A key from the code always gives the same file key, whatever else is in the game; 37-make-
	/// strings.sh fails if two ever meet, and checks its own copy of this rule (37-strings.pl
	/// short_key) against this one for every entry.
	/// </summary>
	public static string ShortKey(string key)
	{
		if (key == null)
		{
			return null;
		}
		return shortKeys.GetOrAdd(key, MakeShortKey);
	}

	private static string MakeShortKey(string key)
	{
		string area = null;
		string rest = key;
		int close = key.IndexOf(')');
		if (key.StartsWith("(") && close > 0)
		{
			area = key.Substring(1, close - 1);
			rest = key.Substring(close + 1);
		}
		StringBuilder readable = new StringBuilder();
		if (area != null)
		{
			foreach (string word in Words(area))
			{
				readable.Append(char.ToUpperInvariant(word[0])).Append(word.Substring(1).ToLowerInvariant());
			}
		}
		foreach (string word in Words(rest))
		{
			readable.Append(char.ToUpperInvariant(word[0])).Append(word.Substring(1));
		}
		bool text = area == "GUI" || area == "COUNT";
		bool plain = !text || IsPlainWords(rest);
		if (readable.Length <= ShortKeyLength && plain)
		{
			return readable.ToString();
		}
		string cut = readable.Length > ShortKeyLength ? readable.ToString(0, ShortKeyLength) : readable.ToString();
		return cut + "_" + Hash12(key);
	}

	private static IEnumerable<string> Words(string s)
	{
		int start = -1;
		for (int i = 0; i <= s.Length; i++)
		{
			bool alnum = i < s.Length && IsAsciiLetterOrDigit(s[i]);
			if (alnum && start < 0)
			{
				start = i;
			}
			else if (!alnum && start >= 0)
			{
				yield return s.Substring(start, i - start);
				start = -1;
			}
		}
	}

	private static bool IsAsciiLetterOrDigit(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');

	/// <summary>Words of letters and digits, one space between each - nothing a key would lose.</summary>
	private static bool IsPlainWords(string s)
	{
		if (s.Length == 0 || s[0] == ' ' || s[s.Length - 1] == ' ')
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == ' ' ? s[i - 1] == ' ' : !IsAsciiLetterOrDigit(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static string Hash12(string key)
	{
		using (SHA1 sha = SHA1.Create())
		{
			byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
			StringBuilder hex = new StringBuilder(12);
			for (int i = 0; i < 6; i++)
			{
				hex.Append(hash[i].ToString("x2"));
			}
			return hex.ToString();
		}
	}

	/// <summary>Whether a file's key is already in the short form - letters, digits and "_".</summary>
	public static bool IsShortKey(string key)
	{
		foreach (char c in key)
		{
			if (!IsAsciiLetterOrDigit(c) && c != '_')
			{
				return false;
			}
		}
		return key.Length > 0;
	}

	/// <summary>Interface text: the key is "(GUI)" and the English, as in the studio's own file.</summary>
	public static string Text(string english)
	{
		return Text("(GUI)" + english, english);
	}

	/// <summary>
	/// A count with its noun: <c>Count(3, "{0} day", "{0} days")</c> is "3 days". Keyed "(COUNT)"
	/// and the plural English; the template's value is "one|other". A translation lists its own
	/// forms in the same way, as many as its language has, and the number picks one: two forms are
	/// one and the rest; three are East Slavic - 1, 21, 31 / 2-4, 22-24 / the rest, with 11-14 in
	/// the third ("1 день|2 дня|5 дней"). <c>{0}</c> is the number.
	/// </summary>
	public static string Count(int n, string one, string other)
	{
		string forms = Text(CountKey(other), null);
		string chosen;
		if (forms == null)
		{
			chosen = n == 1 ? one : other;
		}
		else
		{
			string[] f = forms.Split('|');
			int i = f.Length switch
			{
				1 => 0,
				2 => n == 1 ? 0 : 1,
				_ => (n % 10 == 1 && n % 100 != 11) ? 0 : (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 12 || n % 100 > 14)) ? 1 : 2,
			};
			chosen = f[i];
			if (!SamePlaceholders(other, chosen))
			{
				chosen = n == 1 ? one : other;
			}
		}
		return string.Format(chosen, n);
	}

	/// <summary>A count's key: "(COUNT)" and the plural English.</summary>
	public static string CountKey(string other) => "(COUNT)" + other;

	/// <summary>
	/// The chosen language's value for <paramref name="key"/>, or <paramref name="english"/>. A
	/// value whose {0}, {1} differ from the English is not used - string.Format would throw on a
	/// {1} the code does not pass - and is reported once.
	/// </summary>
	public static string Text(string key, string english)
	{
		UseChosenCulture();
		if (currentCulture != InvariantCulture && CurrentStrings != null
			&& CurrentStrings.TryGetValue(ShortKey(key), out var value) && !string.IsNullOrEmpty(value))
		{
			if (english == null || SamePlaceholders(english, value))
			{
				return value;
			}
			if (reportedPlaceholders.Add(key))
			{
				GameStateManagement.UnclaimedWorld.LogError("Strings/" + currentCulture + ".xml: \"" + ShortKey(key) + "\" has different {0}/{1} from the English, so the English is shown.", "LANGUAGE");
			}
		}
		return english;
	}

	private static readonly HashSet<string> reportedPlaceholders = new HashSet<string>();

	/// <summary>Whether two texts use the same {n} placeholders (which ones, not how often).</summary>
	public static bool SamePlaceholders(string english, string translated)
	{
		if (english.IndexOf('{') < 0 && translated.IndexOf('{') < 0)
		{
			return true;
		}
		return PlaceholderSet(english) == PlaceholderSet(translated);
	}

	private static string PlaceholderSet(string text)
	{
		var found = new SortedSet<string>(System.StringComparer.Ordinal);
		foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, @"\{(\d+)(?:[,:][^}]*)?\}"))
		{
			found.Add(m.Groups[1].Value);
		}
		return string.Join(",", found);
	}

	/// <summary>The language PORT -> LANGUAGE names, loaded when it changes. A missing or broken file is English.</summary>
	private static void UseChosenCulture()
	{
		string chosen = UWGame.Mods.PortSettings.LanguageName;
		if (chosen == currentCulture)
		{
			return;
		}
		currentCulture = chosen;
		if (chosen == InvariantCulture)
		{
			return;
		}
		try
		{
			Load(ref CurrentStrings, chosen);
		}
		catch (System.Exception ex) when (ex is IOException || ex is System.InvalidOperationException || ex is System.UnauthorizedAccessException)
		{
			CurrentStrings = new Dictionary<string, string>();
			GameStateManagement.UnclaimedWorld.LogError("Strings/" + chosen + ".xml could not be read, so the game stays in English: " + ex.Message, "LANGUAGE");
		}
	}

	public static List<string> GetCultures()
	{
		List<string> list = new List<string>();
		string folder = Config.GetDataFolderPath(Config.DataType.BaseData, "Strings");
		if (!Directory.Exists(folder))
		{
			return list;
		}
		string[] files = Directory.GetFiles(folder, "*.xml");
		foreach (string path in files)
		{
			list.Add(Path.GetFileNameWithoutExtension(path));
		}
		return list;
	}

	public static void Load(ref Dictionary<string, string> dictionary, string fileName)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<String>));
		List<String> list;
		using (TextReader textReader = new StreamReader(Config.GetDataFolderPath(Config.DataType.BaseData, "Strings", fileName + ".xml")))
		{
			list = (List<String>)xmlSerializer.Deserialize(textReader);
		}
		if (dictionary != null)
		{
			dictionary.Clear();
		}
		else
		{
			dictionary = new Dictionary<string, string>();
		}
		foreach (String item in list)
		{
			// PORT: was Add, which threw on a key written twice - easy to do by hand in a translation.
			// A key in the old form, "(GUI)SAVE GAME", is read as its short form, so a file written
			// before the keys were shortened still works (Locale.ShortKey).
			if (string.IsNullOrEmpty(item.Key))
			{
				continue;
			}
			dictionary[IsShortKey(item.Key) ? item.Key : ShortKey(item.Key)] = item.Value;
		}
	}

	/// <summary>A mod setting's label, keyed "(SETTING)" and its id ("hud.labelList").</summary>
	public static string SettingKey(string id) => "(SETTING)" + id;

	/// <summary>A mod setting's tooltip.</summary>
	public static string SettingTipKey(string id) => "(SETTING TIP)" + id;

	/// <summary>A heading in the MODS section, keyed by its mod id.</summary>
	public static string SettingGroupKey(string modId) => "(SETTING GROUP)" + modId;

	/// <summary>A dropdown choice in the MODS section: "(SETTING CHOICE)hud.example=VALUE".</summary>
	public static string SettingChoiceKey(string id, string value) => "(SETTING CHOICE)" + id + "=" + value;
}
