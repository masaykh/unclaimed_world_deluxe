using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UWGame.SimSide.XmlCollections;

public class SerializableQueue<T> : Queue<T>, IXmlSerializable
{
	private readonly Type m_type = typeof(T);

	public override string ToString()
	{
		return SerializableGenerics.GetTypeName(GetType());
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
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
				Enqueue(item);
			}
			reader.ReadEndElement();
		}
		reader.MoveToContent();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(m_type);
		IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			xmlSerializer.Serialize(writer, enumerator.Current);
		}
	}
}
