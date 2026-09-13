using System.Collections.Generic;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class StructuresProfileLoader
{
	public static List<StructuresProfile> Init()
	{
		return new List<StructuresProfile>
		{
			new StructuresProfile
			{
				Comments = "Low capacity...",
				KeyName = "smallPierProfile",
				StartingStructures = new SerializableDictionary<string, int>
				{
					{ "structure:simplePort", 1 },
					{ "structure:radioHut", 1 }
				}
			},
			new StructuresProfile
			{
				KeyName = "mediumPierProfile",
				StartingStructures = new SerializableDictionary<string, int>
				{
					{ "structure:canopyPort", 1 },
					{ "structure:radioHut", 1 }
				}
			},
			new StructuresProfile
			{
				KeyName = "largePierProfile",
				StartingStructures = new SerializableDictionary<string, int>
				{
					{ "structure:largePier", 1 },
					{ "structure:radioHut", 1 }
				}
			},
			new StructuresProfile
			{
				Comments = "very large capacity helipad, for other sites",
				KeyName = "largeHeliportProfile",
				StartingStructures = new SerializableDictionary<string, int>
				{
					{ "structure:heliportLarge", 1 },
					{ "structure:satelliteGroundStation", 1 }
				}
			},
			new StructuresProfile
			{
				KeyName = "mediumPierSatelliteProfile",
				StartingStructures = new SerializableDictionary<string, int>
				{
					{ "structure:canopyPort", 1 },
					{ "structure:satelliteGroundStation", 1 }
				}
			}
		};
	}
}
