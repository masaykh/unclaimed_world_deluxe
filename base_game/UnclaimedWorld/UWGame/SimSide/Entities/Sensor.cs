using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Sensor : Component
{
	public enum TileStatus
	{
		Seen,
		Unseen
	}

	public bool IsActive;

	private int? oldSensorRadiusInTiles;

	public HashSet<TerrainTile> tilesCurrentlySeen = new HashSet<TerrainTile>();

	private List<TerrainTileID> snapshotCurrentlySeen;

	private List<TerrainTile> tilesToUnsee = new List<TerrainTile>();

	private List<TerrainTile> tilesToSee = new List<TerrainTile>();

	private List<IDetectable> allDetectables = new List<IDetectable>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public void NotifyPartIsBroken()
	{
		IsActive = false;
	}

	public override double? GetUpdateInterval()
	{
		if (!Parent.IsDead && Parent.IsCompleted() && Parent.IsOnPlaySite())
		{
			return 1.0 / (double)GameData.Instance.Constants.DetectionUpdatesPerSecond;
		}
		return null;
	}

	public Sensor()
	{
	}

	public Sensor(Entity parent)
		: base(parent, 1.0 / (double)GameData.Instance.Constants.DetectionUpdatesPerSecond)
	{
	}

	public void UnseeTilesInRange()
	{
		foreach (TerrainTile item in tilesCurrentlySeen)
		{
			item.ChangeTileSeenBy(Parent, TileStatus.Unseen);
		}
	}

	public List<TerrainTile> UpdateTilesSeenBySensor(Point newCenterTilePos)
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		int visionRangeInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
		foreach (TerrainTile item2 in tilesCurrentlySeen)
		{
			if (Common.DistanceOctile(newCenterTilePos, new Point(item2.X, item2.Y)) > (float)visionRangeInTiles)
			{
				tilesToUnsee.Add(item2);
			}
		}
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(tileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(tileMap);
		int num = Math.Max(0, newCenterTilePos.X - visionRangeInTiles);
		int num2 = Math.Max(0, newCenterTilePos.Y - visionRangeInTiles);
		int num3 = Math.Min(jaggedArrayWidth - 1, newCenterTilePos.X + visionRangeInTiles);
		int num4 = Math.Min(jaggedArrayHeight - 1, newCenterTilePos.Y + visionRangeInTiles);
		for (int i = num; i <= num3; i++)
		{
			TerrainTile[] array = tileMap[i];
			for (int j = num2; j <= num4; j++)
			{
				TerrainTile item = array[j];
				if (!tilesCurrentlySeen.Contains(item) && Common.DistanceOctile(newCenterTilePos, new Point(i, j)) <= (float)visionRangeInTiles)
				{
					tilesToSee.Add(array[j]);
				}
			}
		}
		foreach (TerrainTile item3 in tilesToSee)
		{
			SeeTile(item3);
		}
		foreach (TerrainTile item4 in tilesToUnsee)
		{
			UnseeTile(item4);
		}
		tilesToSee.Clear();
		tilesToUnsee.Clear();
		return null;
	}

	private void SeeTile(TerrainTile tile)
	{
		tile.ChangeTileSeenBy(Parent, TileStatus.Seen);
		tilesCurrentlySeen.Add(tile);
	}

	private void UnseeTile(TerrainTile tile)
	{
		tile.ChangeTileSeenBy(Parent, TileStatus.Unseen);
		tilesCurrentlySeen.Remove(tile);
	}

	public void SeeTilesInRange(Point centerTilePos)
	{
		if (!The.Map.TileIsOnMap(centerTilePos))
		{
			return;
		}
		int visionRangeInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
		TerrainTile[][] tileMap = The.Map.TileMap;
		int num = Math.Max(0, centerTilePos.X - visionRangeInTiles);
		int num2 = Math.Max(0, centerTilePos.Y - visionRangeInTiles);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(tileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(tileMap);
		int num3 = Math.Min(jaggedArrayWidth - 1, centerTilePos.X + visionRangeInTiles);
		int num4 = Math.Min(jaggedArrayHeight - 1, centerTilePos.Y + visionRangeInTiles);
		for (int i = num; i <= num3; i++)
		{
			TerrainTile[] array = tileMap[i];
			for (int j = num2; j <= num4; j++)
			{
				if (Common.DistanceOctile(centerTilePos, new Point(i, j)) <= (float)visionRangeInTiles)
				{
					SeeTile(array[j]);
				}
			}
		}
		oldSensorRadiusInTiles = visionRangeInTiles;
	}

	protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
	{
		int visionRangeInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
		if (visionRangeInTiles != oldSensorRadiusInTiles)
		{
			UpdateTilesSeenBySensor(Parent.MapPosition.Value);
			oldSensorRadiusInTiles = visionRangeInTiles;
		}
		RollToDetect();
	}

	public void RollToDetect()
	{
		Point value = Parent.MapPosition.Value;
		int visionRangeInTiles = Parent.GetVisionRangeInTiles(The.Sim.DateAndTime.LightLevel);
		TerrainTile[][] tileMap = The.Map.TileMap;
		Math.Max(0, value.X - visionRangeInTiles);
		Math.Max(0, value.Y - visionRangeInTiles);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(tileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(tileMap);
		Math.Min(jaggedArrayWidth - 1, value.X + visionRangeInTiles);
		Math.Min(jaggedArrayHeight - 1, value.Y + visionRangeInTiles);
		allDetectables.Clear();
		SharedKnowledge sharedKnowledge = Parent.Intelligence.Allegiance.SharedKnowledge;
		foreach (TerrainTile item in tilesCurrentlySeen)
		{
			item.GetDetectablesOnTile(allDetectables);
		}
		RollToDetect(Parent, sharedKnowledge, allDetectables);
	}

	public void RollToDetect(Entity detectingEntity, SharedKnowledge sharedKnowledge, List<IDetectable> allDetectables, bool suppressClientFeedbackAndEvents = false, bool detectAllWhichHasMinimumRange = false, bool unseeAfterDetecting = false, bool rollToDetectHiddenEntities = true, bool doAssert = true)
	{
		if (allDetectables == null)
		{
			return;
		}
		foreach (IDetectable allDetectable in allDetectables)
		{
			bool flag = false;
			if (allDetectable == detectingEntity)
			{
				continue;
			}
			DetectionFactor resourceDetectionFactor = null;
			if (sharedKnowledge == null)
			{
				continue;
			}
			if (sharedKnowledge.UsesMemory(allDetectable) && !sharedKnowledge.AllDetectedEntities.Contains(allDetectable.ID))
			{
				if (allDetectable.RequiresRollToDetect())
				{
					if (rollToDetectHiddenEntities && RollToDetect(allDetectable, sharedKnowledge, out resourceDetectionFactor, detectAllWhichHasMinimumRange))
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				sharedKnowledge.SeeDetectable(allDetectable, suppressClientFeedbackAndEvents, resourceDetectionFactor, detectingEntity, doAssert);
				if (unseeAfterDetecting && allDetectable is Entity entity)
				{
					sharedKnowledge.UnSeeEntity(entity);
				}
			}
		}
	}

	public static void HandleEntityTypeDetectionEvents(Entity detectingEntity, Entity detectedEntity)
	{
		Dictionary<EntityType, List<ActionSets>> detectEntityTypeEvents = detectingEntity.EntityType.IntelligenceType.DetectEntityTypeEvents;
		if (detectEntityTypeEvents == null || detectEntityTypeEvents.Count <= 0 || !detectEntityTypeEvents.TryGetValue(detectedEntity.EntityType, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			ActionSets actionSets = value[num];
			actionSets.Fire(detectingEntity, detectedEntity.EntityID, null, out var isExpired);
			if (isExpired)
			{
				value.Remove(actionSets);
				if (value.Count == 0)
				{
					detectEntityTypeEvents.Remove(detectedEntity.EntityType);
				}
			}
		}
	}

	public static void HandleResourceDetectionEvents(Entity detectingEntity, ResourceType resourceType)
	{
		Dictionary<ResourceType, List<ActionSets>> detectResourceTypeEvents = detectingEntity.EntityType.IntelligenceType.DetectResourceTypeEvents;
		if (detectResourceTypeEvents == null || detectResourceTypeEvents.Count <= 0 || !detectResourceTypeEvents.TryGetValue(resourceType, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			ActionSets actionSets = value[num];
			actionSets.Fire(detectingEntity, null, null, out var isExpired);
			if (isExpired)
			{
				value.Remove(actionSets);
				if (value.Count == 0)
				{
					detectResourceTypeEvents.Remove(resourceType);
				}
			}
		}
	}

	public bool RollToDetect(IDetectable detectable, SharedKnowledge sharedKnowledge, out DetectionFactor resourceDetectionFactor, bool detectAllWhichHasMinimumRange)
	{
		resourceDetectionFactor = null;
		float num = Common.DistanceOctile(Parent.PlaySiteLocation, detectable.Location);
		DetectionType detectionType = Parent.EntityType.SensorType.DetectionType;
		float num2 = 1f;
		bool requiresExamineAction = false;
		float num3 = 0f;
		float num4;
		float num5;
		float value;
		if (detectable.ResourceType != null)
		{
			num4 = 0f;
			if (detectionType.DetectFactors.TryGetValue(detectable.ResourceType, out resourceDetectionFactor))
			{
				value = resourceDetectionFactor.Value;
				num5 = resourceDetectionFactor.DistanceToAlwaysDetect ?? GameData.Instance.Constants.DefaultDistanceToAlwaysDetectResources;
				requiresExamineAction = resourceDetectionFactor.RequiresExamineAction;
				if (resourceDetectionFactor.SkillToUse != null)
				{
					num2 = Parent.Intelligence.GetSkillValue(resourceDetectionFactor.SkillToUse);
				}
				value = Parent.GetEffect(AffectsNumbers.Detection, value, resourceDetectionFactor.TypeKey, resourceDetectionFactor.TypeTag);
			}
			else if (detectionType.DetectionDisabled)
			{
				num5 = 0f;
				value = 0f;
			}
			else
			{
				num5 = GameData.Instance.Constants.DefaultDistanceToAlwaysDetectResources;
				value = 1f;
			}
		}
		else
		{
			Entity entity = detectable as Entity;
			if (sharedKnowledge.TryGetMemoryFacts(entity.EntityID, out var data, out var _))
			{
				if (data != null)
				{
					if (data.EntityType.LocomotorType == null)
					{
						return true;
					}
					num3 = ((!(data.Location == entity.Location)) ? 0.2f : 1f);
				}
				else
				{
					num3 = 0.2f;
				}
			}
			num4 = entity.GetAvoidDetectionFactor();
			if (detectionType.DetectFactors.TryGetValue(detectable.EntityType, out var value2))
			{
				value = value2.Value;
				num5 = value2.DistanceToAlwaysDetect ?? GameData.Instance.Constants.DefaultDistanceToAlwaysDetectHiddenEntities;
				requiresExamineAction = value2.RequiresExamineAction;
				if (value2.SkillToUse != null)
				{
					num2 = Parent.Intelligence.GetSkillValue(value2.SkillToUse);
				}
			}
			else if (detectionType.DetectionDisabled)
			{
				num5 = 0f;
				value = 0f;
			}
			else
			{
				num5 = GameData.Instance.Constants.DefaultDistanceToAlwaysDetectHiddenEntities;
				value = 1f;
			}
		}
		if (num5 > 0f && (detectAllWhichHasMinimumRange || num < num5))
		{
			return true;
		}
		float num6 = Parent.GetDaySensorRange() - num5;
		float num7 = num - num5;
		float num8;
		if (detectable.ResourceType != null)
		{
			num8 = MathHelper.Lerp(1f, 0f, num7 / num6);
			num8 *= num8;
		}
		else
		{
			num8 = MathHelper.Lerp(0f, 1f, num7 / num6);
			num8 = 1f - num8 * num8;
		}
		num8 = Common.Clamp(num8, 0f, 1f);
		float detectionFactor = Parent.GetDetectionFactor(detectable, requiresExamineAction);
		_ = 0f;
		float num9 = num8 * (1f - num4) * detectionFactor * num2 * value + GameData.Instance.AIConstants.DetectionBonusForRememberedEntitiesInSameSpot * num3;
		return The.Sim.GameplayRandomGenerator.NextDouble("Sensor") <= (double)num9;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		tilesCurrentlySeen = new HashSet<TerrainTile>();
		foreach (TerrainTileID item in snapshotCurrentlySeen)
		{
			tilesCurrentlySeen.Add(LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(item));
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		IsActive = sn.DoBool(IsActive);
		oldSensorRadiusInTiles = sn.DoInt32Nullable(oldSensorRadiusInTiles);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotCurrentlySeen = tilesCurrentlySeen.Select((TerrainTile t) => t.ID).ToList();
		}
		snapshotCurrentlySeen = sn.DoList(snapshotCurrentlySeen);
		sn.Ignore(allDetectables);
		sn.Ignore(tilesToSee);
		sn.Ignore(tilesToUnsee);
		sn.Ignore(tilesCurrentlySeen);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
