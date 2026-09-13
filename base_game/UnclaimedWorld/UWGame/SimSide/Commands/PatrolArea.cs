using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class PatrolArea : Command
{
	public long EntityGroupID;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public ZoneCommand ZoneCommand;

	public bool AttackVermin;

	public bool AttackTargetsOutsideZone;

	public int NoOfPatrollers;

	public PatrolArea()
	{
	}

	public PatrolArea(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		Init(giveClientFeedback, entityGroupID, attackVermin, attackTargetsOutsideZone, noOfPatrollers);
	}

	public PatrolArea(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		Init(giveClientFeedback, entityGroupID, attackVermin, attackTargetsOutsideZone, noOfPatrollers);
	}

	private void Init(bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
	{
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
		AttackVermin = attackVermin;
		AttackTargetsOutsideZone = attackTargetsOutsideZone;
		NoOfPatrollers = noOfPatrollers;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		bool flag = DoPatrolArea(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnPatrolOrAttackArea(zone);
		}
	}

	private bool DoPatrolArea(Zone zoneToPatrolIn, EntityGroup expeditionOwner)
	{
		zoneToPatrolIn.PatrolJob = new PatrolJob(zoneToPatrolIn, expeditionOwner, AttackVermin, AttackTargetsOutsideZone, NoOfPatrollers);
		if (Priority.HasValue)
		{
			zoneToPatrolIn.PatrolJob.Priority = Priority.Value;
		}
		return true;
	}
}
