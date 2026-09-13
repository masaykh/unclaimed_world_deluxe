using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.Steam;

namespace UWGame.SimSide.Allegiances.Statistics;

public class KillStatistics : ISnapshot
{
	public Dictionary<EntityType, int> Kills = new Dictionary<EntityType, int>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void AddKillEvent(EntityType victim)
	{
		Common.AddToDictWithSums(Kills, victim);
		CheckAchievements();
	}

	private void CheckAchievements()
	{
		_ = The.Sim.StartGameParams.StartScenarioParams;
		if (The.Sim.StartGameParams.GetRGScenario() == StartGameParams.RGScenario.MuckrootMiningCamp && !The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.swarmerKills) && Kills.TryGetValue(GameData.Instance.AllEntityTypes["entity:swarmer"], out var value) && value >= 300)
		{
			The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.swarmerKills);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Kills = sn.DoDictionary(Kills);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
