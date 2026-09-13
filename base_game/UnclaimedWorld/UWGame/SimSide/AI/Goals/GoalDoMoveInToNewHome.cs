using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalDoMoveInToNewHome : Goal
{
	private EntityID newHome;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDoMoveInToNewHome(Entity owner, EntityID newHome)
		: base(owner)
	{
		this.newHome = newHome;
	}

	public GoalDoMoveInToNewHome()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(newHome, out var data)))
		{
			return;
		}
		if (data is Entity entity)
		{
			if ((entity.Contains as IResidence).Residence.Residents >= entity.EntityType.ContainerType.ResidenceType.LivingCapacity)
			{
				base.Status = Status.Failed;
			}
			else if (!GoalMoveInToNewHome.MoveInToNewHome(personEntity, entity))
			{
				base.Status = Status.Failed;
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
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
		newHome = sn.DoEntityID(newHome);
		return this;
	}
}
