using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class EmbarkAction : MissionAction
{
	public EmbarkActionTemplate EmbarkActionTemplate;

	private MissionActionTemplateID snapshotActionTemplateID;

	public EmbarkAction(Mission parent, EmbarkActionTemplate actionType)
		: base(parent)
	{
		EmbarkActionTemplate = actionType;
	}

	public EmbarkAction()
	{
	}

	public override bool Update(GameTime elapsed)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
		if (allegiance != null && parent.CurrentLocation.Value.ResolveLocation(allegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _) && parent.CurrentLocation.Equals(EmbarkActionTemplate.PassengerListTemplate.StartingLocation) && Common.ListContainsRange(expedition.Members, EmbarkActionTemplate.PassengerListTemplate.Passengers.Select((long p) => (EntityID)p).ToList()))
		{
			List<Entity> list = new List<Entity>();
			foreach (long passenger in EmbarkActionTemplate.PassengerListTemplate.Passengers)
			{
				Entity entity = Entity.FindByID((EntityID)passenger);
				if (entity != null)
				{
					list.Add(entity);
					continue;
				}
				HandleFailedAction();
				return true;
			}
			EmbarkPassengers(list);
			return true;
		}
		HandleFailedAction();
		return true;
	}

	private void EmbarkPassengers(List<Entity> passengers)
	{
		Entity vehicleToLoad = parent.GetVehicleToLoad();
		if (vehicleToLoad != null && !parent.IsOnPlaySite())
		{
			foreach (Entity passenger in passengers)
			{
				vehicleToLoad.Contains.AddToContain(passenger);
			}
		}
		foreach (Entity passenger2 in passengers)
		{
			_ = passenger2;
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(EmbarkActionTemplate).Value;
		return base.DoSnapshot(sn);
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		EmbarkActionTemplate = (EmbarkActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
		base.LoadPostProcess(sn);
	}
}
