using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps.Regions;

public class TerrainRegionMap : RegionMap
{
	private const float refreshIntervalInSeconds = 4f;

	public IDActionEvent RegionsAreInvalid = new IDActionEvent();

	public IDActionEvent RegionsFinished = new IDActionEvent();

	private Regulator regulator;

	public override Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph => RegionGraph;

	public override Dictionary<ushort, Region> BottomRegions => regions;

	public override Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges => null;

	protected override ushort ColorStartOfRange => 1;

	protected override bool CanServiceRequests
	{
		get
		{
			if (RegionGraph != null)
			{
				return RegionGraph.Count > 0;
			}
			return false;
		}
	}

	public bool IsComputing => isComputing;

	public TerrainRegionMap()
	{
	}

	public TerrainRegionMap(SubtileLayers layers)
		: base(layers, layers.Layers[0])
	{
		sectorsAreDirty = true;
	}

	protected override void RecomputeStarted()
	{
		base.RecomputeStarted();
		if (sectorsAreDirty)
		{
			RegionsAreInvalid.Invoke();
		}
	}

	protected override void RecomputeFinished()
	{
		base.RecomputeFinished();
		RegionsFinished.Invoke();
	}

	public override Region GetRegion(ushort color)
	{
		if (color > 0)
		{
			return regions[color];
		}
		return null;
	}

	public override Region GetRegionInProgress(ushort color)
	{
		if (color > 0)
		{
			return newRegions[color];
		}
		return null;
	}

	protected override bool BuildRegionGraphBetweenSectors()
	{
		bool isLastSector;
		SubtileSector currentDirtySectorAndIncrement = GetCurrentDirtySectorAndIncrement(out isLastSector);
		ScanSectorEdges(currentDirtySectorAndIncrement, null);
		return isLastSector;
	}

	protected override void AddEdgesOrConnectors(ushort fromRegion, ushort toRegion)
	{
		AddEdgesIfNotExists(fromRegion, toRegion);
	}

	protected override void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.25, "RegionMap");
	}

	public void Update(GameTime gameTime)
	{
		if (The.Sim.CycleManager.IsRegistered(this) || !regulator.IsReady())
		{
			return;
		}
		if (regions.Count == 0)
		{
			progress = Progress.InitFromScratch;
		}
		else
		{
			if (!sectorsAreDirty)
			{
				return;
			}
			SetProgressAtRepairStart();
		}
		The.Sim.CycleManager.Register(this, CycleManager.Priority.High);
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		RegionsAreInvalid = (IDActionEvent)sn.DoISnapshot(RegionsAreInvalid);
		RegionsFinished = (IDActionEvent)sn.DoISnapshot(RegionsFinished);
		sn.Ignore(regulator);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		RegionsAreInvalid.LoadPostProcess(sn);
		RegionsFinished.LoadPostProcess(sn);
	}
}
