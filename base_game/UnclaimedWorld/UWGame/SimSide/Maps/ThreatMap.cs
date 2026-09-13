using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class ThreatMap : InfluenceMap
{
	private enum Phase
	{
		ClearMap,
		DrawThreats,
		DrawAllies,
		CompareSectors
	}

	public ThreatStance Approach;

	private float threatStanceFactor = 1f;

	private int? maxRadius;

	private Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	private float boldnessFactor;

	private const int fallOffValueEachTile = 8;

	private Phase phase;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public EntityType EntityType { get; private set; }

	public ThreatMap()
	{
	}

	public ThreatMap(SharedKnowledge shared, int mapWidth, int mapHeight, EntityType entityType, ThreatStance approach)
		: base(mapWidth, mapHeight)
	{
		allegiance = shared.Allegiance;
		EntityType = entityType;
		Approach = approach;
		BlockingLimit = (byte)Approach;
		base.Map = new TileLayer((ushort)mapWidth, (ushort)mapHeight);
		if (Approach != ThreatStance.Bold)
		{
			base.Map.BlockingLimit = (byte)Approach;
		}
		switch (Approach)
		{
		case ThreatStance.Bold:
			threatStanceFactor = 1f;
			maxRadius = 2;
			break;
		case ThreatStance.Normal:
			threatStanceFactor = 1f;
			maxRadius = null;
			break;
		case ThreatStance.Cautious:
			threatStanceFactor = 2f;
			maxRadius = null;
			break;
		}
		boldnessFactor = 1f - EntityType.IntelligenceType.Boldness + 0.5f;
	}

	private int GetThreatValue(double dangerLevel)
	{
		if (dangerLevel > 0.0)
		{
			return Common.ClampTop((int)((double)(threatStanceFactor * boldnessFactor * 60f) * dangerLevel) + 8, 100);
		}
		return 0;
	}

	private int GetAllyStrengthValue(double allyStrengthLevel)
	{
		return -1 * ((int)((double)(boldnessFactor * 15f) * allyStrengthLevel) + 8);
	}

	private void DrawAllies()
	{
		if (Approach == ThreatStance.Bold)
		{
			return;
		}
		foreach (Entity member in allegiance.Members)
		{
			if (member.IsOnPlaySite() && member.StrengthRating.HasValue && (member.EntityType.NonLivingType == null || member.NonLivingEntity.IsCompleted()) && (member.EntityType.IntelligenceType == null || member.Intelligence.IsAwakeAndActive))
			{
				double num = GetStrengthRatio(member.StrengthRating.Value, GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating]);
				if (num > 0.0)
				{
					int allyStrengthValue = GetAllyStrengthValue(num);
					base.Map.DrawLinearInfluenceCircle(member.MapPosition.Value, allyStrengthValue, Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, -8);
				}
			}
		}
	}

	private static float GetStrengthRatio(float enemyRating, float ourStrengthRating)
	{
		float num = Common.ClampBottom(ourStrengthRating, 0.05f);
		return 0.5f * (enemyRating / num);
	}

	private void DrawThreats()
	{
		if (Approach == ThreatStance.Bold)
		{
			return;
		}
		List<EntityID> invalidEntityIDs = null;
		SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;
		foreach (KeyValuePair<EntityID, EntityID> item in sharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite)
		{
			DrawThreat(ref invalidEntityIDs, item.Key);
		}
		foreach (KeyValuePair<EntityID, EntityID> allKnownThreatSource in sharedKnowledge.PlaySiteKnowledge.AllKnownThreatSources)
		{
			DrawThreat(ref invalidEntityIDs, allKnownThreatSource.Key);
		}
		sharedKnowledge.RemoveInvalidEntityIDs(invalidEntityIDs);
	}

	private void DrawThreat(ref List<EntityID> invalidEntityIDs, EntityID entityID)
	{
		if (GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(entityID, out var data)))
		{
			Common.AddToList(ref invalidEntityIDs, entityID);
		}
		else
		{
			if (!data.StrengthRating.HasValue)
			{
				return;
			}
			double dangerLevel = GetStrengthRatio(data.StrengthRating.Value, GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating]);
			int threatValue = GetThreatValue(dangerLevel);
			if ((float)threatValue > 0f)
			{
				Vector2 pos = data.PlaySiteLocation.ToVector2();
				if (data is Entity { Locomotor: { } locomotor } entity && locomotor.IsMoving())
				{
					pos += entity.FacingNormal.ToVector2() * locomotor.GetTargetSpeed();
				}
				Point pos2 = MapManager.WorldPosToTile(pos);
				base.Map.DrawLinearInfluenceCircle(pos2, threatValue, Operation.AddToExisting, Falloff.Yes, CircleParameter.FalloffEachTile, 8, maxRadius);
			}
		}
	}

	public override string ToString()
	{
		return IDName;
	}

	public override bool DoCycle()
	{
		switch (phase)
		{
		case Phase.ClearMap:
			base.IsReady = false;
			base.Map.BeginDrawing();
			base.Map.ClearMaps();
			phase = Phase.DrawThreats;
			return false;
		case Phase.DrawThreats:
			DrawThreats();
			phase = Phase.DrawAllies;
			return false;
		case Phase.DrawAllies:
			DrawAllies();
			base.IsReady = true;
			phase = Phase.CompareSectors;
			return false;
		case Phase.CompareSectors:
			RemoveEmptySectors();
			base.Map.CompareOldAndNewSectors();
			phase = Phase.ClearMap;
			return true;
		default:
			return true;
		}
	}

	private void RemoveEmptySectors()
	{
		HashSet<Point> hashSet = new HashSet<Point>();
		hashSet.UnionWith(base.Map.GetAllSectors());
		hashSet.ExceptWith(base.Map.AffectedSectors);
		base.Map.RemoveSectors(hashSet);
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Approach = sn.DoEnum(Approach);
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		boldnessFactor = sn.DoFloat(boldnessFactor);
		maxRadius = sn.DoInt32Nullable(maxRadius);
		phase = sn.DoEnum(phase);
		threatStanceFactor = sn.DoFloat(threatStanceFactor);
		EntityType = sn.DoGameData(EntityType);
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
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
	}
}
