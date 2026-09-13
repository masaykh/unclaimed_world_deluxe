using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents;

namespace UWGame.SimSide.Maps.MapEditor;

public class MapData
{
	[XmlIgnore]
	public string FolderName;

	public string Name;

	public Point Dimensions;

	public PolledEventType[] Events;

	public string[] PredefinedEvents;

	public List<EntityData> SavedMapEntities;

	public List<EntityData> Trees;

	public List<Tile> Tiles;
}
