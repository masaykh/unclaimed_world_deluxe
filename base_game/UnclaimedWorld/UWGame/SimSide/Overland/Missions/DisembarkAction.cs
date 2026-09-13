using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class DisembarkAction : MissionAction
{
	private DisembarkActionTemplate template;

	private MissionActionTemplateID snapshotActionTemplateID;

	public DisembarkAction()
	{
	}

	public DisembarkAction(Mission parent, DisembarkActionTemplate template)
		: base(parent)
	{
		this.template = template;
	}

	public override bool Update(GameTime elapsed)
	{
		base.Update(elapsed);
		Unload();
		return true;
	}

	private void Unload()
	{
		Allegiance destinationAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.CurrentLocation.Value.AllegianceID.Value);
		LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)parent.CurrentLocation.Value.ExpeditionID.Value);
		PassengerListTemplate passengerListTemplate = parent.MissionTemplate.GetPassengerListTemplate();
		if (passengerListTemplate == null)
		{
			return;
		}
		foreach (long passenger in passengerListTemplate.Passengers)
		{
			UnloadEntity((EntityID)passenger, destinationAllegiance);
		}
	}

	private void UnloadEntity(EntityID entityID, Allegiance destinationAllegiance)
	{
		Entity entity = Entity.FindByID(entityID);
		Entity vehicleToLoad = parent.GetVehicleToLoad();
		if (vehicleToLoad != null)
		{
			if (destinationAllegiance.Site != null && destinationAllegiance.Site.IsPlaySite)
			{
				SendGetOffMessage(entity);
			}
			else
			{
				vehicleToLoad.Contains.Uncontain(entity);
			}
		}
	}

	private void SendGetOffMessage(Entity agent)
	{
		if (agent.EntityType.IntelligenceType != null)
		{
			Entity container;
			if (agent.IsAwakeAndActive())
			{
				Message msg = new Message(Message.MessageTypes.Disembark);
				agent.Intelligence.Brain.SendMessage(msg);
			}
			else if (agent.GetContainedBy(out container))
			{
				container.Contains.Uncontain(agent);
			}
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(template).Value;
		sn.Ignore(template);
		return base.DoSnapshot(sn);
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		template = (DisembarkActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
		base.LoadPostProcess(sn);
	}
}
