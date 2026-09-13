using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Buildings;

public class Structure : Component
{
	public enum Edge
	{
		Left,
		Bottom,
		Right
	}

	public List<IAddon> AddOns = new List<IAddon>();

	public EntityID AddonTo;

	protected StructureStates state;

	public EntityID? AnchorID;

	private Regulator createClearingJobsRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public StructureStates State
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				state = value;
				UpdateRenderableState();
				Parent.RecomputeUpdateInterval();
			}
		}
	}

	public string GetStateDescription()
	{
		return State switch
		{
			StructureStates.PlacedButNotStarted => "Planned", 
			StructureStates.UnderConstruction => "Under construction", 
			StructureStates.ConstructionPaused => "Construction started", 
			StructureStates.Operational => "", 
			StructureStates.Mothballed => "Mothballed", 
			_ => "", 
		};
	}

	public bool ConstructionHasStarted()
	{
		if (State != StructureStates.PlacedButNotStarted)
		{
			return State != StructureStates.BeingPlaced;
		}
		return false;
	}

	public void UpdateRenderable()
	{
		UpdateRenderableState();
	}

	private void CreateRegulators()
	{
		createClearingJobsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "StructureCreateClearingJobs");
	}

	public Structure(Entity parent)
		: base(parent)
	{
		UpdateRenderable();
		CreateRegulators();
	}

	private void UpdateRenderableState()
	{
		if (Parent.Renderable != null)
		{
			switch (state)
			{
			case StructureStates.BeingPlaced:
			case StructureStates.PlacedButNotStarted:
				Parent.SetSpriteStateFlag(StateModifier.Ordered);
				break;
			case StructureStates.UnderConstruction:
				Parent.ClearSpriteStateFlag(StateModifier.Ordered);
				Parent.SetSpriteStateFlag(StateModifier.BeingBuilt);
				Parent.Renderable.SetOverlayRendering(value: false);
				break;
			default:
				Parent.ClearSpriteStateFlag(StateModifier.BeingBuilt);
				Parent.ClearSpriteStateFlag(StateModifier.Ordered);
				break;
			}
		}
	}

	public Structure()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		AddonTo = sn.DoEnum(AddonTo);
		AnchorID = sn.DoEnumNullable(AnchorID);
		state = sn.DoEnum(state);
		sn.Ignore(createClearingJobsRegulator);
		sn.Ignore(AddOns);
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
		CreateRegulators();
	}

	public void PlaceBuilding()
	{
		State = StructureStates.PlacedButNotStarted;
		The.Sim.AllStructures.Add(Parent.EntityID);
	}

	public void Destroy()
	{
		The.Sim.AllStructures.Remove(Parent.EntityID);
		Entity.FindByID(AddonTo)?.Structure.AddOns.Remove(Parent);
		if (AnchorID.HasValue)
		{
			Entity entity = Entity.FindByID(AnchorID);
			if (entity != null && entity.EntityType.IsSpecialActionOutput(Parent.EntityType))
			{
				entity.EnableSpecialActionsUsingAnchor();
			}
		}
	}

	private bool TestAllSubtilesClear(byte[][] subtileMap, Entity exceptForEntity = null)
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		new Vector3(48 * Parent.TopLeftMapPosition.Value.X, 48 * Parent.TopLeftMapPosition.Value.Y, 0f);
		for (int i = 0; i < Parent.EntityType.StructureType.WidthInTiles; i++)
		{
			for (int j = 0; j < Parent.EntityType.StructureType.HeightInTiles; j++)
			{
				_ = tileMap[Parent.TopLeftMapPosition.Value.X + i][Parent.TopLeftMapPosition.Value.Y + j];
			}
		}
		return true;
	}

	public void ConstructionStarted(EntityID? anchorID)
	{
		AnchorID = anchorID;
		State = StructureStates.UnderConstruction;
		if (Parent.DirectionalLayout != null)
		{
			Parent.DirectionalLayout.RedrawTerrainCosts();
		}
		if (Parent.GeometryLayout != null)
		{
			Parent.FootprintIsDirty = true;
		}
	}

	public void ConstructionFinished(EntityID? anchorID, bool createParts = false)
	{
		AnchorID = anchorID;
		if (Parent.NonLivingEntity != null)
		{
			Parent.NonLivingEntity.Progress = 1f;
		}
		State = StructureStates.Operational;
		if (Parent.DirectionalLayout != null)
		{
			Parent.DirectionalLayout.ClearTerrainCosts(redraw: true);
		}
		else if (Parent.PointLayout != null)
		{
			Parent.PointLayout.ClearTerrainCosts(redraw: true);
		}
		if (Parent.GeometryLayout != null)
		{
			Parent.GeometryLayout.ClearTerrainCosts(redraw: true);
		}
		if (Parent.EntityType.TerrainType != null && Parent.EntityType.TerrainType.PathType != null)
		{
			TerrainTile tile = The.Map.GetTile(Parent.MapPosition.Value);
			Parent.TerrainPath.Value = 1f;
			tile.ClearPaths(Parent.DirectionalLayout.EdgePosition);
		}
		if (createParts)
		{
			Parent.CreateParts();
		}
		Parent.SetAccessPointDirty();
	}

	protected bool StartBuildingJob(EntityGroup ownerOfBuilding)
	{
		if (!GameData.Instance.ProcessYieldsThisOutput.TryGetValue(Parent.EntityType, out var value))
		{
			return false;
		}
		ProcessJob processJob = new ProcessJob(null, ChooseBuildProcess(value, ownerOfBuilding), Parent.EntityType, ownerOfBuilding);
		processJob.BuildingJob = new BuildingJob(processJob);
		LookUp<SimProcess, SimProcessID>.FindByID(processJob.ProductionProcess).AddOutputEntity(Parent);
		_ = The.Map;
		Edge edge = (Edge)The.Sim.GameplayRandomGenerator.Next(0, 3, "Structure");
		bool num = PlaceMaterialHaulingJobs(processJob, ownerOfBuilding, edge);
		if (!num)
		{
			processJob.Destroy(removeTakers: true);
		}
		return num;
	}

	/// <summary>
	/// Which recipe to build this structure with, when more than one produces it.
	///
	/// This was <c>value[0]</c>: the first recipe registered, always, whatever the colony had in
	/// store. No structure in the stock tables has a second recipe, so nothing ever noticed - and
	/// nothing changes for them here either, because with one candidate this returns that one.
	///
	/// It matters the moment a mod adds an alternative. A campfire that can be built with firewood
	/// OR peat has two recipes, and picking the first regardless means the second can never be
	/// used: the job is ordered against a recipe whose material nobody has, and the haulers wait
	/// for firewood while the peat sits in the stockpile.
	///
	/// So: the first candidate the owner can actually supply, and the original first candidate
	/// when they can supply none of them - which is the existing behaviour of ordering the build
	/// and waiting for the materials to arrive.
	///
	/// The order of the list is the order the recipes were registered, so the stock recipe is
	/// tried before anything a mod added. A mod's alternative is a fallback, never a substitute.
	/// </summary>
	private static ProcessType ChooseBuildProcess(List<ProcessType> candidates, EntityGroup ownerOfBuilding)
	{
		if (candidates.Count == 1)
		{
			return candidates[0];
		}
		foreach (ProcessType candidate in candidates)
		{
			if (OwnerCanSupply(candidate, ownerOfBuilding))
			{
				return candidate;
			}
		}
		return candidates[0];
	}

	/// <summary>
	/// Whether the owner has every input this recipe names, in the amount it names.
	///
	/// CountAvailableItems is the game's own notion of "have": it excludes items that are
	/// unfinished, on another site, part of something else, or owned by somebody else. Using it
	/// rather than a raw count of the item list is what keeps this from choosing a recipe whose
	/// materials are visible but unusable.
	/// </summary>
	private static bool OwnerCanSupply(ProcessType process, EntityGroup ownerOfBuilding)
	{
		if (process.Inputs == null)
		{
			return true;
		}
		Input[] inputs = process.Inputs;
		foreach (Input input in inputs)
		{
			if (input.EntityType == null)
			{
				continue;
			}
			int needed = ((input.Amount != null && input.Amount.NoOfItems.HasValue) ? input.Amount.NoOfItems.Value : 1);
			if (ownerOfBuilding.CountAvailableItems(input.EntityType) < needed)
			{
				return false;
			}
		}
		return true;
	}

	private bool CreateClearingJobs(Edge materialsAlongEdge, EntityGroup ownerOfJob)
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		Point? point = null;
		for (int i = 0; i < Parent.EntityType.StructureType.WidthInTiles; i++)
		{
			for (int j = 0; j < Parent.EntityType.StructureType.HeightInTiles; j++)
			{
				TerrainTile terrainTile = tileMap[Parent.TopLeftMapPosition.Value.X + i][Parent.TopLeftMapPosition.Value.Y + j];
				if (terrainTile.EntitiesOnTile == null)
				{
					continue;
				}
				foreach (Entity item in terrainTile.EntitiesOnTile)
				{
					if (!item.Find<Item>(out var _) || !item.IsUnassigned(ownerOfJob.GetAllegiance().SharedKnowledge))
					{
						continue;
					}
					if (!point.HasValue)
					{
						point = FindDumpingAreaOffSite(materialsAlongEdge, new Point(terrainTile.X, terrainTile.Y));
						if (!point.HasValue)
						{
							return false;
						}
					}
					new HaulingJobSpecificItem(MapManager.TileToWorldPos(point.Value), null, ownerOfJob, item, null, haulToStorage: true, haulToTrade: false, ownerOfJob);
				}
			}
		}
		return true;
	}

	private bool PlaceMaterialHaulingJobs(ProcessJob buildJob, EntityGroup ownerOfBuilding, Edge edge)
	{
		StructureType structureType = Parent.EntityType.StructureType;
		int num = The.Sim.GameplayRandomGenerator.Next(2, "Structure") + 2;
		int num2 = 0;
		bool oneTileBorder = false;
		byte[][] subTileMap = null;
		Tuple<bool, Vector2>[][] isBlockedData = null;
		if (Parent.CurrentSimState != null && Parent.CurrentSimState.GeometryLayoutType != null)
		{
			Parent.GeometryLayout.CreateBlockedMapOverBoundsAndPadding(Parent.FlipHorizontally, out isBlockedData, out subTileMap);
			int circleRadius = 1;
			int centerValue = -8;
			int circleRadius2 = 2;
			int centerValue2 = -18;
			DrawInfluenceMapForMaterialPlacement(edge, subTileMap, isBlockedData, 1, centerValue);
			if (!FindDropOffPointForBuildingMaterials(subTileMap, isBlockedData, oneTileBorder, out var foundLocation, circleRadius, centerValue))
			{
				bool[][] array = null;
				array = CreateOneTileBorderIsBlockedMapForConstructionJobs(Parent);
				oneTileBorder = true;
				subTileMap = null;
				Common.InitJaggedArray(ref subTileMap, (structureType.WidthInTiles + 2) * 3, (structureType.HeightInTiles + 2) * 3);
				DrawInfluenceMapForMaterialPlacement(edge, subTileMap, isBlockedData, 1, centerValue);
				if (!FindDropOffPointForBuildingMaterials(subTileMap, array, oneTileBorder, out foundLocation, circleRadius2, centerValue2))
				{
					return false;
				}
			}
			if (buildJob.ProcessType.InputsByType != null)
			{
				if (!buildJob.GetAssignedInputs(out var inputs))
				{
					return false;
				}
				foreach (KeyValuePair<EntityType, Input> item in buildJob.ProcessType.InputsByType)
				{
					Input value = item.Value;
					int num3 = buildJob.ProcessType.InputsByType[value.EntityType].Amount.NoOfItems.Value - inputs[value.EntityType].Count;
					for (int i = 0; i < num3; i++)
					{
						if (num2 >= num)
						{
							num = The.Sim.GameplayRandomGenerator.Next(2, "Structure") + 2;
							num2 = 0;
							FindDropOffPointForBuildingMaterials(subTileMap, isBlockedData, oneTileBorder, out foundLocation, circleRadius2, centerValue2);
						}
						HaulingJobAnyItemOfType haulingJobAnyItemOfType = new HaulingJobAnyItemOfType(foundLocation, ownerOfBuilding, value.EntityType, ownerOfBuilding.ID, null);
						num2++;
						haulingJobAnyItemOfType.RequiredByProcessJob = buildJob;
					}
				}
			}
			return true;
		}
		return false;
	}

	private void DrawInfluenceMapForMaterialPlacement(Edge edge, byte[][] subtileMap, Tuple<bool, Vector2>[][] IsBlockedData, int circleRadius, int centerValue)
	{
		PlaceMainOverlaysForConstructionMaterials(edge, subtileMap, 20);
	}

	private void PlaceMainOverlaysForConstructionMaterials(Edge edge, byte[][] subtileMap, byte centerValue)
	{
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subtileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subtileMap);
		float num = 0.25f;
		int maxRandomToAdd = centerValue / 6;
		byte endValue = (byte)(centerValue / 2);
		switch (edge)
		{
		case Edge.Left:
			InfluenceMap.DrawGradientRectangle(subtileMap, Point.Zero, new Point((int)(num * (float)jaggedArrayWidth), jaggedArrayHeight - 1), addToExistingValues: false, centerValue, endValue, maxRandomToAdd, InfluenceMap.GradientDirection.TopToBottom);
			break;
		case Edge.Bottom:
			InfluenceMap.DrawGradientRectangle(subtileMap, new Point(0, (int)((1f - num) * (float)jaggedArrayHeight)), new Point(jaggedArrayWidth - 1, jaggedArrayHeight - 1), addToExistingValues: false, centerValue, endValue, maxRandomToAdd, InfluenceMap.GradientDirection.LeftToRight);
			break;
		default:
			InfluenceMap.DrawGradientRectangle(subtileMap, new Point((int)((1f - num) * (float)jaggedArrayWidth), 0), new Point(jaggedArrayWidth - 1, jaggedArrayHeight - 1), addToExistingValues: false, centerValue, endValue, maxRandomToAdd, InfluenceMap.GradientDirection.TopToBottom);
			break;
		}
	}

	private void DrawInfluenceFromItemsOnSubtileMap(byte[][] subtileMap, Vector3 topLeftSubtilePosition, int circleRadius, int centerValue)
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		Parent.GeometryLayout.GetBoundsWithPadding(out var _, out var _);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subtileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subtileMap);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				if (tileMap[i][j].EntitiesOnTile == null)
				{
					continue;
				}
				foreach (Entity item in tileMap[i][j].EntitiesOnTile)
				{
					if (item.Find<Item>(out var _))
					{
						InfluenceMap.DrawLinearInfluenceCircle(subtileMap, MapManager.WorldPosToRelativeSubtile(item.PlaySiteLocation, topLeftSubtilePosition), centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
					}
				}
			}
		}
	}

	private bool FindDropOffPointForBuildingMaterials(byte[][] subtileMap, bool[][] isBlockedMap, bool oneTileBorder, out Vector3 foundLocation, int circleRadius, int centerValue)
	{
		foundLocation = new Vector3(-1f);
		if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap, isBlockedMap, out var bestSubtilePoint) > -1)
		{
			Point point = Parent.TopLeftMapPosition.Value;
			if (oneTileBorder)
			{
				point = Common.AddPoints(point, new Point(-1, -1));
			}
			MapManager.SubtileAndTilePosToWorldPos(bestSubtilePoint, point, out foundLocation);
			InfluenceMap.DrawLinearInfluenceCircle(subtileMap, bestSubtilePoint, centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, circleRadius);
			return true;
		}
		return false;
	}

	private bool FindDropOffPointForBuildingMaterials(byte[][] subtileMap, Tuple<bool, Vector2>[][] isBlockedData, bool oneTileBorder, out Vector3 foundLocation, int circleRadius, int centerValue)
	{
		foundLocation = new Vector3(-1f);
		if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileMap, isBlockedData, out var bestSubtileWorldPosition) > -1)
		{
			foundLocation = bestSubtileWorldPosition.ToVector3();
			return true;
		}
		return false;
	}

	public static bool[][] CreateIsBlockedMapAvoidConstructions(Rectangle tileArea, SubtileLayers costMapToUse)
	{
		bool[][] map = null;
		Common.InitJaggedArray(ref map, tileArea.Width * 3, tileArea.Height * 3);
		List<Entity> list = null;
		Point point = new Point(tileArea.X * 3, tileArea.Y * 3);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(map);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(map);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				Point tile = new Point(tileArea.X + i / 3, tileArea.Y + j / 3);
				if (!The.Map.TileIsOnMap(tile))
				{
					map[i][j] = true;
					continue;
				}
				if (MapManager.IsBlocked(costMapToUse.GetValue(point.X + i, point.Y + j)))
				{
					map[i][j] = true;
					continue;
				}
				_ = The.Map.TileMap[tile.X][tile.Y];
				list?.Clear();
			}
		}
		return map;
	}

	public static bool[][] CreateOneTileBorderIsBlockedMapForConstructionJobs(IKnownEntityData structureData)
	{
		StructureType structureType = structureData.EntityType.StructureType;
		return CreateIsBlockedMapAvoidConstructions(new Rectangle(structureData.TopLeftMapPosition.Value.X - 1, structureData.TopLeftMapPosition.Value.Y - 1, structureType.WidthInTiles + 2, structureType.HeightInTiles + 2), The.Map.TerrainCosts[SurfaceType.TransportType.Foot]);
	}

	public Point? FindDumpingAreaOffSite(Edge edge, Point from)
	{
		SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		Point point = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(-1, Parent.EntityType.StructureType.HeightInTiles / 2));
		bool flag = The.Map.TileIsAccessible(map, from, point);
		Point point2 = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(Parent.EntityType.StructureType.WidthInTiles / 2, Parent.EntityType.StructureType.HeightInTiles));
		bool flag2 = The.Map.TileIsAccessible(map, from, point2);
		Point point3 = Common.AddPoints(Parent.TopLeftMapPosition.Value, new Point(Parent.EntityType.StructureType.WidthInTiles, Parent.EntityType.StructureType.HeightInTiles / 2));
		bool flag3 = The.Map.TileIsAccessible(map, from, point3);
		switch (edge)
		{
		case Edge.Left:
			if (flag)
			{
				return point;
			}
			break;
		case Edge.Bottom:
			if (flag2)
			{
				return point2;
			}
			break;
		case Edge.Right:
			if (flag3)
			{
				return point3;
			}
			break;
		}
		if (flag)
		{
			return point;
		}
		if (flag3)
		{
			return point3;
		}
		if (flag2)
		{
			return point2;
		}
		return null;
	}

	public static bool IsNotBlockedOrReserved(MapManager.SubtileValue value)
	{
		if (value != MapManager.SubtileValue.Blocked)
		{
			return !value.HasFlag(MapManager.SubtileValue.Reserved);
		}
		return false;
	}

	public static bool CanBuildOnSubtile(bool structureIsInShapes, bool structureIsInPad, MapManager.SubtileValue mapValue)
	{
		if (structureIsInShapes)
		{
			if (MapManager.IsBlocked(mapValue) || mapValue.HasFlag(MapManager.SubtileValue.Pad) || mapValue.HasFlag(MapManager.SubtileValue.Reserved))
			{
				return false;
			}
		}
		else if (structureIsInPad && (MapManager.IsBlocked(mapValue) || mapValue.HasFlag(MapManager.SubtileValue.Reserved)))
		{
			return false;
		}
		return true;
	}

	public bool IsPlacementValid(Vector3 location)
	{
		bool isValid = true;
		SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		if (Parent.GeometryLayout != null)
		{
			GeometryLayoutType geoType = Parent.CurrentSimState.GeometryLayoutType;
			float padRadiusSquared = Parent.GeometryLayout.GetPadRadiusSquared();
			Parent.GeometryLayout.IterateSubtiles(delegate(Vector2 worldPos)
			{
				if (The.Map.WorldLocationIsOnMap(worldPos))
				{
					Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
					MapManager.SubtileValue value = terrainCosts.GetValue(subtilePos);
					bool structureIsInShapes = Parent.GeometryLayout.IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds: false);
					bool structureIsInPad = Parent.GeometryLayout.IsWithinPad(geoType, padRadiusSquared, worldPos);
					if (!CanBuildOnSubtile(structureIsInShapes, structureIsInPad, value))
					{
						isValid = false;
					}
				}
				else
				{
					isValid = false;
				}
			});
		}
		return isValid;
	}

	public bool IsPlacementValid(Point pos, Common.Direction? dir = null)
	{
		return true;
	}

	public static bool PrepareAndStartBuildingJob(string structureType, EntityGroup entityGroup, IOwner ownerOfNewStructure, Vector3 location)
	{
		Entity entity = new Entity(GameData.Instance.AllEntityTypes[structureType], isStructureBeingPlaced: true);
		entity.Structure.State = StructureStates.PlacedButNotStarted;
		entity.NonLivingEntity.Progress = 0f;
		entity.Initialize(The.Sim.PlaySite, entityGroup.GetAllegiance());
		entity.InitializeModelAndOnScreenFunctionality();
		entity.SetPosition(location);
		return entity.Structure.PrepareAndStartBuildingJob(entityGroup, ownerOfNewStructure, location);
	}

	public bool PrepareAndStartBuildingJob(EntityGroup entityGroup, IOwner newOwnerOfBuilding, Vector3? location)
	{
		Parent.NonLivingEntity.Progress = 0f;
		if (StartBuildingJob(entityGroup))
		{
			Parent.PlaceEntityOnPlaySite(location.Value, null, Entity.StructureState.Ordered, new Entity.SetOwnerInfo(newOwnerOfBuilding));
			Parent.Renderable.SetOverlayRendering(value: true);
			return true;
		}
		if (LookUpOwners.ResolveEntityOwner((IKnownEntityData)Parent, out IOwner owner) && owner != null)
		{
			owner.OwnedEntities.DeleteEntity(Parent);
		}
		else
		{
			newOwnerOfBuilding.OwnedEntities.DeleteEntity(Parent);
		}
		return false;
	}
}
