using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.Steam;

namespace UWGame.SimSide.Allegiances.Statistics;

public class GroupStatistics : ISnapshot
{
	public Dictionary<RatingTypes, Rating> Ratings;

	public PopulationStatistics PopulationStatistics;

	public ProductionStatistics ProductionStatistics;

	public KillStatistics KillStatistics;

	public NutrientStatistics NutrientStatistics;

	public CanIterateEntitiesID CanIterateEntitiesID;

	public EntityType RepresentativeEntityType;

	private Regulator achievementsRegulator;

	private const float securityWeight = 1f;

	private const float comfortWeight = 1f;

	private const float foodWeight = 1f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public event Action RatingsChanged;

	public GroupStatistics()
	{
	}

	public GroupStatistics(ICanIterateEntities group, EntityType representativeEntityType)
	{
		CanIterateEntitiesID = group.ID;
		RepresentativeEntityType = representativeEntityType;
		Common.AddToDictionary(ref Ratings, RatingTypes.Food, new FoodStatisticsForAllegiance(this));
		Common.AddToDictionary(ref Ratings, RatingTypes.Comfort, new ComfortStatisticsForAllegiance(this));
		Common.AddToDictionary(ref Ratings, RatingTypes.Security, new SecurityStatisticsForAllegiance(this));
		if (GatherStatisticsForDisplayOnly(group.GetAllegiance))
		{
			PopulationStatistics = new PopulationStatistics();
			ProductionStatistics = new ProductionStatistics();
			NutrientStatistics = new NutrientStatistics();
			KillStatistics = new KillStatistics();
		}
		CreateRegulators();
	}

	public static bool GatherStatisticsForDisplayOnly(Allegiance allegiance)
	{
		return allegiance.AllegianceType == AllegianceType.Player;
	}

	public GroupStatistics(CanIterateEntitiesID groupID, Allegiance parentAllegiance)
	{
		CanIterateEntitiesID = groupID;
		RepresentativeEntityType = parentAllegiance.RepresentativeEntityType;
		Common.AddToDictionary(ref Ratings, RatingTypes.Food, new FoodStatisticsForMembers(this, parentAllegiance));
		Common.AddToDictionary(ref Ratings, RatingTypes.Comfort, new ComfortStatisticsForMembers(this, parentAllegiance));
		Common.AddToDictionary(ref Ratings, RatingTypes.Security, new SecurityStatisticsForMembers(this, parentAllegiance));
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		achievementsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GroupStatistics");
	}

	public static GroupStatistics CreateFromStatsData(StatsData statsData, ICanIterateEntities group, EntityType representativeEntityType)
	{
		GroupStatistics groupStatistics = new GroupStatistics(group, representativeEntityType);
		AddSharedRating(groupStatistics, RatingTypes.Food, statsData.GetRating(RatingTypes.Food));
		AddSharedRating(groupStatistics, RatingTypes.Comfort, statsData.GetRating(RatingTypes.Comfort));
		AddSharedRating(groupStatistics, RatingTypes.Security, statsData.GetRating(RatingTypes.Security));
		return groupStatistics;
	}

	private static void AddSharedRating(GroupStatistics stats, RatingTypes ratingType, float value)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		Rating rating = stats.Ratings[ratingType];
		rating.Ratings.Add(new DataPoint<float>
		{
			Time = currentTimeDateYear,
			Value = value
		});
		rating.AddSharedRating(value);
	}

	public void ChangeAllegiance(Allegiance newAllegiance)
	{
		foreach (KeyValuePair<RatingTypes, Rating> rating in Ratings)
		{
			rating.Value.ChangeAllegiance(newAllegiance);
		}
	}

	public void UpdateOnce()
	{
		foreach (KeyValuePair<RatingTypes, Rating> rating in Ratings)
		{
			rating.Value.ToggleComposeBreakdown(compose: true);
			rating.Value.GatherPolledData();
			rating.Value.ToggleComposeBreakdown(compose: false);
		}
	}

	public void Update(GameTime gameTime)
	{
		foreach (KeyValuePair<RatingTypes, Rating> rating in Ratings)
		{
			rating.Value.Update(gameTime);
		}
		if (this.RatingsChanged != null)
		{
			this.RatingsChanged();
		}
	}

	public void CheckAchievements()
	{
		if (!achievementsRegulator.IsReady())
		{
			return;
		}
		Allegiance allegiance = null;
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(CanIterateEntitiesID);
		if (canIterateEntities == null)
		{
			return;
		}
		allegiance = canIterateEntities.GetAllegiance;
		StatsAndAchievements statsAndAchievements = The.Sim.Controller.StatsAndAchievements;
		switch (The.Sim.StartGameParams.GetRGScenario())
		{
		case StartGameParams.RGScenario.TheClayPit:
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.claypitRatings) && allegiance.IndependentMembers.Count >= 15)
			{
				decimal? tradeCredits = allegiance.TradeCredits;
				decimal num = 200;
				if (tradeCredits.GetValueOrDefault() >= num && tradeCredits.HasValue && Ratings[RatingTypes.Food].GetLatestValue() >= 0.25f && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.25f && Ratings[RatingTypes.Security].GetLatestValue() >= 0.25f)
				{
					statsAndAchievements.UnlockAchievement(AchievementID.claypitRatings);
				}
			}
			break;
		case StartGameParams.RGScenario.FieldsOfTauCeti:
		{
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.fieldsOfTauCetiTrader) && (The.Sim.GetDifficultyKey() == "hard" || The.Sim.GetDifficultyKey() == "normal") && allegiance.IndependentMembers.Count >= 12 && Ratings[RatingTypes.Food].GetLatestValue() >= 0.35f && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.35f && Ratings[RatingTypes.Security].GetLatestValue() >= 0.35f)
			{
				Dictionary<EntityType, int> produced2 = ProductionStatistics.Totals[ProductionStatistics.StatTypes.Produced];
				if (!HasProduced("structure:smallPlot", produced2) && !HasProduced("structure:largePlot", produced2) && !HasProduced("structure:fishTrapCreekNet", produced2) && !HasProduced("structure:fishTrapCoast", produced2) && !HasProduced("structure:fishTrapShoreHoopNet", produced2) && !HasProduced("structure:fishTrapShoreBasket", produced2) && !HasProduced("structure:greenhouse", produced2) && !HasProduced("structure:improvisedGreenhouse", produced2))
				{
					statsAndAchievements.UnlockAchievement(AchievementID.fieldsOfTauCetiTrader);
				}
			}
			if (statsAndAchievements.IsAchievementUnlocked(AchievementID.hunterGatherers) || !(The.Sim.GetDifficultyKey() == "hard") || allegiance.IndependentMembers.Count < 5 || !(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays >= 24.0))
			{
				break;
			}
			bool flag = false;
			foreach (Expedition expedition in allegiance.Expeditions)
			{
				if (expedition.Policy.TierIsUnlocked(RatingTypes.Security, GameData.Instance.AllTierTypes["basic"]) || expedition.Policy.TierIsUnlocked(RatingTypes.Food, GameData.Instance.AllTierTypes["basic"]) || expedition.Policy.TierIsUnlocked(RatingTypes.Comfort, GameData.Instance.AllTierTypes["basic"]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				statsAndAchievements.UnlockAchievement(AchievementID.hunterGatherers);
			}
			break;
		}
		case StartGameParams.RGScenario.MuckrootMiningCamp:
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.muckrootNoRefining))
			{
				if (allegiance.IndependentMembers.Count >= 15 && Ratings[RatingTypes.Food].GetLatestValue() >= 0.45f && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.45f && Ratings[RatingTypes.Security].GetLatestValue() >= 0.45f)
				{
					Dictionary<EntityType, int> produced = ProductionStatistics.Totals[ProductionStatistics.StatTypes.Produced];
					if (!HasProduced("item:scandium", produced) && !HasProduced("item:terbium", produced))
					{
						statsAndAchievements.UnlockAchievement(AchievementID.muckrootNoRefining);
					}
				}
			}
			else if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.muckrootMining) && allegiance.IndependentMembers.Count >= 20)
			{
				decimal? tradeCredits = allegiance.TradeCredits;
				decimal num = 1000;
				if (tradeCredits.GetValueOrDefault() >= num && tradeCredits.HasValue && Ratings[RatingTypes.Food].GetLatestValue() >= 0.45f && Ratings[RatingTypes.Comfort].GetLatestValue() >= 0.45f && Ratings[RatingTypes.Security].GetLatestValue() >= 0.45f)
				{
					statsAndAchievements.UnlockAchievement(AchievementID.muckrootMining);
				}
			}
			break;
		}
	}

	private bool HasProduced(string entityTypeKey, Dictionary<EntityType, int> produced)
	{
		if (GameData.Instance.AllEntityTypes.TryGetValue(entityTypeKey, out var value) && produced.TryGetValue(value, out var value2))
		{
			return value2 > 0;
		}
		return false;
	}

	public void NotifyPopulationChanged(int members)
	{
		if (PopulationStatistics != null)
		{
			PopulationStatistics.SetPopulation(members);
		}
	}

	public void AddViolentEvent(Entity victim, string description, ViolentEventType type)
	{
		Ratings.TryGetValue(RatingTypes.Security, out var value);
		((SecurityStatistics)value).AddViolentEvent(victim, description, type);
	}

	public void AddProductionEvent(EntityType entityType, ProductionStatistics.StatTypes statType, int amount)
	{
		if (ProductionStatistics != null)
		{
			ProductionStatistics.AddEvent(statType, entityType, amount);
		}
	}

	public void AddProductivityEvent(EntityType entityType, IKnownProcess processData)
	{
		if (ProductionStatistics != null)
		{
			ProductionStatistics.AddProductivityEvent(entityType, processData);
		}
	}

	public void AddNutrientEvent(FoodNutrientType needType, NutrientStatistics.StatTypes statType, float amount)
	{
		if (NutrientStatistics != null)
		{
			NutrientStatistics.AddEvent(statType, needType, amount);
		}
	}

	public void AddKillEvent(EntityType victim)
	{
		if (KillStatistics != null)
		{
			KillStatistics.AddKillEvent(victim);
		}
	}

	public static bool GatherStatisticsForEntity(Entity entity)
	{
		return entity.Intelligence.IsIndependent();
	}

	public Rating GetStatisticByKey(RatingTypes statType)
	{
		Ratings.TryGetValue(statType, out var value);
		return value;
	}

	public float GetRating(RatingTypes ratingType)
	{
		return GetStatisticByKey(ratingType).GetLatestValue();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Rating value in Ratings.Values)
		{
			stringBuilder.Append(value.ToString());
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}

	public double GetOverallRating()
	{
		int num = 3;
		Ratings.TryGetValue(RatingTypes.Food, out var value);
		double num2 = value.GetLatestValue();
		Ratings.TryGetValue(RatingTypes.Security, out value);
		double num3 = value.GetLatestValue();
		Ratings.TryGetValue(RatingTypes.Comfort, out value);
		double num4 = value.GetLatestValue();
		return (num3 * 1.0 + num4 * 1.0 + num2 * 1.0) / (double)num;
	}

	internal void RecordDeathOrEmigration(bool isDestroyed)
	{
		if (PopulationStatistics != null)
		{
			if (isDestroyed)
			{
				PopulationStatistics.RecordDeath();
			}
			else
			{
				PopulationStatistics.RecordEmigration();
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Ratings = sn.DoDictionary(Ratings);
		CanIterateEntitiesID = sn.DoEnum(CanIterateEntitiesID);
		PopulationStatistics = (PopulationStatistics)sn.DoISnapshot(PopulationStatistics);
		ProductionStatistics = (ProductionStatistics)sn.DoISnapshot(ProductionStatistics);
		NutrientStatistics = (NutrientStatistics)sn.DoISnapshot(NutrientStatistics);
		KillStatistics = (KillStatistics)sn.DoISnapshot(KillStatistics);
		RepresentativeEntityType = sn.DoGameData(RepresentativeEntityType);
		sn.Ignore(this.RatingsChanged);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (PopulationStatistics != null)
		{
			PopulationStatistics.LoadPostProcess(sn);
		}
		if (ProductionStatistics != null)
		{
			ProductionStatistics.LoadPostProcess(sn);
		}
		if (NutrientStatistics != null)
		{
			NutrientStatistics.LoadPostProcess(sn);
		}
		if (KillStatistics != null)
		{
			KillStatistics.LoadPostProcess(sn);
		}
		foreach (KeyValuePair<RatingTypes, Rating> rating in Ratings)
		{
			rating.Value.LoadPostProcess(sn);
			rating.Value.Parent = this;
		}
		CreateRegulators();
	}
}
