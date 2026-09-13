using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

public class RegionPath : ILookUp<RegionPath, RegionPathID>, ISnapshot
{
	public float Cost;

	public List<RegionPathNode> PathNodes = new List<RegionPathNode>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private RegionPathID id = RegionPathID.Invalid;

	private static RegionPathID IDCounter = RegionPathID.First;

	public bool IsSnapshotted { get; set; }

	public RegionPathID ID
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

	public RegionPath()
	{
	}

	public RegionPath(float cost)
	{
		AddToLookup();
		Cost = cost;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public void AddNode(RegionPathFinderNodeAStar pathNode)
	{
		RegionPathNode item = new RegionPathNode(pathNode);
		PathNodes.Add(item);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = sn.DoEnum(id);
		PathNodes = sn.DoList(PathNodes);
		Cost = sn.DoFloat(Cost);
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
		foreach (RegionPathNode pathNode in PathNodes)
		{
			pathNode.LoadPostProcess(sn);
		}
	}

	public RegionPathID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= RegionPathID.Invalid)
		{
			throw new Exception("Astounding, RegionPathID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public RegionPathID SnapshotID(Snapshotter sn, RegionPathID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != RegionPathID.Invalid)
		{
			LookUp<RegionPath, RegionPathID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = RegionPathID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<RegionPath, RegionPathID>.Remove(this);
	}

	void ILookUp<RegionPath, RegionPathID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = RegionPathID.First;
	}

	void ILookUp<RegionPath, RegionPathID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<RegionPath, RegionPathID>.Create();
	}
}
