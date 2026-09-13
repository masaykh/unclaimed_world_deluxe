using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class EntityDataLoader
{
	public static List<EntityData> Init()
	{
		return new List<EntityData>
		{
			new EntityData
			{
				KeyName = "patrician#1",
				EntityKey = "entity:patrician",
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.YoungAdult
				}
			},
			new EntityData
			{
				KeyName = "patrician#2",
				EntityKey = "entity:patrician",
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "mudWorm#1",
				EntityKey = "entity:mudWorm",
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "studdedThunderChicken#1",
				EntityKey = "entity:studdedThunderChicken",
				Location = new Vector3(3024f, 432f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "studdedThunderChicken#2",
				EntityKey = "entity:studdedThunderChicken",
				Location = new Vector3(3024f, 582f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "studdedThunderChicken#3",
				EntityKey = "entity:studdedThunderChicken",
				Location = new Vector3(2736f, 342f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "bird#1",
				EntityKey = "entity:bird",
				Location = new Vector3(1824f, 1152f, 0f),
				Rotation = 100f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "dark"
				}
			},
			new EntityData
			{
				KeyName = "bird#2",
				EntityKey = "entity:bird",
				Location = new Vector3(1865f, 1172f, 0f),
				Rotation = 190f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "pale"
				}
			},
			new EntityData
			{
				KeyName = "bird#3",
				EntityKey = "entity:bird",
				Location = new Vector3(1200f, 2304f, 0f),
				Rotation = 100f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "black"
				}
			},
			new EntityData
			{
				KeyName = "bird#4",
				EntityKey = "entity:bird",
				Location = new Vector3(672f, 2112f, 0f),
				Rotation = 175f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "red"
				}
			},
			new EntityData
			{
				KeyName = "bird#5",
				EntityKey = "entity:bird",
				Location = new Vector3(624f, 2134f, 0f),
				Rotation = 250f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "dark"
				}
			},
			new EntityData
			{
				KeyName = "bird#6",
				EntityKey = "entity:bird",
				Location = new Vector3(336f, 1728f, 0f),
				Rotation = 290f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "dark"
				}
			},
			new EntityData
			{
				KeyName = "binalRat#1Allegiance#1",
				EntityKey = "entity:binalRat",
				Location = new Vector3(626f, 1932f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "binalRat#1Allegiance#2",
				EntityKey = "entity:binalRat",
				Location = new Vector3(2076f, 1874f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "binalRat#2Allegiance#2",
				EntityKey = "entity:binalRat",
				Location = new Vector3(1824f, 1364f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "binalRat#1Allegiance#3",
				EntityKey = "entity:binalRat",
				Location = new Vector3(1726f, 892f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "binalRat#2Allegiance#3",
				EntityKey = "entity:binalRat",
				Location = new Vector3(1252f, 1208f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			}
		};
	}
}
