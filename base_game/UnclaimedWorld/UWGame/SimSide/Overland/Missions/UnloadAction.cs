using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class UnloadAction : MissionAction
{
	private UnloadActionTemplate unloadActionType;

	private MissionActionTemplateID snapshotActionTemplateID;

	public UnloadAction()
	{
	}

	public UnloadAction(Mission parent, UnloadActionTemplate template)
		: base(parent)
	{
		unloadActionType = template;
	}

	public override bool Update(GameTime elapsed)
	{
		base.Update(elapsed);
		Unload();
		return true;
	}

	private bool LoadAsTradeGood(Entity entity)
	{
		if (!Garrison.EntityBelongs(entity) || entity.EntityType.IntelligenceType.ServantForEntityTypeTag != null)
		{
			return true;
		}
		return false;
	}

	private void Unload()
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.CurrentLocation.Value.AllegianceID.Value);
		LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)parent.CurrentLocation.Value.ExpeditionID.Value);
		Entity vehicleToUnload = parent.GetVehicleToLoad();
		if (vehicleToUnload != null)
		{
			vehicleToUnload.Contains.IterateContained(delegate(Entity e)
			{
				if (LoadAsTradeGood(e))
				{
					vehicleToUnload.Contains.Uncontain(e);
					if (e.AssignedToJob == parent.MissionJob)
					{
						e.AssignedToJob = null;
					}
				}
			});
		}
		if (allegiance.AllegianceType == AllegianceType.Player)
		{
			GameData.Instance.AllegianceEvents.TryGetValue(AllegianceEvents.CargoDeliveredToPlayer, out var value);
			Goal.FireEventActions(vehicleToUnload, null, value);
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(unloadActionType).Value;
		sn.Ignore(unloadActionType);
		return base.DoSnapshot(sn);
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		unloadActionType = (UnloadActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
		base.LoadPostProcess(sn);
	}
}
