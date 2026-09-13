using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class SalvageJob : ISnapshot
{
	public ProcessJob ProcessJob;

	private JobID snapshotJob;

	private Dictionary<EntityID, Point> workersSubtilePositions = new Dictionary<EntityID, Point>();

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public SalvageJob()
	{
	}

	public SalvageJob(ProcessJob processJob)
	{
		ProcessJob = processJob;
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
		return new StringBuilder("Salvage ").ToString();
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
