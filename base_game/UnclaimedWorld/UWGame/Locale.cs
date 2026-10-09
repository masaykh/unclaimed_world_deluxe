using System.Collections.Generic;
using System.IO;
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
public class Locale
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
		if (CurrentStrings != null && CurrentStrings.TryGetValue(key, out var value))
		{
			return value;
		}
		return InvariantStrings[key];
	}

	/// <summary>Interface text: the key is "(GUI)" and the English, as in the studio's own file.</summary>
	public static string Text(string english)
	{
		return Text("(GUI)" + english, english);
	}

	/// <summary>The chosen language's value for <paramref name="key"/>, or <paramref name="english"/>.</summary>
	public static string Text(string key, string english)
	{
		UseChosenCulture();
		if (currentCulture != InvariantCulture && CurrentStrings != null
			&& CurrentStrings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
		{
			return value;
		}
		return english;
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
			dictionary[item.Key] = item.Value;
		}
	}

	/// <summary>
	/// PORT: item names and descriptions in the chosen language, keyed "(ITEM)" and
	/// "(ITEM DESCRIPTION)" and the item's KeyName. Called at the start of
	/// GameData.PostDataCompleteInitialize, so whatever is built from the names afterwards uses the
	/// translated ones. Only what is shown changes: the game identifies items by KeyName.
	/// </summary>
	public static void TranslateItems(IEnumerable<UWGame.SimSide.Entities.EntityType> types)
	{
		foreach (UWGame.SimSide.Entities.EntityType type in types)
		{
			if (type.ItemType == null)
			{
				continue;
			}
			type.Name = Text(ItemNameKey(type.KeyName), type.Name);
			type.Description = Text(ItemDescriptionKey(type.KeyName), type.Description);
		}
	}

	public static string ItemNameKey(string keyName) => "(ITEM)" + keyName;

	public static string ItemDescriptionKey(string keyName) => "(ITEM DESCRIPTION)" + keyName;

	/// <summary>A mod setting's label, keyed "(SETTING)" and its id ("hud.labelList").</summary>
	public static string SettingKey(string id) => "(SETTING)" + id;

	/// <summary>A mod setting's tooltip.</summary>
	public static string SettingTipKey(string id) => "(SETTING TIP)" + id;

	/// <summary>A heading in the MODS section, keyed by its mod id.</summary>
	public static string SettingGroupKey(string modId) => "(SETTING GROUP)" + modId;
}
