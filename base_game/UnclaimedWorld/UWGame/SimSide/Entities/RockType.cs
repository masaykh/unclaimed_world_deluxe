using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class RockType
{
	public float Bulk;

	[XmlIgnore]
	public bool Animates;
}
