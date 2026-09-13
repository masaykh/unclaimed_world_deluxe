using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Jobs;

namespace UWGame.SimSide.AI.Goals;

public class WeaponInstanceComboJobData
{
	public AttackJob Job;

	public BodyPart.AttackDirection? AttackDirection;
}
