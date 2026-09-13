using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI.Pathfinding;

public class RegionSearchPlanner : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	public List<RegionSearchRequestID> SearchRequests = new List<RegionSearchRequestID>();

	private RegionSearcher searcher;

	private CyclableID regionMapID;

	public bool IsBFS;

	public static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused { get; private set; }

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public double? UpdateInterval => null;

	public ushort Start => searcher.Start;

	public ushort Destination => searcher.Destination;

	public CyclableID ID
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

	public int LoadPostProcessOrder => 10;

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public RegionSearchPlanner()
	{
	}

	public RegionSearchPlanner(RegionMap regionMap, ushort start)
	{
		IsBFS = true;
		searcher = new RegionSearcher(regionMap, start);
		regionMapID = regionMap.ID;
		Init(regionMap);
	}

	public RegionSearchPlanner(RegionMap regionMap, ushort start, ushort destination)
	{
		searcher = new RegionSearcher(regionMap, start, destination);
		regionMapID = regionMap.ID;
		Init(regionMap);
	}

	private void Init(RegionMap regionMap)
	{
		AddToLookup();
		regionMap.AddSearchPlanner(this);
		The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		RemoveIDEntry();
	}

	public void PrintInfo(StringBuilder text)
	{
		if (IsBFS)
		{
			text.Append($"Region search from {searcher.Start}, requests: {SearchRequests.Count}, {searcher.PrintInfo()}");
			return;
		}
		text.Append($"Region search from {searcher.Start}, to {searcher.destination}, requests: {SearchRequests.Count}, {searcher.PrintInfo()}");
	}

	public static void GetPathDirectly(RegionSearcher.PathInfo pathInfo, RegionMap regionMap, ushort start, ushort end, ref RegionPath pathList, ref float cost)
	{
		new RegionSearcher(regionMap, start, end).FindPath(pathInfo, ref pathList, ref cost);
	}

	public static ushort GetClosestRegionToDestination(RegionSearcher.PathInfo pathInfo, RegionMap regionMap, ushort start, int searchLimit, Vector3 destination)
	{
		RegionSearcher obj = new RegionSearcher(regionMap, start, 0)
		{
			DestinationCenterLocation = destination.ToVector2(),
			SearchLimit = searchLimit,
			ReturnPathToClosestPoint = true
		};
		float distance = 0f;
		RegionPath pathList = null;
		obj.FindPath(pathInfo, ref pathList, ref distance);
		return obj.ClosestRegion.Value;
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public bool CycleOnce()
	{
		bool flag = false;
		if (IsBFS)
		{
			if (searcher.CycleBFS() == RegionSearcher.SearchStatus.Finished)
			{
				flag = true;
				((RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID)).PlannerBFSFinished(this, searcher.GetBFSResult());
			}
		}
		else
		{
			flag = true;
			float distance = 0f;
			RegionPath pathList = null;
			searcher.FindPath(RegionSearcher.PathInfo.FullPath, ref pathList, ref distance);
			((RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID)).PlannerAStarFinished(this, pathList);
		}
		if (flag)
		{
			RemoveIDEntry();
			return true;
		}
		return false;
	}

	public void AddSearchRequest(RegionSearchRequest request)
	{
		SearchRequests.Add(request.ID);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		SearchRequests = sn.DoList(SearchRequests);
		ID = sn.DoEnum(ID);
		IsPaused = sn.DoBool(IsPaused);
		searcher = (RegionSearcher)sn.DoISnapshot(searcher);
		regionMapID = sn.DoEnum(regionMapID);
		IsBFS = sn.DoBool(IsBFS);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
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
		if (searcher != null)
		{
			searcher.LoadPostProcess(sn);
		}
	}
}
