using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class BuySellActionTemplate : MissionActionTemplate
{
	public bool BuyReducedAmountsIfNeeded = true;

	public decimal? MaxAmountToSpend;

	public bool isSelling;

	public ContractTemplate ContractTemplate;

	[XmlIgnore]
	private ContractTemplateID snapshotContractTemplate;

	public override string Name
	{
		get
		{
			if (isSelling)
			{
				return "Sell";
			}
			return "Buy";
		}
	}

	public override ActionTypes ActionType
	{
		get
		{
			if (isSelling)
			{
				return ActionTypes.Sell;
			}
			return ActionTypes.Buy;
		}
	}

	public BuySellActionTemplate()
	{
	}

	public BuySellActionTemplate(MissionTemplate parent, MissionStopTemplate missionStopTemplate, Dictionary<EntityType, List<EntityID>> orders, bool buyReducedAmountsIfNeeded, decimal? maxAmountToSpend, OwnerID buyerID, OwnerID sellerID, bool allowDeleting)
		: base(missionStopTemplate, allowDeleting)
	{
		ContractTemplate = new ContractTemplate
		{
			Entities = new SerializableDictionary<string, List<long>>(orders.ToDictionary((KeyValuePair<EntityType, List<EntityID>> k) => k.Key.KeyName, (KeyValuePair<EntityType, List<EntityID>> k) => k.Value.Select((EntityID e) => (long)e).ToList())),
			BuyerID = (long)buyerID,
			SellerID = (long)sellerID
		};
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID).Allegiance == allegiance)
		{
			isSelling = true;
		}
		else
		{
			isSelling = false;
		}
		BuyReducedAmountsIfNeeded = buyReducedAmountsIfNeeded;
		MaxAmountToSpend = maxAmountToSpend;
	}

	public static bool ValidateWorkingTerminalCanBuy(MissionTemplate parent, IKnownEntityData terminalData, ref List<string> errors)
	{
		if (!ValidateWorkingTradeTerminal(terminalData, ref errors))
		{
			return false;
		}
		if (terminalData.OwnedBy.HasValue && terminalData.OwnedBy == (OwnerID?)parent.OwnerID)
		{
			Common.AddToList(ref errors, "The terminal is owned by us.");
			return false;
		}
		return true;
	}

	public static bool ValidateWorkingTerminalCanSell(MissionTemplate parent, IKnownEntityData terminalData, ref List<string> errors)
	{
		if (!ValidateWorkingTradeTerminal(terminalData, ref errors))
		{
			return false;
		}
		if (!terminalData.OwnedBy.HasValue || terminalData.OwnedBy != (OwnerID?)parent.OwnerID)
		{
			Common.AddToList(ref errors, "The terminal is not owned by us.");
			return false;
		}
		return true;
	}

	private static bool ValidateWorkingTradeTerminal(IKnownEntityData terminalData, ref List<string> errors)
	{
		if (terminalData.EntityType.ContainerType == null || !(terminalData.EntityType.ContainerType is TerminalContainerType))
		{
			Common.AddToList(ref errors, "The terminal does not support trading.");
			return false;
		}
		if (!MissionActionTemplate.ValidateWorkingTerminal(terminalData, ref errors))
		{
			return false;
		}
		return true;
	}

	public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (allegiance == null || !MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var allegiance2, out var expedition, out var terminalData))
		{
			return false;
		}
		if (isSelling)
		{
			if (!ValidateWorkingTerminalCanSell(parent, terminalData, ref errors))
			{
				return false;
			}
		}
		else if (!ValidateWorkingTerminalCanBuy(parent, terminalData, ref errors))
		{
			return false;
		}
		Expedition homeExpedition = parent.GetHomeExpedition();
		SharedKnowledge sharedKnowledge = allegiance2.SharedKnowledge;
		if (ContractTemplate != null)
		{
			OwnerID sellerID = (OwnerID)ContractTemplate.SellerID;
			foreach (KeyValuePair<string, List<long>> entity in ContractTemplate.Entities)
			{
				for (int num = entity.Value.Count - 1; num >= 0; num--)
				{
					hasMeaning = true;
					EntityID entityID = (EntityID)entity.Value[num];
					if (!ItemIsValidForSale(sharedKnowledge, entityID, terminalData, sellerID, homeExpedition))
					{
						AddInvalidItemError(ref errors, expedition);
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool ItemIsValidForSale(SharedKnowledge sharedKnowledge, EntityID entityID, IKnownEntityData terminalData, OwnerID sellerID, Expedition tradingExpeditionWithPolicy)
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out var data)))
		{
			if (data.ContainedBy == terminalData.EntityID)
			{
				OwnerID? ownedBy = data.OwnedBy;
				if (ownedBy.GetValueOrDefault() == sellerID && ownedBy.HasValue && !data.PartOfID.HasValue && data.IsCompleted() && Entity.IsFunctional(data) && (tradingExpeditionWithPolicy == null || tradingExpeditionWithPolicy.Policy.CanProduceOrTrade(data.EntityType.TierOrAreaType)))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private static List<string> AddInvalidItemError(ref List<string> errors, Expedition expedition)
	{
		Common.AddToList(ref errors, $"Some of the items ordered at {expedition.Name} are no longer valid. Review the order.");
		return errors;
	}

	public override MissionAction CreateMissionAction(Mission mission)
	{
		return new BuySellAction(mission, this);
	}

	public override float ComputeTotalCargoBulk()
	{
		float num = 0f;
		if (ContractTemplate.Entities != null)
		{
			foreach (KeyValuePair<string, List<long>> entity in ContractTemplate.Entities)
			{
				EntityType entityType = GameData.Instance.AllEntityTypes[entity.Key];
				if (entityType.ItemType != null && entityType.ItemType.MaximumBulk.HasValue)
				{
					float? num2 = entityType.ItemType.MaximumBulk.Value;
					num += (float)entity.Value.Count * num2.Value;
				}
			}
		}
		return num;
	}

	public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		decimal num = default(decimal);
		boughtItemsCost = default(decimal);
		soldItemsCost = default(decimal);
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (allegiance == null || !MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var _, out var _, out var _))
		{
			return 0m;
		}
		IOwner owner = LookUpOwners.FindByID((OwnerID)ContractTemplate.BuyerID);
		IOwner owner2 = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);
		if (ContractTemplate.Entities != null)
		{
			foreach (KeyValuePair<string, List<long>> entity in ContractTemplate.Entities)
			{
				decimal? tradePrice = GetTradePrice(GameData.Instance.AllEntityTypes[entity.Key], owner.OwnedEntities, owner2.OwnedEntities);
				num += (decimal)entity.Value.Count * tradePrice.Value;
			}
		}
		if (isSelling)
		{
			soldItemsCost = num;
			return -1m * num;
		}
		boughtItemsCost = num;
		return num;
	}

	public static decimal? GetTradePrice(EntityType entityType, EntityGroup buyer, EntityGroup seller)
	{
		if (buyer == null || buyer.GetAllegiance().AllegianceType == AllegianceType.Player)
		{
			return (decimal?)seller.GetSellPrice(entityType);
		}
		return (decimal?)buyer.GetBuyPrice(entityType);
	}

	public override void AssignIDs()
	{
		base.AssignIDs();
		ContractTemplate.AssignIDs();
	}

	public override void Destroy()
	{
		base.Destroy();
		ContractTemplate.Destroy();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotContractTemplate = sn.SnapshotID<ContractTemplate, ContractTemplateID>(ContractTemplate).Value;
		BuyReducedAmountsIfNeeded = sn.DoBool(BuyReducedAmountsIfNeeded);
		MaxAmountToSpend = sn.DoDecimalNullable(MaxAmountToSpend);
		isSelling = sn.DoBool(isSelling);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		ContractTemplate = LookUp<ContractTemplate, ContractTemplateID>.FindByID(snapshotContractTemplate);
	}
}
