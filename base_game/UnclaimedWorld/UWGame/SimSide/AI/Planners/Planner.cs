using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal abstract class Planner : ISnapshot
{
	protected Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	protected Expedition expedition;

	private ExpeditionID snapshotExpedition;

	private List<RegionKnowledge> RegionKnowledge;

	protected Motivation motivation;

	protected List<GoapAction> currentPlan = new List<GoapAction>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	protected abstract void CreatePlan();

	protected abstract void DestroyPlan();

	public void Update(GameTime gameTime)
	{
		InternalUpdate(gameTime);
	}

	protected abstract void InternalUpdate(GameTime gameTime);

	protected abstract double ScoreMotivation();

	protected void MonitorCurrentPlan()
	{
		if (currentPlan == null)
		{
			return;
		}
		List<GoapAction> list = null;
		foreach (GoapAction item in currentPlan)
		{
			if (!item.MonitorAction())
			{
				LearnAboutFailedAction(item);
				item.Destroy();
				Common.AddToList(ref list, item);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (GoapAction item2 in list)
		{
			currentPlan.Remove(item2);
		}
	}

	protected void LearnAboutFailedAction(GoapAction action)
	{
	}

	protected void AddNewAction(GoapAction action)
	{
		if (action.Init())
		{
			currentPlan.Add(action);
		}
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		snapshotExpedition = sn.SnapshotID<Expedition, ExpeditionID>(expedition).Value;
		motivation = (Motivation)sn.DoISnapshot(motivation);
		currentPlan = sn.DoList(currentPlan);
		sn.Postpone(RegionKnowledge);
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
		expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);
		if (motivation != null)
		{
			motivation.LoadPostProcess(sn);
		}
		if (currentPlan == null)
		{
			return;
		}
		foreach (GoapAction item in currentPlan)
		{
			item.LoadPostProcess(sn);
		}
	}
}
