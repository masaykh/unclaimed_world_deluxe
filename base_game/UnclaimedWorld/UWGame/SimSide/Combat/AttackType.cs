using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.Client.Audio;
using UWGame.Client.Particles;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.SimEffects;
using UWGame.Steam;

namespace UWGame.SimSide.Combat;

[DebuggerDisplay("{KeyName}")]
public class AttackType : IGameData, IXmlSerializable
{
	public enum RangeTypes
	{
		Melee,
		Ray,
		Ballistic,
		Rocket
	}

	public string Comments;

	public float? DefenseRating;

	public string Damage;

	[XmlIgnore]
	public DamageType DamageFinal;

	public float DamageMean;

	public float DamageStandardDeviation;

	public AreaAttack AreaAttack;

	public float AccuracyFactor = 1f;

	public string[] EffectsOnVictim;

	[XmlIgnore]
	public List<EffectProfileType> FinalEffectsOnVictim;

	public BodyPartType[] DependsOn;

	public float DurationInSeconds;

	public float? MissDurationInSeconds;

	public float ActionPointInSeconds;

	public float? ElectricEnergyCost;

	public float? WeaponConditionDamageMaxFraction;

	public float? WeaponConditionDamageMinFraction;

	public BulletEffect BulletEffect;

	public Effects StartEffects;

	public Effects ImpactEffects;

	public Effects ActionPointEffects;

	public SoundData ImpactSound;

	public SoundData ActionPointSound;

	public SoundData SoundAtStart;

	[XmlIgnore]
	public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();

	public AnimModifier[] AnimationStates;

	[XmlIgnore]
	public List<AnimModifier> AnimationStatesList;

	public string UsesAmmo;

	[XmlIgnore]
	public EntityType UsesAmmoType;

	public int? RoundsToSpend;

	public bool IsDownAttack;

	public string RequiredSkill;

	[XmlIgnore]
	public SkillType RequiredSkillType;

	public RangeTypes RangeType;

	public double? ChanceToRest;

	public float? MaxRestTimeInSeconds;

	public float? MinRestTimeInSeconds;

	[XmlIgnore]
	public float? RestTimeMean;

	[XmlIgnore]
	public float? RestTimeStandardDeviation;

	public float? MinRange;

	public float? MaxRange;

	[XmlIgnore]
	public float? MinRangeSquared;

	[XmlIgnore]
	public float? MaxRangeSquared;

	[XmlIgnore]
	public float? ConditionDamageMean;

	[XmlIgnore]
	public float? ConditionDamageStandardDeviation;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AttackType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public override string ToString()
	{
		return Name ?? KeyName;
	}

	public void Initialize()
	{
		if (AnimationStates != null)
		{
			AnimationStatesList = AnimationStates.ToList();
		}
		if (MinRange.HasValue)
		{
			MinRangeSquared = MinRange.Value * MinRange.Value;
		}
		if (MaxRange.HasValue)
		{
			MaxRangeSquared = MaxRange.Value * MaxRange.Value;
		}
		if (WeaponConditionDamageMinFraction.HasValue && WeaponConditionDamageMaxFraction.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(WeaponConditionDamageMinFraction.Value, WeaponConditionDamageMaxFraction.Value, out ConditionDamageMean, out ConditionDamageStandardDeviation);
		}
		if (MinRestTimeInSeconds.HasValue && MaxRestTimeInSeconds.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(MinRestTimeInSeconds.Value, MaxRestTimeInSeconds.Value, out RestTimeMean, out RestTimeStandardDeviation);
		}
		if (AreaAttack != null)
		{
			AreaAttack.Initialize();
		}
		if (RequiredSkill != null)
		{
			RequiredSkillType = GameData.Instance.AllSkillTypes[RequiredSkill];
		}
	}

	public void PostLoadContentInitialize()
	{
		if (GameData.Instance.EventHooksByAttackType.TryGetValue(this, out var value))
		{
			foreach (AttackTypeActionHook item in value)
			{
				if (!EventActions.TryGetValue(item.Hook, out var value2))
				{
					value2 = new List<ActionSets>();
					EventActions.Add(item.Hook, value2);
				}
				value2.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
			}
		}
		DamageFinal = GameData.Instance.AllDamageTypes[Damage];
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (RangeType != RangeTypes.Melee)
		{
			EntityType.ValidateRequiredValue(ref listOfErrors, "Max range", MaxRange.HasValue);
		}
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
		if (UsesAmmo != null)
		{
			UsesAmmoType = GameData.Instance.AllEntityTypes[UsesAmmo];
		}
		if (EffectsOnVictim != null)
		{
			FinalEffectsOnVictim = new List<EffectProfileType>();
			string[] effectsOnVictim = EffectsOnVictim;
			foreach (string key in effectsOnVictim)
			{
				FinalEffectsOnVictim.Add(GameData.Instance.AllEffectProfileTypes[key]);
			}
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public bool HitTargets(Entity attacker, AttackJob job, OwnerID? ownerOfCarcass, BodyPartID bodyPartToAttackID, Entity targetAsEntity)
	{
		if (AreaAttack != null)
		{
			DoAreaAttack(attacker, job, targetAsEntity, ownerOfCarcass);
		}
		return HitTarget(attacker.ID, job, ownerOfCarcass, bodyPartToAttackID, targetAsEntity);
	}

	private void DoAreaAttack(Entity entity, AttackJob job, Entity targetAsEntity, OwnerID? ownerOfCarcass)
	{
		List<Entity> entitiesInArea = AreaAttack.GetEntitiesInArea(entity.PlaySiteLocation, entity.FacingNormal.ToVector2());
		if (entitiesInArea == null)
		{
			return;
		}
		entitiesInArea.RemoveAll((Entity e) => e == entity || e == targetAsEntity || (!AreaAttack.DamageOtherAllegianceMembers && e.AllegianceID == entity.AllegianceID));
		foreach (Entity item in entitiesInArea)
		{
			BodyPart.AttackDirection attackDirection = GoalDoAttack.GetAttackDirection(entity, item.PlaySiteLocation, item.Rotation);
			BodyPart randomBodyPartToHit = item.Body.GetRandomBodyPartToHit(attackDirection);
			HitTarget(entity.EntityID, job, ownerOfCarcass, randomBodyPartToHit.BodyPartID, item);
		}
	}

	private bool HitTarget(EntityID? attackerID, AttackJob job, OwnerID? ownerOfCarcass, BodyPartID bodyPartToAttackID, Entity targetAsEntity)
	{
		Entity entity = null;
		if (attackerID.HasValue)
		{
			entity = Entity.FindByID(attackerID.Value);
		}
		float energyLevelFactorOnDamage = GoalDoAttack.GetEnergyLevelFactorOnDamage(entity, this);
		BodyPart bodyPart = targetAsEntity.Body.FindBodyPart(bodyPartToAttackID);
		ComputeDamage(bodyPart.BodyPartType, out var damage, out var _, out var reductionConstant, out var armorLayer, doAttackRoll: true, energyLevelFactorOnDamage);
		StartImpactEffects(targetAsEntity, entity);
		string text = "";
		if (damage > 0f)
		{
			damage = bodyPart.DoDamage(damage);
			if (entity != null)
			{
				The.Client.AddLogEvent(The.Client.Log.CombatEvent, entity, $"hit {targetAsEntity.ToLink()} on the {bodyPart.BodyPartType.Name.ToLower(Config.Culture)} for {(int)damage} damage.{text}");
			}
		}
		else if (reductionConstant > 0f && entity != null)
		{
			The.Client.AddLogEvent(The.Client.Log.CombatEvent, entity, $"hit {targetAsEntity.ToLink()} on the {bodyPart.BodyPartType.Name.ToLower(Config.Culture)} for 0 damage. {armorLayer.Name} gave protection.{text}");
		}
		if (FinalEffectsOnVictim != null)
		{
			foreach (EffectProfileType item in FinalEffectsOnVictim)
			{
				targetAsEntity.SimEffects.Start(item);
			}
		}
		targetAsEntity.Intelligence.DamageMorale(damage, bodyPart);
		targetAsEntity.GetStatus(out var isDead, out var isUnconscious, out var _, out var _);
		if (isDead || isUnconscious)
		{
			if (isDead)
			{
				FireEventActionsWhenKillingTarget(job, targetAsEntity, entity);
				if (entity != null)
				{
					The.Client.LogKilledTarget(targetAsEntity, entity);
				}
			}
			CheckAchievements(entity, targetAsEntity);
			targetAsEntity.SendMessage(new Message(entity, Message.MessageTypes.HitAndCollapse, ownerOfCarcass));
		}
		else
		{
			FireEventActionsWhenHitting(job, targetAsEntity, entity);
			targetAsEntity.SendMessage(new Message(entity, Message.MessageTypes.Hit, damage));
		}
		if (ImpactSound != null)
		{
			targetAsEntity.Renderable.PlayActionSound(ImpactSound);
		}
		if (isDead)
		{
			return true;
		}
		return false;
	}

	private void CheckAchievements(Entity attacker, Entity killedTarget)
	{
		if (attacker != null && attacker.Intelligence != null && attacker.Intelligence.Allegiance.AllegianceType == AllegianceType.Player && The.Sim.StartGameParams.GetRGScenario() == StartGameParams.RGScenario.TwinklerIsland && (The.Sim.GetDifficultyKey() == "normal" || The.Sim.GetDifficultyKey() == "easy"))
		{
			StatsAndAchievements statsAndAchievements = The.Sim.Controller.StatsAndAchievements;
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.improviser) && killedTarget.EntityType.KeyName == "entity:twinkler" && KeyName == "shootImprovedFireExtinguisherBushDragonPoison")
			{
				statsAndAchievements.UnlockAchievement(AchievementID.improviser);
			}
		}
	}

	public void ComputeDamage(BodyPartType bodyPart, out float damage, out float resistance, out float reductionConstant, out BodyLayerType armorLayer, bool doAttackRoll, float energyFactor, float? attackDamageToUse = null)
	{
		armorLayer = null;
		resistance = 0f;
		reductionConstant = 0f;
		if (bodyPart.ArmorLayerType != null)
		{
			armorLayer = bodyPart.ArmorLayerType;
			resistance = bodyPart.ArmorLayerType.DamageReductionFactorFinal[DamageFinal];
			reductionConstant = bodyPart.ArmorLayerType.DamageReductionConstantFinal[DamageFinal];
		}
		float num = ((!doAttackRoll) ? attackDamageToUse.Value : ((float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(DamageMean, DamageStandardDeviation)));
		damage = energyFactor * num * (1f - resistance) - reductionConstant;
		damage = Common.ClampBottom(damage, 0f);
	}

	private void FireEventActionsWhenKillingTarget(AttackJob job, Entity targetAsEntity, Entity attacker)
	{
		FireEventActions(job, targetAsEntity, attacker, AgentActionHooks.KilledEnemy, AgentActionHooks.KilledPrey);
	}

	public void FireEventActions(AttackJob job, Entity targetAsEntity, Entity attacker, AgentActionHooks enemyHook, AgentActionHooks preyHook, AgentActionHooks? anyHook = null)
	{
		AgentActionHooks agentActionHooks;
		if (job != null)
		{
			agentActionHooks = ((!(job is ThreatJob)) ? preyHook : enemyHook);
		}
		else
		{
			if (!anyHook.HasValue)
			{
				return;
			}
			agentActionHooks = anyHook.Value;
		}
		Dictionary<AgentActionHooks, List<ActionSets>> defaultEventActions = null;
		if (attacker != null && attacker.EntityType.IntelligenceType != null)
		{
			defaultEventActions = attacker.EntityType.IntelligenceType.EventActions;
		}
		Goal.FireEventActions(attacker, targetAsEntity.EntityID, agentActionHooks, defaultEventActions, agentActionHooks, EventActions);
	}

	private void FireEventActionsWhenHitting(AttackJob job, Entity targetAsEntity, Entity attacker)
	{
		FireEventActions(job, targetAsEntity, attacker, AgentActionHooks.HitEnemy, AgentActionHooks.HitPrey);
	}

	public void StartStartEffects(Entity targetAsEntity, Entity attacker)
	{
		if (StartEffects != null)
		{
			BeginStartEffects(StartEffects.ParticleEmitters, attacker);
		}
		if (SoundAtStart != null)
		{
			attacker?.Renderable.PlayActionSound(SoundAtStart);
		}
	}

	public void StartActionPointEffects(Entity targetAsEntity, Entity attacker, bool willHitTarget)
	{
		if (attacker != null)
		{
			MapClient.StartBulletEffect(BulletEffect, MaxRange, targetAsEntity, attacker, willHitTarget);
			if (ActionPointEffects != null)
			{
				BeginStartEffects(ActionPointEffects.ParticleEmitters, attacker);
			}
			if (ActionPointSound != null)
			{
				attacker.Renderable.PlayActionSound(ActionPointSound);
			}
		}
	}

	public void StartImpactEffects(Entity targetAsEntity, Entity attacker)
	{
		if (ImpactEffects != null)
		{
			BeginStartEffects(ImpactEffects.ParticleEmitters, attacker);
		}
	}

	private void BeginStartEffects(ParticleEmitterEffect[] effects, Entity entityToAttachTo)
	{
		foreach (ParticleEmitterEffect particleEmitterEffect in effects)
		{
			if (particleEmitterEffect.AttachToEntity)
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect.ParticleSystemKey, entityToAttachTo.Renderable, null, null, particleEmitterEffect.EmitParticlesInParentDirection, particleEmitterEffect.DurationInSeconds, particleEmitterEffect.Offset);
			}
			else
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect.ParticleSystemKey, entityToAttachTo.Renderable.Location.Value.ToVector2(), null, null, particleEmitterEffect.DurationInSeconds, particleEmitterEffect.Offset);
			}
		}
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
