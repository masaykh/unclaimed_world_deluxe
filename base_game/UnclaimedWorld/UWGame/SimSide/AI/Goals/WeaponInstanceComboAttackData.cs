using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Body;

namespace UWGame.SimSide.AI.Goals;

public class WeaponInstanceComboAttackData
{
	public AttackType AttackType;

	public BodyPartID bodyPartID;

	public float EstimatedDamageScore;

	public WeaponInstanceComboJobData JobData;

	public double? AttackScore;
}
