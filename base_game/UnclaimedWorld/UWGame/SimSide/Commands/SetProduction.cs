using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetProduction : Command
{
	public long ExpeditionID;

	public string EntityTypeKey;

	public int NewCount;

	public bool GiveClientFeedback;

	public SetProduction()
	{
	}

	public SetProduction(ExpeditionID expeditionID, string entityTypeKey, int newCount, bool giveClientFeedback)
	{
		EntityTypeKey = entityTypeKey;
		NewCount = newCount;
		ExpeditionID = (long)expeditionID;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoSetProduction();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.SetProduction(EntityTypeKey);
		}
	}

	private bool DoSetProduction()
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		EntityType entityType = GameData.Instance.AllEntityTypes[EntityTypeKey];
		expedition.OwnedEntities.ProductionOrders.SetDirectOrder(entityType, NewCount);
		if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value))
		{
			foreach (ProcessType item in value)
			{
				if (item.Outputs == null)
				{
					continue;
				}
				Output[] outputs = item.Outputs;
				foreach (Output output in outputs)
				{
					if (!output.IsWasteProduct)
					{
						expedition.OwnedEntities.ProductionOrders.SetDirectOrder(output.FinalEntityTypeToCreate, NewCount);
					}
				}
			}
		}
		expedition.JobManager.UpdateDirectOrderJobs(entityType);
		return true;
	}
}
