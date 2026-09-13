using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class XmlDictionary<T, V> : Dictionary<T, V>, IXmlSerializable
{
	[XmlType("Entry")]
	public struct Entry
	{
		[XmlElement("Key")]
		public T Key { get; set; }

		[XmlElement("Value")]
		public V Value { get; set; }

		public Entry(T key, V value)
		{
			this = default(Entry);
			Key = key;
			Value = value;
		}
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		Clear();
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Entry>));
		reader.Read();
		foreach (Entry item in (List<Entry>)xmlSerializer.Deserialize(reader))
		{
			Add(item.Key, item.Value);
		}
		reader.ReadEndElement();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		List<Entry> list = new List<Entry>(base.Count);
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<T, V> current = enumerator.Current;
				list.Add(new Entry(current.Key, current.Value));
			}
		}
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		new XmlSerializer(list.GetType()).Serialize(writer, list, xmlSerializerNamespaces);
	}
}
