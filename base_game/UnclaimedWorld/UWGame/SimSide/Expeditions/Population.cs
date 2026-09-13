using System;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions;

public class Population : ISnapshot
{
	private enum SpawnResult
	{
		Done,
		Processing,
		Fail
	}

	public int StartMembers;

	public int MaxMembers;

	public StringChance[] RandomMembers;

	public string[] StartMembersList;

	public float? GrowthInPercentagePerDay;

	public float? GrowthInMembersPerDay;

	private float timeInDaysElapsedSinceMemberSpawn;

	private Regulator regulator;

	public Expedition Expedition;

	public string[] SpawnSources;

	public string[] StartSpawnSources;

	public float? GrowthInSpawnSourcesPerDay;

	public float? SpawnRadius;

	private bool isWaitingForRegions;

	private int membersToSpawn;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Population()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			CreateRegulators();
		}
	}

	public static Population CreateFromPopulationData(Expedition exp, PopulationData populationData)
	{
		Population population = new Population();
		population.MaxMembers = populationData.MaxMembers;
		population.StartMembers = populationData.StartMembers;
		population.RandomMembers = populationData.RandomMembers;
		population.StartMembersList = populationData.StartMembersList;
		population.StartSpawnSources = populationData.StartSpawnSources;
		population.GrowthInMembersPerDay = populationData.GrowthInMembersPerDay;
		population.GrowthInPercentagePerDay = populationData.GrowthInPercentagePerDay;
		population.SpawnRadius = populationData.SpawnRadius;
		if (populationData.SpawnSources != null)
		{
			population.SpawnSources = new string[populationData.SpawnSources.Length];
			Array.Copy(populationData.SpawnSources, population.SpawnSources, populationData.SpawnSources.Length);
		}
		if (populationData.StartSpawnSources != null)
		{
			population.StartSpawnSources = new string[populationData.StartSpawnSources.Length];
			Array.Copy(populationData.StartSpawnSources, population.StartSpawnSources, populationData.StartSpawnSources.Length);
		}
		population.Expedition = exp;
		return population;
	}

	public void SpawnStartingPopulation()
	{
		if (StartSpawnSources != null)
		{
			string[] startSpawnSources = StartSpawnSources;
			foreach (string entityDataKey in startSpawnSources)
			{
				SpawnMember(entityDataKey, ignoreDanger: true);
			}
		}
		if (StartMembersList != null)
		{
			string[] startSpawnSources = StartMembersList;
			foreach (string entityDataKey2 in startSpawnSources)
			{
				SpawnMember(entityDataKey2, ignoreDanger: true);
			}
		}
		if (StartMembers - Expedition.Members.Count > 0)
		{
			int amount = StartMembers - Expedition.Members.Count;
			SpawnMembers(ref amount, ignoreDanger: true);
		}
	}

	public void Update(GameTime gameTime)
	{
		if (isWaitingForRegions)
		{
			if (SpawnMembers(ref membersToSpawn, ignoreDanger: false) != SpawnResult.Processing)
			{
				isWaitingForRegions = false;
			}
			return;
		}
		double millisecondsSinceLastReady = 0.0;
		if (!regulator.IsReady(ref millisecondsSinceLastReady))
		{
			return;
		}
		float num = (float)(millisecondsSinceLastReady / 1000.0 / DateAndTime.secondsPerDay);
		int amountToFill = MaxMembers - Expedition.Members.Count;
		if (GrowthInMembersPerDay.HasValue)
		{
			membersToSpawn = GetNoToSpawn(GrowthInMembersPerDay.Value, num, ref timeInDaysElapsedSinceMemberSpawn, amountToFill);
			if (SpawnMembers(ref membersToSpawn, ignoreDanger: false) == SpawnResult.Processing)
			{
				isWaitingForRegions = true;
			}
		}
	}

	public static int GetStepsFromProgress(float growthPerDay, double daysElapsed, ref float progress, int currentAmount, int minAmount, int maxAmount)
	{
		int num = 0;
		if ((growthPerDay > 0f && currentAmount < maxAmount) || (growthPerDay < 0f && currentAmount > minAmount))
		{
			progress += (float)daysElapsed * growthPerDay;
			if (growthPerDay > 0f)
			{
				if (progress > 1f)
				{
					num = (int)Math.Floor(progress);
					progress -= num;
				}
				num = Math.Min(num, maxAmount - currentAmount);
			}
			else
			{
				if (progress < 0f)
				{
					num = (int)Math.Floor(Math.Abs(progress) + 1f);
					progress += num;
				}
				num = Math.Min(num, currentAmount - minAmount);
				num *= -1;
			}
		}
		return num;
	}

	public static int GetNoToSpawn(float growthPerDay, double daysElapsed, ref float timeInDaysElapsedSinceSpawn, int amountToFill)
	{
		int result = 0;
		if (amountToFill > 0)
		{
			timeInDaysElapsedSinceSpawn += (float)daysElapsed;
			float num = 1f / growthPerDay;
			if (timeInDaysElapsedSinceSpawn > num)
			{
				result = (int)(timeInDaysElapsedSinceSpawn / num);
				timeInDaysElapsedSinceSpawn -= (float)result * num;
				result = Math.Min(result, amountToFill);
			}
		}
		return result;
	}

	private SpawnResult SpawnMembers(ref int amount, bool ignoreDanger)
	{
		int num = 0;
		for (int i = 0; i < amount; i++)
		{
			SpawnResult spawnResult;
			if (RandomMembers != null)
			{
				int stairstep;
				StringChance stairStepIndex = Common.GetStairStepIndex(RandomMembers, out stairstep, The.Sim.GameplayRandomGenerator);
				spawnResult = SpawnMember(stairStepIndex.String, ignoreDanger);
			}
			else
			{
				spawnResult = SpawnMember(ignoreDanger);
			}
			switch (spawnResult)
			{
			case SpawnResult.Processing:
				amount -= num;
				return SpawnResult.Processing;
			case SpawnResult.Fail:
				amount -= num;
				return SpawnResult.Fail;
			}
			num++;
		}
		amount = 0;
		return SpawnResult.Done;
	}

	private SpawnResult FindRandomSpawnLocation(EntityData entityData, bool ignoreDanger, out Vector3? randomLocation)
	{
		randomLocation = null;
		if (entityData.BioEntity != null && SpawnSources != null)
		{
			foreach (string item in Common.Randomize(SpawnSources.ToList(), The.Sim.GameplayRandomGenerator))
			{
				if (The.Sim.PlaySite.EntitiesByName.TryGetValue(item, out var value))
				{
					Entity entity = Entity.FindByID(value);
					if (entity != null)
					{
						randomLocation = entity.AccessPoint.Value;
						return SpawnResult.Done;
					}
				}
			}
			return SpawnResult.Fail;
		}
		RegionMap regionMap = ((!ignoreDanger) ? Expedition.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, GameData.Instance.AllEntityTypes[entityData.EntityKey], ThreatStance.Normal).Layers[SurfaceType.TransportType.Foot].RegionMap : The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap);
		float max = SpawnRadius ?? GameData.Instance.Constants.DefaultSpawnRadius;
		int num = 20;
		int num2 = 0;
		while (num2 < num)
		{
			Vector2 vector = new Vector2(Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, 1f), Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, 1f));
			vector.Normalize();
			randomLocation = Expedition.Center + (vector * Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0f, max)).ToVector3();
			randomLocation = The.Map.ClampWorldPosition(randomLocation.Value);
			float distance = 0f;
			RegionMap.Result distance2 = regionMap.GetDistance(null, MapManager.WorldPosToSubtile(Expedition.Center.Value), MapManager.WorldPosToSubtile(randomLocation.Value), ref distance, sendMessageToEntity: false, null, registerIfNotReady: false);
			num2++;
			switch (distance2)
			{
			case RegionMap.Result.OK:
				return SpawnResult.Done;
			case RegionMap.Result.Wait:
				return SpawnResult.Processing;
			}
		}
		randomLocation = Expedition.Center;
		return SpawnResult.Done;
	}

	public static float GetRandomAgeAndCasteForMapSpawn(EntityType entityType, ref CasteType caste)
	{
		if (caste == null)
		{
			caste = Common.GetStairStepIndexComputeLastEdge(entityType.BiologicalType.Castes, out var _, The.Sim.GameplayRandomGenerator);
		}
		float? num = null;
		float? num2 = null;
		float? num3 = null;
		foreach (AgeGroupType ageGroupType in caste.AgeGroupTypes)
		{
			if (!num.HasValue)
			{
				num = ((ageGroupType.AIAgeGroup != AIAgeGroup.Baby) ? new float?(0f) : new float?(ageGroupType.Edge + 0.0001f));
			}
			num3 = ageGroupType.Edge;
		}
		if (!num2.HasValue)
		{
			num2 = num3;
		}
		return MathHelper.Lerp(num.Value, num2.Value, (float)The.Sim.GameplayRandomGenerator.NextDouble("Population"));
	}

	private SpawnResult SpawnMember(bool ignoreDanger)
	{
		CasteType caste = null;
		float randomAgeAndCasteForMapSpawn = GetRandomAgeAndCasteForMapSpawn(Expedition.Allegiance.RepresentativeEntityType, ref caste);
		EntityData entityData = new EntityData
		{
			EntityKey = Expedition.Allegiance.RepresentativeEntityType.KeyName,
			BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
			{
				CasteKey = caste.KeyName,
				AgeInYears = new NormalDistribution
				{
					Mean = randomAgeAndCasteForMapSpawn
				}
			}
		};
		return SpawnMember(entityData, ignoreDanger);
	}

	private SpawnResult SpawnMember(string entityDataKey, bool ignoreDanger)
	{
		EntityData entityData = GameData.Instance.AllEntityData[entityDataKey];
		return SpawnMember(entityData, ignoreDanger);
	}

	private SpawnResult SpawnMember(EntityData entityData, bool ignoreDanger)
	{
		Vector3? randomLocation = null;
		if (!entityData.Location.HasValue)
		{
			SpawnResult spawnResult = FindRandomSpawnLocation(entityData, ignoreDanger, out randomLocation);
			if (spawnResult != SpawnResult.Done)
			{
				return spawnResult;
			}
		}
		_ = randomLocation.HasValue;
		float? age = null;
		string caste = null;
		if (entityData.BioEntity != null && !entityData.BioEntity.AgeGroup.HasValue && entityData.BioEntity.AgeInYears == null && entityData.BioEntity.CultureTemplates == null)
		{
			EntityType entityType = GameData.Instance.AllEntityTypes[entityData.EntityKey];
			CasteType caste2 = null;
			if (entityData.BioEntity.CasteKey != null)
			{
				caste2 = entityType.BiologicalType.Castes.FirstOrDefault((CasteType c) => c.KeyName == entityData.BioEntity.CasteKey);
			}
			age = GetRandomAgeAndCasteForMapSpawn(entityType, ref caste2);
			caste = caste2.KeyName;
		}
		bool flag = default(bool);
		Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out flag, null, null, offerForSale: false, isProductionOutput: false, memberOfAllegianceKey: Expedition.Allegiance.KeyName, memberOfExpeditionKey: Expedition.KeyName, locationToUse: randomLocation, owningAllegianceKey: null, owningExpeditionKey: null, siteKey: null, anchorID: null, assertContainment: true, upgradeCategory: null, age: age, caste: caste);
		if (flag)
		{
			entity.Destroy();
		}
		return SpawnResult.Done;
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.2, "Expedition");
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		StartMembers = sn.DoInt32(StartMembers);
		StartMembersList = sn.DoArray(StartMembersList);
		MaxMembers = sn.DoInt32(MaxMembers);
		timeInDaysElapsedSinceMemberSpawn = sn.DoFloat(timeInDaysElapsedSinceMemberSpawn);
		SpawnRadius = sn.DoFloatNullable(SpawnRadius);
		GrowthInMembersPerDay = sn.DoFloatNullable(GrowthInMembersPerDay);
		GrowthInPercentagePerDay = sn.DoFloatNullable(GrowthInPercentagePerDay);
		GrowthInSpawnSourcesPerDay = sn.DoFloatNullable(GrowthInSpawnSourcesPerDay);
		membersToSpawn = sn.DoInt32(membersToSpawn);
		isWaitingForRegions = sn.DoBool(isWaitingForRegions);
		RandomMembers = sn.DoArray(RandomMembers);
		SpawnSources = sn.DoArray(SpawnSources);
		StartSpawnSources = sn.DoArray(StartSpawnSources);
		sn.Ignore(Expedition);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (RandomMembers != null)
		{
			StringChance[] randomMembers = RandomMembers;
			for (int i = 0; i < randomMembers.Length; i++)
			{
				randomMembers[i].LoadPostProcess(sn);
			}
		}
		CreateRegulators();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
