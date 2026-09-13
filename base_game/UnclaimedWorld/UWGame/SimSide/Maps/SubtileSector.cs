using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class SubtileSector : ISnapshot
{
	public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

	public double? CreatedOn;

	public Rectangle SubtileArea;

	public MapManager.SubtileValue[][] Values;

	private bool blockedStatusHasChanged = true;

	public const int RegionRadius = 8;

	public const int RegionInterval = 16;

	private ushort[][] regionsOnSubtiles;

	private byte[][] allNodeDistances;

	private List<SubtilePos> listOfUnassignedPoints = new List<SubtilePos>(200);

	private HashSet<ushort> regions = new HashSet<ushort>();

	private ushort[][] newRegionsOnSubtiles;

	private HashSet<ushort> newRegions = new HashSet<ushort>();

	private Point currentCenterRelative = new Point(-8, 8);

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Point Coords { get; private set; }

	public bool BlockedStatusHasChanged
	{
		get
		{
			return blockedStatusHasChanged;
		}
		set
		{
			if (blockedStatusHasChanged != value)
			{
				blockedStatusHasChanged = value;
				AddLog("Blocked status set to: " + value);
			}
		}
	}

	public bool HasFinishedFirstRun { get; private set; }

	public bool IsSnapshotted { get; set; }

	public SubtileSector()
	{
	}

	public SubtileSector(SubtileLayer parent, int sectorCoordsX, int sectorCoordsY, bool setHasFinished = false)
	{
		int sectorSize = parent.SectorSize;
		HasFinishedFirstRun = setHasFinished;
		Coords = new Point(sectorCoordsX, sectorCoordsY);
		SubtileArea = default(Rectangle);
		SubtileArea.X = sectorCoordsX * sectorSize;
		SubtileArea.Y = sectorCoordsY * sectorSize;
		SubtileArea.Width = Common.ClampTop(sectorSize, sectorSize - (SubtileArea.X + sectorSize - parent.Width));
		SubtileArea.Height = Common.ClampTop(sectorSize, sectorSize - (SubtileArea.Y + sectorSize - parent.Height));
		Common.InitJaggedArray(ref Values, SubtileArea.Width, SubtileArea.Height);
		Common.InitJaggedArray(ref regionsOnSubtiles, SubtileArea.Width, SubtileArea.Height);
		Init();
		CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
		AddLog("Created");
	}

	public void AddLog(string text)
	{
	}

	public HashSet<ushort> GetRegions()
	{
		return regions;
	}

	public HashSet<ushort> GetRegionsInProgress()
	{
		return newRegions;
	}

	public void InitCopyVariablesSubtiles()
	{
		Common.CopyJaggedArray(regionsOnSubtiles, newRegionsOnSubtiles);
		newRegions.Clear();
		foreach (ushort region in regions)
		{
			newRegions.Add(region);
		}
	}

	public void InitCopyVariablesClearDistances()
	{
		Common.ClearJaggedArray(allNodeDistances);
	}

	public void FinishRegions()
	{
		Common.CopyJaggedArray(newRegionsOnSubtiles, regionsOnSubtiles);
		regions.Clear();
		foreach (ushort newRegion in newRegions)
		{
			regions.Add(newRegion);
		}
		if (!HasFinishedFirstRun)
		{
			HasFinishedFirstRun = true;
		}
		AddLog("FinishRegions");
	}

	public ushort GetRegion(Point subtile)
	{
		return GetRegion(subtile.X, subtile.Y);
	}

	public ushort GetRegion(int relativeX, int relativeY)
	{
		return regionsOnSubtiles[relativeX][relativeY];
	}

	public ushort GetRegionInProgress(Point subtile)
	{
		return GetRegionInProgress(subtile.X, subtile.Y);
	}

	public ushort GetRegionInProgress(int relativeX, int relativeY)
	{
		return newRegionsOnSubtiles[relativeX][relativeY];
	}

	public void ClearRegions(List<ushort> clearedRegions)
	{
		Common.ClearJaggedArray(newRegionsOnSubtiles);
		clearedRegions.AddRange(newRegions);
		newRegions.Clear();
		currentCenterRelative = new Point(-8, 8);
	}

	public bool FloodFill(RegionMap parent)
	{
		if (!ScanRectangleWithIntervals())
		{
			if (Coords.X == 0)
			{
				_ = Coords.Y;
				_ = 1;
			}
			if (!GetUnassignedPoint())
			{
				return true;
			}
		}
		CreateRegionUsingFloodfill(parent);
		return false;
	}

	public void BuildRegionGraph(RegionMap regionMap)
	{
		int num = 0;
		int num2 = SubtileArea.Width - 1;
		int num3 = SubtileArea.Height - 1;
		for (int i = 0; i < num2; i++)
		{
			for (int j = num; j < num3; j++)
			{
				ExamineQuadForEdges(regionMap, i, j, i + 1, j + 1);
			}
		}
	}

	private void ExamineQuadForEdges(RegionMap regionMap, int fromX, int fromY, int toX, int toY)
	{
		ushort[] obj = newRegionsOnSubtiles[fromX];
		ushort fromRegionColor = obj[fromY];
		ushort[] obj2 = newRegionsOnSubtiles[toX];
		ushort toRegionColor = obj2[fromY];
		regionMap.AddEdgesIfNotExists(fromRegionColor, toRegionColor);
		ushort toRegionColor2 = obj2[toY];
		regionMap.AddEdgesIfNotExists(fromRegionColor, toRegionColor2);
		ushort num = obj[toY];
		regionMap.AddEdgesIfNotExists(fromRegionColor, num);
		regionMap.AddEdgesIfNotExists(num, toRegionColor);
	}

	private void CreateRegionUsingFloodfill(RegionMap parent)
	{
		Region region = parent.CreateRegion(new Point(SubtileArea.X + currentCenterRelative.X, SubtileArea.Y + currentCenterRelative.Y));
		parent.FloodFill.DoFloodFill(Values, newRegionsOnSubtiles, allNodeDistances, currentCenterRelative, region.Color, 9);
		newRegions.Add(region.Color);
	}

	private bool ScanRectangleWithIntervals()
	{
		Rectangle subtileArea = SubtileArea;
		if (currentCenterRelative.X < 0 || currentCenterRelative.X >= subtileArea.Width || currentCenterRelative.Y < 0 || currentCenterRelative.Y >= subtileArea.Height)
		{
			currentCenterRelative.X = GetStartXPosForScan();
			currentCenterRelative.X -= 16;
			if (subtileArea.Height > 8)
			{
				currentCenterRelative.Y = 8;
			}
			else
			{
				currentCenterRelative.Y = subtileArea.Height / 2;
			}
		}
		do
		{
			currentCenterRelative.X += 16;
			if (currentCenterRelative.X >= subtileArea.Width)
			{
				currentCenterRelative.X = GetStartXPosForScan();
				currentCenterRelative.Y += 16;
				if (currentCenterRelative.Y >= subtileArea.Height)
				{
					return false;
				}
			}
		}
		while (newRegionsOnSubtiles[currentCenterRelative.X][currentCenterRelative.Y] > 0 || MapManager.IsBlocked(Values[currentCenterRelative.X][currentCenterRelative.Y]));
		return true;
	}

	private int GetStartXPosForScan()
	{
		if (SubtileArea.Width > 8)
		{
			return 8;
		}
		return SubtileArea.Width / 2;
	}

	private bool GetUnassignedPoint()
	{
		if (listOfUnassignedPoints.Count == 0)
		{
			ScanAreaForUnassignedSubtiles();
			if (listOfUnassignedPoints.Count > 0)
			{
				currentCenterRelative = listOfUnassignedPoints.ElementAt(0).ToPoint();
				return true;
			}
			return false;
		}
		return ExamineUnassignedPoints();
	}

	private bool ExamineUnassignedPoints()
	{
		int count = listOfUnassignedPoints.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			SubtilePos subtilePos = listOfUnassignedPoints.ElementAt(num);
			if (newRegionsOnSubtiles[subtilePos.X][subtilePos.Y] == 0)
			{
				currentCenterRelative = subtilePos.ToPoint();
				listOfUnassignedPoints.RemoveRange(num, count - num);
				return true;
			}
		}
		listOfUnassignedPoints.Clear();
		return false;
	}

	private void ScanAreaForUnassignedSubtiles()
	{
		for (int i = 0; i < SubtileArea.Width; i++)
		{
			ushort[] array = newRegionsOnSubtiles[i];
			MapManager.SubtileValue[] array2 = Values[i];
			for (int j = 0; j < SubtileArea.Height; j++)
			{
				if (array[j] == 0 && !MapManager.IsBlocked(array2[j]))
				{
					listOfUnassignedPoints.Add(new SubtilePos((ushort)i, (ushort)j));
				}
			}
		}
	}

	public void CreateRegionUsingFloodfill(FloodFill floodFill, byte[][] allNodeDistances, Point from, ushort regionColor, int radius)
	{
	}

	private void Init()
	{
		Common.InitJaggedArray(ref newRegionsOnSubtiles, SubtileArea.Width, SubtileArea.Height);
		Common.InitJaggedArray(ref allNodeDistances, SubtileArea.Width, SubtileArea.Height);
	}

	public MapManager.SubtileValue? GetValue(int relativeX, int relativeY)
	{
		if (relativeX < SubtileArea.Width && relativeY < SubtileArea.Height)
		{
			return Values[relativeX][relativeY];
		}
		return null;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		blockedStatusHasChanged = sn.DoBool(blockedStatusHasChanged);
		Values = sn.DoJaggedArray(Values);
		Coords = sn.DoPoint(Coords);
		SubtileArea = sn.DoRectangle(SubtileArea);
		regionsOnSubtiles = sn.DoJaggedArray(regionsOnSubtiles);
		regions = sn.DoHashSet(regions);
		HasFinishedFirstRun = sn.DoBool(HasFinishedFirstRun);
		sn.Ignore(listOfUnassignedPoints);
		sn.Ignore(newRegionsOnSubtiles);
		sn.Ignore(newRegions);
		sn.Ignore(allNodeDistances);
		sn.Ignore(currentCenterRelative);
		sn.Ignore(CreatedOn);
		sn.Ignore(Log);
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
		Init();
		Common.CopyJaggedArray(regionsOnSubtiles, newRegionsOnSubtiles);
		newRegions = new HashSet<ushort>(regions);
	}
}
