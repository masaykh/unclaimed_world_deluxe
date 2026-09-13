using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class MissionStopTemplate : ISnapshot, ILookUp<MissionStopTemplate, MissionStopTemplateID>
{
	public int Number;

	public bool IsLocked;

	public TravelLocation TravelLocation;

	public SerializableQueue<MissionActionTemplate> Actions = new SerializableQueue<MissionActionTemplate>();

	[XmlIgnore]
	private Queue<MissionActionTemplateID> snapshotActions;

	public TravelActionTemplate TravelAction;

	[XmlIgnore]
	private MissionActionTemplateID? snapshotTravelAction;

	private MissionStopTemplateID id = MissionStopTemplateID.Invalid;

	private static MissionStopTemplateID IDCounter = MissionStopTemplateID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	[XmlIgnore]
	public MissionStopTemplateID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	[XmlIgnore]
	public bool IsSnapshotted { get; set; }

	public MissionStopTemplate(bool createID)
	{
		if (createID)
		{
			AddToLookup();
		}
	}

	public MissionStopTemplate()
	{
	}

	public bool IsStart()
	{
		return Number == 0;
	}

	public void AssignIDs()
	{
		AddToLookup();
		if (Actions != null)
		{
			foreach (MissionActionTemplate action in Actions)
			{
				action.SetParentID(this);
				action.AssignIDs();
			}
		}
		if (TravelAction != null)
		{
			TravelAction.AssignIDs();
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
		foreach (MissionActionTemplate action in Actions)
		{
			action.Destroy();
		}
		if (TravelAction != null)
		{
			TravelAction.Destroy();
		}
		Actions.Clear();
	}

	public void RecalculateNumbers()
	{
		SetNumber(Number);
	}

	public Queue<MissionActionTemplate> GetActionsAtLocation(MissionStopTemplate location)
	{
		if (this == location)
		{
			return Actions;
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.GetActionsAtLocation(location);
		}
		return null;
	}

	public double GetTotalDistance(double distance)
	{
		if (TravelAction != null)
		{
			distance += TravelAction.Distance;
			return TravelAction.ToMissionStop.GetTotalDistance(distance);
		}
		return distance;
	}

	public bool GetOwner(SharedKnowledge sharedKnowledge, Predicate<EntityGroup> matchesPredicate, out EntityGroup otherOwner)
	{
		if (TravelLocation.ResolveLocation(sharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			if (expedition != null && matchesPredicate(expedition.OwnedEntities))
			{
				otherOwner = expedition.OwnedEntities;
				return true;
			}
			if (TravelAction != null)
			{
				return TravelAction.ToMissionStop.GetOwner(sharedKnowledge, matchesPredicate, out otherOwner);
			}
			otherOwner = null;
			return true;
		}
		otherOwner = null;
		return false;
	}

	public void RemoveAction(MissionActionTemplate action)
	{
		List<MissionActionTemplate> list = Actions.ToList();
		list.Remove(action);
		Actions = new SerializableQueue<MissionActionTemplate>();
		foreach (MissionActionTemplate item in list)
		{
			Actions.Enqueue(item);
		}
	}

	public void AddActionAtLocation(MissionActionTemplate action, MissionStopTemplate location)
	{
		if (this == location)
		{
			Actions.Enqueue(action);
		}
		else if (TravelAction != null)
		{
			TravelAction.ToMissionStop.AddActionAtLocation(action, location);
		}
	}

	public bool ContainsLocation(MissionStopTemplate m)
	{
		if (m == this)
		{
			return true;
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.ContainsLocation(m);
		}
		return false;
	}

	public void AddMissionLocation(MissionStopTemplate destination, int number)
	{
		if (TravelAction == null)
		{
			destination.Number = number;
			TravelAction = new TravelActionTemplate(this, destination);
		}
		else
		{
			TravelAction.ToMissionStop.AddMissionLocation(destination, number + 1);
		}
	}

	public void SetNumber(int number)
	{
		Number = number;
		if (TravelAction != null)
		{
			TravelAction.ToMissionStop.SetNumber(number + 1);
		}
	}

	public float ComputeTotalCargoBulk()
	{
		float num = 0f;
		foreach (MissionActionTemplate action in Actions)
		{
			num += action.ComputeTotalCargoBulk();
		}
		if (TravelAction != null)
		{
			num += TravelAction.ComputeTotalCargoBulk();
		}
		return num;
	}

	public decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		decimal result = default(decimal);
		boughtItemsCost = default(decimal);
		soldItemsCost = default(decimal);
		decimal boughtItemsCost2;
		decimal soldItemsCost2;
		foreach (MissionActionTemplate action in Actions)
		{
			result += action.ComputeTotalCost(parent, out boughtItemsCost2, out soldItemsCost2);
			boughtItemsCost += boughtItemsCost2;
			soldItemsCost += soldItemsCost2;
		}
		if (TravelAction != null)
		{
			result += TravelAction.ComputeTotalCost(parent, out boughtItemsCost2, out soldItemsCost2);
			boughtItemsCost += boughtItemsCost2;
			soldItemsCost += soldItemsCost2;
		}
		return result;
	}

	public bool ValidateMissionActions(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
	{
		if (Actions != null && Actions.Count > 0)
		{
			foreach (MissionActionTemplate action in Actions)
			{
				action.Validate(parent, ref hasMeaning, ref errors);
			}
		}
		if (TravelAction != null)
		{
			TravelAction.Validate(parent, ref hasMeaning, ref errors);
			return TravelAction.ToMissionStop.ValidateMissionActions(parent, ref hasMeaning, ref errors);
		}
		if (hasMeaning)
		{
			if (errors != null)
			{
				return errors.Count == 0;
			}
			return true;
		}
		return false;
	}

	public bool SelectRoute(MissionTemplate parent, EntityType transport)
	{
		if (TravelAction != null)
		{
			TravelAction.SetRoute(null, useAirRoute: false);
			VehicleContainerType vehicleContainerType = transport.ContainerType as VehicleContainerType;
			if (vehicleContainerType.Aircraft != null)
			{
				TravelAction.SetRoute(null, useAirRoute: true);
			}
			else
			{
				Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
				if (!TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var site, out var _, out var _, out var _))
				{
					return false;
				}
				if (!TravelAction.ToMissionStop.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var site2, out var _, out var _, out var _))
				{
					return false;
				}
				List<Tuple<Route, double>> routesAndDistances = The.Sim.World.GetRoutesAndDistances(site, site2);
				if (routesAndDistances != null)
				{
					foreach (Tuple<Route, double> item2 in routesAndDistances)
					{
						double item = item2.Item2;
						if (item2.Item1 == null)
						{
							if (vehicleContainerType.CanUseRoute(null, isAirRoute: true, item))
							{
								TravelAction.SetRoute(null, useAirRoute: true);
								break;
							}
						}
						else if (vehicleContainerType.CanUseRoute(item2.Item1.RouteType, isAirRoute: false, item))
						{
							TravelAction.SetRoute(item2.Item1, useAirRoute: false);
							break;
						}
					}
				}
			}
			return TravelAction.ToMissionStop.SelectRoute(parent, transport);
		}
		return true;
	}

	public Expedition GetAllegianceExpeditionOnRoute(Allegiance missionAllegiance)
	{
		if (TravelLocation.ResolveLocation(missionAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _) && expedition.Allegiance == missionAllegiance)
		{
			return expedition;
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.GetAllegianceExpeditionOnRoute(missionAllegiance);
		}
		return null;
	}

	public Expedition GetExpeditionOnRoute(SharedKnowledge sharedKnowledge, Predicate<Expedition> matches)
	{
		if (TravelLocation.ResolveLocation(sharedKnowledge, out var _, out var _, out var expedition, out var _) && matches(expedition))
		{
			return expedition;
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.GetExpeditionOnRoute(sharedKnowledge, matches);
		}
		return null;
	}

	public PassengerListTemplate GetPassengerListTemplate()
	{
		if (Actions != null)
		{
			foreach (MissionActionTemplate action in Actions)
			{
				if (action is EmbarkActionTemplate embarkActionTemplate)
				{
					return embarkActionTemplate.PassengerListTemplate;
				}
			}
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.GetPassengerListTemplate();
		}
		return null;
	}

	public bool ValidateMissionStops(MissionTemplate parent)
	{
		Site site = null;
		Allegiance allegiance = null;
		Expedition expedition = null;
		IKnownEntityData terminalData = null;
		Allegiance allegiance2 = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
		if (allegiance2 == null || !TravelLocation.ResolveLocation(allegiance2.SharedKnowledge, out site, out allegiance, out expedition, out terminalData))
		{
			return false;
		}
		if (TravelAction != null)
		{
			return TravelAction.ToMissionStop.ValidateMissionStops(parent);
		}
		return true;
	}

	public MissionStopTemplateID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= MissionStopTemplateID.Invalid)
		{
			throw new Exception("Astounding, MissionStopTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MissionStopTemplateID.Invalid)
		{
			LookUp<MissionStopTemplate, MissionStopTemplateID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MissionStopTemplateID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<MissionStopTemplate, MissionStopTemplateID>.Remove(this);
	}

	void ILookUp<MissionStopTemplate, MissionStopTemplateID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MissionStopTemplateID.First;
	}

	void ILookUp<MissionStopTemplate, MissionStopTemplateID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<MissionStopTemplate, MissionStopTemplateID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotActions = new Queue<MissionActionTemplateID>(Actions.Select((MissionActionTemplate a) => a.ID));
		}
		snapshotActions = sn.DoQueue(snapshotActions);
		snapshotTravelAction = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(TravelAction);
		IsLocked = sn.DoBool(IsLocked);
		TravelLocation = sn.DoTravelLocation(TravelLocation);
		Number = sn.DoInt32(Number);
		sn.Ignore(TravelAction);
		sn.Ignore(Actions);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotActions != null)
		{
			Actions = new SerializableQueue<MissionActionTemplate>();
			while (snapshotActions.Count > 0)
			{
				MissionActionTemplateID missionActionTemplateID = snapshotActions.Dequeue();
				Actions.Enqueue(LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(missionActionTemplateID));
			}
		}
		TravelAction = (TravelActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotTravelAction);
	}
}
