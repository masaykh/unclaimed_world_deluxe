using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace UWGame;

public class Locale
{
	private static Dictionary<string, string> InvariantStrings;

	private static Dictionary<string, string> CurrentStrings;

	public static void Init()
	{
		Load(ref InvariantStrings, "English (US)");
		CurrentStrings = InvariantStrings;
	}

	public static string Get(string key)
	{
		if (CurrentStrings.TryGetValue(key, out var value))
		{
			return value;
		}
		return InvariantStrings[key];
	}

	public static List<string> GetCultures()
	{
		List<string> list = new List<string>();
		string[] files = Directory.GetFiles(Config.GetDataFolderPath(Config.DataType.BaseData, "Strings"), "*.xml");
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
			dictionary.Add(item.Key, item.Value);
		}
	}
}
