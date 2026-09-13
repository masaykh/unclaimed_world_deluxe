using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class ContractTemplate : ISnapshot, ILookUp<ContractTemplate, ContractTemplateID>
{
	public SerializableDictionary<string, List<long>> Entities;

	public long BuyerID;

	public long SellerID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private ContractTemplateID id = ContractTemplateID.Invalid;

	private static ContractTemplateID IDCounter = ContractTemplateID.First;

	public bool IsSnapshotted { get; set; }

	[XmlIgnore]
	public ContractTemplateID ID
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

	public void AssignIDs()
	{
		AddToLookup();
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		Entities = sn.DoSerializableMultiMap(Entities);
		BuyerID = sn.DoInt64(BuyerID);
		SellerID = sn.DoInt64(SellerID);
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
	}

	public ContractTemplateID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ContractTemplateID.Invalid)
		{
			throw new Exception("Astounding, ContractTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ContractTemplateID.Invalid)
		{
			LookUp<ContractTemplate, ContractTemplateID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ContractTemplateID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<ContractTemplate, ContractTemplateID>.Remove(this);
	}

	void ILookUp<ContractTemplate, ContractTemplateID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ContractTemplateID.First;
	}

	void ILookUp<ContractTemplate, ContractTemplateID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ContractTemplate, ContractTemplateID>.Create();
	}
}
