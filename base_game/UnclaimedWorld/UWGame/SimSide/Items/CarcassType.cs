using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Items;

public class CarcassType
{
	public string[] EdibleByDesignerTags;

	[XmlIgnore]
	public List<EntityType> EdibleBy;

	public void Initialize()
	{
	}
}
