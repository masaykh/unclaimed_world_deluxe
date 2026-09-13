using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI.Pathfinding;

public class RegionSearcher : ISnapshot
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct PathFinderNodeFast
	{
		public int F;

		public int G;

		public ushort PX;

		public ushort PY;

		public byte Status;
	}

	public enum SearchStatus
	{
		Finished,
		Incomplete
	}

	public enum SearchType
	{
		AllPathCosts,
		SinglePath
	}

	public enum PathInfo
	{
		CostOnly,
		FullPath
	}

	internal class CompareRegionPFNodeDictionary : IComparer<RegionPathFinderNodeBFS>
	{
		public int Compare(RegionPathFinderNodeBFS nodeA, RegionPathFinderNodeBFS nodeB)
		{
			if (nodeA.G > nodeB.G)
			{
				return 1;
			}
			if (nodeA.G < nodeB.G)
			{
				return -1;
			}
			return 0;
		}
	}

	internal class CompareRegionPFNodeDictionaryAStar : IComparer<RegionPathFinderNodeAStar>
	{
		public int Compare(RegionPathFinderNodeAStar nodeA, RegionPathFinderNodeAStar nodeB)
		{
			if (nodeA.F > nodeB.F)
			{
				return 1;
			}
			if (nodeA.F < nodeB.F)
			{
				return -1;
			}
			return 0;
		}
	}

	[Author("Franco, Gustavo")]
	internal class ComparePFNodeMatrix : IComparer<int>
	{
		private PathFinderNodeFast[] mMatrix;

		public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
		{
			mMatrix = matrix;
		}

		public int Compare(int a, int b)
		{
			if (mMatrix[a].F > mMatrix[b].F)
			{
				return 1;
			}
			if (mMatrix[a].F < mMatrix[b].F)
			{
				return -1;
			}
			return 0;
		}
	}

	private const int nodesPerCycle = 20;

	private PriorityQueueB<RegionPathFinderNodeBFS> BFSOpen;

	private PriorityQueueB<RegionPathFinderNodeAStar> AStarOpen;

	private RegionMap regionMap;

	private CyclableID regionMapID;

	public const byte StatusOpen = 1;

	public const byte StatusClosed = 2;

	private int mCloseNodeCounter;

	private float newG;

	private Dictionary<ushort, RegionPathFinderNodeBFS> BFSOpenAndClosed;

	private Dictionary<ushort, RegionPathFinderNodeAStar> AStarOpenAndClosed;

	private ushort start;

	public ushort destination;

	public Vector2 DestinationCenterLocation;

	private SearchType searchType;

	public int? SearchLimit;

	public bool ReturnPathToClosestPoint;

	public ushort? ClosestRegion;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ushort Start => start;

	public ushort Destination => destination;

	public bool IsSnapshotted { get; set; }

	public RegionSearcher()
	{
	}

	public RegionSearcher(RegionMap regionMap, ushort start)
	{
		searchType = SearchType.AllPathCosts;
		this.regionMap = regionMap;
		if (start == 0)
		{
			throw new Exception();
		}
		this.start = start;
		InitializeBFS();
	}

	public RegionSearcher(RegionMap regionMap, ushort start, ushort end)
	{
		searchType = SearchType.SinglePath;
		this.regionMap = regionMap;
		if (start == 0)
		{
			throw new Exception();
		}
		this.start = start;
		destination = end;
		InitializeAStarSearch();
	}

	public string PrintInfo()
	{
		if (searchType == SearchType.AllPathCosts)
		{
			return $"nodes: {BFSOpenAndClosed.Count}";
		}
		return $"{searchType}, nodes: {AStarOpenAndClosed.Count}";
	}

	private void InitializeAStarSearch()
	{
		AStarOpenAndClosed = new Dictionary<ushort, RegionPathFinderNodeAStar>();
		AStarOpen = new PriorityQueueB<RegionPathFinderNodeAStar>(new CompareRegionPFNodeDictionaryAStar());
		mCloseNodeCounter = 0;
		if (destination > 0)
		{
			DestinationCenterLocation = regionMap.GetRegion(destination).CenterLocation;
		}
		RegionPathFinderNodeAStar regionPathFinderNodeAStar = new RegionPathFinderNodeAStar();
		regionPathFinderNodeAStar.Color = start;
		regionPathFinderNodeAStar.Parent = start;
		regionPathFinderNodeAStar.CenterLocation = regionMap.GetRegion(start).CenterLocation;
		regionPathFinderNodeAStar.G = 0f;
		regionPathFinderNodeAStar.Status = 1;
		AStarOpen.Push(regionPathFinderNodeAStar);
		AStarOpenAndClosed.Add(regionPathFinderNodeAStar.Color, regionPathFinderNodeAStar);
	}

	private void InitializeBFS()
	{
		BFSOpenAndClosed = new Dictionary<ushort, RegionPathFinderNodeBFS>();
		BFSOpen = new PriorityQueueB<RegionPathFinderNodeBFS>(new CompareRegionPFNodeDictionary());
		mCloseNodeCounter = 0;
		BFSOpen.Clear();
		RegionPathFinderNodeBFS regionPathFinderNodeBFS = new RegionPathFinderNodeBFS();
		regionPathFinderNodeBFS.Color = start;
		regionPathFinderNodeBFS.G = 0f;
		regionPathFinderNodeBFS.Status = 1;
		BFSOpen.Push(regionPathFinderNodeBFS);
		BFSOpenAndClosed.Add(regionPathFinderNodeBFS.Color, regionPathFinderNodeBFS);
	}

	public Dictionary<ushort, RegionPathFinderNodeBFS> GetBFSResult()
	{
		return BFSOpenAndClosed;
	}

	public SearchStatus CycleBFS()
	{
		int num = 0;
		if (BFSOpen.Count == 0)
		{
			return SearchStatus.Finished;
		}
		while (num < 20 && BFSOpen.Count > 0)
		{
			RegionPathFinderNodeBFS regionPathFinderNodeBFS = BFSOpen.Pop();
			if (regionPathFinderNodeBFS.Status == 2)
			{
				continue;
			}
			if (regionMap.GetConnectors(regionPathFinderNodeBFS.Color, out var connections))
			{
				foreach (KeyValuePair<ushort, RegionEdge> item in connections)
				{
					newG = regionPathFinderNodeBFS.G + item.Value.Length;
					if (BFSOpenAndClosed.TryGetValue(item.Key, out var value))
					{
						if ((value.Status == 1 || value.Status == 2) && value.G <= newG)
						{
							continue;
						}
					}
					else
					{
						value = new RegionPathFinderNodeBFS();
						value.Color = item.Key;
						BFSOpenAndClosed.Add(item.Key, value);
					}
					value.G = newG;
					BFSOpen.Push(value);
					value.Status = 1;
				}
			}
			mCloseNodeCounter++;
			regionPathFinderNodeBFS.Status = 2;
			num++;
		}
		return SearchStatus.Incomplete;
	}

	public void FindPath(PathInfo pathInfo, ref RegionPath pathList, ref float distance)
	{
		if (AStarOpen.Count == 0)
		{
			return;
		}
		RegionPathFinderNodeAStar regionPathFinderNodeAStar = null;
		bool flag = false;
		while (AStarOpen.Count > 0)
		{
			regionPathFinderNodeAStar = AStarOpen.Pop();
			if (regionPathFinderNodeAStar.Status == 2)
			{
				continue;
			}
			if (regionPathFinderNodeAStar.Color == destination)
			{
				regionPathFinderNodeAStar.Status = 2;
				flag = true;
				break;
			}
			if (SearchLimit.HasValue && mCloseNodeCounter > SearchLimit.Value)
			{
				if (ReturnPathToClosestPoint)
				{
					FindClosestRegion();
				}
				return;
			}
			_ = regionPathFinderNodeAStar.Color;
			_ = 40105;
			if (regionMap.GetConnectors(regionPathFinderNodeAStar.Color, out var connections))
			{
				foreach (KeyValuePair<ushort, RegionEdge> item in connections)
				{
					if (item.Key >= 1530)
					{
						_ = item.Key;
						_ = 1545;
					}
					newG = regionPathFinderNodeAStar.G + item.Value.Length;
					if (AStarOpenAndClosed.TryGetValue(item.Key, out var value))
					{
						if ((value.Status == 1 || value.Status == 2) && value.G <= newG)
						{
							continue;
						}
					}
					else
					{
						value = new RegionPathFinderNodeAStar();
						value.Color = item.Key;
						value.CenterLocation = regionMap.GetRegion(value.Color).CenterLocation;
						AStarOpenAndClosed.Add(item.Key, value);
					}
					value.Parent = regionPathFinderNodeAStar.Color;
					value.G = newG;
					float num = Math.Abs(value.CenterLocation.X - DestinationCenterLocation.X);
					float num2 = Math.Abs(value.CenterLocation.Y - DestinationCenterLocation.Y);
					float num3 = 0f;
					num3 = ((!(num > num2)) ? (num2 + num * 0.5f) : (num + num2 * 0.5f));
					value.F = newG + num3;
					AStarOpen.Push(value);
					value.Status = 1;
				}
			}
			mCloseNodeCounter++;
			regionPathFinderNodeAStar.Status = 2;
		}
		if (flag)
		{
			if (pathInfo == PathInfo.FullPath)
			{
				RegionPathFinderNodeAStar regionPathFinderNodeAStar2 = AStarOpenAndClosed[destination];
				pathList = new RegionPath(regionPathFinderNodeAStar2.G);
				while (regionPathFinderNodeAStar2.Color != regionPathFinderNodeAStar2.Parent)
				{
					pathList.AddNode(regionPathFinderNodeAStar2);
					regionPathFinderNodeAStar2 = AStarOpenAndClosed[regionPathFinderNodeAStar2.Parent];
				}
				pathList.AddNode(regionPathFinderNodeAStar2);
				pathList.PathNodes.Reverse();
				distance = pathList.Cost;
			}
			else
			{
				distance = regionPathFinderNodeAStar.G;
			}
		}
		else if (ReturnPathToClosestPoint)
		{
			FindClosestRegion();
		}
	}

	private void FindClosestRegion()
	{
		float num = 10000000f;
		ushort? closestRegion = null;
		foreach (KeyValuePair<ushort, RegionPathFinderNodeAStar> item in AStarOpenAndClosed)
		{
			if (item.Value.Status != 1 && item.Value.Status != 2)
			{
				continue;
			}
			float x = item.Value.CenterLocation.X;
			float y = item.Value.CenterLocation.Y;
			float num2 = Math.Abs(x - DestinationCenterLocation.X);
			float num3 = Math.Abs(y - DestinationCenterLocation.Y);
			float num4 = ((!(num2 > num3)) ? (num3 + num2 * 0.5f) : (num2 + num3 * 0.5f));
			if (num4 < num)
			{
				num = num4;
				closestRegion = item.Key;
				if (num == 1f)
				{
					break;
				}
			}
		}
		ClosestRegion = closestRegion;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		destination = sn.DoUInt16(destination);
		DestinationCenterLocation = sn.DoVector2(DestinationCenterLocation);
		start = sn.DoUInt16(start);
		SearchLimit = sn.DoInt32Nullable(SearchLimit);
		searchType = sn.DoEnum(searchType);
		regionMapID = sn.SnapshotID<ICyclable, CyclableID>(regionMap).Value;
		ReturnPathToClosestPoint = sn.DoBool(ReturnPathToClosestPoint);
		sn.Ignore((byte)2);
		sn.Ignore((byte)1);
		sn.Ignore(AStarOpen);
		sn.Ignore(BFSOpen);
		sn.Ignore(AStarOpenAndClosed);
		sn.Ignore(BFSOpenAndClosed);
		sn.Ignore(ClosestRegion);
		sn.Ignore(mCloseNodeCounter);
		sn.Ignore(regionMap);
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
		regionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);
		if (searchType == SearchType.SinglePath)
		{
			InitializeAStarSearch();
		}
		else
		{
			InitializeBFS();
		}
	}
}
