using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps;

[DebuggerDisplay("{MapArea}")]
public class Zone : ILookUp<Zone, ZoneID>, ISnapshot
{
	public string Name;

	public MapArea MapArea;

	public HashSet<TerrainTile> EdgeTiles = new HashSet<TerrainTile>();

	private List<TerrainTileID> snapshotEdgeTiles;

	private ScoutingJob forageJob;

	private JobID? snapshotForageJob;

	private ScoutingJob scoutingJob;

	private JobID? snapshotScoutingJob;

	private PatrolJob patrolJob;

	private JobID? snapshotPatrolJob;

	private AttackAreaJob attackAreaJob;

	private JobID? snapshotAttackAreaJob;

	public Dictionary<ResourceType, List<ProcessJob>> HarvestJobs = new Dictionary<ResourceType, List<ProcessJob>>();

	private Dictionary<ResourceType, List<JobID>> snapshotHarvestJobs;

	public HashSet<ResourceType> AllowStandingOrderHarvest = new HashSet<ResourceType>();

	public ZoneHunt ZoneHunt;

	private Stockpile stockpile;

	private EntityGroupID? ownerID;

	private ZoneID id = ZoneID.Invalid;

	private static ZoneID IDCounter = ZoneID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public PatrolJob PatrolJob
	{
		get
		{
			return patrolJob;
		}
		set
		{
			if (patrolJob != value)
			{
				patrolJob = value;
				RemoveZoneOrFireOrdersChangedEvent();
			}
		}
	}

	public ScoutingJob ScoutingJob
	{
		get
		{
			return scoutingJob;
		}
		set
		{
			if (scoutingJob != value)
			{
				scoutingJob = value;
				RemoveZoneOrFireOrdersChangedEvent();
			}
		}
	}

	public ScoutingJob ExamineJob
	{
		get
		{
			return forageJob;
		}
		set
		{
			if (forageJob != value)
			{
				forageJob = value;
				RemoveZoneOrFireOrdersChangedEvent();
			}
		}
	}

	public AttackAreaJob AttackAreaJob
	{
		get
		{
			return attackAreaJob;
		}
		set
		{
			if (attackAreaJob != value)
			{
				attackAreaJob = value;
				RemoveZoneOrFireOrdersChangedEvent();
			}
		}
	}

	public Stockpile Stockpile
	{
		get
		{
			return stockpile;
		}
		set
		{
			stockpile = value;
			RemoveZoneOrFireOrdersChangedEvent();
		}
	}

	public EntityGroupID? Owner => ownerID;

	public ZoneID ID
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

	public bool IsSnapshotted { get; set; }

	public event EventHandler ZoneOrdersChanged;

	public Zone(EntityGroup zoneOwner, MapArea mapArea)
	{
		AddToLookup();
		if (zoneOwner != null)
		{
			ownerID = zoneOwner.ID;
			zoneOwner.Zones.Add(this);
		}
		MapArea = new MapArea(mapArea, this);
		ZoneHunt = new ZoneHunt();
		Allegiance allegiance = zoneOwner.GetAllegiance();
		MapArea.IterateArea(delegate(TerrainTile tile)
		{
			tile.AddZone(allegiance, this);
		});
		ComputeEdges();
	}

	public Zone()
	{
	}

	public ZoneID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ZoneID.Invalid)
		{
			throw new Exception("Astounding, ZoneID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ZoneID SnapshotID(Snapshotter sn, ZoneID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ZoneID.Invalid)
		{
			LookUp<Zone, ZoneID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ZoneID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Zone, ZoneID>.Remove(this);
	}

	void ILookUp<Zone, ZoneID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ZoneID.First;
	}

	void ILookUp<Zone, ZoneID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Zone, ZoneID>.Create();
	}

	public string GetDisplayName()
	{
		return Name ?? ("#" + ID);
	}

	public void ComputeEdges()
	{
		EdgeTiles.Clear();
		if (!MapArea.BoundingRectangle.HasValue)
		{
			return;
		}
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID);
		if (entityGroup == null)
		{
			return;
		}
		Allegiance allegiance = entityGroup.GetAllegiance();
		for (int i = MapArea.BoundingRectangle.Value.Top; i < MapArea.BoundingRectangle.Value.Bottom; i++)
		{
			bool isInZone = false;
			TerrainTile previousTile = null;
			for (int j = MapArea.BoundingRectangle.Value.Left; j < MapArea.BoundingRectangle.Value.Right; j++)
			{
				bool isLastTile = j == MapArea.BoundingRectangle.Value.Right - 1;
				TerrainTile terrainTile = The.Map.TileMap[j][i];
				HandleTile(ref isInZone, allegiance, terrainTile, previousTile, isLastTile);
				previousTile = terrainTile;
			}
		}
		for (int k = MapArea.BoundingRectangle.Value.Left; k < MapArea.BoundingRectangle.Value.Right; k++)
		{
			bool isInZone = false;
			TerrainTile previousTile = null;
			for (int l = MapArea.BoundingRectangle.Value.Top; l < MapArea.BoundingRectangle.Value.Bottom; l++)
			{
				bool isLastTile = l == MapArea.BoundingRectangle.Value.Bottom - 1;
				TerrainTile terrainTile = The.Map.TileMap[k][l];
				HandleTile(ref isInZone, allegiance, terrainTile, previousTile, isLastTile);
				previousTile = terrainTile;
			}
		}
	}

	private void HandleTile(ref bool isInZone, Allegiance allegiance, TerrainTile currentTile, TerrainTile previousTile, bool isLastTile)
	{
		if (!isInZone)
		{
			if (currentTile.IsInZone(allegiance, this))
			{
				isInZone = true;
				EdgeTiles.Add(currentTile);
			}
		}
		else if (!isLastTile)
		{
			if (!currentTile.IsInZone(allegiance, this))
			{
				isInZone = false;
				EdgeTiles.Add(previousTile);
			}
		}
		else if (currentTile.IsInZone(allegiance, this))
		{
			EdgeTiles.Add(currentTile);
		}
	}

	public void RemoveZoneOrFireOrdersChangedEvent()
	{
		if (!HasOrders())
		{
			Destroy();
		}
		else if (this.ZoneOrdersChanged != null)
		{
			this.ZoneOrdersChanged(this, null);
		}
	}

	public RegionMap.Result GetClosestSafeEdgeTile(Job job, Vector3 aSourceLocation, RegionMap regionMap, ThreatStance threatStance, Entity entity, out float? distanceToClosestTile, out TerrainTile closestTile, EntityGroup owner = null)
	{
		float? distanceResult = null;
		closestTile = null;
		distanceToClosestTile = null;
		RegionMap.Result result = RegionMap.Result.NoAccess;
		foreach (TerrainTile edgeTile in EdgeTiles)
		{
			RegionMap.Result result2 = CalculateDistanceToTile(aSourceLocation, edgeTile, regionMap, entity, ref distanceResult);
			switch (result2)
			{
			case RegionMap.Result.Wait:
				return RegionMap.Result.Wait;
			case RegionMap.Result.NoAccess:
				continue;
			}
			if (GoalEvaluator.WorkSiteIsSafe(entity, MapManager.TileToWorldPos(edgeTile), threatStance))
			{
				if (!distanceToClosestTile.HasValue)
				{
					distanceToClosestTile = distanceResult.Value;
					closestTile = edgeTile;
					result = result2;
				}
				else if (distanceResult.Value < distanceToClosestTile.Value)
				{
					distanceToClosestTile = distanceResult.Value;
					closestTile = edgeTile;
					result = result2;
				}
			}
		}
		if (owner != null)
		{
			if (result == RegionMap.Result.OK)
			{
				The.Client.SetJobInaccessible(job, owner.Parent, isInaccessible: false);
			}
			if (result == RegionMap.Result.NoAccess)
			{
				The.Client.SetJobInaccessible(job, owner.Parent, isInaccessible: true);
				bool flag = false;
				float distance = 0f;
				foreach (TerrainTile edgeTile2 in EdgeTiles)
				{
					Point fromSubtile = MapManager.WorldPosToSubtile(aSourceLocation);
					Point toSubtile = MapManager.TileToCenterSubtile(new Point(edgeTile2.X, edgeTile2.Y));
					if (The.Map.FootTerrainRegionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity: false) == RegionMap.Result.OK)
					{
						The.Client.SetJobBlockedByThreat(job, owner.Parent, isBlocked: true);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					The.Client.SetJobBlockedByThreat(job, owner.Parent, isBlocked: false);
				}
			}
		}
		return result;
	}

	private static RegionMap.Result CalculateDistanceToTile(Vector3 sourceLocation, TerrainTile destinationTile, RegionMap regionMap, Entity entity, ref float? distanceResult)
	{
		float distance = 0f;
		Point fromSubtile = MapManager.WorldPosToSubtile(sourceLocation);
		Point toSubtile = MapManager.TileToCenterSubtile(new Point(destinationTile.X, destinationTile.Y));
		if (regionMap.ID == (CyclableID)170uL && entity != null && entity.ID == (EntityID)4582L && fromSubtile.X == 114 && fromSubtile.Y == 44)
		{
			_ = The.Sim.TotalUnPausedGameTimeInSeconds;
			_ = 35.0;
		}
		RegionMap.Result distance2 = regionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance);
		if (distance2 != RegionMap.Result.OK)
		{
			return distance2;
		}
		distanceResult = distance;
		return RegionMap.Result.OK;
	}

	public bool HasOrders()
	{
		if (PatrolJob == null && ScoutingJob == null && ExamineJob == null && AttackAreaJob == null && Stockpile == null && !HasHarvestJobs() && !ZoneHunt.HasFindPreyJobs() && !ZoneHunt.HasHuntOrders() && AllowStandingOrderHarvest.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool HasHarvestJobs()
	{
		if (HarvestJobs != null)
		{
			foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in HarvestJobs)
			{
				if (harvestJob.Value.Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetTotalBulkOfItems()
	{
		float total = 0f;
		MapArea.IterateArea(delegate(TerrainTile tile)
		{
			total += tile.GetTotalBulkOfItems();
		});
		return total;
	}

	public float GetBulkCapacity()
	{
		return (float)MapArea.Count * GameData.Instance.Constants.BulkCapacityForSingleTile;
	}

	public void Destroy()
	{
		LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID)?.Zones.Remove(this);
		if (HarvestJobs != null)
		{
			foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in HarvestJobs)
			{
				for (int num = harvestJob.Value.Count - 1; num >= 0; num--)
				{
					harvestJob.Value[num].Destroy(removeTakers: true);
				}
			}
		}
		if (scoutingJob != null)
		{
			scoutingJob.Destroy(removeTakers: true);
		}
		if (forageJob != null)
		{
			forageJob.Destroy(removeTakers: true);
		}
		if (patrolJob != null)
		{
			patrolJob.Destroy(removeTakers: true);
		}
		if (attackAreaJob != null)
		{
			attackAreaJob.Destroy(removeTakers: true);
		}
		ZoneHunt.Destroy();
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerID);
		Allegiance allegiance = null;
		if (entityGroup != null)
		{
			allegiance = entityGroup.GetAllegiance();
		}
		MapArea.IterateArea(delegate(TerrainTile tile)
		{
			if (allegiance != null)
			{
				tile.RemoveZone(allegiance, this);
			}
			else
			{
				tile.RemoveZone(this);
			}
		});
		if (The.InGameUI.SelectedZone == this)
		{
			The.InGameUI.SelectedZone = null;
		}
	}

	public void AddHarvestJob(ProcessJob job)
	{
		job.HarvestJob.Zone = this;
		if (!HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out var value))
		{
			value = new List<ProcessJob>();
			HarvestJobs.Add(job.HarvestJob.ResourceType, value);
		}
		int count = value.Count;
		value.Add(job);
		if (count == 0)
		{
			RemoveZoneOrFireOrdersChangedEvent();
		}
	}

	public void RemoveHarvestJob(ProcessJob job)
	{
		if (HarvestJobs != null && HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out var value))
		{
			value.Remove(job);
			if (value.Count == 0)
			{
				RemoveZoneOrFireOrdersChangedEvent();
			}
		}
	}

	public void RemoveFindPreyJob(FindPreyJob job)
	{
		if (ZoneHunt.FindPreyJobs.Remove(job))
		{
			RemoveZoneOrFireOrdersChangedEvent();
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		Name = sn.DoString(Name);
		MapArea = (MapArea)sn.DoISnapshot(MapArea);
		ownerID = sn.DoEnumNullable(ownerID);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotEdgeTiles = EdgeTiles.Select((TerrainTile k) => k.ID).ToList();
			snapshotHarvestJobs = new Dictionary<ResourceType, List<JobID>>();
			foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in HarvestJobs)
			{
				snapshotHarvestJobs.Add(harvestJob.Key, harvestJob.Value.Select((ProcessJob j) => j.ID).ToList());
			}
		}
		snapshotEdgeTiles = sn.DoList(snapshotEdgeTiles);
		snapshotPatrolJob = sn.SnapshotID<Job, JobID>(patrolJob);
		snapshotAttackAreaJob = sn.SnapshotID<Job, JobID>(attackAreaJob);
		snapshotScoutingJob = sn.SnapshotID<Job, JobID>(scoutingJob);
		snapshotForageJob = sn.SnapshotID<Job, JobID>(forageJob);
		snapshotHarvestJobs = sn.DoMultiMap(snapshotHarvestJobs);
		stockpile = (Stockpile)sn.DoISnapshot(stockpile);
		AllowStandingOrderHarvest = sn.DoHashSet(AllowStandingOrderHarvest);
		ZoneHunt = (ZoneHunt)sn.DoISnapshot(ZoneHunt);
		sn.Ignore(HarvestJobs);
		sn.Ignore(EdgeTiles);
		sn.Ignore(scoutingJob);
		sn.Ignore(forageJob);
		sn.Ignore(patrolJob);
		sn.Ignore(attackAreaJob);
		sn.Ignore(this.ZoneOrdersChanged);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (TerrainTileID snapshotEdgeTile in snapshotEdgeTiles)
		{
			EdgeTiles.Add(LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotEdgeTile));
		}
		foreach (KeyValuePair<ResourceType, List<JobID>> snapshotHarvestJob in snapshotHarvestJobs)
		{
			HarvestJobs.Add(snapshotHarvestJob.Key, snapshotHarvestJob.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		if (snapshotScoutingJob.HasValue)
		{
			scoutingJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotScoutingJob.Value);
		}
		if (snapshotPatrolJob.HasValue)
		{
			patrolJob = (PatrolJob)LookUp<Job, JobID>.FindByID(snapshotPatrolJob.Value);
		}
		if (snapshotAttackAreaJob.HasValue)
		{
			attackAreaJob = (AttackAreaJob)LookUp<Job, JobID>.FindByID(snapshotAttackAreaJob.Value);
		}
		if (snapshotForageJob.HasValue)
		{
			forageJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotForageJob.Value);
		}
		if (ZoneHunt != null)
		{
			ZoneHunt.LoadPostProcess(sn);
		}
		MapArea.LoadPostProcess(sn);
	}
}
