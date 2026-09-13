using System;
using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body;

public abstract class BodyPart : ISnapshot, IHasBodyParts, IHasExposedProperties
{
	public enum AttackDirection
	{
		Front,
		Back,
		Left,
		Right
	}

	public BodyPartType BodyPartType;

	private string snapshotBodyPartName;

	public Body Body;

	private EntityID? snapshotParentEntity;

	private MemoryFactID? snapshotParentMemoryFact;

	public bool Exists = true;

	public float Hitpoints;

	public float MaxHitpoints;

	public BodyPartID BodyPartID;

	private static BodyPartID idCounter;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string KeyName { get; private set; }

	public List<BodyPart> BodyParts { get; set; }

	public bool IsSnapshotted { get; set; }

	public BodyPart(BodyPartType bodyPartType, Body body)
	{
		BodyPartID = idCounter;
		idCounter++;
		BodyPartType = bodyPartType;
		Body = body;
		KeyName = Body.Parent.EntityType.KeyName + ":" + BodyPartType.Name;
	}

	protected BodyPart(BodyPart original)
	{
		BodyPartID = original.BodyPartID;
		BodyPartType = original.BodyPartType;
		Hitpoints = original.Hitpoints;
		Exists = original.Exists;
		MaxHitpoints = original.MaxHitpoints;
		KeyName = original.KeyName;
	}

	public BodyPart()
	{
	}

	public static void ResetBodyPartCounter()
	{
		idCounter = BodyPartID.First;
	}

	public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
	}

	public void GatherBodyParts(List<Body.BodyPartChance> bodyParts, AttackDirection attackDirection)
	{
		bodyParts.Add(new Body.BodyPartChance
		{
			BodyPart = this,
			Score = GetToHitProfile(attackDirection)
		});
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GatherBodyParts(bodyParts, attackDirection);
		}
	}

	public virtual void ChangeOwnershipOnParts(IOwner newOwner)
	{
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.ChangeOwnershipOnParts(newOwner);
		}
	}

	public float DoDamage(float damage)
	{
		if (The.Sim.TotalUnPausedGameTimeInSeconds > 23.0)
		{
			_ = Body.Parent.ID;
			_ = 19;
		}
		float damageDone = GetDamageDone(damage, Hitpoints);
		Hitpoints -= damageDone;
		Hitpoints = Common.ClampBottom(Hitpoints, 0f);
		Body.GlobalHitpoints -= damageDone;
		Body.GlobalHitpoints = Common.ClampBottom(Body.GlobalHitpoints, 0f);
		if (!IsFunctional())
		{
			AffectParentOnLossOfBodyPart();
		}
		return damageDone;
	}

	public static float GetDamageDone(float damage, float Hitpoints)
	{
		return Math.Min(damage, Hitpoints);
	}

	private void AffectParentOnLossOfBodyPart()
	{
		if (BodyPartType.Functions != null)
		{
			BodyPartFunction[] functions = BodyPartType.Functions;
			foreach (BodyPartFunction bodyPartFunction in functions)
			{
				if (bodyPartFunction.Function == BodyPartFunction.FunctionType.Locomotion)
				{
					Body.Parent.ImpairMovement(bodyPartFunction.Weight);
				}
			}
		}
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.AffectParentOnLossOfBodyPart();
		}
	}

	public bool IsBodyPartDamageFatal()
	{
		if (Hitpoints <= 0f && IsVital())
		{
			return true;
		}
		if (BodyParts != null)
		{
			foreach (BodyPart bodyPart in BodyParts)
			{
				if (bodyPart.IsBodyPartDamageFatal())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsBodyPartDamageCausingCollapse()
	{
		if (IsVital() && Hitpoints <= GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * MaxHitpoints)
		{
			return true;
		}
		if (BodyParts != null)
		{
			foreach (BodyPart bodyPart in BodyParts)
			{
				if (bodyPart.IsBodyPartDamageCausingCollapse())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void InitializeHitpoints()
	{
		Hitpoints = BodyPartType.HitpointsFraction * Body.GlobalHitpoints;
		MaxHitpoints = Hitpoints;
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.InitializeHitpoints();
		}
	}

	public void UpdateHitpointsWithNewBulk(float percentageIncrease)
	{
		if (Hitpoints > 0f)
		{
			Hitpoints = (1f + percentageIncrease) * Hitpoints;
			MaxHitpoints = BodyPartType.HitpointsFraction * Body.MaxHitpoints;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.UpdateHitpointsWithNewBulk(percentageIncrease);
		}
	}

	public float GetToHitModifier(float attackerSize, AttackDirection direction)
	{
		float num = 1f;
		num = GetToHitProfile(direction);
		float num2 = ((Body.Parent == null) ? Body.ParentMemoryFact.Bulk : Body.Parent.Bulk);
		return Common.Clamp(num2 / attackerSize * num, 0.5f, 3f);
	}

	private float GetToHitProfile(AttackDirection direction)
	{
		float result = 0f;
		switch (direction)
		{
		case AttackDirection.Front:
			result = BodyPartType.ToHitProfileFront;
			break;
		case AttackDirection.Back:
			result = BodyPartType.ToHitProfileBack;
			break;
		case AttackDirection.Left:
			result = BodyPartType.ToHitProfileLeft;
			break;
		case AttackDirection.Right:
			result = BodyPartType.ToHitProfileRight;
			break;
		}
		return result;
	}

	public bool IsVital()
	{
		return BodyPartType.IsVital();
	}

	public bool IsFunctional()
	{
		return Hitpoints > 0f;
	}

	public override string ToString()
	{
		return BodyPartType.Name;
	}

	public void Regain(float totalRegainAmount)
	{
		if (!IsFunctional())
		{
			return;
		}
		if (Hitpoints < MaxHitpoints)
		{
			Hitpoints += BodyPartType.HitpointsFraction * totalRegainAmount;
			Hitpoints = Common.ClampTop(Hitpoints, MaxHitpoints);
		}
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.Regain(totalRegainAmount);
		}
	}

	public BodyPart FindFirstMatchingBodyPart(Predicate<BodyPart> predicate)
	{
		if (predicate(this))
		{
			return this;
		}
		if (BodyParts != null)
		{
			foreach (BodyPart bodyPart2 in BodyParts)
			{
				BodyPart bodyPart = bodyPart2.FindFirstMatchingBodyPart(predicate);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public void GetHurtVitalBodyPartFactor(ref float currentFactor)
	{
		if (IsVital())
		{
			float num = 1f - GetMoraleDecreaseFactor();
			if (num < currentFactor)
			{
				currentFactor = num;
			}
		}
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GetHurtVitalBodyPartFactor(ref currentFactor);
		}
	}

	public void GetModifiedHitpointsForPresentation(ref float totalHitpoints, ref float totalMaxHitpoints)
	{
		totalMaxHitpoints += MaxHitpoints;
		float num = Hitpoints;
		if (IsVital() && num < MaxHitpoints)
		{
			float num2 = num / MaxHitpoints;
			float num3 = num2 * num2;
			num = MaxHitpoints * num3;
		}
		totalHitpoints += num;
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GetModifiedHitpointsForPresentation(ref totalHitpoints, ref totalMaxHitpoints);
		}
	}

	private float GetMoraleDecreaseFactor()
	{
		float damageToAffectMorale = 1f / GameData.Instance.OneOverMeanDamageFromHumanPunch;
		return GetMoraleDecreaseFactor(damageToAffectMorale);
	}

	public float GetMoraleDecreaseFactor(float damageToAffectMorale)
	{
		if (Hitpoints == MaxHitpoints)
		{
			return 0f;
		}
		if (damageToAffectMorale == 0f)
		{
			return 0f;
		}
		float num = (int)(GetHitpointsUntilUnconsciousness() / damageToAffectMorale);
		if (num < 0f)
		{
			num = 0f;
		}
		float num2 = (int)(GetMaxHitpointsUntilUnconsciousness() / damageToAffectMorale);
		if (num2 <= 0f)
		{
			return 1f;
		}
		float num3 = num / num2;
		return 1f - num3;
	}

	private float GetMaxHitpointsUntilUnconsciousness()
	{
		return MaxHitpoints * (1f - GameData.Instance.Constants.FractionOfHitpointsCausingCollapse);
	}

	private float GetHitpointsUntilUnconsciousness()
	{
		float num = MaxHitpoints - Hitpoints;
		return GetMaxHitpointsUntilUnconsciousness() - num;
	}

	static BodyPart()
	{
		idCounter = BodyPartID.First;
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		exposedPropertyValueFunctions.Add("Injuries", GetHealthFraction);
		exposedPropertyValueFunctions.Add("MovementImpairments", GetMovementValue);
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		PropertyResult? result = null;
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (customFields != null && customFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return BodyPartType.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = null;
	}

	public void GetList<Type>(ref List<Type> listToFillWithProperties) where Type : IHasExposedProperties
	{
		if (listToFillWithProperties == null)
		{
			listToFillWithProperties = new List<Type>();
		}
		Type item = (Type)(object)this;
		listToFillWithProperties.Add(item);
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GetList(ref listToFillWithProperties);
		}
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		if (Body.Parent != null)
		{
			return true;
		}
		return false;
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public static PropertyResult? GetHealthFraction(IHasExposedProperties bodyPartToGetFractionFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((BodyPart)bodyPartToGetFractionFrom).GetHealthFraction();
	}

	public PropertyResult GetHealthFraction()
	{
		return new PropertyResult
		{
			NumberResult = Hitpoints / MaxHitpoints
		};
	}

	public static PropertyResult? GetMovementValue(IHasExposedProperties bodyPartToGetFractionFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((BodyPart)bodyPartToGetFractionFrom).GetMovementValue();
	}

	public PropertyResult? GetMovementValue()
	{
		if (BodyPartType.Functions == null)
		{
			return null;
		}
		BodyPartFunction[] functions = BodyPartType.Functions;
		foreach (BodyPartFunction bodyPartFunction in functions)
		{
			if (bodyPartFunction.Function == BodyPartFunction.FunctionType.Locomotion)
			{
				PropertyResult value = default(PropertyResult);
				if (IsFunctional())
				{
					value.NumberResult = 1f;
				}
				else
				{
					value.NumberResult = 1f - bodyPartFunction.Weight;
				}
				return value;
			}
		}
		return null;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		BodyPartID = sn.DoEnum(BodyPartID);
		idCounter = sn.DoEnum(idCounter);
		BodyParts = sn.DoList(BodyParts);
		snapshotBodyPartName = sn.DoString((BodyPartType != null) ? BodyPartType.Name : null);
		KeyName = sn.DoString(KeyName);
		MaxHitpoints = sn.DoFloat(MaxHitpoints);
		Hitpoints = sn.DoFloat(Hitpoints);
		Exists = sn.DoBool(Exists);
		customFields = sn.DoDictionary(customFields);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotParentEntity = null;
			if (Body.Parent != null)
			{
				snapshotParentEntity = Body.Parent.EntityID;
			}
			snapshotParentMemoryFact = null;
			if (Body.ParentMemoryFact != null)
			{
				snapshotParentMemoryFact = Body.ParentMemoryFact.ID;
			}
		}
		snapshotParentEntity = sn.DoEntityIDNullable(snapshotParentEntity);
		snapshotParentMemoryFact = sn.DoEnumNullable(snapshotParentMemoryFact);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(Body);
		sn.Ignore(BodyPartType);
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotParentEntity.HasValue)
		{
			Body = Entity.FindByID(snapshotParentEntity.Value).Body;
		}
		snapshotParentEntity = null;
		if (snapshotParentMemoryFact.HasValue)
		{
			Body = LookUp<MemoryFact, MemoryFactID>.FindByID(snapshotParentMemoryFact.Value).Body;
		}
		snapshotParentMemoryFact = null;
		EntityType entityType = ((Body.Parent == null) ? Body.ParentMemoryFact.EntityType : Body.Parent.EntityType);
		BodyPartType = entityType.BodyType.FindBodyPart(snapshotBodyPartName);
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.LoadPostProcess(sn);
		}
	}
}
