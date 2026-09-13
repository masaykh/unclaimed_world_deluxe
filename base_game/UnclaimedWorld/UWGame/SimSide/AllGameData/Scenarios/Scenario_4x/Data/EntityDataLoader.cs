using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data;

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
				Location = new Vector3(1584f, 1296f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "leafcutterAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "fieldQuaditeNest3",
				EntityKey = "terrain:fieldQuaditeNest",
				Name = "Field quadite nest 3",
				Location = new Vector3(3600f, 1248f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "leafcutterAllegiance#1"
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
				Location = new Vector3(2640f, 3312f, 0f),
				Rotation = 100f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "purple"
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
				Location = new Vector3(2699f, 3351f, 0f),
				Rotation = 175f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "purple"
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
				Location = new Vector3(2736f, 3312f, 0f),
				Rotation = 250f,
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
				Location = new Vector3(3370f, 2199f, 0f),
				Rotation = 165f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "veryDarkGreen"
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
				Location = new Vector3(3408f, 2209f, 0f),
				Rotation = 195f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "veryDarkGreen"
				}
			},
			new EntityData
			{
				KeyName = "bird#6",
				EntityKey = "entity:bird",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(3395f, 2219f, 0f),
				Rotation = 145f,
				BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
				{
					AgeGroup = AIAgeGroup.Adult,
					RaceKey = "veryDarkGreen"
				}
			}
		};
	}
}
