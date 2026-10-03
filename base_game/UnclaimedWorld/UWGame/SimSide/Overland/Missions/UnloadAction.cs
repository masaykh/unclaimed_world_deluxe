using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
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
					// PORT FIX, a backstop. The studio unloads dogs and robots as cargo on purpose
					// ("// dogs + robots", "// NEW: dogs also" in LoadAsTradeGood). EntityGroup.Buy
					// makes a bought creature the buyer's PROPERTY while it is still a MEMBER of the
					// trader's expedition (TradeManager.ProduceItems spawns it as one). Normally the
					// arrival has already fixed that - TravelAction.TransferToPlaySite joins
					// everything aboard to the destination expedition. This covers an arrival that
					// named no expedition, and a creature ejected into a terminal rather than onto
					// the ground. The dog standing at Kastuk's dock was the colony's member, owner
					// and all; what actually kept it there was the passenger
					// wait TravelAction gives cargo; GoalWaitAsPassenger.IsAboard ends it. Items and
					// colonists are untouched: JoinOwningExpedition does nothing to what does not
					// think, and passengers leave through DisembarkAction.
					if (e.EntityType.IntelligenceType != null && LookUpOwners.FindByID(e.OwnedBy) is Expedition owningExpedition)
					{
						e.JoinOwningExpedition(owningExpedition);
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
