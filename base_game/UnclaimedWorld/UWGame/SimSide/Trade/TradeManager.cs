using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade;

public class TradeManager : ISnapshot
{
	private class TradeGroupParams
	{
		public int Priority;

		public NormalDistribution NormalDistribution;

		public bool AllowDemand;

		public bool AllowProduction;

		public float SizeFactor;

		public PricesProfile PricesProfile;
	}

	private EntityGroup owner;

	private EntityGroupID snapshotOwnerID;

	private Regulator linearProductionRegulator;

	private Regulator variableProductionRegulator;

	private Dictionary<EntityType, VehiclesForHireAmount> VehiclesForHire = new Dictionary<EntityType, VehiclesForHireAmount>();

	private Dictionary<EntityType, TradeAmount> TradeAmounts = new Dictionary<EntityType, TradeAmount>();

	private Dictionary<TradeGroup, TradeGroupParams> overridingTradeGroups;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public TradeManager()
	{
	}

	public TradeManager(EntityGroup owner)
	{
		this.owner = owner;
		CreateRegulators();
	}

	public static TradeManager CreateFromTradeAmounts(EntityGroup entityGroup, SerializableDictionary<string, TradeAmountType> AvailableForTrade, SerializableDictionary<string, VehiclesForHireType> VehiclesForHire, PricesProfile pricesProfile)
	{
		TradeManager tradeManager = new TradeManager(entityGroup);
		if (AvailableForTrade != null)
		{
			tradeManager.SetTradeProperties(AvailableForTrade, pricesProfile);
		}
		tradeManager.SetVehiclesForHire(VehiclesForHire);
		return tradeManager;
	}

	public void SetVehiclesForHire(SerializableDictionary<string, VehiclesForHireType> VehiclesForHire)
	{
		if (VehiclesForHire == null)
		{
			return;
		}
		foreach (KeyValuePair<string, VehiclesForHireType> item in VehiclesForHire)
		{
			if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out var value))
			{
				SetVehiclesOfferedForHireProperties(value, item.Value, 1f);
			}
		}
	}

	public void SetTradeProperties(SerializableDictionary<string, TradeAmountType> AvailableForTrade, PricesProfile pricesProfile)
	{
		foreach (KeyValuePair<string, TradeAmountType> item in AvailableForTrade)
		{
			if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out var value))
			{
				SetTradeProperties(value, item.Value, 1f, null, pricesProfile, allowDemand: true, allowProduction: true);
			}
		}
	}

	public void SpawnStartingVehicles()
	{
		foreach (KeyValuePair<EntityType, VehiclesForHireAmount> item in VehiclesForHire)
		{
			VehiclesForHireAmount value = item.Value;
			for (int i = 0; i < value.StartAmount; i++)
			{
				Site site = owner.Parent.Allegiance.Site;
				Entity entity = Entity.CreateAndInitEntity(item.Key, site, null, null, owner.Parent.Allegiance);
				entity.PlaceEntityOnOtherSite(site, null, null, new Entity.SetOwnerInfo((IOwner)owner.Parent), null);
				entity.ComeOnline();
			}
		}
	}

	private void CreateRandomTradeAmountsFromGroup(TradeProfile profile, RandomTrade randomTrade, NormalDistribution scaleDistribution, float sizeFactor, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		if (randomTrade == null)
		{
			return;
		}
		List<string> list = randomTrade.Options.ToList();
		for (int i = 0; i < randomTrade.NoOfGroups; i++)
		{
			string randomListMember = Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator);
			list.Remove(randomListMember);
			TradeGroup tradeGroup = GameData.Instance.AllTradeGroups[randomListMember];
			CreateTradeAmounts(profile, scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);
			if (list.Count == 0)
			{
				break;
			}
		}
	}

	private void CreateTradeAmountsFromGroup(TradeProfile tradeProfile, string[] tradeGroups, NormalDistribution scaleDistribution, float sizeFactor, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		if (tradeGroups != null)
		{
			foreach (string key in tradeGroups)
			{
				TradeGroup tradeGroup = GameData.Instance.AllTradeGroups[key];
				CreateTradeAmounts(tradeProfile, scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);
			}
		}
	}

	private void CreateTradeAmounts(TradeProfile tradeProfile, NormalDistribution scaleDistribution, float sizeFactor, TradeGroup tradeGroup, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		if (tradeProfile.TradeGroupPriority != null && tradeProfile.TradeGroupPriority.TryGetValue(tradeGroup.KeyName, out var value))
		{
			TradeGroupParams value2 = new TradeGroupParams
			{
				AllowDemand = allowDemand,
				AllowProduction = allowProduction,
				Priority = value,
				SizeFactor = sizeFactor,
				NormalDistribution = scaleDistribution,
				PricesProfile = pricesProfile
			};
			Common.AddToDictionary(ref overridingTradeGroups, tradeGroup, value2);
		}
		else
		{
			CreateTradeAmountsNow(scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);
		}
	}

	private void CreateTradeAmountsNow(NormalDistribution scaleDistribution, float sizeFactor, TradeGroup tradeGroup, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		OfferDemandProfile profile = null;
		if (tradeGroup.OfferDemandProfile != null)
		{
			profile = GameData.Instance.AllOfferDemandProfiles[tradeGroup.OfferDemandProfile];
		}
		TradeAmountType[] availableForTrade = tradeGroup.AvailableForTrade;
		foreach (TradeAmountType tradeAmountType in availableForTrade)
		{
			EntityType entityType = GameData.Instance.AllEntityTypes[tradeAmountType.GetEntityTypeKey()];
			SetTradeProperties(entityType, tradeAmountType, sizeFactor * (float)scaleDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true), profile, pricesProfile, allowDemand, allowProduction);
		}
	}

	public void FillFromTradeProfile(TradeProfile tradeProfile, float sizeFactor, PricesProfile pricesProfile)
	{
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighTrade, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumTrade, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowTrade, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowImport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowExport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighTrade, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumTrade, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowTrade, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: true, allowProduction: false);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowExport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, allowDemand: false, allowProduction: true);
		if (overridingTradeGroups == null || overridingTradeGroups.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<TradeGroup, TradeGroupParams> item in overridingTradeGroups.OrderByDescending((KeyValuePair<TradeGroup, TradeGroupParams> k) => k.Value.Priority))
		{
			CreateTradeAmountsNow(item.Value.NormalDistribution, item.Value.SizeFactor, item.Key, item.Value.PricesProfile, item.Value.AllowDemand, item.Value.AllowProduction);
		}
	}

	public void FillFromVehiclesProfile(VehiclesProfile tradeProfile, float sizeFactor)
	{
		if (tradeProfile.VehiclesForHire == null)
		{
			return;
		}
		foreach (KeyValuePair<string, VehiclesForHireType> item in tradeProfile.VehiclesForHire)
		{
			if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out var value))
			{
				SetVehiclesOfferedForHireProperties(value, item.Value, sizeFactor);
			}
		}
	}

	public float? GetSellPrice(EntityType itemType)
	{
		if (TradeAmounts.TryGetValue(itemType, out var value))
		{
			return value.SellPrice;
		}
		return null;
	}

	public float? GetBuyPrice(EntityType itemType)
	{
		if (TradeAmounts.TryGetValue(itemType, out var value))
		{
			return value.BuyPrice;
		}
		return null;
	}

	public int GetBuyAmount(EntityType itemType)
	{
		if (TradeAmounts.TryGetValue(itemType, out var value))
		{
			return value.AmountToBuy;
		}
		return 0;
	}

	public decimal? GetPriceToHire(EntityType vehicleType, out decimal? pricePerKilometer)
	{
		if (VehiclesForHire.TryGetValue(vehicleType, out var value))
		{
			pricePerKilometer = value.PricePerKilometer;
			return value.Price;
		}
		pricePerKilometer = null;
		return null;
	}

	private void SetTradeProperties(EntityType entityType, TradeAmountType amountType, float scaleAmounts, OfferDemandProfile profile, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		TradeAmount value = new TradeAmount(entityType.KeyName, amountType, scaleAmounts, profile, pricesProfile, allowDemand, allowProduction);
		TradeAmounts[entityType] = value;
	}

	public void SetVehiclesOfferedForHireProperties(EntityType entityType, VehiclesForHireType amountType, float sizeFactor)
	{
		VehiclesForHireAmount value = new VehiclesForHireAmount(amountType, sizeFactor);
		VehiclesForHire.Add(entityType, value);
	}

	public void Update(GameTime gameTime)
	{
		double millisecondsSinceLastReady = 0.0;
		if (linearProductionRegulator.IsReady(ref millisecondsSinceLastReady))
		{
			double daysElapsed = The.Sim.DateAndTime.MillisecondsToDays(millisecondsSinceLastReady);
			ProduceTradeItems(daysElapsed);
			SimulateConsumingTradeItems(daysElapsed);
		}
		if (variableProductionRegulator.IsReady(ref millisecondsSinceLastReady))
		{
			double daysElapsed2 = The.Sim.DateAndTime.MillisecondsToDays(millisecondsSinceLastReady);
			ProduceVariableTradeItems(daysElapsed2);
		}
	}

	private void CreateRegulators()
	{
		linearProductionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "TradeManager1");
		variableProductionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.03, "TradeManager2");
	}

	public void Destroy()
	{
	}

	public void SpawnStartingTradeItems()
	{
		if (!GetTerminals(out var terminals))
		{
			return;
		}
		List<Tuple<EntityType, EntityData, int>> list = null;
		foreach (KeyValuePair<EntityType, TradeAmount> tradeAmount in TradeAmounts)
		{
			tradeAmount.Key.KeyName.Contains("hauling");
			TradeAmount value = tradeAmount.Value;
			int? num = null;
			if (value.StartAmount.HasValue)
			{
				num = value.StartAmount.Value;
			}
			else if (value.MaxAmountForSale > 0)
			{
				num = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0, value.MaxAmountForSale.Value + 1);
			}
			if (num > 0)
			{
				Common.AddToList(ref list, new Tuple<EntityType, EntityData, int>(tradeAmount.Key, value.EntityData, num.Value));
			}
		}
		DistributeProducedItems(list, terminals);
	}

	private bool GetTerminals(out List<Entity> terminals)
	{
		terminals = GetTradeTerminals();
		if (terminals == null)
		{
			return false;
		}
		terminals = Common.Randomize(terminals, The.Sim.GameplayRandomGenerator);
		return true;
	}

	private int GetItemsForSaleByOtherSite(EntityType entityType)
	{
		int num = 0;
		if (owner.AllEntities.TryGetValue(entityType, out var value))
		{
			foreach (EntityID item in value)
			{
				if (ItemIsValidForSale(item))
				{
					num++;
				}
			}
		}
		return num;
	}

	private bool ItemIsValidForSale(EntityID entityID)
	{
		Entity entity = Entity.FindByID(entityID);
		if (entity != null)
		{
			if (!entity.ContainedBy.HasValue || entity.OwnedBy != ((IOwner)owner.Parent).ID)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private List<Entity> GetTradeTerminals()
	{
		List<Entity> list = null;
		foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in owner.Terminals)
		{
			foreach (EntityID item in terminal.Value)
			{
				Entity entity = Entity.FindByID(item);
				if (entity != null && entity.OfferedEntitiesByType != null)
				{
					Common.AddToList(ref list, entity);
				}
			}
		}
		return list;
	}

	private void ProduceVariableTradeItems(double daysElapsed)
	{
		if (!GetTerminals(out var terminals))
		{
			return;
		}
		List<Entity> list = new List<Entity>();
		List<Tuple<EntityType, EntityData, int>> list2 = null;
		foreach (KeyValuePair<EntityType, TradeAmount> tradeAmount in TradeAmounts)
		{
			list.Clear();
			list.AddRange(terminals);
			TradeAmount value = tradeAmount.Value;
			if (value.OfferDemandProfile != null)
			{
				int itemsForSaleByOtherSite = GetItemsForSaleByOtherSite(tradeAmount.Key);
				int num;
				if (value.OfferDemandProfile.States != null)
				{
					num = Population.GetStepsFromProgress((float)Common.GetStairStepIndex(value.OfferDemandProfile.States, out var _, The.Sim.GameplayRandomGenerator).OfferDemandChange.GetRandomValue(The.Sim.GameplayRandomGenerator), daysElapsed, ref value.Progress, itemsForSaleByOtherSite, 0, value.MaxAmountForSale.Value);
				}
				else if (value.OfferedForTradeNoise != null)
				{
					NoiseParams nonlinearAmountForSale = value.OfferDemandProfile.NonlinearAmountForSale;
					float value2 = value.OfferedForTradeNoise.Generate1D((float)The.Sim.TotalUnPausedGameTimeInSeconds, nonlinearAmountForSale.NoiseFrequency, value.OfferDemandProfileScaleFactor * (nonlinearAmountForSale.NoiseAmplitude ?? 1f), value.OfferDemandProfileScaleFactor * (nonlinearAmountForSale.NoiseAddend ?? 0f));
					num = (int)Math.Abs(value2);
					num *= Math.Sign(value2);
					num = Common.ClampTop(num, value.MaxAmountForSale.Value - itemsForSaleByOtherSite);
					num = Common.ClampBottom(num, -itemsForSaleByOtherSite);
				}
				else
				{
					num = 0;
				}
				if (num > 0)
				{
					Common.AddToList(ref list2, new Tuple<EntityType, EntityData, int>(tradeAmount.Key, value.EntityData, num));
				}
				else if (num < 0)
				{
					DestroyItems(tradeAmount.Key, Math.Abs(num), list);
				}
			}
		}
		DistributeProducedItems(list2, terminals);
	}

	private void DistributeProducedItems(List<Tuple<EntityType, EntityData, int>> spawnAmounts, List<Entity> terminals)
	{
		if (spawnAmounts == null)
		{
			return;
		}
		List<Entity> list = new List<Entity>();
		do
		{
			for (int num = spawnAmounts.Count - 1; num >= 0; num--)
			{
				list.Clear();
				list.AddRange(terminals);
				Tuple<EntityType, EntityData, int> tuple = spawnAmounts[num];
				if (tuple.Item3 > 0)
				{
					if (!ProduceItems(tuple.Item1, tuple.Item2, 1, list))
					{
						return;
					}
					int num2 = tuple.Item3 - 1;
					if (num2 == 0)
					{
						spawnAmounts.RemoveAt(num);
					}
					else
					{
						spawnAmounts[num] = new Tuple<EntityType, EntityData, int>(tuple.Item1, tuple.Item2, num2);
					}
				}
				else
				{
					spawnAmounts.RemoveAt(num);
				}
			}
		}
		while (spawnAmounts.Count > 0);
	}

	private void ProduceTradeItems(double daysElapsed)
	{
		if (!GetTerminals(out var terminals))
		{
			return;
		}
		List<Tuple<EntityType, EntityData, int>> list = null;
		foreach (KeyValuePair<EntityType, TradeAmount> tradeAmount in TradeAmounts)
		{
			TradeAmount value = tradeAmount.Value;
			if (!(value.IncreasePerDay > 0f))
			{
				continue;
			}
			int itemsForSaleByOtherSite = GetItemsForSaleByOtherSite(tradeAmount.Key);
			if (value.MaxAmountForSale.HasValue)
			{
				int stepsFromProgress = Population.GetStepsFromProgress(value.IncreasePerDay, daysElapsed, ref value.Progress, itemsForSaleByOtherSite, 0, value.MaxAmountForSale.Value);
				if (stepsFromProgress > 0)
				{
					Common.AddToList(ref list, new Tuple<EntityType, EntityData, int>(tradeAmount.Key, value.EntityData, stepsFromProgress));
				}
			}
		}
		DistributeProducedItems(list, terminals);
	}

	private void DestroyItems(EntityType entityType, int itemsToDestroy, List<Entity> terminals)
	{
		int num = 0;
		int num2 = 0;
		do
		{
			Entity entity = terminals[num2 % terminals.Count];
			if (entity != null)
			{
				if (entity.OfferedEntitiesByType.TryGetValue(entityType, out var value) && value.Count > 0)
				{
					Entity entity2 = Entity.FindByID(value[0]);
					if (entity2 != null)
					{
						entity2.Destroy();
						num++;
					}
				}
				else
				{
					terminals.Remove(entity);
				}
			}
			num2++;
		}
		while (num < itemsToDestroy && terminals.Count > 0);
	}

	public static float? GetBulkOfTradeItem(EntityType entityType)
	{
		if (entityType.ItemType != null)
		{
			return entityType.ItemType.MaximumBulk;
		}
		if (entityType.BiologicalType != null)
		{
			return entityType.BiologicalType.GetMaxBulk();
		}
		if (entityType.BodyType != null)
		{
			return entityType.BodyType.Bulk;
		}
		throw new Exception("Cannot trade this type: " + entityType.KeyName);
	}

	private bool ProduceItems(EntityType entityType, EntityData entityData, int itemsToSpawn, List<Entity> terminals)
	{
		entityType.KeyName.Contains("hauling");
		float value = GetBulkOfTradeItem(entityType).Value;
		Allegiance allegiance = owner.GetAllegiance();
		Expedition expedition = owner.GetExpedition();
		for (int i = 0; i < itemsToSpawn; i++)
		{
			Entity terminalWithRoom = GetTerminalWithRoom(terminals, value, i % terminals.Count);
			if (terminalWithRoom != null)
			{
				Site site = owner.Parent.Allegiance.Site;
				bool flag;
				if (entityData != null)
				{
					MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out var placementFailed, terminalWithRoom, null, offerForSale: true, isProductionOutput: false, null, allegiance.KeyName, expedition.KeyName, allegiance.KeyName, expedition.KeyName, site.KeyName, null, assertContainment: false, null, null, null, logProductionStatistics: false, suppressSpawningEvents: true);
					flag = !placementFailed;
				}
				else
				{
					flag = Entity.CreateAndInitEntity(entityType, site, null, null, allegiance).PlaceEntityOnOtherSite(site, terminalWithRoom, null, new Entity.SetOwnerInfo((IOwner)owner.Parent), null, StorageCompartment.OfferedForTrade);
				}
				if (!flag)
				{
					terminals.Remove(terminalWithRoom);
					if (terminals.Count == 0)
					{
						return false;
					}
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private Entity GetTerminalWithRoom(List<Entity> terminals, float bulk, int index)
	{
		do
		{
			Entity entity = terminals[index];
			TerminalContainer terminalContainer = entity.Contains as TerminalContainer;
			if (terminalContainer.TotalTradeItemStorageCapacity - terminalContainer.TotalTradeItemsStored < bulk)
			{
				terminals.RemoveAt(index);
				continue;
			}
			return entity;
		}
		while (terminals.Count > 0);
		return null;
	}

	private void SimulateConsumingTradeItems(double daysElapsed)
	{
		foreach (KeyValuePair<EntityType, TradeAmount> tradeAmount in TradeAmounts)
		{
			TradeAmount value = tradeAmount.Value;
			if (value.ConsumptionPerDay > 0f && value.MaxAmountToBuy.HasValue && value.AmountToBuy < value.MaxAmountToBuy.Value)
			{
				int amountToBuy = value.AmountToBuy;
				int amountToFill = value.MaxAmountToBuy.Value - amountToBuy;
				int noToSpawn = Population.GetNoToSpawn(value.ConsumptionPerDay, daysElapsed, ref value.TimeInDaysElapsedSinceItemConsumed, amountToFill);
				if (noToSpawn > 0)
				{
					value.AmountToBuy += noToSpawn;
					value.AmountToBuy = Common.ClampTop(value.AmountToBuy, value.MaxAmountToBuy.Value);
				}
			}
		}
	}

	public void Buy(EntityType item, int amount)
	{
		if (TradeAmounts.TryGetValue(item, out var value))
		{
			value.AmountToBuy -= amount;
			value.AmountToBuy = Common.ClampBottom(value.AmountToBuy, 0);
			if (value.MaxAmountToBuy.HasValue)
			{
				value.AmountToBuy = Common.ClampTop(value.AmountToBuy, value.MaxAmountToBuy.Value);
			}
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		TradeAmounts = sn.DoDictionary(TradeAmounts);
		VehiclesForHire = sn.DoDictionary(VehiclesForHire);
		snapshotOwnerID = sn.SnapshotID<EntityGroup, EntityGroupID>(owner).Value;
		sn.Ignore(overridingTradeGroups);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (VehiclesForHire != null)
		{
			foreach (KeyValuePair<EntityType, VehiclesForHireAmount> item in VehiclesForHire)
			{
				item.Value.LoadPostProcess(sn);
			}
		}
		owner = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerID);
		if (TradeAmounts != null)
		{
			foreach (KeyValuePair<EntityType, TradeAmount> tradeAmount in TradeAmounts)
			{
				tradeAmount.Value.LoadPostProcess(sn);
			}
		}
		CreateRegulators();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
