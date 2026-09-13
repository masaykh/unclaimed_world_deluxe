using System.Collections.Generic;
using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Commands;

public class CreateMission : Command
{
	public long MissionTemplate;

	public SerializableDictionary<string, List<long>> Vehicles;

	public long OwnerID;

	public CreateMission(MissionTemplateID templateID, EntityGroupID ownerID, Dictionary<EntityType, List<Entity>> vehicles)
	{
		MissionTemplate = (long)templateID;
		if (vehicles != null)
		{
			Vehicles = new SerializableDictionary<string, List<long>>();
			foreach (KeyValuePair<EntityType, List<Entity>> vehicle in vehicles)
			{
				foreach (Entity item in vehicle.Value)
				{
					Common.AddToMultiList(Vehicles, vehicle.Key.KeyName, (long)item.EntityID);
				}
			}
		}
		OwnerID = (long)ownerID;
	}

	public CreateMission()
	{
	}

	public override void Execute(bool giveClientFeedback)
	{
		MissionTemplate missionType = LookUp<UWGame.SimSide.Overland.Missions.Templates.MissionTemplate, MissionTemplateID>.FindByID((MissionTemplateID)MissionTemplate);
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)OwnerID);
		Dictionary<EntityType, List<EntityID>> dictionary = null;
		if (Vehicles != null)
		{
			dictionary = new Dictionary<EntityType, List<EntityID>>();
			foreach (KeyValuePair<string, List<long>> vehicle in Vehicles)
			{
				foreach (long item in vehicle.Value)
				{
					Common.AddToMultiList(dictionary, GameData.Instance.AllEntityTypes[vehicle.Key], (EntityID)item);
				}
			}
		}
		new Mission(entityGroup, missionType, dictionary).StartMission();
	}
}
