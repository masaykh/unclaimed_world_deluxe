using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalSearchArea : CompositeGoal
{
	private Zone zone;

	private ZoneID snapshotZone;

	private List<Vector2> locationsStillToVisit;

	private float lowestDistance;

	private Vector2 closestLocation;

	private int currentDestinationIndex;

	private float distance;

	private bool isStealthy;

	private bool examine;

	private int numberOfVisitedLocations;

	private float sampleDistance = 100f;

	private Regulator setSneakingRegulator;

	private bool waitingForDistance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalSearchArea(Entity owner, Zone mapArea, List<EntityGroupID> ownersOfVehicles, bool isStealthy, bool examine, float sampleDistance = 100f)
		: base(owner)
	{
		base.ownersOfVehicles = ownersOfVehicles;
		this.sampleDistance = sampleDistance;
		zone = mapArea;
		lowestDistance = float.MaxValue;
		this.isStealthy = isStealthy;
		this.examine = examine;
	}

	public GoalSearchArea()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (isStealthy)
		{
			UpdateSneaking();
		}
		ComputeLocationsToVisit();
		RemoveAllSubgoals();
	}

	private bool FindClosestLocation()
	{
		Point fromSubtile = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
		RegionMap regionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap;
		for (int i = 0; i < locationsStillToVisit.Count; i++)
		{
			switch (regionMap.GetDistance(entity, fromSubtile, MapManager.WorldPosToSubtile(locationsStillToVisit[i]), ref distance))
			{
			case RegionMap.Result.OK:
				if (distance < lowestDistance)
				{
					lowestDistance = distance;
					closestLocation = locationsStillToVisit[i];
					currentDestinationIndex = i;
				}
				break;
			case RegionMap.Result.NoAccess:
				locationsStillToVisit.RemoveAt(i);
				i--;
				break;
			case RegionMap.Result.Wait:
				AddSubgoal(new GoalWait(entity));
				waitingForDistance = true;
				return false;
			}
		}
		return true;
	}

	private void ComputeLocationsToVisit()
	{
		int num = 0;
		while ((locationsStillToVisit == null || locationsStillToVisit.Count == 0) && num < 3)
		{
			Rectangle? boundingBoxInTiles = zone.MapArea.GetBoundingBoxInTiles();
			if (boundingBoxInTiles.HasValue)
			{
				locationsStillToVisit = UniformPoissonDiskSampler.SampleRectangle(MapManager.TileToWorldPos(boundingBoxInTiles.Value.Location).ToVector2(), MapManager.TileToWorldPos(new Point(boundingBoxInTiles.Value.Right - 1, boundingBoxInTiles.Value.Bottom - 1)).ToVector2(), sampleDistance);
				for (int i = 0; i < locationsStillToVisit.Count; i++)
				{
					Vector2 pos = locationsStillToVisit[i];
					bool remove = true;
					Point currentTilePos = MapManager.WorldPosToTile(pos);
					zone.MapArea.IterateAreaBreakOnTrue(delegate(TerrainTile tile)
					{
						if (currentTilePos.X == tile.X && currentTilePos.Y == tile.Y)
						{
							remove = false;
							return true;
						}
						return false;
					});
					if (remove)
					{
						locationsStillToVisit.RemoveAt(i);
					}
				}
			}
			num++;
		}
		if (locationsStillToVisit == null || locationsStillToVisit.Count == 0)
		{
			base.Status = Status.Completed;
		}
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction || examine)
		{
			return DetectionFactor.DetectGood;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (!requiresExamineAction || examine)
		{
			return DetectionFactor.DetectGood;
		}
		return DetectionFactor.CannotDetect;
	}

	protected override bool ArePreconditionsOK()
	{
		return true;
	}

	public override string GetStatus()
	{
		return "Searching area";
	}

	private bool LocationsAreLeft()
	{
		if (locationsStillToVisit.Count == 0)
		{
			return false;
		}
		return true;
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		setSneakingRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GoalSearchAreaSneaking");
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!ArePreconditionsOK())
		{
			base.Status = Status.Failed;
			return;
		}
		if (isStealthy && setSneakingRegulator.IsReady())
		{
			UpdateSneaking();
		}
		if (Subgoals.Count == 0)
		{
			if (!FindClosestLocation())
			{
				base.Status = Status.Active;
				return;
			}
			if (!LocationsAreLeft())
			{
				if (numberOfVisitedLocations > 0)
				{
					base.Status = Status.Completed;
				}
				else
				{
					base.Status = Status.Failed;
				}
			}
			else
			{
				CheckToStopAndLookAround();
				AddSubgoal(new GoalMoveToPosition(entity, closestLocation.ToVector3(), ownersOfVehicles));
			}
		}
		if (base.Status == Status.Failed)
		{
			return;
		}
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Completed)
		{
			if (currentDestinationIndex < locationsStillToVisit.Count)
			{
				locationsStillToVisit.RemoveAt(currentDestinationIndex);
			}
			lowestDistance = float.MaxValue;
			numberOfVisitedLocations++;
			if (locationsStillToVisit.Count != 0)
			{
				base.Status = Status.Active;
			}
		}
	}

	private void UpdateSneaking()
	{
		if (GoalHunt.OtherActiveAllegianceMembersAreStandingNearby(entity, entityIntelligence, 90f))
		{
			entity.SetSneaking(value: false);
		}
		else
		{
			entity.SetSneaking(value: true);
		}
	}

	private void CheckToStopAndLookAround()
	{
		Vector2 value = closestLocation - entity.PlaySiteLocation.ToVector2();
		value.Normalize();
		float num;
		float num2;
		float num3;
		float num4;
		if (examine)
		{
			num = GameData.Instance.AIConstants.ChanceToStopAndLookWhenSearchingResourcesFactor;
			num2 = GameData.Instance.AIConstants.MinimumChanceToStopAndLookWhenSearchingResources;
			num3 = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingResourcesMean;
			num4 = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingResourcesStdDev;
		}
		else
		{
			num = GameData.Instance.AIConstants.ChanceToStopAndLookWhenSearchingFactor;
			num2 = GameData.Instance.AIConstants.MinimumChanceToStopAndLookWhenSearching;
			num3 = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingMean;
			num4 = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingStdDev;
		}
		bool flag = numberOfVisitedLocations == 0 && locationsStillToVisit.Count == 1;
		bool flag2;
		if (!flag)
		{
			double num5 = Vector2.Dot(entity.FacingNormal.ToVector2(), value);
			num5 *= -1.0;
			num5 *= 0.5;
			num5 += 0.5;
			num5 = num5 * (double)(1f - num2) + (double)num2;
			num5 *= (double)num;
			flag2 = The.Sim.GameplayRandomGenerator.NextDouble("GoalSearchArea") < num5;
		}
		else
		{
			flag2 = true;
		}
		if (!flag2)
		{
			return;
		}
		double num6 = ((!flag) ? The.Sim.GameplayRandomGenerator.RandomNormalDistribution(num3, num4) : ((double)num3));
		if (!(num6 > 1.0))
		{
			return;
		}
		if (entity.HasStance())
		{
			StanceType stanceType = ((!(num6 > 2.0) || !(The.Sim.GameplayRandomGenerator.NextDouble("GoalSearchArea") > 0.4000000059604645)) ? entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SearchStancesBriefWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType) : entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SearchStancesLongerWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType));
			ChangeStance(stanceType);
			if (stanceType.AnimModifier.HasValue)
			{
				AddSubgoal(new GoalWait(entity, num6, AnimAction.Scouting, stanceType.AnimModifier.Value));
			}
			else
			{
				AddSubgoal(new GoalWait(entity, num6, AnimAction.Scouting));
			}
		}
		else
		{
			AddSubgoal(new GoalWait(entity, num6, AnimAction.Scouting));
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if (messageType == Message.MessageTypes.DistanceFound || messageType == Message.MessageTypes.DistanceFoundNoAccess)
			{
				if (waitingForDistance)
				{
					RemoveAllSubgoals();
					waitingForDistance = false;
					return true;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		closestLocation = sn.DoVector2(closestLocation);
		currentDestinationIndex = sn.DoInt32(currentDestinationIndex);
		distance = sn.DoFloat(distance);
		isStealthy = sn.DoBool(isStealthy);
		locationsStillToVisit = sn.DoList(locationsStillToVisit);
		lowestDistance = sn.DoFloat(lowestDistance);
		numberOfVisitedLocations = sn.DoInt32(numberOfVisitedLocations);
		examine = sn.DoBool(examine);
		waitingForDistance = sn.DoBool(waitingForDistance);
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(zone).Value;
		sn.Ignore(setSneakingRegulator);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
	}
}
