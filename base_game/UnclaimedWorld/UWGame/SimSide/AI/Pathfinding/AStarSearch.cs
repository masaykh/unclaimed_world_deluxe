using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

public class AStarSearch : ISnapshot
{
	public enum HeuristicFormula
	{
		Manhattan = 1,
		MaxDXDY,
		DiagonalShortCut,
		Euclidean,
		EuclideanNoSQR,
		Custom1,
		None
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Author("Franco, Gustavo")]
	internal struct PathFinderNodeFast
	{
		public int F;

		public int G;

		public ushort PX;

		public ushort PY;

		public byte Status;
	}

	public delegate bool DijkstraTestNodeDelegate(int X, int Y);

	public enum SearchStatus
	{
		TargetFound,
		TargetNotFound,
		Incomplete
	}

	internal class ComparePFNodeDictionary : IComparer<PathFinderNode>
	{
		public int Compare(PathFinderNode nodeA, PathFinderNode nodeB)
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

	public SubtileLayers Layers;

	private SubtileLayersID snapshotLayersID;

	private RegionPath highLevelPath;

	private RegionPathID? snapshotHighLevelPath;

	private int highLevelPathIndex;

	private PriorityQueueB<PathFinderNode> mOpen;

	private List<PathFinderNode> pathNodes = new List<PathFinderNode>();

	private bool mStop;

	private bool mStopped = true;

	private HeuristicFormula mFormula = HeuristicFormula.Manhattan;

	private const int mHEstimate = 2;

	private int mSearchLimit = 10000;

	private double mCompletedTime;

	public byte StatusOpen = 1;

	public byte StatusClosed = 2;

	private int mH;

	private ushort relativeX;

	private ushort relativeY;

	private ushort absoluteX;

	private ushort absoluteY;

	private ushort absoluteNextX;

	private ushort absoluteNextY;

	private int closeNodeCounter;

	private ushort width;

	private ushort height;

	private static sbyte[][] mDirection = new sbyte[8][]
	{
		new sbyte[2] { 0, -1 },
		new sbyte[2] { 1, 0 },
		new sbyte[2] { 0, 1 },
		new sbyte[2] { -1, 0 },
		new sbyte[2] { 1, -1 },
		new sbyte[2] { 1, 1 },
		new sbyte[2] { -1, 1 },
		new sbyte[2] { -1, -1 }
	};

	private readonly int searchLimitForHighLevelPath = (int)Math.Ceiling(3.5 * Math.Pow(16.0, 2.0));

	private Dictionary<int, PathFinderNode> openAndClosed;

	private bool returnPathToClosestPoint;

	public Point? ClosestPointToDestination;

	private Point start;

	private Point destination;

	private Point finalDestination;

	private const int nodesPerCycle = 40;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public List<PathFinderNode> Path => pathNodes;

	public bool Stopped => mStopped;

	public HeuristicFormula Formula
	{
		get
		{
			return mFormula;
		}
		set
		{
			mFormula = value;
		}
	}

	public bool ReturnPathToClosestPoint
	{
		get
		{
			return returnPathToClosestPoint;
		}
		set
		{
			returnPathToClosestPoint = value;
		}
	}

	public int SearchLimit
	{
		get
		{
			return mSearchLimit;
		}
		set
		{
			mSearchLimit = value;
		}
	}

	public bool IsSnapshotted { get; set; }

	public AStarSearch(SubtileLayers layers, Point start, Point destination, RegionPath highLevelPath)
		: this(layers, start)
	{
		this.highLevelPath = highLevelPath;
		_ = this.highLevelPath;
		this.destination = destination;
		finalDestination = destination;
		Initialize();
	}

	public AStarSearch(SubtileLayers layers, Point start)
	{
		Layers = layers;
		this.start = start;
		Formula = HeuristicFormula.None;
		Initialize();
	}

	public AStarSearch()
	{
	}

	public string PrintInfo()
	{
		return $"nodes: {closeNodeCounter}";
	}

	private void Initialize()
	{
		width = (ushort)The.Map.mapSubtileWidth;
		height = (ushort)The.Map.mapSubtileHeight;
		openAndClosed = new Dictionary<int, PathFinderNode>();
		mOpen = new PriorityQueueB<PathFinderNode>(new ComparePFNodeDictionary());
		mStop = false;
		mStopped = false;
		closeNodeCounter = 0;
		StatusOpen += 2;
		StatusClosed += 2;
		mOpen.Clear();
		pathNodes.Clear();
		Layers.Layers[0].GetSectorFromAbsoluteCoords(start.X, start.Y, out var sectorX, out var sectorY);
		PathFinderNode pathFinderNode = new PathFinderNode();
		pathFinderNode.AbsoluteX = (ushort)start.X;
		pathFinderNode.AbsoluteY = (ushort)start.Y;
		pathFinderNode.G = 0;
		pathFinderNode.F = 2;
		pathFinderNode.SectorX = (byte)sectorX;
		pathFinderNode.SectorY = (byte)sectorY;
		pathFinderNode.ParentAbsoluteX = (ushort)start.X;
		pathFinderNode.ParentAbsoluteY = (ushort)start.Y;
		pathFinderNode.Status = StatusOpen;
		SetStartingNode(pathFinderNode);
		if (highLevelPath != null)
		{
			highLevelPathIndex = 1;
			if (highLevelPath.PathNodes.Count > 1)
			{
				destination = highLevelPath.PathNodes[highLevelPathIndex].RegionCenterInSubtiles;
			}
		}
	}

	public void FindPathStop()
	{
		mStop = true;
	}

	private bool ComputeSuccessor(PathFinderNode node, SubtileSector sector, int directionIndex, out PathFinderNode nextNode)
	{
		nextNode = null;
		sbyte[] array = mDirection[directionIndex];
		int num = relativeX + array[0];
		int num2 = relativeY + array[1];
		if (num == 40)
		{
			_ = 8;
		}
		int num3 = -1;
		int num4;
		if (num >= sector.SubtileArea.Width)
		{
			if (sector.Coords.X == The.Map.NoOfSectorsAcrossWidth - 1)
			{
				return false;
			}
			num = 0;
			num4 = sector.Coords.X + 1;
		}
		else if (num < 0)
		{
			if (sector.Coords.X == 0)
			{
				return false;
			}
			num = 47;
			num4 = sector.Coords.X - 1;
		}
		else
		{
			num4 = sector.Coords.X;
		}
		if (num2 >= sector.SubtileArea.Height)
		{
			if (sector.Coords.Y == The.Map.NoOfSectorsAcrossHeight - 1)
			{
				return false;
			}
			num2 = 0;
			num3 = sector.Coords.Y + 1;
		}
		else if (num2 < 0)
		{
			if (sector.Coords.Y == 0)
			{
				return false;
			}
			num2 = 47;
			num3 = sector.Coords.Y - 1;
		}
		else
		{
			num3 = sector.Coords.Y;
		}
		SubtileSector subtileSector = ((num4 == sector.Coords.X && num3 == sector.Coords.Y) ? sector : Layers.GetSector(num4, num3));
		MapManager.SubtileValue? subtileValue = subtileSector.GetValue(num, num2).Value;
		if (!subtileValue.HasValue)
		{
			return false;
		}
		if (MapManager.IsBlocked(subtileValue.Value))
		{
			return false;
		}
		byte cost = MapManager.GetCost(subtileValue.Value);
		int num5 = ((directionIndex >= 4) ? (node.G + (int)(1.414f * (float)(int)cost)) : (node.G + cost));
		absoluteNextX = (ushort)(subtileSector.SubtileArea.X + num);
		absoluteNextY = (ushort)(subtileSector.SubtileArea.Y + num2);
		int key = absoluteNextY * width + absoluteNextX;
		if (openAndClosed.TryGetValue(key, out nextNode))
		{
			if ((nextNode.Status == StatusOpen || nextNode.Status == StatusClosed) && nextNode.G <= num5)
			{
				return false;
			}
		}
		else
		{
			nextNode = new PathFinderNode();
			nextNode.AbsoluteX = absoluteNextX;
			nextNode.AbsoluteY = absoluteNextY;
			nextNode.SectorX = (byte)num4;
			nextNode.SectorY = (byte)num3;
			openAndClosed.Add(key, nextNode);
		}
		nextNode.ParentAbsoluteX = absoluteX;
		nextNode.ParentAbsoluteY = absoluteY;
		nextNode.G = num5;
		return true;
	}

	public void Draw()
	{
		int num = 16;
		byte b = 100;
		byte b2 = 70;
		byte b3 = 100;
		Color colour = Color.Black;
		colour.A = b;
		int num2 = 3 * (The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically);
		int num3 = 3 * (The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally);
		int num4 = The.MapUI.mapWindowTileY * 3;
		int num5 = The.MapUI.mapWindowTileX * 3;
		int num6 = 3 * The.Map.mapTileWidth;
		Vector2 topLeft;
		for (int i = num4; i < num2; i++)
		{
			for (int j = num5; j < num3; j++)
			{
				topLeft = The.MapUI.SubtileEdgeToScreen(j, i);
				if (MapManager.IsBlocked(Layers.GetValue(j, i)))
				{
					colour = Color.Red;
					colour.A = b;
					Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
				}
				int key = i * num6 + j;
				if (openAndClosed.TryGetValue(key, out var value))
				{
					if (value.Status == StatusOpen)
					{
						colour = Color.Gold;
					}
					else if (value.Status == StatusClosed)
					{
						colour = Color.DarkGreen;
					}
					colour.A = b;
					Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
				}
				b = ((b == b3) ? b2 : b3);
			}
			b = ((b == b3) ? b2 : b3);
		}
		topLeft = The.MapUI.SubtileEdgeToScreen(start.X, start.Y);
		colour = Color.White;
		colour.A = 160;
		Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
		topLeft = The.MapUI.SubtileEdgeToScreen(finalDestination.X, finalDestination.Y);
		colour = Color.Black;
		colour.A = 160;
		Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
		if (highLevelPath != null && highLevelPath.PathNodes.Count > 1)
		{
			for (int k = 1; k < highLevelPath.PathNodes.Count; k++)
			{
				colour = ((highLevelPathIndex >= k) ? ((highLevelPathIndex != k) ? Color.Cyan : Color.Beige) : Color.DarkRed);
				RegionPathNode regionPathNode = highLevelPath.PathNodes[k];
				topLeft = The.MapUI.SubtileEdgeToScreen(regionPathNode.RegionCenterInSubtiles);
				Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
			}
		}
	}

	public SearchStatus CycleSearch()
	{
		bool flag = false;
		int num = 0;
		if (mOpen.Count == 0)
		{
			return SearchStatus.TargetNotFound;
		}
		if (MapManager.IsBlocked(Layers.GetValue(destination)) || MapManager.IsBlocked(Layers.GetValue(start)))
		{
			return SearchStatus.TargetNotFound;
		}
		if (closeNodeCounter > 10000)
		{
		}
		while (num < 40 && mOpen.Count > 0 && !mStop)
		{
			PathFinderNode pathFinderNode = mOpen.Pop();
			if (pathFinderNode.Status == StatusClosed)
			{
				continue;
			}
			absoluteX = pathFinderNode.AbsoluteX;
			absoluteY = pathFinderNode.AbsoluteY;
			if (absoluteX == destination.X && absoluteY == destination.Y)
			{
				if (highLevelPath == null)
				{
					pathFinderNode.Status = StatusClosed;
					flag = true;
					break;
				}
				if (highLevelPathIndex == highLevelPath.PathNodes.Count - 2)
				{
					highLevelPathIndex += 2;
					SetNextDestinationPoint(pathFinderNode, finalDestination);
				}
				else if (highLevelPathIndex < highLevelPath.PathNodes.Count - 1)
				{
					highLevelPathIndex++;
					SetNextDestinationPoint(pathFinderNode, highLevelPath.PathNodes[highLevelPathIndex].RegionCenterInSubtiles);
				}
				else
				{
					if (!(destination != finalDestination))
					{
						pathFinderNode.Status = StatusClosed;
						flag = true;
						break;
					}
					SetNextDestinationPoint(pathFinderNode, finalDestination);
				}
			}
			else
			{
				if (closeNodeCounter > mSearchLimit || (highLevelPath != null && openAndClosed.Count > searchLimitForHighLevelPath))
				{
					mStopped = true;
					return SearchStatus.TargetNotFound;
				}
				ushort sectorX = pathFinderNode.SectorX;
				ushort sectorY = pathFinderNode.SectorY;
				SubtileSector sector = Layers.GetSector(sectorX, sectorY);
				relativeX = (ushort)(absoluteX - sector.SubtileArea.X);
				relativeY = (ushort)(absoluteY - sector.SubtileArea.Y);
				for (int i = 0; i < 8; i++)
				{
					if (ComputeSuccessor(pathFinderNode, sector, i, out var nextNode))
					{
						int num2 = Math.Abs(absoluteNextX - destination.X);
						int num3 = Math.Abs(absoluteNextY - destination.Y);
						if (num2 > num3)
						{
							mH = (int)(2f * ((float)num2 + (float)num3 * 0.5f));
						}
						else
						{
							mH = (int)(2f * ((float)num3 + (float)num2 * 0.5f));
						}
						nextNode.F = nextNode.G + mH;
						mOpen.Push(nextNode);
						nextNode.Status = StatusOpen;
					}
				}
				pathFinderNode.Status = StatusClosed;
			}
			closeNodeCounter++;
			num++;
		}
		if (flag)
		{
			CreatePathSegment(destination);
			pathNodes.Reverse();
			mStopped = true;
			return SearchStatus.TargetFound;
		}
		return SearchStatus.Incomplete;
	}

	private void CreatePathSegment(Point destinationToUse)
	{
		int x = destinationToUse.X;
		int y = destinationToUse.Y;
		PathFinderNode pathFinderNode = openAndClosed[destinationToUse.Y * width + destinationToUse.X];
		int num = 0;
		int num2 = 0;
		bool flag = pathNodes.Count == 0;
		while (pathFinderNode.AbsoluteX != pathFinderNode.ParentAbsoluteX || pathFinderNode.AbsoluteY != pathFinderNode.ParentAbsoluteY)
		{
			pathNodes.Insert(num2, pathFinderNode);
			x = pathFinderNode.ParentAbsoluteX;
			y = pathFinderNode.ParentAbsoluteY;
			pathFinderNode = openAndClosed[y * width + x];
			num2++;
			num++;
			_ = 30;
		}
		if (flag)
		{
			pathNodes.Add(pathFinderNode);
		}
	}

	private void SetNextDestinationPoint(PathFinderNode newStartingNode, Point newDestination)
	{
		CreatePathSegment(destination);
		destination = newDestination;
		mOpen.Clear();
		openAndClosed.Clear();
		SetStartingNode(newStartingNode);
		newStartingNode.Status = StatusOpen;
		newStartingNode.ParentAbsoluteX = newStartingNode.AbsoluteX;
		newStartingNode.ParentAbsoluteY = newStartingNode.AbsoluteY;
	}

	private void SetStartingNode(PathFinderNode startingNode)
	{
		mOpen.Push(startingNode);
		openAndClosed.Add(startingNode.AbsoluteY * width + startingNode.AbsoluteX, startingNode);
	}

	public List<PathFinderNode> FindPathDirectly(DijkstraTestNodeDelegate testNodePredicate)
	{
		bool flag = false;
		while (mOpen.Count > 0 && !mStop)
		{
			PathFinderNode pathFinderNode = mOpen.Pop();
			if (pathFinderNode.Status == StatusClosed)
			{
				continue;
			}
			absoluteX = pathFinderNode.AbsoluteX;
			absoluteY = pathFinderNode.AbsoluteY;
			if (testNodePredicate != null)
			{
				if (testNodePredicate(absoluteX, absoluteY))
				{
					pathFinderNode.Status = StatusClosed;
					destination.X = absoluteX;
					destination.Y = absoluteY;
					ClosestPointToDestination = destination;
					flag = true;
					break;
				}
			}
			else if (absoluteX == destination.X && absoluteY == destination.Y)
			{
				ClosestPointToDestination = destination;
				pathFinderNode.Status = StatusClosed;
				flag = true;
				break;
			}
			if (closeNodeCounter > mSearchLimit)
			{
				if (!returnPathToClosestPoint)
				{
					mStopped = true;
					return null;
				}
				int num = 10000000;
				Point value = new Point(-1, -1);
				foreach (KeyValuePair<int, PathFinderNode> item in openAndClosed)
				{
					if (item.Value.Status != StatusOpen && item.Value.Status != StatusClosed)
					{
						continue;
					}
					int num2 = item.Value.AbsoluteX;
					int num3 = item.Value.AbsoluteY;
					int num4 = Math.Abs(num2 - destination.X) + Math.Abs(num3 - destination.Y);
					if (num4 < num)
					{
						num = num4;
						value.X = num2;
						value.Y = num3;
						if (num == 1)
						{
							break;
						}
					}
				}
				ClosestPointToDestination = value;
				break;
			}
			ushort sectorX = pathFinderNode.SectorX;
			ushort sectorY = pathFinderNode.SectorY;
			SubtileSector sector = Layers.GetSector(sectorX, sectorY);
			relativeX = (ushort)(absoluteX - sector.SubtileArea.X);
			relativeY = (ushort)(absoluteY - sector.SubtileArea.Y);
			for (int i = 0; i < 8; i++)
			{
				if (ComputeSuccessor(pathFinderNode, sector, i, out var nextNode))
				{
					mH = GetHeuristic();
					nextNode.F = nextNode.G + mH;
					mOpen.Push(nextNode);
					nextNode.Status = StatusOpen;
				}
			}
			closeNodeCounter++;
			pathFinderNode.Status = StatusClosed;
		}
		if (flag)
		{
			pathNodes.Clear();
			int x = destination.X;
			int y = destination.Y;
			PathFinderNode pathFinderNode2 = openAndClosed[destination.Y * width + destination.X];
			while (pathFinderNode2.AbsoluteX != pathFinderNode2.ParentAbsoluteX || pathFinderNode2.AbsoluteY != pathFinderNode2.ParentAbsoluteY)
			{
				pathNodes.Add(pathFinderNode2);
				x = pathFinderNode2.ParentAbsoluteX;
				y = pathFinderNode2.ParentAbsoluteY;
				pathFinderNode2 = openAndClosed[y * width + x];
			}
			pathNodes.Add(pathFinderNode2);
			pathNodes.Reverse();
			mStopped = true;
			return pathNodes;
		}
		mStopped = true;
		return null;
	}

	private int GetHeuristic()
	{
		switch (mFormula)
		{
		default:
			mH = 2 * (Math.Abs(absoluteNextX - destination.X) + Math.Abs(absoluteNextY - destination.Y));
			break;
		case HeuristicFormula.MaxDXDY:
			mH = 2 * Math.Max(Math.Abs(absoluteNextX - destination.X), Math.Abs(absoluteNextY - destination.Y));
			break;
		case HeuristicFormula.DiagonalShortCut:
		{
			int num3 = Math.Min(Math.Abs(absoluteNextX - destination.X), Math.Abs(absoluteNextY - destination.Y));
			int num4 = Math.Abs(absoluteNextX - destination.X) + Math.Abs(absoluteNextY - destination.Y);
			mH = 4 * num3 + 2 * (num4 - 2 * num3);
			break;
		}
		case HeuristicFormula.Euclidean:
			mH = (int)(2.0 * Math.Sqrt(Math.Pow(absoluteNextY - destination.X, 2.0) + Math.Pow(absoluteNextY - destination.Y, 2.0)));
			break;
		case HeuristicFormula.EuclideanNoSQR:
			mH = (int)(2.0 * (Math.Pow(absoluteNextX - destination.X, 2.0) + Math.Pow(absoluteNextY - destination.Y, 2.0)));
			break;
		case HeuristicFormula.Custom1:
		{
			Point point = new Point(Math.Abs(destination.X - absoluteNextX), Math.Abs(destination.Y - absoluteNextY));
			int num = Math.Abs(point.X - point.Y);
			int num2 = Math.Abs((point.X + point.Y - num) / 2);
			mH = 2 * (num2 + num + point.X + point.Y);
			break;
		}
		case HeuristicFormula.None:
			mH = 0;
			break;
		}
		return mH;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		destination = sn.DoPoint(destination);
		start = sn.DoPoint(start);
		finalDestination = sn.DoPoint(finalDestination);
		SearchLimit = sn.DoInt32(SearchLimit);
		snapshotLayersID = sn.SnapshotID<SubtileLayers, SubtileLayersID>(Layers).Value;
		returnPathToClosestPoint = sn.DoBool(returnPathToClosestPoint);
		mFormula = sn.DoEnum(mFormula);
		if (sn.mode != Snapshotter.Mode.Load && highLevelPath != null)
		{
			if (highLevelPath.ID != RegionPathID.Invalid)
			{
				snapshotHighLevelPath = highLevelPath.ID;
			}
			else
			{
				snapshotHighLevelPath = null;
			}
		}
		snapshotHighLevelPath = sn.DoEnumNullable(snapshotHighLevelPath);
		sn.Ignore(highLevelPath);
		sn.Ignore(highLevelPathIndex);
		sn.Ignore(Layers);
		sn.Ignore(ClosestPointToDestination);
		sn.Ignore(mStop);
		sn.Ignore(mStopped);
		sn.Ignore(StatusOpen);
		sn.Ignore(StatusClosed);
		sn.Ignore(width);
		sn.Ignore(height);
		sn.Ignore(mCompletedTime);
		sn.Ignore(openAndClosed);
		sn.Ignore(mOpen);
		sn.Ignore(pathNodes);
		sn.Ignore(mH);
		sn.Ignore(mDirection);
		sn.Ignore(absoluteNextX);
		sn.Ignore(absoluteNextY);
		sn.Ignore(absoluteX);
		sn.Ignore(absoluteY);
		sn.Ignore(relativeX);
		sn.Ignore(relativeY);
		sn.Ignore(closeNodeCounter);
		sn.Ignore(searchLimitForHighLevelPath);
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
		Layers = LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotLayersID);
		highLevelPath = LookUp<RegionPath, RegionPathID>.FindByID(snapshotHighLevelPath);
		Initialize();
	}
}
