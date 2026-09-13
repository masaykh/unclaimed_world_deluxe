using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Overland;

public class PlaySite : ISnapshot
{
	public WeatherManager Weather;

	private CyclableID weatherID;

	public SleepyUpdater<SimProcess> Processes;

	private List<SimProcessID> snapshotProcesses;

	public SleepyUpdater<TileResourceContainer> TileResources;

	private List<ResourceID> snapshotResources;

	public CollisionManager<Expedition> ExpeditionRadiusQuadTree;

	private List<ExpeditionID> snapshotExpeditionRadiusQuadTree;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public PlaySite()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			if (The.Sim.Mode == Sim.EngineMode.Game)
			{
				int maxCollidablesPerNode = 999;
				ExpeditionRadiusQuadTree = new CollisionManager<Expedition>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxCollidablesPerNode);
			}
			Weather = new WeatherManager();
			weatherID = Weather.ID;
			InitProcesses();
			InitTileResources();
		}
	}

	private void InitProcesses()
	{
		Processes = new SleepyUpdater<SimProcess>(Module.Sim, staggerUpdates: true);
	}

	public void AddProcess(SimProcess process)
	{
		Processes.Add(process);
	}

	public void RemoveProcess(SimProcess process)
	{
		Processes.Remove(process);
	}

	private void InitTileResources()
	{
		TileResources = new SleepyUpdater<TileResourceContainer>(Module.Sim, staggerUpdates: true);
	}

	public void AddTileResourceContainer(TileResourceContainer resource)
	{
		TileResources.Add(resource);
	}

	public void RemoveTileResourceContainer(TileResourceContainer resource)
	{
		TileResources.Remove(resource);
	}

	public void Update(GameTime gameTime)
	{
		Weather.Update(gameTime);
		Processes.Update(gameTime);
		TileResources.Update(gameTime);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (Processes != null)
			{
				snapshotProcesses = new List<SimProcessID>();
				Processes.IterateItems(delegate(SimProcess e)
				{
					snapshotProcesses.Add(e.ID);
				});
			}
			if (TileResources != null)
			{
				snapshotResources = new List<ResourceID>();
				TileResources.IterateItems(delegate(TileResourceContainer e)
				{
					snapshotResources.Add(e.ID);
				});
			}
			snapshotExpeditionRadiusQuadTree = (from e in ExpeditionRadiusQuadTree.GetAllObjects()
				select e.ID).ToList();
		}
		ExpeditionRadiusQuadTree = (CollisionManager<Expedition>)sn.DoISnapshot(ExpeditionRadiusQuadTree);
		snapshotExpeditionRadiusQuadTree = sn.DoList(snapshotExpeditionRadiusQuadTree);
		snapshotProcesses = sn.DoList(snapshotProcesses);
		snapshotResources = sn.DoList(snapshotResources);
		weatherID = sn.DoEnum(weatherID);
		sn.Ignore(Weather);
		sn.Ignore(Processes);
		sn.Ignore(TileResources);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		InitProcesses();
		if (snapshotProcesses != null)
		{
			foreach (SimProcessID snapshotProcess in snapshotProcesses)
			{
				Processes.Add(LookUp<SimProcess, SimProcessID>.FindByID(snapshotProcess), null, keepExistingTimepoint: true);
			}
			snapshotProcesses.Clear();
		}
		InitTileResources();
		if (snapshotResources != null)
		{
			foreach (ResourceID snapshotResource in snapshotResources)
			{
				TileResources.Add((TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(snapshotResource), null, keepExistingTimepoint: true);
			}
			snapshotResources.Clear();
		}
		List<Collidable<Expedition>> preLoadPostProcess = snapshotExpeditionRadiusQuadTree.Select((ExpeditionID t) => Expedition.FindByID(t).Collidable).ToList();
		ExpeditionRadiusQuadTree.SetPreLoadPostProcess(preLoadPostProcess);
		ExpeditionRadiusQuadTree.LoadPostProcess(sn);
		Weather = (WeatherManager)LookUp<ICyclable, CyclableID>.FindByID(weatherID);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
