using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalReturnHome : CompositeGoal, ITopLevelGoal
{
	private Vector3 locationOfExpedition;

	private bool isBold;

	private Vector3? teleportTo;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalReturnHome(Entity owner, List<EntityGroupID> ownersVehicles, bool isBold, Vector3? teleportTo)
		: base(owner)
	{
		this.isBold = isBold;
		this.teleportTo = teleportTo;
		ownersOfVehicles = ownersVehicles;
	}

	public GoalReturnHome()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (teleportTo.HasValue)
		{
			if (entity.ContainedBy.HasValue && entity.GetContainedBy(out Entity container))
			{
				container.Contains.Remove(entity);
			}
			entity.Location = teleportTo.Value;
		}
		Vector3? returnLocation = GetReturnLocation();
		if (returnLocation.HasValue)
		{
			AddSubgoal(new GoalMoveToPosition(entity, returnLocation.Value, ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			if (isBold)
			{
				entity.Intelligence.SetBoldStance();
			}
			locationOfExpedition = entityIntelligence.CurrentExpedition.Center.Value;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private Vector3? GetReturnLocation()
	{
		SubtileInfluence subtileInfluence = SubtileInfluence.FindFreeSpotNearLocation(entityIntelligence.CurrentExpedition.Center.Value, 15, entity.Location, entity, useMovementMap: true);
		if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileInfluence.Values, out var bestSubtilePoint) == -1)
		{
			return null;
		}
		return MapManager.SubTileToWorldPos(new Point(subtileInfluence.TopLeftSubtilePositionOfMap.X + bestSubtilePoint.X, subtileInfluence.TopLeftSubtilePositionOfMap.Y + bestSubtilePoint.Y)).ToVector3();
	}

	protected override bool ArePreconditionsOK()
	{
		if (entityIntelligence.CurrentExpedition.Center != locationOfExpedition)
		{
			return false;
		}
		return true;
	}

	public override string GetStatus()
	{
		return "Returning home";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!ArePreconditionsOK())
		{
			base.Status = Status.Failed;
		}
		else
		{
			base.Status = ProcessSubgoals(elapsed);
		}
	}

	public override bool RequiresBoldStance()
	{
		return isBold;
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
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
		locationOfExpedition = sn.DoVector3(locationOfExpedition);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		isBold = sn.DoBool(isBold);
		teleportTo = sn.DoVector3Nullable(teleportTo);
		return this;
	}
}
