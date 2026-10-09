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

	/// <summary>The data tables' members that hold text a player reads, by name.</summary>
	private static readonly string[] TextMembers = { "Name", "Description", "ShortDescription", "Text", "Tooltip", "ToolTip", "DisplayName", "Heading" };

	/// <summary>
	/// The data tables whose text is translated: a table's type, and the area its keys carry. Not
	/// every table with a Name: some names are identifiers (sounds, icons, renderables), and the
	/// map's own entities (EntityData) are people and places.
	/// </summary>
	private static readonly Dictionary<System.Type, string> TranslatedTables = new Dictionary<System.Type, string>
	{
		// EntityType is split by what the entity is - see AreaOf. Terrain features are left out:
		// their names are asset names ("sulfurrock", "S: Bird 1, single") nobody is shown.
		{ typeof(UWGame.SimSide.Entities.EntityType), null },
		{ typeof(UWGame.SimSide.Processes.ProcessType), "PROCESS" },
		{ typeof(UWGame.SimSide.Resources.ResourceType), "RESOURCE" },
		{ typeof(UWGame.SimSide.Resources.ResourceCategory), "RESOURCE CATEGORY" },
		{ typeof(UWGame.SimSide.Entities.SkillType), "SKILL" },
		{ typeof(UWGame.SimSide.Entities.SkillCategory), "SKILL CATEGORY" },
		{ typeof(UWGame.SimSide.Entities.Skills.ProfessionType), "PROFESSION" },
		{ typeof(UWGame.SimSide.Entities.EntityCategory), "CATEGORY" },
		{ typeof(UWGame.SimSide.Items.DegradeType), "DEGRADE" },
		{ typeof(UWGame.SimSide.Items.FoodNutrientType), "NUTRIENT" },
		{ typeof(UWGame.SimSide.Entities.Containers.UpgradeCategory), "UPGRADE" },
		{ typeof(UWGame.SimSide.Entities.Containers.StorageCondition), "STORAGE" },
		{ typeof(UWGame.SimSide.Entities.Containers.DefaultStorageSettings), "STORAGE SETTINGS" },
		{ typeof(UWGame.SimSide.Tiers.TierType), "TIER" },
		{ typeof(UWGame.SimSide.Tiers.TierArea), "TIER AREA" },
		{ typeof(UWGame.SimSide.Entities.Biological.BioOrderType), "ORDER" },
		{ typeof(UWGame.SimSide.SimEffects.EffectProfileType), "EFFECT PROFILE" },
		{ typeof(UWGame.SimSide.SimEffects.EffectType), "EFFECT" },
		{ typeof(UWGame.SimSide.Entities.Substances.SubstanceType), "SUBSTANCE" },
		{ typeof(UWGame.SimSide.Entities.Body.BodyLayerType), "BODY LAYER" },
		{ typeof(UWGame.ClientSide.PropertyPresentation.PresentationTypeCategory), "PROPERTY GROUP" },
		{ typeof(UWGame.SimSide.Vegetation.LowVegetationType), "VEGETATION" },
		{ typeof(UWGame.SimSide.Soil.SoilComponentType), "SOIL" },
		{ typeof(UWGame.SimSide.Expeditions.ExpeditionData), "EXPEDITION" },
		{ typeof(UWGame.SimSide.Overland.SiteData), "SITE" },
		{ typeof(UWGame.SimSide.Overland.Templates.SiteTemplate), "SITE TEMPLATE" },
		{ typeof(UWGame.SimSide.Allegiances.AllegianceData), "ALLEGIANCE" },
	};

	/// <summary>
	/// One piece of a data table's text: the key it is translated under, where it lives, and the
	/// English the tables were built with.
	/// </summary>
	public struct DataText
	{
		public string Key;
		public object Owner;
		public System.Reflection.MemberInfo Member;
		public string English;
	}

	/// <summary>
	/// PORT: every translatable text in the data tables, keyed "(TABLE)" + KeyName for a Name and
	/// "(TABLE FIELD)" + KeyName for the rest - "(ITEM)item:knife", "(PROCESS DESCRIPTION)cook".
	/// The one list both <see cref="TranslateData"/> and the template (tools/build/37-make-strings.sh)
	/// read, so the template has exactly what the game asks for.
	/// </summary>
	public static List<DataText> DataTexts(UWGame.SimSide.GameData data)
	{
		var texts = new List<DataText>();
		foreach (KeyValuePair<System.Type, UWGame.SimSide.IGameDataCollection> collection in data.AllGameDataCollections)
		{
			if (!TranslatedTables.TryGetValue(collection.Key, out string table))
			{
				continue;
			}
			var members = new List<System.Reflection.MemberInfo>();
			foreach (string name in TextMembers)
			{
				// A field first: SoilComponentType hides the base Name property with a field of its own.
				System.Reflection.MemberInfo member = (System.Reflection.MemberInfo)collection.Key.GetField(name)
					?? collection.Key.GetProperty(name, typeof(string));
				if (member is System.Reflection.PropertyInfo p && p.CanWrite && p.GetSetMethod() != null
					|| member is System.Reflection.FieldInfo f && f.FieldType == typeof(string) && !f.IsInitOnly)
				{
					members.Add(member);
				}
			}
			if (!(collection.Value is System.Collections.IDictionary entries))
			{
				continue;
			}
			foreach (object entry in entries.Values)
			{
				if (!(entry is UWGame.SimSide.IGameData record) || string.IsNullOrEmpty(record.KeyName))
				{
					continue;
				}
				string area = table ?? AreaOf((UWGame.SimSide.Entities.EntityType)entry);
				if (area == null)
				{
					continue;
				}
				foreach (System.Reflection.MemberInfo member in members)
				{
					string english = member is System.Reflection.PropertyInfo p ? (string)p.GetValue(entry) : (string)((System.Reflection.FieldInfo)member).GetValue(entry);
					if (string.IsNullOrWhiteSpace(english))
					{
						continue;
					}
					string field = member.Name == "Name" ? "" : " " + member.Name.ToUpperInvariant();
					texts.Add(new DataText { Key = "(" + area + field + ")" + record.KeyName, Owner = entry, Member = member, English = english });
				}
			}
		}
		return texts;
	}

	/// <summary>An entity type's area, by what it is; null for a terrain feature, which is not translated.</summary>
	private static string AreaOf(UWGame.SimSide.Entities.EntityType type)
	{
		if (type.ItemType != null) return "ITEM";
		if (type.StructureType != null) return "STRUCTURE";
		if (type.TreeType != null) return "TREE";
		if (type.TerrainType != null) return null;
		if (type.BiologicalType != null) return "CREATURE";
		return "ENTITY";
	}

	/// <summary>
	/// PORT: the data tables' text in the chosen language. Called at the start of
	/// GameData.PostDataCompleteInitialize, so whatever is built from the names afterwards uses the
	/// translated ones. Only what is shown changes: the game identifies everything by KeyName, and
	/// --export-data writes the tables before this runs, so an export stays English.
	/// </summary>
	public static void TranslateData(UWGame.SimSide.GameData data)
	{
		UseChosenCulture();
		if (currentCulture == InvariantCulture)
		{
			return;
		}
		foreach (DataText text in DataTexts(data))
		{
			string translated = Text(text.Key, text.English);
			if (ReferenceEquals(translated, text.English))
			{
				continue;
			}
			if (text.Member is System.Reflection.PropertyInfo p)
			{
				p.SetValue(text.Owner, translated);
			}
			else
			{
				((System.Reflection.FieldInfo)text.Member).SetValue(text.Owner, translated);
			}
		}
	}

	/// <summary>A mod setting's label, keyed "(SETTING)" and its id ("hud.labelList").</summary>
	public static string SettingKey(string id) => "(SETTING)" + id;

	/// <summary>A mod setting's tooltip.</summary>
	public static string SettingTipKey(string id) => "(SETTING TIP)" + id;

	/// <summary>A heading in the MODS section, keyed by its mod id.</summary>
	public static string SettingGroupKey(string modId) => "(SETTING GROUP)" + modId;
}
