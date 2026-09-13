using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ProcessAction : EventActionType
{
	public string ActingOnEntityName;

	public TargetObject ActingOnEntityObject;

	public string ProcessType;

	public string WorkerName;

	public TargetObject WorkerObject;

	public bool FinishProcessImmediately;

	public bool SuppressSpawningEvents = true;

	public ProcessAction(string keyName)
		: base(keyName)
	{
	}

	public ProcessAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		if (ActingOnEntityName != null || ActingOnEntityObject != null)
		{
			_ = ActingOnEntityName == "Fish trap spot Saltwater 2";
			if (!EventActionType.GetEntity(ActingOnEntityName, ActingOnEntityObject, action, out entity, ref failReason))
			{
				return false;
			}
		}
		Entity entity2 = null;
		if ((WorkerName != null || WorkerObject != null) && !EventActionType.GetEntity(WorkerName, WorkerObject, action, out entity2, ref failReason))
		{
			return false;
		}
		EntityAndRoot? actingOnEntity = null;
		if (entity != null)
		{
			actingOnEntity = entity.GetAsEntityAndRoot();
		}
		ProcessType processType = null;
		if (ProcessType != null)
		{
			processType = GameData.Instance.AllProcessTypes[ProcessType];
		}
		SimProcess simProcess = new SimProcess(processType, actingOnEntity, FinishProcessImmediately, SuppressSpawningEvents);
		if (simProcess.Start(entity2, null, null, null) == SimProcess.StatusOfProcess.Failed)
		{
			failReason = "Failed to start process";
			return false;
		}
		if (entity2 != null)
		{
			simProcess.AssignWorker(entity2, null);
		}
		if (FinishProcessImmediately)
		{
			simProcess.ProduceTillCompletion();
		}
		DetectEntity(entity, entity2);
		return true;
	}

	public static void DetectEntity(Entity actingOnEntity, Entity worker)
	{
		if (worker != null && actingOnEntity != null && worker.ID != EntityID.Invalid && actingOnEntity.ID != EntityID.Invalid)
		{
			Allegiance allegiance = worker.Intelligence.Allegiance;
			DetectEntity(actingOnEntity, allegiance);
		}
	}

	public static void DetectEntity(Entity actingOnEntity, Allegiance allegiance)
	{
		allegiance.SharedKnowledge.SeeDetectableIfRelevant(actingOnEntity, testForUsesMemory: true, suppressClientFeedback: true, null, null, doAssert: false);
		Point playSiteMapPosition = actingOnEntity.PlaySiteMapPosition;
		if (!The.Map.GetTile(playSiteMapPosition).AllegiancesThatSeeThisTile.Contains(allegiance))
		{
			allegiance.SharedKnowledge.UnSeeEntity(actingOnEntity);
		}
	}

	public override string ToString()
	{
		return "Process: " + ProcessType + ", acting on: " + ActingOnEntityName;
	}
}
