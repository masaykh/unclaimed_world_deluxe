using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.TimeSlicing;

public class CycleManager : ISnapshot
{
	public enum Priority
	{
		Medium,
		High
	}

	private List<ICyclable> highPriorityCycleUpdateRequests = new List<ICyclable>();

	private List<CyclableID> highPriorityCycleUpdateRequestIDs = new List<CyclableID>();

	private List<ICyclable> mediumPriorityCycleUpdateRequests = new List<ICyclable>();

	private List<CyclableID> mediumPriorityCycleUpdateRequestIDs = new List<CyclableID>();

	private HighResolutionTime timer;

	private double timeTaken;

	private CyclableID? nextHighPriorityCyclable;

	private CyclableID? nextMediumPriorityCyclable;

	public double TotalComputation;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public CycleManager()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			timer = new HighResolutionTime();
		}
	}

	/// <summary>
	/// PORT DEVIATION 21. How many CycleOnce calls the last Update made, and - during a replay -
	/// how many it must make.
	///
	/// THIS IS WHY REPLAYS DIVERGE. CycleRequests below runs until a WALL-CLOCK budget is spent:
	/// TimeAllocatedInSecondsForTimeSlicedSystems is 0.009, nine milliseconds, measured with
	/// QueryPerformanceCounter. So the amount of AI work done in a frame depends on how fast the
	/// machine happened to run that frame - and that work creates entities and updates sensors,
	/// every one of which draws from the gameplay random stream.
	///
	/// Measured, on Fields of Tau Ceti - Cudgel Hills (L): at frame 5 the recording had made 6885
	/// draws and the replay 10115, with the simulation clock identical to the last digit and the
	/// representative entity in exactly the same place. The recording reaches 10122 at frame 8.
	/// The replay was not wrong, it was three frames AHEAD - it ran the same work sooner, because
	/// nothing was loading and nine milliseconds bought more of it.
	///
	/// So the answer to "is the simulation deterministic" is: yes, given the same work schedule.
	/// The schedule is what is not deterministic, and it is not floating point.
	///
	/// A replay sets CycleBudget to the count the recording made on that frame, and the loop then
	/// runs on the count instead of the clock. Live play is untouched: CycleBudget is null and the
	/// nine milliseconds decide, exactly as the studio wrote it.
	/// </summary>
	public int CyclesLastUpdate { get; private set; }

	/// <summary>
	/// The number of cycles this frame must run, when a replay is driving. Null in normal play,
	/// and then the wall-clock budget decides.
	/// </summary>
	public int? CycleBudget { get; set; }

	private int cyclesThisUpdate;

	/// <summary>Whether the budget for this frame is spent, by whichever measure is in force.</summary>
	private bool BudgetSpent =>
		CycleBudget.HasValue
			? cyclesThisUpdate >= CycleBudget.Value
			: timeTaken >= GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems;

	public void Update()
	{
		timer.Start();
		timeTaken = 0.0;
		cyclesThisUpdate = 0;
		CycleRequests(highPriorityCycleUpdateRequests, ref nextHighPriorityCyclable, timer);
		CycleRequests(mediumPriorityCycleUpdateRequests, ref nextMediumPriorityCyclable, timer);
		TotalComputation += timeTaken;
		CyclesLastUpdate = cyclesThisUpdate;
	}

	private void CycleRequests(List<ICyclable> requests, ref CyclableID? nextCyclable, HighResolutionTime timer)
	{
		if (BudgetSpent)
		{
			return;
		}
		double num = 0.0;
		int num2 = requests.Count - 1;
		if (nextCyclable.HasValue)
		{
			CyclableID nextCyclableValue = nextCyclable.Value;
			int num3 = requests.FindIndex((ICyclable c) => c.ID == nextCyclableValue);
			if (num3 >= 0)
			{
				num2 = num3;
			}
		}
		nextCyclable = null;
		List<ICyclable> list = new List<ICyclable>();
		list.AddRange(requests);
		while (list.Count > 0 && !BudgetSpent)
		{
			for (int num4 = num2; num4 >= 0; num4--)
			{
				ICyclable cyclable = list[num4];
				if (!cyclable.IsPaused)
				{
					bool num5 = cyclable.CycleOnce();
					cyclesThisUpdate++;
					num = timer.GetTime() - timeTaken;
					timeTaken += num;
					cyclable.TotalComputationAllInstancesInSeconds += num;
					cyclable.ComputationTimeSpentInSeconds += num;
					if (The.Sim != null)
					{
						_ = The.Sim.TotalUnPausedGameTimeInSeconds - cyclable.StartedOnTimeInSeconds;
						_ = 5.0;
					}
					if (num5)
					{
						UnRegister(cyclable);
						list.Remove(cyclable);
					}
					if (The.Sim == null || The.Sim.IsGameOver)
					{
						return;
					}
					if (BudgetSpent)
					{
						int num6 = num4 - 1;
						if (num6 >= 0 && num6 < requests.Count)
						{
							nextCyclable = list[num6].ID;
						}
						return;
					}
				}
				else
				{
					list.Remove(cyclable);
				}
			}
			num2 = list.Count - 1;
		}
	}


	public void PrintPerformance(StringBuilder text)
	{
		text.AppendLine("Unpaused time (s): " + The.Sim.TotalUnPausedGameTimeInSeconds);
		text.AppendLine("Total computation time (s): " + Common.DecimalToString(TotalComputation));
		text.AppendLine($"Usage of allocated time ({Common.DecimalToString(GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems)} s): {Common.PercentageToString(timeTaken / GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems)}");
		text.AppendLine("Waiting agents: " + The.Sim.WaitingAgents.Count);
		text.AppendLine("Average agent waiting time (s): " + Common.DecimalToString((The.Sim.WaitingAgents.Count == 0) ? 0.0 : The.Sim.WaitingAgents.Average((KeyValuePair<EntityID, Tuple<Sim.WaitingFor, double>> a) => The.Sim.TotalUnPausedGameTimeInSeconds - a.Value.Item2)));
		text.AppendLine("High priority cyclables (movement maps): " + highPriorityCycleUpdateRequests.Count);
		text.AppendLine("Medium priority cyclables (searches): " + mediumPriorityCycleUpdateRequests.Count);
		text.AppendLine("");
		PrintCyclableClassInfo(text, "EventManager", EventManager.totalComputationAllInstancesInSeconds);
		PrintCyclableClassInfo(text, "MovementMap", MovementMap.totalComputationAllInstancesInSeconds);
		PrintCyclableClassInfo(text, "PathPlanner", PathPlanner.totalComputationAllInstancesInSeconds);
		PrintCyclableClassInfo(text, "RegionSearchPlanner", RegionSearchPlanner.totalComputationAllInstancesInSeconds);
		PrintCyclableClassInfo(text, "HaulingJobManager", HaulingJobManager.totalComputationAllInstancesInSeconds);
		PrintCyclableClassInfo(text, "Terrain region map", RegionMap.totalComputationAllInstancesInSeconds);
		text.AppendLine("");
		text.AppendLine("High prio cyclables:");
		foreach (ICyclable highPriorityCycleUpdateRequest in highPriorityCycleUpdateRequests)
		{
			PrintCyclable(text, highPriorityCycleUpdateRequest);
		}
		text.AppendLine("");
		text.AppendLine("Other cyclables:");
		foreach (ICyclable mediumPriorityCycleUpdateRequest in mediumPriorityCycleUpdateRequests)
		{
			PrintCyclable(text, mediumPriorityCycleUpdateRequest);
		}
		text.AppendLine("");
		text.AppendLine("Waiting agents:");
		List<EntityID> list = null;
		foreach (KeyValuePair<EntityID, Tuple<Sim.WaitingFor, double>> item in The.Sim.WaitingAgents.OrderByDescending((KeyValuePair<EntityID, Tuple<Sim.WaitingFor, double>> a) => The.Sim.TotalUnPausedGameTimeInSeconds - a.Value.Item2))
		{
			Entity entity = Entity.FindByID(item.Key);
			if (entity != null)
			{
				text.AppendLine($"{entity.GetDisplayName()} ({entity.ID}) {MapManager.WorldPosToSubtile(entity.PlaySiteLocation)} waiting for {item.Value.Item1} : {Common.DecimalToString(The.Sim.TotalUnPausedGameTimeInSeconds - item.Value.Item2)} s");
			}
			else
			{
				Common.AddToList(ref list, item.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityID item2 in list)
		{
			The.Sim.WaitingAgents.Remove(item2);
		}
	}

	private void PrintCyclable(StringBuilder text, ICyclable item)
	{
		double num = The.Sim.TotalUnPausedGameTimeInSeconds - item.StartedOnTimeInSeconds;
		item.PrintInfo(text);
		text.Append($" {Common.DecimalToString(num)} s,");
		text.Append(" (");
		text.Append(SecondsAsMilliseconds(item.ComputationTimeSpentInSeconds));
		text.Append(" ms)");
		if (item.UpdateInterval.HasValue && item.UpdateInterval.Value < num)
		{
			text.Append("!!!");
		}
		text.AppendLine();
	}

	private string SecondsAsMilliseconds(double seconds)
	{
		return $"{(int)(seconds * 1000.0)}";
	}

	public void PrintCyclableClassInfo(StringBuilder description, string name, double totalComputation)
	{
		description.AppendLine($"{name}: {Common.DecimalToString(totalComputation)} ({Common.PercentageToString(totalComputation / TotalComputation)})");
	}

	public bool IsRegistered(ICyclable request)
	{
		if (!mediumPriorityCycleUpdateRequests.Contains(request))
		{
			return highPriorityCycleUpdateRequests.Contains(request);
		}
		return true;
	}

	public void Register(ICyclable request, Priority priority)
	{
		request.StartedOnTimeInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds;
		request.ComputationTimeSpentInSeconds = 0.0;
		_ = request is DependentRegionMap;
		switch (priority)
		{
		case Priority.High:
			highPriorityCycleUpdateRequests.Add(request);
			break;
		case Priority.Medium:
			mediumPriorityCycleUpdateRequests.Add(request);
			break;
		}
	}

	public void UnRegister(ICyclable request)
	{
		mediumPriorityCycleUpdateRequests.Remove(request);
		highPriorityCycleUpdateRequests.Remove(request);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.CRC)
		{
			UnregisterBeforeSnapshot(highPriorityCycleUpdateRequests);
			UnregisterBeforeSnapshot(mediumPriorityCycleUpdateRequests);
		}
		mediumPriorityCycleUpdateRequestIDs = mediumPriorityCycleUpdateRequests.Select((ICyclable c) => c.ID).ToList();
		mediumPriorityCycleUpdateRequestIDs = sn.DoList(mediumPriorityCycleUpdateRequestIDs);
		highPriorityCycleUpdateRequestIDs = highPriorityCycleUpdateRequests.Select((ICyclable c) => c.ID).ToList();
		highPriorityCycleUpdateRequestIDs = sn.DoList(highPriorityCycleUpdateRequestIDs);
		sn.Ignore(timeTaken);
		sn.Ignore(TotalComputation);
		sn.Ignore(highPriorityCycleUpdateRequests);
		sn.Ignore(mediumPriorityCycleUpdateRequests);
		sn.Ignore(nextHighPriorityCyclable);
		sn.Ignore(nextMediumPriorityCyclable);
		sn.Ignore(timer);
		return this;
	}

	private void UnregisterBeforeSnapshot(List<ICyclable> list)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ICyclable cyclable = list[num];
			if (cyclable.UnregisterBeforeSnapshot)
			{
				UnRegister(cyclable);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		mediumPriorityCycleUpdateRequests = mediumPriorityCycleUpdateRequestIDs.Select((CyclableID c) => LookUp<ICyclable, CyclableID>.FindByID(c)).ToList();
		highPriorityCycleUpdateRequests = highPriorityCycleUpdateRequestIDs.Select((CyclableID c) => LookUp<ICyclable, CyclableID>.FindByID(c)).ToList();
		timer = new HighResolutionTime();
	}
}
