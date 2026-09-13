using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class AttackEntityAction : EventActionType
{
	public TargetObject Attacker;

	public TargetObject TargetToAttack;

	public EvalNode AttackTypeKey;

	public Ownership? OwnershipType;

	public TargetObject OwnerOfCarcass;

	public AttackEntityAction(string keyName)
		: base(keyName)
	{
	}

	public AttackEntityAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		Entity attacker = null;
		List<IHasExposedProperties> result = TargetToAttack.GetResult(action);
		if (result.Count == 0)
		{
			failReason = "Target lookup did not give any results.";
			return false;
		}
		entity = (Entity)result[0];
		if (Attacker != null)
		{
			result = Attacker.GetResult(action);
			if (result.Count == 0)
			{
				failReason = "Attacker lookup did not give any results.";
				return false;
			}
			attacker = (Entity)result[0];
		}
		PropertyResult? propertyResult = AttackTypeKey.Evaluate(action);
		if (propertyResult.HasValue)
		{
			GameData.Instance.AllAttackTypes.TryGetValue(propertyResult.Value.StringResult, out var value);
			IOwner owner = null;
			if (OwnershipType.HasValue && OwnerOfCarcass != null)
			{
				owner = ClaimEntityAction.ResolveNewOwner(OwnershipType.Value, OwnerOfCarcass, action);
				if (owner == null)
				{
					failReason = "Owner lookup did not give any results.";
					return false;
				}
			}
			if (entity != null && value != null)
			{
				Attack(attacker, entity, value, owner);
				return true;
			}
			return false;
		}
		failReason = "Failed to find attack type key";
		return false;
	}

	private void Attack(Entity attacker, Entity targetEntity, AttackType attackType, IOwner ownerOfCarcass)
	{
		targetEntity.GetStatus(out var isDead, out var _, out var _, out var _);
		if (!isDead)
		{
			attackType.StartStartEffects(targetEntity, attacker);
			BodyPart.AttackDirection attackDirection = ((attacker != null) ? GoalDoAttack.GetAttackDirection(attacker, targetEntity.Location.Value, targetEntity.Rotation) : BodyPart.AttackDirection.Front);
			BodyPart randomBodyPartToHit = targetEntity.Body.GetRandomBodyPartToHit(attackDirection);
			BodyPart hitBodyPart;
			bool flag = GoalDoAttack.RollToHit(attacker, targetEntity, attackType, randomBodyPartToHit, attackDirection, out hitBodyPart);
			if (flag)
			{
				OwnerID? ownerOfCarcass2 = ownerOfCarcass?.ID;
				attackType.HitTargets(attacker, null, ownerOfCarcass2, hitBodyPart.BodyPartID, targetEntity);
			}
			attackType.StartActionPointEffects(targetEntity, attacker, flag);
		}
	}

	public override string ToString()
	{
		return "Attack entity";
	}
}
