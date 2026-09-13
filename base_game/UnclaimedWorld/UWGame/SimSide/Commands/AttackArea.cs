using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class AttackArea : Command
{
	public long EntityGroupID;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public ZoneCommand ZoneCommand;

	public bool AttackVermin;

	public bool AttackThreats;

	public int NoOfAttackers;

	public AttackArea()
	{
	}

	public AttackArea(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackThreats, int noOfPatrollers)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		Init(giveClientFeedback, entityGroupID, attackVermin, attackThreats, noOfPatrollers);
	}

	public AttackArea(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackThreats, int noOfPatrollers)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		Init(giveClientFeedback, entityGroupID, attackVermin, attackThreats, noOfPatrollers);
	}

	private void Init(bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackThreats, int noOfPatrollers)
	{
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
		AttackVermin = attackVermin;
		AttackThreats = attackThreats;
		NoOfAttackers = noOfPatrollers;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		bool flag = DoAttackArea(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnPatrolOrAttackArea(zone);
		}
	}

	private bool DoAttackArea(Zone zoneToPatrolIn, EntityGroup expeditionOwner)
	{
		zoneToPatrolIn.AttackAreaJob = new AttackAreaJob(zoneToPatrolIn, expeditionOwner, AttackVermin, AttackThreats, NoOfAttackers);
		if (Priority.HasValue)
		{
			zoneToPatrolIn.AttackAreaJob.Priority = Priority.Value;
		}
		return true;
	}
}
