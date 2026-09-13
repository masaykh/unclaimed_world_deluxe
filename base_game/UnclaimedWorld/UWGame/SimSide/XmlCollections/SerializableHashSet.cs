using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UWGame.SimSide.XmlCollections;

[XmlRoot("hashset")]
public class SerializableHashSet<T> : HashSet<T>, IXmlSerializable
{
	private readonly Type m_type = typeof(T);

	public SerializableHashSet()
	{
	}

	public SerializableHashSet(IEqualityComparer<T> comparer)
		: base(comparer)
	{
	}

	protected SerializableHashSet(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(m_type);
		bool isEmptyElement = reader.IsEmptyElement;
		reader.ReadStartElement();
		if (!isEmptyElement)
		{
			reader.MoveToContent();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				T item = (T)xmlSerializer.Deserialize(reader);
				Add(item);
			}
			reader.ReadEndElement();
		}
		reader.MoveToContent();
	}

	public void WriteXml(XmlWriter writer)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(m_type);
		IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			xmlSerializer.Serialize(writer, enumerator.Current);
		}
	}
}
