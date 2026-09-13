using System;
using System.Xml.Serialization;

namespace UWGame.SimSide;

[Serializable]
[XmlRoot(ElementName = "KeyValuePair")]
public class KVP<K, V>
{
	public K Key { get; set; }

	public V Value { get; set; }

	public KVP()
	{
	}

	public KVP(K key, V value)
	{
		Key = key;
		Value = value;
	}
}
