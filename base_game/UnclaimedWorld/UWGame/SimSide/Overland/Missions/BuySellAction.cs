using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class BuySellAction : MissionAction
{
	public BuySellActionTemplate BuyActionType;

	private MissionActionTemplateID snapshotActionTemplateID;

	public bool TransactionIsFinished;

	public BuySellAction(Mission parent, BuySellActionTemplate buyActionType)
		: base(parent)
	{
		BuyActionType = buyActionType;
	}

	public BuySellAction()
	{
	}

	public override bool Update(GameTime elapsed)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
		if (allegiance != null && parent.CurrentLocation.Value.ResolveLocation(allegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			if (!TransactionIsFinished)
			{
				MakeTransaction(expedition, out var isOK);
				if (!isOK)
				{
					HandleFailedAction();
					return true;
				}
			}
			LoadItems();
			return true;
		}
		HandleFailedAction();
		return true;
	}

	private void MakeTransaction(Expedition expedition, out bool isOK)
	{
		isOK = true;
		IOwner owner = LookUpOwners.FindByID((OwnerID)BuyActionType.ContractTemplate.BuyerID);
		if (owner == null)
		{
			isOK = false;
			return;
		}
		List<Entity> boughtItems = null;
		Dictionary<EntityType, List<EntityID>> order = BuyActionType.ContractTemplate.Entities.ToDictionary((KeyValuePair<string, List<long>> k) => GameData.Instance.AllEntityTypes[k.Key], (KeyValuePair<string, List<long>> k) => k.Value.Select((long e) => (EntityID)e).ToList());
		if (!expedition.OwnedEntities.Buy(owner, order, BuyActionType.BuyReducedAmountsIfNeeded, BuyActionType.MaxAmountToSpend, out var spentAmount, out boughtItems))
		{
			isOK = false;
			return;
		}
		Contract item = new Contract
		{
			ContractTemplate = BuyActionType.ContractTemplate,
			PaidAmount = spentAmount,
			TransferredEntities = boughtItems.Select((Entity e) => e.ID).ToList()
		};
		parent.Contracts.Add(item);
		TransactionIsFinished = true;
	}

	public override void StartMission()
	{
		if (!TransactionIsFinished)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
			if (BuyActionType.MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var allegiance2, out var expedition, out var _) && allegiance2 != null && Communicates.IsInCommunicationRange(allegiance, allegiance2, out var _))
			{
				MakeTransaction(expedition, out var _);
			}
		}
	}

	private void LoadItems()
	{
		Entity vehicleToLoad = parent.GetVehicleToLoad();
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
		foreach (EntityID transferredEntity in parent.Contracts.FirstOrDefault((Contract c) => c.ContractTemplate == BuyActionType.ContractTemplate).TransferredEntities)
		{
			Entity entity = Entity.FindByID(transferredEntity);
			if (entity != null)
			{
				if (entity.ContainedBy.HasValue)
				{
					Entity.FindByID(entity.ContainedBy.Value)?.Contains.Uncontain(entity, destroy: false, shouldQueue: false, null, vehicleToLoad);
				}
				else
				{
					vehicleToLoad.Contains.AddToContain(entity);
				}
				entity.AssignedToJob = missionJob.ID;
			}
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(BuyActionType).Value;
		TransactionIsFinished = sn.DoBool(TransactionIsFinished);
		sn.Ignore(BuyActionType);
		return base.DoSnapshot(sn);
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		BuyActionType = (BuySellActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
		base.LoadPostProcess(sn);
	}
}
