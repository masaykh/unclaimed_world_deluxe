using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AI.Goals;

public class WeaponInstanceCombo : IEdge, IScore
{
	public IKnownEntityData Weapon;

	public Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> ReplenishItemsForWeapon;

	public WeaponInstanceComboAttackData AttackData;

	public float Score { get; set; }

	public float Edge { get; set; }
}
