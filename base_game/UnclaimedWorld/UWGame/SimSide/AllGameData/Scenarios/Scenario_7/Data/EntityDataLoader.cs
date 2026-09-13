using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;

public class EntityDataLoader
{
	public static List<EntityData> Init()
	{
		return new List<EntityData>
		{
			new EntityData
			{
				KeyName = "swarmerNest1",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 1",
				Location = new Vector3(1080f, 1080f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "swarmerNest5",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 5",
				Location = new Vector3(1200f, 1344f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "swarmerNest6",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 6",
				Location = new Vector3(576f, 1766f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "swarmerNest3",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 3",
				Location = new Vector3(2400f, 1685f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "swarmerNest2",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 2",
				Location = new Vector3(1584f, 2160f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "swarmerNest4",
				EntityKey = "terrain:swarmerNest",
				Name = "Swarmer nest 4",
				Location = new Vector3(1872f, 1344f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "swarmerAllegiance#1"
				}
			},
			new EntityData
			{
				KeyName = "fieldQuaditeNest1",
				EntityKey = "terrain:fieldQuaditeNest",
				Name = "Field Quadite Nest 1",
				Location = new Vector3(4368f, 4848f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "leafcutterAllegiance#1"
				}
			},
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
				KeyName = "bird#1",
				EntityKey = "entity:bird",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "birdAllegiance"
				},
				Location = new Vector3(2706f, 5230f, 0f),
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
				Location = new Vector3(2716f, 5280f, 0f),
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
				Location = new Vector3(2726f, 5260f, 0f),
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
				Location = new Vector3(5510f, 4075f, 0f),
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
				Location = new Vector3(5500f, 4065f, 0f),
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
				Location = new Vector3(5490f, 4055f, 0f),
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
