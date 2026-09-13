using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI.Pathfinding;

public class PathPlanner : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private EntityID entityID;

	public AStarSearch search;

	private const int searchLimitToGetUnblocked = 800;

	public static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version;

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

	public bool IsPaused { get; set; }

	public List<PathFinderNode> Path => search.Path;

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

	public int LoadPostProcessOrder => 0;

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public PathPlanner(Entity entity)
	{
		entityID = entity.EntityID;
		AddToLookup();
	}

	public PathPlanner()
	{
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		RemoveIDEntry();
	}

	public List<PathFinderNode> FindShortPathDirectly(SubtileLayers layers, Vector3 start, Vector3 destination, int searchLimit)
	{
		Point point = MapManager.WorldPosToSubtile(start);
		Point point2 = MapManager.WorldPosToSubtile(destination);
		if (MapManager.IsBlocked(layers.GetValue(point2)))
		{
			return null;
		}
		if (MapManager.IsBlocked(layers.GetValue(point)))
		{
			point = GetClosestPointToDestination(layers, start, 800, destination);
		}
		RegionPathID? regionPathID = null;
		float distance = 0f;
		if (layers.RegionMap.GetDistance(null, point, point2, ref distance, sendMessageToEntity: true, null, registerIfNotReady: false) == RegionMap.Result.NoAccess)
		{
			return null;
		}
		search = new AStarSearch(layers, point, point2, LookUp<RegionPath, RegionPathID>.FindByID(regionPathID));
		search.Formula = AStarSearch.HeuristicFormula.Manhattan;
		search.SearchLimit = searchLimit;
		List<PathFinderNode> result = null;
		AStarSearch.SearchStatus searchStatus = AStarSearch.SearchStatus.Incomplete;
		while (true)
		{
			switch (searchStatus)
			{
			case AStarSearch.SearchStatus.Incomplete:
				goto IL_009f;
			case AStarSearch.SearchStatus.TargetFound:
				result = search.Path;
				break;
			}
			break;
			IL_009f:
			searchStatus = search.CycleSearch();
		}
		return result;
	}

	public bool FindPathByRequest(SubtileLayers layers, Vector3 start, Vector3 destination)
	{
		Point point = MapManager.WorldPosToSubtile(start);
		Point point2 = MapManager.WorldPosToSubtile(destination);
		RegionPathID? regionPath = layers.RegionMap.GetRegionPath(point, point2);
		The.Sim.CycleManager.UnRegister(this);
		if (MapManager.IsBlocked(layers.GetValue(point2)))
		{
			return false;
		}
		if (MapManager.IsBlocked(layers.GetValue(point)))
		{
			point = GetClosestPointToDestination(layers, start, 800, destination);
		}
		search = new AStarSearch(layers, point, point2, LookUp<RegionPath, RegionPathID>.FindByID(regionPath));
		The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
		return true;
	}

	public Point GetClosestPointToDestination(SubtileLayers mapCosts, Vector3 destination, int searchLimit, Vector3 start)
	{
		Point point = MapManager.WorldPosToSubtile(start);
		Point destination2 = MapManager.WorldPosToSubtile(destination);
		search = new AStarSearch(mapCosts, point, destination2, null);
		search.Formula = AStarSearch.HeuristicFormula.Manhattan;
		search.SearchLimit = searchLimit;
		search.ReturnPathToClosestPoint = true;
		search.FindPathDirectly(null);
		if (search.ClosestPointToDestination.HasValue)
		{
			return search.ClosestPointToDestination.Value;
		}
		return point;
	}

	public List<PathFinderNode> FindItemAndGetPath(SubtileLayers mapCosts, AStarSearch.DijkstraTestNodeDelegate testNodePredicate, int searchLimit, Vector3 start)
	{
		Point start2 = MapManager.WorldPosToSubtile(start);
		search = new AStarSearch(mapCosts, start2);
		search.SearchLimit = searchLimit;
		return search.FindPathDirectly(testNodePredicate);
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"Pathfinding: {ID}, {search.PrintInfo()}");
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
		AStarSearch.SearchStatus searchStatus = search.CycleSearch();
		if (searchStatus == AStarSearch.SearchStatus.TargetNotFound || searchStatus == AStarSearch.SearchStatus.TargetFound)
		{
			_ = entityID;
			_ = 24374;
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				switch (searchStatus)
				{
				case AStarSearch.SearchStatus.TargetNotFound:
					entity.SendMessage(new Message(Message.MessageTypes.PathNotFound));
					break;
				case AStarSearch.SearchStatus.TargetFound:
					entity.SendMessage(new Message(Message.MessageTypes.PathFound));
					break;
				}
			}
			return true;
		}
		return false;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		entityID = sn.DoEnum(entityID);
		search = (AStarSearch)sn.DoISnapshot(search);
		IsPaused = sn.DoBool(IsPaused);
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
		if (search != null)
		{
			search.LoadPostProcess(sn);
		}
	}
}
