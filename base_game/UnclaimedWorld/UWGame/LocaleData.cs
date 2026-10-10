using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UWGame.SimSide;

namespace UWGame;

/// <summary>
/// PORT: the text in the data tables - names, descriptions, help pages, spoken lines - and its
/// translation. See Locale for the interface's.
///
/// WHERE THE TEXT IS. Not only on the tables' own entries. A recipe's quality words sit in its
/// thresholds, a help topic's pages in its layout elements, and a colonist's spoken line in a
/// TalkAction defined inline inside an action set, several objects deep. So <see cref="DataTexts"/>
/// walks every table entry's objects, the way a serializer would, and stops only at an object that
/// is an entry of another table (it is walked as its own) or one already seen.
///
/// WHAT IS TEXT. A member is text by its name - <see cref="TextMembers"/>: Description, PluralName,
/// DefaultText, Term... - anywhere. "Name" only on the entries of <see cref="TranslatedTables"/>
/// and on help topics: elsewhere a Name is often an identifier (a body part is saved by its name,
/// a map's spawn point is found by it), and "String" is never text here (culture and spawn keys).
///
/// KEYS. "(AREA)" + KeyName for an entry's name, "(AREA FIELD)" + KeyName for its other text -
/// "(ITEM)item:knife", "(PROCESS DESCRIPTION)cook". Text deeper down is keyed by the nearest
/// object with a KeyName of its own - "(TALKACTION DEFAULTTEXT)8dc3ca94-..." - or else by the path
/// from its entry: "(PROCESS)cook/Thresholds[2].Term". The walk is ordered (types, keys, members by
/// name), so the game and the template find the same keys.
///
/// SCENARIOS. A scenario loads its own tables and overrides some entities' text
/// (EntityTypeDescription.ApplyDescriptions). Where a scenario's English differs from the base
/// game's for one key, the template carries "key@scenario name"; <see cref="TranslateData"/> tries
/// that first. And a translation is used only while the text is still the exact English the
/// template has for its key - so a scenario's own text is never replaced by the base game's
/// translation, and English the code changed later is not replaced by a stale one.
/// </summary>
public partial class Locale
{
	/// <summary>Members that hold text a player reads, wherever they are.</summary>
	private static readonly HashSet<string> TextMembers = new HashSet<string>(StringComparer.Ordinal)
	{
		"Description", "SummaryDescription", "ShortDescription", "PluralName", "DisplayName", "Heading",
		"Text", "Tooltip", "ToolTip", "DefaultText", "LoseScreenText", "UserCannotCancelReason",
		"SpecialActionCaption", "Term", "TermTooltip", "StaticString", "TextWithPlaceholders",
		// GUIConstants' headings for the tool options of a recipe.
		"FirstToolOption", "SecondToolOption", "ThirdToolOption",
	};

	/// <summary>
	/// The tables whose entries' Name is translated, and the area their keys carry. Not every
	/// table with a Name: sounds, icons and renderables are named for their files, and the map's
	/// own entities (EntityData) are people and places.
	/// </summary>
	private static readonly Dictionary<Type, string> TranslatedTables = new Dictionary<Type, string>
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
	/// Tables not walked at all: their text is a copy of another table's (EntityTypeDescription is
	/// applied to the entity types before translation) or not shown (the map's own entities).
	/// </summary>
	private static readonly HashSet<Type> SkippedTables = new HashSet<Type>
	{
		typeof(UWGame.SimSide.Entities.EntityTypeDescription),
		typeof(UWGame.SimSide.Maps.MapEditor.EntityData),
	};

	/// <summary>The scenario whose tables are loaded - set by DataLoader.QueueInitGameData - or null.</summary>
	public static string CurrentScenario { get; set; }

	/// <summary>
	/// One piece of the tables' text: the key it is translated under, where it lives, and the
	/// English it holds now.
	/// </summary>
	public struct DataText
	{
		public string Key;
		public object Owner;
		public MemberInfo Member;
		public string English;
	}

	/// <summary>Every translatable text in the loaded tables, in a fixed order.</summary>
	public static List<DataText> DataTexts(GameData data)
	{
		var walk = new TextWalk();
		foreach (KeyValuePair<Type, IGameDataCollection> collection in data.AllGameDataCollections.OrderBy(c => c.Key.FullName, StringComparer.Ordinal))
		{
			if (!SkippedTables.Contains(collection.Key) && collection.Value is IDictionary entries)
			{
				foreach (object entry in entries.Values)
				{
					walk.Entries.Add(entry);
				}
			}
		}
		foreach (KeyValuePair<Type, IGameDataCollection> collection in data.AllGameDataCollections.OrderBy(c => c.Key.FullName, StringComparer.Ordinal))
		{
			if (SkippedTables.Contains(collection.Key) || !(collection.Value is IDictionary entries))
			{
				continue;
			}
			bool names = TranslatedTables.TryGetValue(collection.Key, out string area);
			foreach (string key in entries.Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal))
			{
				object entry = entries[key];
				string entryArea = area;
				if (entry is UWGame.SimSide.Entities.EntityType type)
				{
					entryArea = AreaOf(type);
					if (entryArea == null)
					{
						continue;
					}
				}
				walk.Root(entry, entryArea ?? AreaName(collection.Key), names);
			}
		}
		foreach (var topic in data.AllHelpTopics.OrderBy(t => t.Key, StringComparer.Ordinal))
		{
			walk.Root(topic.Value, "HELP", names: true);
		}
		foreach (var topic in data.AllTutorialTopics.OrderBy(t => t.Key, StringComparer.Ordinal))
		{
			walk.Root(topic.Value, "TUTORIAL", names: true);
		}
		// The interface's own data (GUIConstants.xml): storage locations' names, tool option headings.
		if (data.GUIConstants != null)
		{
			walk.Root(data.GUIConstants, "GUI CONSTANTS", names: false, key: "guiConstants");
		}
		return walk.Texts;
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

	/// <summary>A type's area when no table names one: its name, upper case ("TALKACTION").</summary>
	private static string AreaName(Type type) => type.Name.ToUpperInvariant();

	private sealed class TextWalk
	{
		public readonly List<DataText> Texts = new List<DataText>();

		/// <summary>Every table's entries: walked as their own roots, never into from another.</summary>
		public readonly HashSet<object> Entries = new HashSet<object>(ReferenceEqualityComparer.Instance);

		private readonly HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

		/// <summary>Whether a Name below the root is text too - in a scenario header, a difficulty's or an option's.</summary>
		public bool NestedNames;

		private static readonly Dictionary<Type, MemberInfo[]> membersOf = new Dictionary<Type, MemberInfo[]>();

		public void Root(object entry, string area, bool names, string key = null)
		{
			key = key ?? (entry as IGameData)?.KeyName;
			if (string.IsNullOrEmpty(key) || !seen.Add(entry))
			{
				return;
			}
			Visit(entry, area, key, null, names, 0);
		}

		/// <summary>
		/// <paramref name="area"/> and <paramref name="key"/> name the nearest object with a key;
		/// <paramref name="path"/> is the way from it to <paramref name="o"/>, null at that object.
		/// </summary>
		private void Visit(object o, string area, string key, string path, bool names, int depth)
		{
			if (depth > 12)
			{
				return;
			}
			foreach (MemberInfo member in Members(o.GetType()))
			{
				object value = member is FieldInfo f ? f.GetValue(o) : GetOrNull((PropertyInfo)member, o);
				if (value is string english)
				{
					bool isName = member.Name == "Name";
					if ((isName ? (path == null ? names : NestedNames) : TextMembers.Contains(member.Name)) && Writable(member) && !string.IsNullOrWhiteSpace(english))
					{
						string field = isName ? "" : " " + member.Name.ToUpperInvariant();
						string k = path == null ? "(" + area + field + ")" + key : "(" + area + ")" + key + "/" + path + "." + member.Name;
						Texts.Add(new DataText { Key = k, Owner = o, Member = member, English = english });
					}
					continue;
				}
				Into(value, area, key, Join(path, member.Name), depth);
			}
		}

		private void Into(object value, string area, string key, string path, int depth)
		{
			if (value == null || value is string || value.GetType().IsPrimitive || value.GetType().IsEnum || Entries.Contains(value))
			{
				return;
			}
			if (value is IDictionary dictionary)
			{
				foreach (object k in dictionary.Keys.Cast<object>().OrderBy(k => Convert.ToString(k, System.Globalization.CultureInfo.InvariantCulture), StringComparer.Ordinal))
				{
					Into(dictionary[k], area, key, path + "[" + Convert.ToString(k, System.Globalization.CultureInfo.InvariantCulture) + "]", depth + 1);
				}
				return;
			}
			if (value is IEnumerable list)
			{
				int i = 0;
				foreach (object item in list)
				{
					Into(item, area, key, path + "[" + i++ + "]", depth + 1);
				}
				return;
			}
			Type t = value.GetType();
			if (t.Namespace == null || !t.Namespace.StartsWith("UWGame", StringComparison.Ordinal) || !seen.Add(value))
			{
				return;
			}
			// An object with a key of its own is keyed by it: a TalkAction's line by its id.
			string own = (value as IGameData)?.KeyName;
			if (!string.IsNullOrEmpty(own))
			{
				Visit(value, AreaName(t), own, null, names: NestedNames, depth + 1);
			}
			else
			{
				Visit(value, area, key, path, names: false, depth + 1);
			}
		}

		private static string Join(string path, string member) => path == null ? member : path + "." + member;

		private static object GetOrNull(PropertyInfo p, object o)
		{
			try
			{
				return p.GetValue(o);
			}
			catch (TargetInvocationException)
			{
				return null;
			}
		}

		private static bool Writable(MemberInfo member) =>
			member is FieldInfo f ? !f.IsInitOnly : ((PropertyInfo)member).GetSetMethod() != null;

		/// <summary>Public fields and settable properties, by name; a field before a property it hides.</summary>
		private static MemberInfo[] Members(Type type)
		{
			if (membersOf.TryGetValue(type, out MemberInfo[] members))
			{
				return members;
			}
			var byName = new SortedDictionary<string, MemberInfo>(StringComparer.Ordinal);
			foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (p.CanRead && p.GetIndexParameters().Length == 0 && p.GetSetMethod() != null && !byName.ContainsKey(p.Name))
				{
					byName[p.Name] = p;
				}
			}
			// SoilComponentType hides the base Name property with a field of its own: the field wins.
			foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!byName.TryGetValue(f.Name, out MemberInfo existing) || existing is PropertyInfo)
				{
					byName[f.Name] = f;
				}
			}
			members = byName.Values.ToArray();
			membersOf[type] = members;
			return members;
		}
	}

	/// <summary>
	/// A scenario header's text for the New Game screens - its DisplayName and descriptions, its
	/// difficulties' and options' names and descriptions - keyed "(SCENARIO ...)" and the
	/// scenario's Name. The header's own Name is never text: it is how the game finds the scenario.
	/// </summary>
	public static List<DataText> ScenarioTexts(UWGame.SimSide.Scenarios.Scenario scenario)
	{
		var walk = new TextWalk { NestedNames = true };
		if (scenario?.Name != null)
		{
			walk.Root(scenario, "SCENARIO", names: false, key: scenario.Name);
		}
		return walk.Texts;
	}

	/// <summary>
	/// PORT: a scenario header in the chosen language, for the New Game screens
	/// (SelectScenarioPanel, SelectScenarioScreen). The game started from them keeps this header
	/// (CustomizeScenarioScreen passes it on), but only its text is changed: its Name - how the game
	/// and a save find the scenario - its keys and its options are the same in every language.
	/// </summary>
	public static void TranslateScenario(UWGame.SimSide.Scenarios.Scenario scenario)
	{
		UseChosenCulture();
		if (currentCulture == InvariantCulture || scenario == null)
		{
			return;
		}
		EnsureInvariant();
		string saved = CurrentScenario;
		CurrentScenario = null;
		foreach (DataText text in ScenarioTexts(scenario))
		{
			Apply(text, TranslatedData(text.Key, text.English));
		}
		CurrentScenario = saved;
	}

	private static void Apply(DataText text, string translated)
	{
		if (translated == null)
		{
			return;
		}
		if (text.Member is PropertyInfo p)
		{
			p.SetValue(text.Owner, translated);
		}
		else
		{
			((FieldInfo)text.Member).SetValue(text.Owner, translated);
		}
	}

	/// <summary>
	/// PORT: the tables' text in the chosen language. Called at the start of
	/// GameData.PostDataCompleteInitialize, so whatever is built from the names afterwards uses the
	/// translated ones. Only what is shown changes: the game identifies everything by KeyName, and
	/// --export-data writes the tables before this runs, so an export stays English.
	/// </summary>
	public static void TranslateData(GameData data)
	{
		UseChosenCulture();
		if (currentCulture == InvariantCulture)
		{
			return;
		}
		EnsureInvariant();
		foreach (DataText text in DataTexts(data))
		{
			Apply(text, TranslatedData(text.Key, text.English));
		}
	}

	/// <summary>
	/// The translation for a table's text, or null: the scenario's own key first, then the shared
	/// one - each only while the text is the English the template holds for it.
	/// </summary>
	private static string TranslatedData(string key, string english)
	{
		foreach (string candidate in CurrentScenario == null ? new[] { key } : new[] { key + "@" + CurrentScenario, key })
		{
			if (InvariantStrings != null && InvariantStrings.TryGetValue(ShortKey(candidate), out string template)
				&& string.Equals(Normalize(template), Normalize(english), StringComparison.Ordinal))
			{
				string translated = Text(candidate, null);
				return translated != null && SamePlaceholders(english, translated) ? translated : null;
			}
		}
		return null;
	}

	private static string Normalize(string text) => text.Replace("\r\n", "\n");

	/// <summary>The template, English (US).xml, loaded once; empty when it cannot be read.</summary>
	private static void EnsureInvariant()
	{
		if (InvariantStrings != null)
		{
			return;
		}
		try
		{
			Load(ref InvariantStrings, InvariantCulture);
		}
		catch (Exception ex) when (ex is System.IO.IOException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
		{
			InvariantStrings = new Dictionary<string, string>();
		}
	}
}
