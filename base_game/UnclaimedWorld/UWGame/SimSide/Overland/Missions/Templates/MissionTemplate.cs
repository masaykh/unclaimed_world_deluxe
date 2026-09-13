using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class MissionTemplate : ISnapshot, ILookUp<MissionTemplate, MissionTemplateID>
{
	public long Allegiance;

	public long OwnerID;

	public TransportationTemplate TransportationType;

	public MissionStopTemplate StartMissionStopTemplate;

	private MissionStopTemplateID snapshotStartMissionStopTemplate;

	private MissionTemplateID id = MissionTemplateID.Invalid;

	private static MissionTemplateID IDCounter = MissionTemplateID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	[XmlIgnore]
	public MissionTemplateID ID
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

	public MissionTemplate(AllegianceID allegiance, OwnerID owner, bool createID)
	{
		Allegiance = (long)allegiance;
		OwnerID = (long)owner;
		if (createID)
		{
			AddToLookup();
		}
	}

	public MissionTemplate()
	{
	}

	public void AddMissionLocation(MissionStopTemplate destination)
	{
		StartMissionStopTemplate.AddMissionLocation(destination, 1);
	}

	public Queue<MissionActionTemplate> GetActionsAtLocation(MissionStopTemplate location)
	{
		return StartMissionStopTemplate.GetActionsAtLocation(location);
	}

	public void AddAction(MissionActionTemplate action, MissionStopTemplate location)
	{
		StartMissionStopTemplate.AddActionAtLocation(action, location);
	}

	public bool SelectRoutes()
	{
		if (TransportationType != null)
		{
			EntityType mainTransportation = TransportationType.GetMainTransportation();
			return StartMissionStopTemplate.SelectRoute(this, mainTransportation);
		}
		return true;
	}

	public PassengerListTemplate GetPassengerListTemplate()
	{
		if (StartMissionStopTemplate != null)
		{
			return StartMissionStopTemplate.GetPassengerListTemplate();
		}
		return null;
	}

	public bool ContainsLocation(MissionStopTemplate m)
	{
		if (StartMissionStopTemplate == m)
		{
			return true;
		}
		if (StartMissionStopTemplate.TravelAction != null)
		{
			return StartMissionStopTemplate.TravelAction.ToMissionStop.ContainsLocation(m);
		}
		return false;
	}

	public void Destroy()
	{
		RemoveIDEntry();
		if (StartMissionStopTemplate != null)
		{
			StartMissionStopTemplate.Destroy();
		}
	}

	public Expedition GetHomeExpedition()
	{
		Allegiance missionAllegiance = LookUp<UWGame.SimSide.Allegiances.Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);
		if (StartMissionStopTemplate != null)
		{
			return StartMissionStopTemplate.GetAllegianceExpeditionOnRoute(missionAllegiance);
		}
		return null;
	}

	public Expedition GetExpeditionMatchingPredicate(Predicate<Expedition> matches)
	{
		Allegiance allegiance = LookUp<UWGame.SimSide.Allegiances.Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);
		if (StartMissionStopTemplate != null)
		{
			return StartMissionStopTemplate.GetExpeditionOnRoute(allegiance.SharedKnowledge, matches);
		}
		return null;
	}

	public bool GetOwner(SharedKnowledge sharedKnowledge, Predicate<EntityGroup> matchesPredicate, out EntityGroup otherOwner)
	{
		if (StartMissionStopTemplate != null)
		{
			return StartMissionStopTemplate.GetOwner(sharedKnowledge, matchesPredicate, out otherOwner);
		}
		otherOwner = null;
		return true;
	}

	public float GetTotalCargoCapacity()
	{
		if (TransportationType != null)
		{
			return ((VehicleContainerType)TransportationType.GetMainTransportation().ContainerType).ItemStorageType.GetTotalCapacity();
		}
		return 0f;
	}

	public double GetTotalDistance()
	{
		if (StartMissionStopTemplate != null)
		{
			return StartMissionStopTemplate.GetTotalDistance(0.0);
		}
		return 0.0;
	}

	public float ComputeTotalCargoBulk()
	{
		float num = 0f;
		if (StartMissionStopTemplate != null)
		{
			num += StartMissionStopTemplate.ComputeTotalCargoBulk();
		}
		return num;
	}

	public decimal ComputeTotalCost(out decimal transportCost, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		decimal num = default(decimal);
		boughtItemsCost = default(decimal);
		soldItemsCost = default(decimal);
		if (StartMissionStopTemplate != null)
		{
			num += StartMissionStopTemplate.ComputeTotalCost(this, out boughtItemsCost, out soldItemsCost);
		}
		transportCost = ComputeTransportationCost(out var _, out var _, out var _);
		return num + transportCost;
	}

	public decimal ComputeTransportationCost(out decimal startFee, out decimal totalDistanceCost, out decimal costPerKilometer)
	{
		if (TransportationType != null)
		{
			return TransportationType.ComputeTransportationCost(this, out startFee, out totalDistanceCost, out costPerKilometer);
		}
		startFee = default(decimal);
		totalDistanceCost = default(decimal);
		costPerKilometer = default(decimal);
		return 0m;
	}

	public bool Validate(ref List<string> errors, ref FieldError? errorFieldCode)
	{
		if (StartMissionStopTemplate == null)
		{
			Common.AddToList(ref errors, "A starting location has not been selected");
			errorFieldCode = FieldError.Start;
			return false;
		}
		if (StartMissionStopTemplate.TravelAction == null)
		{
			Common.AddToList(ref errors, "A destination has not been selected");
			errorFieldCode = FieldError.Destination;
			return false;
		}
		if (TransportationType == null || TransportationType.Vehicles == null)
		{
			Common.AddToList(ref errors, "A vehicle has not been selected.");
			errorFieldCode = FieldError.Transport;
			return false;
		}
		bool hasMeaning = false;
		if (!StartMissionStopTemplate.ValidateMissionActions(this, ref hasMeaning, ref errors))
		{
			if (!hasMeaning)
			{
				Common.AddToList(ref errors, "The mission has no purpose, try adding some actions.");
			}
			return false;
		}
		float num = ComputeTotalCargoBulk();
		float totalCargoCapacity = GetTotalCargoCapacity();
		if (num > totalCargoCapacity)
		{
			Common.AddToList(ref errors, "Too much cargo.");
			return false;
		}
		decimal transportCost;
		decimal boughtItemsCost;
		decimal soldItemsCost;
		decimal num2 = ComputeTotalCost(out transportCost, out boughtItemsCost, out soldItemsCost);
		Allegiance allegiance = LookUp<UWGame.SimSide.Allegiances.Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);
		decimal? tradeCredits = allegiance.TradeCredits;
		if (num2 > tradeCredits.GetValueOrDefault() && tradeCredits.HasValue)
		{
			Common.AddToList(ref errors, "The total cost is more than we can afford.");
			return false;
		}
		return true;
	}

	public bool ValidateMissionStops()
	{
		return StartMissionStopTemplate.ValidateMissionStops(this);
	}

	public void AssignIDs()
	{
		AddToLookup();
		StartMissionStopTemplate.AssignIDs();
	}

	public MissionTemplateID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= MissionTemplateID.Invalid)
		{
			throw new Exception("Astounding, MissionTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MissionTemplateID.Invalid)
		{
			LookUp<MissionTemplate, MissionTemplateID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MissionTemplateID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<MissionTemplate, MissionTemplateID>.Remove(this);
	}

	void ILookUp<MissionTemplate, MissionTemplateID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MissionTemplateID.First;
	}

	void ILookUp<MissionTemplate, MissionTemplateID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<MissionTemplate, MissionTemplateID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		Allegiance = sn.DoInt64(Allegiance);
		OwnerID = sn.DoInt64(OwnerID);
		TransportationType = (TransportationTemplate)sn.DoISnapshot(TransportationType);
		snapshotStartMissionStopTemplate = sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(StartMissionStopTemplate).Value;
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
		StartMissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotStartMissionStopTemplate);
	}
}
