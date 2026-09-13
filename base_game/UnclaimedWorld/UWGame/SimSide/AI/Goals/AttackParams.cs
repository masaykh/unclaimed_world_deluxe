using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;

namespace UWGame.SimSide.AI.Goals;

public struct AttackParams
{
	public AttackType AttackType;

	public EntityAndRoot? Weapon;

	public BodyPartID BodyPartToAttackID;
}
