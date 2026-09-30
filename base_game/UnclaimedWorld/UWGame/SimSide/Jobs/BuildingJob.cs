using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class BuildingJob : ISnapshot
{
	public ProcessJob ProcessJob;

	private JobID snapshotJob;

	private Dictionary<EntityID, Point> workersSubtilePositions = new Dictionary<EntityID, Point>();

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public BuildingJob()
	{
	}

	public BuildingJob(ProcessJob processJob)
	{
		ProcessJob = processJob;
	}

	public bool GetWorkLocation(Entity entity, out Vector3 workLocation, out IKnownEntityData structureToConstruct)
	{
		workLocation = -Vector3.One;
		if (!ProcessJob.GetStructureOutputData(out structureToConstruct))
		{
			return false;
		}
		workLocation = structureToConstruct.AccessPoint.Value;
		workLocation += new Vector3(The.Sim.GameplayRandomGenerator.Next(-12, 12, "BuildingJob"), The.Sim.GameplayRandomGenerator.Next(-12, 12, "BuildingJob"), 0f);
		return true;
	}

	public void DestroyUnstartedStructure()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProcessJob.ProductionProcess);
		if (simProcess == null)
		{
			return;
		}
		foreach (EntityID outputEntity in simProcess.OutputEntities)
		{
			Entity entity = Entity.FindByID(outputEntity);
			if (entity != null && entity.Structure != null && !entity.Structure.ConstructionHasStarted())
			{
				entity.Destroy();
				if (The.InGameUI.SelectedEntity == outputEntity)
				{
					The.InGameUI.SelectEntity(null);
				}
			}
		}
	}

	public void Abandon(Entity entity)
	{
		if (workersSubtilePositions.ContainsKey(entity.EntityID))
		{
			workersSubtilePositions.Remove(entity.EntityID);
		}
	}

	public override string ToString()
	{
		return new StringBuilder("Build ").ToString();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotJob = sn.SnapshotID<Job, JobID>(ProcessJob).Value;
		workersSubtilePositions = sn.DoDictionary(workersSubtilePositions);
		sn.Ignore(ProcessJob);
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
		ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
	}
}
