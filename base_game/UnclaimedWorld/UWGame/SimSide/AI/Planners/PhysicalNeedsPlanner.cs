using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal class PhysicalNeedsPlanner : Planner
{
	private int numberOfScoutingActionsNeeded;

	private int numberOfHuntingActionsNeeded;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public PhysicalNeedsPlanner(Allegiance allegiance, Expedition expedition)
	{
		base.allegiance = allegiance;
		base.expedition = expedition;
	}

	public PhysicalNeedsPlanner()
	{
	}

	protected override void DestroyPlan()
	{
		throw new NotImplementedException();
	}

	protected override void InternalUpdate(GameTime gameTime)
	{
		if (motivation == null)
		{
			motivation = new PhysicalNeedsMotivation(allegiance);
		}
		MonitorCurrentPlan();
		CreatePlan();
	}

	protected override double ScoreMotivation()
	{
		if ((motivation as PhysicalNeedsMotivation).GetActionsWeAreMotivatedToDo(out numberOfScoutingActionsNeeded, out numberOfHuntingActionsNeeded))
		{
			return 1.0;
		}
		return 0.0;
	}

	protected override void CreatePlan()
	{
		if (ScoreMotivation() > 0.0)
		{
			for (int i = 0; i < numberOfScoutingActionsNeeded; i++)
			{
				GoapScoutAction action = new GoapScoutAction(allegiance, expedition);
				AddNewAction(action);
			}
			for (int j = 0; j < numberOfHuntingActionsNeeded; j++)
			{
				GoapFindPreyAction action2 = new GoapFindPreyAction(allegiance, expedition);
				AddNewAction(action2);
			}
		}
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		numberOfScoutingActionsNeeded = sn.DoInt32(numberOfScoutingActionsNeeded);
		numberOfHuntingActionsNeeded = sn.DoInt32(numberOfHuntingActionsNeeded);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
	}
}
