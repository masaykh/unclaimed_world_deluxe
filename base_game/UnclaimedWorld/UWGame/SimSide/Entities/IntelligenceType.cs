using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities;

public class IntelligenceType : IXmlSerializable
{
	public bool IsMobile;

	public string[] Attacks;

	public string[] IntrinsicTools;

	public string[] IntrinsicWeapons;

	[XmlIgnore]
	public List<EntityType> IntrinsicToolTypes;

	[XmlIgnore]
	public List<EntityType> IntrinsicWeaponTypes;

	[XmlIgnore]
	public List<AttackType> AttackTypes;

	public DefendActionType[] DefendActionTypes;

	public ScareActionType[] ScareActionTypes;

	public string ContainerTransactTag;

	[XmlIgnore]
	public int? ContainerTransactValue;

	public float IdleChanceToTalk;

	public bool IsPredator;

	public bool HuntsVermin;

	public bool WillAttackNonThreatsNearby;

	public bool OtherAgentsNearExpeditionCenterAreConsideredThreats;

	public float? MaxDistanceFromExpeditionsToHuntVermin;

	public StrengthRating StrengthRating = StrengthRating.LikeHumans;

	public float Boldness = 0.5f;

	public float Courage = 0.5f;

	public int ForageAndHuntingRadius = 500;

	public float MembersScoutingFraction = 1f;

	public float DropLightDuration = 1.2f;

	public float DropLightActionPointDuration = 0.64f;

	public float DropHeavyDuration = 0.92f;

	public float DropHeavyActionPointDuration = 0.44f;

	public float pickupMountedEquippedDuration = 1.72f;

	public float pickupMountedEquippedActionPointDuration = 0.48f;

	public float pickupEquipDuration = 1.72f;

	public float pickupEquipActionPointDuration = 0.48f;

	public float PickupLightDuration = 1.2f;

	public float PickupLightActionPointDuration = 0.56f;

	public float PickupHeavyDuration = 0.68f;

	public float PickupHeavyActionPointDuration = 0.32f;

	public float pickupMountDuration = 0.96f;

	public float pickupMountActionPointDuration = 0.48f;

	public float SwitchLightToLightDuration = 2.4f;

	public float SwitchLightToLightActionPointDuration = 1.2f;

	public float dropEquippedDuration = 1.32f;

	public float dropEquippedActionPointDuration = 0.8f;

	public float dropMountedDuration = 0.96f;

	public float dropMountedActionPointDuration = 0.36f;

	public float DropLightToLightActionPointDuration = 0.6f;

	public float DropLightToLightDuration = 1.24f;

	public float dropMountToMountActionPointDuration = 0.5f;

	public float dropMountToMountDuration = 1f;

	public float FightOverFleeProbability = 0.5f;

	public float MoraleIncreasePerDay = 60f;

	public string[] ExpeditionPolledEvents;

	[XmlIgnore]
	public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();

	[XmlIgnore]
	public Dictionary<EntityType, List<ActionSets>> DetectEntityTypeEvents = new Dictionary<EntityType, List<ActionSets>>();

	[XmlIgnore]
	public Dictionary<ResourceType, List<ActionSets>> DetectResourceTypeEvents = new Dictionary<ResourceType, List<ActionSets>>();

	public double? ChanceToRestAfterMeleeAttack;

	public double? ChanceToRestAfterRangedAttack;

	public float? MaxRestTimeAfterAttackingInSeconds;

	public float? MinRestTimeAfterAttackingInSeconds;

	[XmlIgnore]
	public float? RestTimeAfterAttackingMean;

	[XmlIgnore]
	public float? RestTimeAfterAttackingStandardDeviation;

	public float MemoryInDays = 6f;

	public bool CanPanic = true;

	public float? AggroRange;

	public float? AssistanceRange;

	public float? ChanceToIdleWalkShortDistanceAway;

	public float? ShortIdleWalkMaxDistance;

	public float? ShortIdleWalkMinDistance;

	public string[] InterestInTriggerTypes;

	[XmlIgnore]
	public Dictionary<TriggerType, bool> HasInterestInTriggers = new Dictionary<TriggerType, bool>();

	public bool? AllowEscapeFromTinyAreas;

	public bool? CanSpeak;

	public bool? CanTradeAndCommunicate;

	public bool? CanScout;

	public bool? CanExamine;

	public bool? CanPatrol;

	public bool? CanDoJobs;

	public bool? CanProduce;

	public bool? CanHaul;

	public bool? CanCheckProgress;

	public bool? CanHunt;

	public bool? CanAttack;

	public bool? CanUseWeapons;

	public bool? CanUseGadgets;

	public bool? CanEmigrate;

	public bool? CanMountTools;

	public bool? CanReplenish;

	public bool RespectsOwnership;

	public string[] Prey;

	[XmlIgnore]
	public HashSet<EntityType> PreyTypes;

	public string ServantForEntityTypeTag;

	public string[] HasServantsTags;

	[XmlIgnore]
	public List<EntityType> HasServants;

	public SerializableDictionary<string, float> Skills;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(IntelligenceType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public void PostLoadContentInitialize(EntityType parent)
	{
		if (ContainerTransactTag != null && GameData.Instance.ContainerTags.TryGetValue(ContainerTransactTag, out var value))
		{
			ContainerTransactValue = value;
		}
		if (HasServantsTags != null)
		{
			HasServants = new List<EntityType>();
			string[] hasServantsTags = HasServantsTags;
			foreach (string key in hasServantsTags)
			{
				if (GameData.Instance.ServantEntityTypeByTag.TryGetValue(key, out var value2))
				{
					HasServants.AddRange(value2);
				}
			}
		}
		if (parent.IntelligenceType != null && parent.IntelligenceType.IsPredator)
		{
			HasInterestInTriggers.Add(GameData.Instance.AllTriggerTypes["prey"], value: true);
		}
		if (GameData.Instance.AgentActionHooksByEntityType.TryGetValue(parent, out var value3))
		{
			foreach (AgentActionHook item in value3)
			{
				_ = item.Hook;
				_ = 23;
				Common.AddToMultiList(EventActions, item.Hook, GameData.Instance.AllActionSets[item.ActionSetsKey]);
			}
		}
		if (GameData.Instance.DetectedEntityHooksByEntityType.TryGetValue(parent, out var value4))
		{
			foreach (DetectEntityTypeHook item2 in value4)
			{
				EntityType key2 = GameData.Instance.AllEntityTypes[item2.DetectedEntityKey];
				Common.AddToMultiList(DetectEntityTypeEvents, key2, GameData.Instance.AllActionSets[item2.ActionSetsKey]);
			}
		}
		if (GameData.Instance.DetectedResourceHooksByEntityType.TryGetValue(parent, out var value5))
		{
			foreach (DetectResourceTypeHook item3 in value5)
			{
				ResourceType key3 = GameData.Instance.AllResourceTypes[item3.DetectedResourceKey];
				if (!DetectResourceTypeEvents.TryGetValue(key3, out var value6))
				{
					value6 = new List<ActionSets>();
					DetectResourceTypeEvents.Add(key3, value6);
				}
				value6.Add(GameData.Instance.AllActionSets[item3.ActionSetsKey]);
			}
		}
		if (IntrinsicTools != null)
		{
			IntrinsicToolTypes = new List<EntityType>();
			string[] hasServantsTags = IntrinsicTools;
			foreach (string key4 in hasServantsTags)
			{
				IntrinsicToolTypes.Add(GameData.Instance.AllEntityTypes[key4]);
			}
		}
		if (IntrinsicWeapons != null)
		{
			IntrinsicWeaponTypes = new List<EntityType>();
			string[] hasServantsTags = IntrinsicWeapons;
			foreach (string key5 in hasServantsTags)
			{
				IntrinsicWeaponTypes.Add(GameData.Instance.AllEntityTypes[key5]);
			}
		}
		if (Prey != null)
		{
			PreyTypes = new HashSet<EntityType>();
			string[] hasServantsTags = Prey;
			foreach (string key6 in hasServantsTags)
			{
				PreyTypes.Add(GameData.Instance.AllEntityTypes[key6]);
			}
		}
	}

	public static bool AttackTypeIsFunctional(UWGame.SimSide.Entities.Body.Body entityBody, AttackType attackType)
	{
		if (attackType.DependsOn == null || attackType.DependsOn.Length == 0)
		{
			return true;
		}
		return AreDependentBodyPartsFunctional(attackType.DependsOn, entityBody);
	}

	public static bool DefendActionTypeIsFunctional(UWGame.SimSide.Entities.Body.Body entityBody, DefendActionType defendType)
	{
		if (defendType.DependsOn == null || defendType.DependsOn.Length == 0)
		{
			return true;
		}
		return AreDependentBodyPartsFunctional(defendType.DependsOn, entityBody);
	}

	public bool CanMountToolsOrWeapons()
	{
		if (CanUseWeapons != true)
		{
			return CanMountTools == true;
		}
		return true;
	}

	private static bool AreDependentBodyPartsFunctional(BodyPartType[] dependentBodyParts, UWGame.SimSide.Entities.Body.Body entityBody)
	{
		foreach (BodyPartType bodyPartType in dependentBodyParts)
		{
			BodyPart bodyPart = entityBody.FindBodyPartOfType(bodyPartType);
			if (bodyPart != null && !bodyPart.IsFunctional())
			{
				return false;
			}
		}
		return true;
	}

	public void Initialize(EntityType parent)
	{
		if (MinRestTimeAfterAttackingInSeconds.HasValue && MaxRestTimeAfterAttackingInSeconds.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(MinRestTimeAfterAttackingInSeconds.Value, MaxRestTimeAfterAttackingInSeconds.Value, out RestTimeAfterAttackingMean, out RestTimeAfterAttackingStandardDeviation);
		}
		if (InterestInTriggerTypes != null)
		{
			string[] interestInTriggerTypes = InterestInTriggerTypes;
			foreach (string key in interestInTriggerTypes)
			{
				HasInterestInTriggers.Add(GameData.Instance.AllTriggerTypes[key], value: true);
			}
		}
		if (Attacks != null)
		{
			AttackTypes = new List<AttackType>();
			string[] interestInTriggerTypes = Attacks;
			foreach (string key2 in interestInTriggerTypes)
			{
				AttackTypes.Add(GameData.Instance.AllAttackTypes[key2]);
			}
		}
		if (!string.IsNullOrEmpty(ServantForEntityTypeTag))
		{
			DataLoader.AddToTagCollection(parent, ServantForEntityTypeTag, GameData.Instance.ServantEntityTypeByTag);
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
