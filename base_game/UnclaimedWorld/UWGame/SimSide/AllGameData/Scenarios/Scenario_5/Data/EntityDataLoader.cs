using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

public class EntityDataLoader
{
	public static List<EntityData> Init()
	{
		return new List<EntityData>
		{
			new EntityData
			{
				KeyName = "fieldQuaditeNest1",
				EntityKey = "terrain:fieldQuaditeNest",
				Name = "Field quadite nest 1",
				Location = new Vector3(864f, 1056f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "leafcutterAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "turnip",
				EntityKey = "entity:turnip",
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					RaceKey = "Pale Turnip"
				}
			},
			new EntityData
			{
				KeyName = "thunderChicken",
				EntityKey = "entity:studdedThunderChicken",
				Name = "ThunderChicken(1584,2736)",
				Location = new Vector3(1384f, 70f, 0f),
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult
				}
			},
			new EntityData
			{
				KeyName = "bird#1",
				EntityKey = "entity:bird",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(200f, 200f, 0f),
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
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(220f, 210f, 0f),
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
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(1680f, 2064f, 0f),
				Rotation = 100f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "purple"
				}
			},
			new EntityData
			{
				KeyName = "bird#4",
				EntityKey = "entity:bird",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(1700f, 2044f, 0f),
				Rotation = 175f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "purple"
				}
			},
			new EntityData
			{
				KeyName = "bird#5",
				EntityKey = "entity:bird",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(1730f, 2020f, 0f),
				Rotation = 250f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "purple"
				}
			}
		};
	}
}
