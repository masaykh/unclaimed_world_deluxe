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

	private void DrawInfluenceMapWorkWorkerPosition(byte[][] subTileMap)
	{
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subTileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subTileMap);
		InfluenceMap.DrawLinearInfluenceCircle(subTileMap, new Point(jaggedArrayWidth / 2, jaggedArrayHeight / 2), 40, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, jaggedArrayWidth / 2);
		InfluenceMap.DrawGradientRectangle(subTileMap, new Point(0, 0), new Point(jaggedArrayWidth - 1, jaggedArrayHeight - 1), addToExistingValues: true, 0, 20, 5, InfluenceMap.GradientDirection.TopToBottom);
		if (workersSubtilePositions.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<EntityID, Point> workersSubtilePosition in workersSubtilePositions)
		{
			InfluenceMap.DrawLinearInfluenceCircle(subTileMap, workersSubtilePosition.Value, -20, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, 2);
		}
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

	private static float GetTotalBulk(List<Entity> items)
	{
		float num = 0f;
		foreach (Entity item in items)
		{
			num += item.Bulk;
		}
		return num;
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
