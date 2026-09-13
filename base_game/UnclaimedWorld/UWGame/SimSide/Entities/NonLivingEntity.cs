using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class NonLivingEntity : Component
{
	public struct EntityOrType
	{
		public EntityType EntityType;

		public Entity Entity;

		public int Level;

		public bool IsJunkPart;

		public EntityOrType(Entity part, EntityType entityType, int level, bool isJunkPart = false)
		{
			Entity = part;
			EntityType = entityType;
			IsJunkPart = isJunkPart;
			Level = level;
		}
	}

	private float? integrity;

	private float condition = 1f;

	private bool compositeConditionIsDirty = true;

	public float? ConditionChangeSpeed;

	public float MaxCondition = 1f;

	private float progress = 1f;

	private List<EntityID> snapshotParts;

	private CompositeID? partOfID;

	private IComposite partOf;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float? Integrity
	{
		get
		{
			return integrity;
		}
		private set
		{
			integrity = value;
			compositeConditionIsDirty = true;
		}
	}

	public float Condition
	{
		get
		{
			if (compositeConditionIsDirty)
			{
				ComputeConditionOfComposite();
				compositeConditionIsDirty = false;
				if (Parent.IsCompositeRoot)
				{
					Parent.UpdateFunctionality();
				}
			}
			return condition;
		}
		set
		{
			if (condition != value)
			{
				condition = value;
				if (Parent.IsLeaf && !Parent.IsRoot)
				{
					Parent.GetRoot().SetConditionDirty();
				}
			}
		}
	}

	public float Progress
	{
		get
		{
			return progress;
		}
		set
		{
			if (progress != value)
			{
				IsCompleted(progress);
				progress = value;
				IsCompleted(value);
				UpdateRenderable();
			}
		}
	}

	public List<Entity> Parts { get; set; }

	public EntityID? ParentEntityID
	{
		get
		{
			if (partOf != null && partOf is Entity entity)
			{
				return entity.ID;
			}
			return null;
		}
	}

	public CompositeID? PartOfID
	{
		get
		{
			if (partOf != null)
			{
				return partOf.ID;
			}
			return null;
		}
	}

	public IComposite PartOf
	{
		get
		{
			return partOf;
		}
		set
		{
			if (value != partOf)
			{
				partOf = value;
				if (Parent.Renderable != null)
				{
					Parent.Renderable.UpdateIsDrawnStatus();
				}
				if (partOf != null)
				{
					Parent.SetLocationPropertiesForLeaf();
					Parent.ClearContainedBy();
				}
			}
		}
	}

	public NonLivingEntity(Entity parent)
		: base(parent, GameData.Instance.Constants.UpdateIntervalForNonLivingTypes)
	{
		if (parent.EntityType.Parts != null && parent.EntityType.Parts.Count > 0)
		{
			Integrity = 1f;
		}
	}

	public NonLivingEntity()
	{
	}

	public void UpdateRenderable()
	{
		if (progress > 0f && Parent.Renderable != null)
		{
			Parent.Renderable.SetNormalRendering();
		}
	}

	protected override void UpdateRegulated(double? timeSinceLastUpdate)
	{
		bool? flag = Parent.IsStarted();
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && (Parent.IsLeaf || Parent.IsCompositeRoot))
		{
			double daysElapsed = timeSinceLastUpdate.Value * The.Sim.DateAndTime.DaysPerSecond;
			Degrade(daysElapsed);
		}
	}

	public override double? GetUpdateInterval()
	{
		if (Parent.IsOnPlaySite() && !Parent.IsDead)
		{
			return GameData.Instance.Constants.UpdateIntervalForNonLivingTypes;
		}
		return null;
	}

	public void SetConditionDirty()
	{
		compositeConditionIsDirty = true;
	}

	public void Degrade(double daysElapsed)
	{
		try
		{
			Storage storage = null;
			if (!Parent.StoredIn(out storage))
			{
				return;
			}
			Entity entity = null;
			if (storage != null)
			{
				entity = Entity.FindByID(storage.Parent);
			}
			if (Parent.EntityType.NonLivingType.FinalDegradeType != null)
			{
				float damage = ((entity == null || !Entity.IsFunctional(entity)) ? ComputeDegradeDamage(Parent, null, null, Parent.PlaySiteMapPosition, daysElapsed) : ComputeDegradeDamage(Parent, storage.StorageConditions, storage.IsPowered, Parent.PlaySiteMapPosition, daysElapsed));
				if (Parent.IsLeaf)
				{
					DoConditionDamage(damage, daysElapsed);
				}
				else if (Parent.IsCompositeRoot)
				{
					DoIntegrityDamage(damage);
					ComputeConditionOfComposite();
					Parent.UpdateFunctionality();
				}
			}
		}
		catch (Exception ex)
		{
			string message = ex.Message;
			message = ((Parent != null) ? (message + Entity.GetExceptionInformation(Parent)) : (message + "PARENT NULL\n"));
			throw new Exception(message);
		}
	}

	public static float ComputeDegradeDamage(IKnownEntityData item, StorageCondition storedIn, bool? storageIsPowered, Point mapPosition, double daysElapsed)
	{
		float temperature = The.Map.GetTile(mapPosition).Temperature;
		return ComputeDegradeDamage(item.EntityType.NonLivingType.FinalDegradeType, storedIn, item.IsWeatherProof(), temperature, The.Sim.PlaySite.PlaySite.Weather.SunIntensity, storageIsPowered, daysElapsed);
	}

	public static float ComputeDegradeDamage(DegradeType degradeType, StorageCondition storedIn, bool isWeatherProof, float temperature, float lightLevel, bool? storageIsPowered, double daysElapsed)
	{
		float light = 0f;
		float moisture = 0f;
		if (degradeType == null)
		{
			return 0f;
		}
		try
		{
			if (storedIn != null)
			{
				temperature = storedIn.GetTemperature(storageIsPowered.Value, temperature);
				moisture = storedIn.FixedMoisture ?? 0f;
				light = storedIn.FixedLightLevel ?? lightLevel;
			}
			else
			{
				GetEnvironment(isWeatherProof, ref temperature, ref light, ref moisture);
			}
			float interpolatedFunctionValue = Common.GetInterpolatedFunctionValue(temperature, degradeType.TemperatureDamage);
			float interpolatedFunctionValue2 = Common.GetInterpolatedFunctionValue(light, degradeType.LightDamage);
			float interpolatedFunctionValue3 = Common.GetInterpolatedFunctionValue(moisture, degradeType.MoistureDamage);
			return (float)(daysElapsed * (double)(interpolatedFunctionValue + interpolatedFunctionValue2 + interpolatedFunctionValue3));
		}
		catch (Exception innerException)
		{
			throw new Exception("DegradeType: " + degradeType.KeyName, innerException);
		}
	}

	private static void GetEnvironment(bool isWeatherProof, ref float temperature, ref float light, ref float moisture)
	{
		if (isWeatherProof)
		{
			temperature = StorageCondition.ComputeIsolatedTemperature(temperature);
			moisture = 0f;
			light = 0.2f * The.Sim.PlaySite.PlaySite.Weather.SunIntensity;
		}
		else
		{
			moisture = 0f;
			light = The.Sim.PlaySite.PlaySite.Weather.SunIntensity;
		}
	}

	public bool DoIntegrityDamage(float damage)
	{
		if (!Integrity.HasValue)
		{
			Integrity = 1f;
		}
		if (Common.IsZero(Parent.EntityType.NonLivingType.IntegrityWeightInCondition))
		{
			return false;
		}
		Integrity -= damage;
		Integrity = Common.ClampBottom(integrity.Value, 0f);
		if (Integrity <= 0f)
		{
			return true;
		}
		return false;
	}

	public void Repair(RepairAction repairAction, float progress)
	{
		switch (repairAction)
		{
		case RepairAction.Integrity:
			if (Parent.IsCompositeRoot)
			{
				Integrity = progress;
				Integrity = Common.ClampTop(integrity.Value, 1f);
			}
			break;
		case RepairAction.PartsCondition:
		case RepairAction.Condition:
			if (Parent.IsLeaf)
			{
				Condition = progress;
			}
			break;
		}
	}

	public bool DoConditionDamage(float damage, double? daysElapsed)
	{
		Condition -= damage;
		if (daysElapsed.HasValue && !Common.IsZero(daysElapsed.Value))
		{
			ConditionChangeSpeed = (float)((double)damage / daysElapsed).Value;
		}
		MaxCondition -= 0.3f * Parent.EntityType.NonLivingType.Repairability * damage;
		if (Common.IsLessThanOrEqual(Condition, 0f))
		{
			DestroyOrTurnToJunk();
			return true;
		}
		return false;
	}

	public float GetRepairProgress(RepairAction repairAction)
	{
		return repairAction switch
		{
			RepairAction.Integrity => Integrity.Value, 
			RepairAction.Condition => Condition, 
			RepairAction.PartsCondition => Condition, 
			_ => 1f, 
		};
	}

	public static double GetDaysLeftUntilBreakdown(double condition, float conditionChangeSpeed)
	{
		return condition / (double)conditionChangeSpeed;
	}

	private void DestroyOrTurnToJunk()
	{
		LookUpOwners.ResolveEntityOwner((IKnownEntityData)Parent, out IOwner owner);
		if (owner != null && owner.Allegiance.SharedKnowledge.GetKnownData(Parent.ID, out var _) == EntityResult.SeenDirectly)
		{
			owner.Allegiance.Statistics.AddProductionEvent(Parent.EntityType, ProductionStatistics.StatTypes.Degraded, 1);
		}
		if (Parent.EntityType.NonLivingType.DegradesTo != null)
		{
			TurnIntoJunk(owner);
		}
		else
		{
			Parent.Destroy();
		}
	}

	private void TurnIntoJunk(IOwner oldOwner)
	{
		Entity entity = new Entity(Parent.EntityType.NonLivingType.DegradesToType);
		entity.Initialize(The.Sim.PlaySite);
		entity.InitializeModelAndOnScreenFunctionality();
		entity.ComeOnline();
		if (entity.EntityType.ItemType.HasNoMaximumBulk)
		{
			entity.Bulk = Parent.Bulk;
		}
		if (Parent.PartOf == null && Parent.GetContainedBy(out Container container))
		{
			Parent.CarriedByAgent(out var carrier);
			if (container != null)
			{
				if (Parent.Contains != null)
				{
					Parent.Contains.IterateContained(delegate(Entity e)
					{
						Parent.Contains.Uncontain(e);
					});
				}
				container.SwitchEntities(Parent, entity);
			}
			carrier?.Intelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity, testForUsesMemory: true, suppressClientFeedback: false, null, carrier);
		}
		entity.PartOf = Parent.PartOf;
		if (entity.PartOf != null)
		{
			entity.PartOf.SetPart(entity);
		}
		if (!entity.ContainedBy.HasValue && entity.PartOf == null)
		{
			entity.SetPosition(Parent.PlaySiteLocation);
		}
		entity.ChangeOwnership(oldOwner);
		Parent.Destroy();
	}

	public void ComputeConditionOfComposite()
	{
		if (Parent.IsCompositeRoot)
		{
			float integrityWeightInCondition = Parent.EntityType.NonLivingType.IntegrityWeightInCondition;
			ComputeConditionOfPart();
			if (Parent.IsCompositeRoot && integrity.HasValue)
			{
				if (Common.IsLessThanOrEqual(integrity.Value, 0.0))
				{
					condition = 0f;
				}
				else
				{
					condition = integrityWeightInCondition * integrity.Value + (1f - integrityWeightInCondition) * condition;
				}
			}
		}
		compositeConditionIsDirty = false;
	}

	public float ComputeConditionOfPart()
	{
		if (Parent.Parts != null)
		{
			float num = 0f;
			foreach (Entity part in Parent.Parts)
			{
				num += part.NonLivingEntity.ComputeConditionOfPart();
			}
			condition = num / (float)Parent.Parts.Count;
			return condition;
		}
		return condition;
	}

	public bool IsCompleted()
	{
		return Common.IsGreaterThanOrEqual(Progress, 1f);
	}

	public static bool IsCompleted(float? progress)
	{
		if (progress.HasValue)
		{
			return Common.IsGreaterThanOrEqual(progress.Value, 1f);
		}
		return true;
	}

	public bool IsStarted()
	{
		return Common.IsGreaterThan(Progress, 0f);
	}

	public void CreateParts()
	{
		if (Parent.EntityType.Parts == null || Parent.EntityType.Parts.Count <= 0 || Parts != null)
		{
			return;
		}
		Parts = new List<Entity>();
		foreach (KeyValuePair<EntityType, int> part in Parent.EntityType.Parts)
		{
			for (int i = 0; i < part.Value; i++)
			{
				CreatePart(part.Key);
			}
		}
	}

	private void CreatePart(EntityType typeOfPart)
	{
		Entity entity = new Entity(typeOfPart);
		SetPart(entity);
		if (entity.EntityType.Parts != null)
		{
			entity.CreateParts();
		}
	}

	public void SetPartsOrCreateNew(List<Entity> suppliedParts)
	{
		if (Parent.EntityType.Parts == null)
		{
			return;
		}
		Parts = new List<Entity>();
		foreach (KeyValuePair<EntityType, int> part in Parent.EntityType.Parts)
		{
			for (int i = 0; i < part.Value; i++)
			{
				Entity entity = suppliedParts?.Find((Entity p) => p.EntityType == part.Key);
				if (entity != null)
				{
					SetPart(entity);
					suppliedParts.Remove(entity);
				}
				else
				{
					CreatePart(part.Key);
				}
			}
		}
	}

	public void SetPart(Entity newPart)
	{
		Parts.Add(newPart);
		newPart.PartOf = Parent;
		newPart.Site = Parent.Site;
		if (Parent.EntityType.IntelligenceType != null)
		{
			if (Parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
			{
				AddIntrinsicTool(newPart);
			}
			if (Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
			{
				AddIntrinsicWeapon(newPart);
			}
		}
	}

	public void RemovePart(Entity part, bool setPartOfToNull = true)
	{
		if (part.PartOf != Parent)
		{
			return;
		}
		Parts.Remove(part);
		if (setPartOfToNull)
		{
			part.PartOf = null;
		}
		if (Parent.EntityType.IntelligenceType != null)
		{
			if (Parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
			{
				RemoveIntrinsicTool(part);
			}
			if (Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
			{
				RemoveIntrinsicWeapon(part);
			}
		}
	}

	private void AddIntrinsicTool(Entity newPart)
	{
		if (MatchesIntrinsicToolType(newPart) && !Parent.Intelligence.IntrinsicTools.TryGetValue(newPart.EntityType, out var _))
		{
			Parent.Intelligence.IntrinsicTools.Add(newPart.EntityType, newPart.ID);
		}
		else
		{
			if (newPart.Parts == null)
			{
				return;
			}
			foreach (Entity part in newPart.Parts)
			{
				AddIntrinsicTool(part);
			}
		}
	}

	private void AddIntrinsicWeapon(Entity newPart)
	{
		if (MatchesIntrinsicWeaponType(newPart) && !Parent.Intelligence.IntrinsicWeapons.TryGetValue(newPart.EntityType, out var _))
		{
			Parent.Intelligence.IntrinsicWeapons.Add(newPart.EntityType, newPart.ID);
		}
		else
		{
			if (newPart.Parts == null)
			{
				return;
			}
			foreach (Entity part in newPart.Parts)
			{
				AddIntrinsicWeapon(part);
			}
		}
	}

	private void RemoveIntrinsicTool(Entity part)
	{
		if (MatchesIntrinsicToolType(part) && Parent.Intelligence.IntrinsicTools.TryGetValue(part.EntityType, out var value) && value == part.ID)
		{
			Parent.Intelligence.IntrinsicTools.Remove(part.EntityType);
		}
		if (part.Parts == null)
		{
			return;
		}
		foreach (Entity part2 in part.Parts)
		{
			RemoveIntrinsicTool(part2);
		}
	}

	private void RemoveIntrinsicWeapon(Entity part)
	{
		if (MatchesIntrinsicWeaponType(part) && Parent.Intelligence.IntrinsicWeapons.TryGetValue(part.EntityType, out var value) && value == part.ID)
		{
			Parent.Intelligence.IntrinsicWeapons.Remove(part.EntityType);
		}
		if (part.Parts == null)
		{
			return;
		}
		foreach (Entity part2 in part.Parts)
		{
			RemoveIntrinsicWeapon(part2);
		}
	}

	private bool MatchesIntrinsicWeaponType(Entity newPart)
	{
		if (newPart.EntityType.ItemType != null && newPart.EntityType.ItemType.WeaponType != null)
		{
			bool? isIntrinsic = newPart.EntityType.ItemType.WeaponType.IsIntrinsic;
			bool flag = true;
			if (isIntrinsic == true == flag && isIntrinsic.HasValue && Parent.EntityType.IntelligenceType.IntrinsicWeaponTypes.Contains(newPart.EntityType))
			{
				return true;
			}
		}
		return false;
	}

	private bool MatchesIntrinsicToolType(Entity newPart)
	{
		if (newPart.EntityType.ToolType != null)
		{
			ToolHandlingType? toolHandling = newPart.EntityType.ToolType.ToolHandling;
			ToolHandlingType toolHandlingType = ToolHandlingType.Intrinsic;
			if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && Parent.EntityType.IntelligenceType.IntrinsicToolTypes.Contains(newPart.EntityType))
			{
				return true;
			}
		}
		return false;
	}

	public bool NeedsRepair()
	{
		if (Parent.IsRoot)
		{
			float conditionLimit = GetConditionLimit();
			if (NeedsIntegrityRepair(conditionLimit))
			{
				return true;
			}
			List<EntityOrType> partsNeedingRepair = null;
			GatherPartsWithProblems(ref partsNeedingRepair, conditionLimit);
			if (partsNeedingRepair != null)
			{
				return true;
			}
		}
		return false;
	}

	private float GetConditionLimit()
	{
		if (Parent.EntityType.ContainerType != null && Parent.EntityType.ContainerType.ResidenceType != null)
		{
			return GameData.Instance.AIConstants.ResidenceConditionToStartRepair;
		}
		return GameData.Instance.AIConstants.OtherStructureConditionToStartRepair;
	}

	private bool NeedsIntegrityRepair(float conditionLimit)
	{
		if (Integrity < conditionLimit)
		{
			return true;
		}
		return false;
	}

	public RepairPackage ComputeBestRepairPackage()
	{
		float conditionLimit = GetConditionLimit();
		List<EntityOrType> partsNeedingRepair = null;
		GatherPartsWithProblems(ref partsNeedingRepair, conditionLimit);
		List<RepairPackage> packages = CreateRepairOptions(partsNeedingRepair, conditionLimit);
		return SelectBestRepairPackage(packages);
	}

	private RepairPackage SelectBestRepairPackage(List<RepairPackage> packages)
	{
		if (packages.Count > 0)
		{
			return packages[packages.Count - 1];
		}
		return null;
	}

	private List<RepairPackage> CreateRepairOptions(List<EntityOrType> partsNeedingRepair, float conditionLimit)
	{
		List<RepairPackage> list = new List<RepairPackage>();
		CreateIntegrityRepair(list, conditionLimit);
		if (partsNeedingRepair != null)
		{
			foreach (EntityOrType item in partsNeedingRepair)
			{
				HandleRepairOfPart(list, item);
			}
		}
		return list;
	}

	private void CreateIntegrityRepair(List<RepairPackage> repairPackages, float conditionLimit)
	{
		if (NeedsIntegrityRepair(conditionLimit) && CanRepairIntegrity(out var processType))
		{
			CreateOrAddToExistingPackage(repairPackages, null, new RepairPackageAction
			{
				RepairAction = RepairAction.Integrity,
				RepairProcess = processType
			});
		}
	}

	private void CreateOrAddToExistingPackage(List<RepairPackage> list, Entity part, RepairPackageAction partRepairAction)
	{
		if (list.Count > 0)
		{
			foreach (RepairPackage item in list)
			{
				if (!item.IncludesPartRepair(part))
				{
					item.Actions.Add(partRepairAction);
				}
			}
			return;
		}
		list.Add(new RepairPackage
		{
			Actions = new List<RepairPackageAction> { partRepairAction }
		});
	}

	private void HandleRepairOfPart(List<RepairPackage> repairPackages, EntityOrType leafPart)
	{
		List<RepairPackage> list = null;
		List<RepairPackage> list2 = null;
		List<RepairPackage> list3 = null;
		if (leafPart.Entity != null && CanReconditionPart(leafPart.Entity, out var processType))
		{
			list = CopyPackages(repairPackages);
			RepairPackageAction partRepairAction = new RepairPackageAction
			{
				RepairAction = RepairAction.PartsCondition,
				Part = leafPart.Entity,
				RepairProcess = processType
			};
			CreateOrAddToExistingPackage(list, leafPart.Entity, partRepairAction);
		}
		if (list != null)
		{
			repairPackages.AddRange(list);
		}
		if (list2 != null)
		{
			repairPackages.AddRange(list2);
		}
		if (list3 != null)
		{
			repairPackages.AddRange(list3);
		}
	}

	private static List<RepairPackage> CopyPackages(List<RepairPackage> repairPackages)
	{
		List<RepairPackage> list = new List<RepairPackage>();
		foreach (RepairPackage repairPackage in repairPackages)
		{
			RepairPackage item = new RepairPackage(repairPackage);
			list.Add(item);
		}
		return list;
	}

	private bool CanReconditionPart(Entity part, out ProcessType processType)
	{
		if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsCondition != null && Parent.EntityType.NonLivingType.EntityRepairProfile.PartsCondition.TryGetValue(part.EntityType, out processType))
		{
			return true;
		}
		processType = null;
		return false;
	}

	private bool CanRepairIntegrity(out ProcessType processType)
	{
		if (Parent.EntityType.NonLivingType.EntityRepairProfile.Integrity != null)
		{
			processType = Parent.EntityType.NonLivingType.EntityRepairProfile.Integrity;
			return true;
		}
		processType = null;
		return false;
	}

	private bool CanRecondition(Entity part, out ProcessType processType)
	{
		if (part.EntityType.NonLivingType.EntityRepairProfile.Condition != null)
		{
			processType = part.EntityType.NonLivingType.EntityRepairProfile.Condition;
			return true;
		}
		processType = null;
		return false;
	}

	private bool CanReplace(Entity part, out ProcessType processType)
	{
		if (Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement != null && Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement.TryGetValue(part.EntityType, out processType))
		{
			return true;
		}
		processType = null;
		return false;
	}

	private bool CanReplaceParentPart(Entity part, ref List<Entity> parentParts)
	{
		Entity entity = part.PartOf as Entity;
		while (entity != null && Parent.EntityType.NonLivingType.EntityRepairProfile.PartsReplacement.ContainsKey(entity.EntityType))
		{
			Common.AddToList(ref parentParts, entity);
			entity = entity.PartOf as Entity;
		}
		return true;
	}

	private void GatherMissingParts(ref List<EntityOrType> partsNeedingRepair, int level)
	{
		foreach (KeyValuePair<EntityType, int> part in Parent.EntityType.Parts)
		{
			int num = part.Value - Parts.Count;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					Common.AddToList(ref partsNeedingRepair, new EntityOrType(null, part.Key, level));
				}
			}
			if (part.Key.Parts != null)
			{
				GatherMissingParts(ref partsNeedingRepair, level + 1);
			}
		}
	}

	private void GatherPartsWithProblems(ref List<EntityOrType> partsNeedingRepair, float conditionLimit)
	{
		GatherPartsNeedingRepair(ref partsNeedingRepair, conditionLimit, 0);
	}

	private void GatherPartsNeedingRepair(ref List<EntityOrType> partsNeedingRepair, float conditionLimit, int level)
	{
		if (Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			if (part.IsLeaf)
			{
				if (part.Condition < (double)conditionLimit)
				{
					Common.AddToList(ref partsNeedingRepair, new EntityOrType(part, part.EntityType, level));
				}
			}
			else
			{
				part.NonLivingEntity.GatherPartsNeedingRepair(ref partsNeedingRepair, conditionLimit, level + 1);
			}
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		condition = sn.DoFloat(condition);
		compositeConditionIsDirty = sn.DoBool(compositeConditionIsDirty);
		ConditionChangeSpeed = sn.DoFloatNullable(ConditionChangeSpeed);
		MaxCondition = sn.DoFloat(MaxCondition);
		progress = sn.DoFloat(progress);
		integrity = sn.DoFloatNullable(integrity);
		if (partOf != null)
		{
			partOfID = partOf.ID;
		}
		else
		{
			partOfID = null;
		}
		partOfID = sn.DoEnumNullable(partOfID);
		if (Parts != null)
		{
			snapshotParts = Parts.Select((Entity p) => p.EntityID).ToList();
		}
		snapshotParts = sn.DoList(snapshotParts);
		sn.Ignore(partOf);
		sn.Ignore(Parts);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (partOfID.HasValue)
		{
			partOf = LookUpIComposites.FindByID(partOfID.Value);
		}
		if (snapshotParts != null)
		{
			Parts = snapshotParts.Select((EntityID p) => Entity.FindByID(p)).ToList();
			snapshotParts = null;
		}
	}
}
