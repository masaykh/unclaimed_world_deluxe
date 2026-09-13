using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalDoProduceAtomic : Goal
{
	private float maxPossibleProgress;

	private double goalProgress;

	private static Queue<GoalDoProduceAtomic> freeGoals = new Queue<GoalDoProduceAtomic>();

	private GoalDoProduce parentGoal;

	private GoalID snapshotParentGoal;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private void Init(Entity owner, GoalDoProduce parentGoal)
	{
		this.parentGoal = parentGoal;
		goalProgress = 0.0;
		Init(owner);
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public static void ClearPool()
	{
		freeGoals.Clear();
	}

	public static GoalDoProduceAtomic GetGoal(Entity owner, GoalDoProduce parentGoal)
	{
		if (freeGoals.Count == 0)
		{
			for (int i = 0; i < 30; i++)
			{
				freeGoals.Enqueue(new GoalDoProduceAtomic());
			}
		}
		GoalDoProduceAtomic goalDoProduceAtomic = freeGoals.Dequeue();
		goalDoProduceAtomic.Init(owner, parentGoal);
		return goalDoProduceAtomic;
	}

	public override void RetireGoal()
	{
		freeGoals.Enqueue(this);
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
	}

	public override void OnEnter()
	{
		if (!GoalEvaluator.ProcessDataResultCausesSkip(entityIntelligence.GetKnownProcessData(parentGoal.ProductionProcess, out var data)))
		{
			SetProcessAnimStates(data.ProcessType);
		}
		base.OnEnter();
	}

	private void EmptyToolContainers()
	{
	}

	private bool Produce(GameTime elapsed)
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(parentGoal.ProductionProcess);
		if (simProcess == null)
		{
			base.Status = Status.Failed;
			return false;
		}
		if (!simProcess.IsStarted)
		{
			OwnerID? ownerOfProduct = parentGoal.OwnerOfProduct;
			if (!GoalDoProduce.GetStationaryTools(parentGoal.Tools, out var stationaryTools))
			{
				return false;
			}
			if (simProcess.Start(entity, ownerOfProduct, parentGoal.ToolTypeCombination, stationaryTools) == SimProcess.StatusOfProcess.Failed)
			{
				base.Status = Status.Failed;
				return false;
			}
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		_ = entity.ID;
		_ = 4941;
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(parentGoal.ProductionProcess);
		float progress;
		if (simProcess == null)
		{
			base.Status = Status.Failed;
		}
		else if (!simProcess.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out progress))
		{
			base.Status = Status.Failed;
		}
		else if (!NonLivingEntity.IsCompleted(progress))
		{
			if (!Produce(elapsed))
			{
				base.Status = Status.Failed;
				return;
			}
			_ = entity.ID;
			_ = 4941;
			goalProgress += elapsed.ElapsedGameTime.TotalSeconds;
			if (goalProgress > GameData.Instance.AIConstants.AtomicGoalPeriodInSeconds)
			{
				base.Status = Status.Completed;
			}
		}
		else
		{
			base.Status = Status.Completed;
		}
	}

	public override float GetExertionLevel()
	{
		if (!GoalEvaluator.ProcessDataResultCausesSkip(entityIntelligence.GetKnownProcessData(parentGoal.ProductionProcess, out var data)))
		{
			return data.ProcessType.PhysicalWorkFactor ?? GameData.Instance.Constants.PhysicalWork.DefaultWork;
		}
		return base.GetExertionLevel();
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
		maxPossibleProgress = sn.DoFloat(maxPossibleProgress);
		goalProgress = sn.DoDouble(goalProgress);
		snapshotParentGoal = sn.SnapshotID<Goal, GoalID>(parentGoal).Value;
		sn.Ignore(freeGoals);
		sn.Ignore(parentGoal);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		parentGoal = (GoalDoProduce)LookUpGoals.FindByID(snapshotParentGoal);
	}
}
