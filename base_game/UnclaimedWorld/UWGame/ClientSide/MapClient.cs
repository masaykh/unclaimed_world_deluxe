using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using InputEventSystem;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trees;
using WindowSystem;

namespace UWGame.ClientSide;

public class MapClient
{
	public enum ButtonState
	{
		MouseOver,
		MouseOut,
		Pressed,
		Released
	}

	private class Bullet
	{
		public Vector3 Muzzle;

		public Vector3 Impact;

		public Microsoft.Xna.Framework.Color Color1;

		public Microsoft.Xna.Framework.Color Color2;

		public Bullet(Vector3 muzzleLocation, Vector3 impactLocation, Microsoft.Xna.Framework.Color startColor, Microsoft.Xna.Framework.Color endColor)
		{
			Muzzle = muzzleLocation;
			Impact = impactLocation;
			Color1 = startColor;
			Color2 = endColor;
		}

		public void Fade()
		{
			Color1.R -= 10;
			Color1.G -= 10;
			Color1.B -= 10;
			if (Color1.R < 10 || Color1.G < 10 || Color1.B < 10)
			{
				bullets.Remove(this);
			}
		}
	}

	private class Marker
	{
		public Vector3 loc;

		public Microsoft.Xna.Framework.Color color;

		public float size;

		public Marker(Vector3 l, Microsoft.Xna.Framework.Color c, float s = 5f)
		{
			loc = l;
			size = s;
			color = c;
		}
	}

	public class OverlayLimit : IEdge
	{
		public Microsoft.Xna.Framework.Color Color;

		public float Edge { get; set; }
	}

	private ButtonState leftMouseButtonStateInMap = ButtonState.Released;

	private ButtonState rightMouseButtonStateInMap = ButtonState.Released;

	private InputData inputData;

	private Microsoft.Xna.Framework.Point previousMouseScreenPosition;

	private Vector3? previousMouseWorldLocation;

	private TilePos? previousMouseTilePosition;

	private SubtilePos? previousMouseSubtilePosition;

	private Microsoft.Xna.Framework.Point? startLeftDragMouseScreenPosition;

	private SubtilePos? startLeftDragMouseSubtilePosition;

	private TilePos? startLeftDragMouseTilePosition;

	private bool hasRightDragged;

	private Microsoft.Xna.Framework.Point? lastRightDragMousePosition;

	private Microsoft.Xna.Framework.Point mapWindowTilePosClamped;

	public int mapWindowWidth;

	public int mapWindowHeight;

	public int noOfTilesToDisplayHorizontally = 22;

	public int noOfTilesToDisplayVertically = 19;

	public int noOfTilesLeftOfDetailsInterface = 10;

	public int widthOfWindowLeftOfDetailsInterface;

	public int iMapDisplayOffsetX;

	public int iMapDisplayOffsetY;

	private Vector2 mapWindowWorldPosition;

	private Vector2 oldMapWindowWorldPosition;

	private bool isScrolling;

	public List<object> Overlays = new List<object>();

	public List<ResourceType> ResourceOverlays = new List<ResourceType>();

	public bool RenderedTerrainIsDirty = true;

	private static Common.Direction[,] mouseToDirection = new Common.Direction[4, 4]
	{
		{
			Common.Direction.NorthWest,
			Common.Direction.West,
			Common.Direction.West,
			Common.Direction.SouthWest
		},
		{
			Common.Direction.North,
			Common.Direction.North,
			Common.Direction.South,
			Common.Direction.South
		},
		{
			Common.Direction.North,
			Common.Direction.North,
			Common.Direction.South,
			Common.Direction.South
		},
		{
			Common.Direction.NorthEast,
			Common.Direction.East,
			Common.Direction.East,
			Common.Direction.SouthEast
		}
	};

	public TilePos MouseTilePosition = new TilePos(-1, -1);

	public SubtilePos MouseSubtilePosition;

	public Vector3 MouseWorldLocation;

	public Common.Direction MouseMapDirection;

	private bool isRightScrolling;

	private bool allowLeftDraggingToStart = true;

	private Keys limitSelectionKey = Keys.LeftAlt;

	private byte strobe;

	private static List<Bullet> bullets;

	private static Dictionary<string, Marker> VisitorMarkers;

	private Dictionary<object, List<Marker>> RenderedObjectMarkers = new Dictionary<object, List<Marker>>();

	private static ulong hash = 0uL;

	private float fogOfWarFadeRate = 0.03f;

	private float fogOfWarTint = 0.7f;

	private float cloudSharpness = 5f;

	private float cloudOpacity = 0.7f;

	private List<OverlayLimit> threatMapLimits = new List<OverlayLimit>();

	public int mapWindowTileX => mapWindowTilePosClamped.X;

	public int mapWindowTileY => mapWindowTilePosClamped.Y;

	public Vector2 MapWindowWorldPosition
	{
		get
		{
			return mapWindowWorldPosition;
		}
		set
		{
			if (mapWindowWorldPosition != value)
			{
				mapWindowWorldPosition = value;
				mapWindowTilePosClamped = MapManager.WorldPosToTile(mapWindowWorldPosition);
				ClampTileMapPosition();
				if (The.InGameUI.Minimap != null)
				{
					The.InGameUI.Minimap.SetFrameDirty();
				}
			}
		}
	}

	public RectangleF ScreenRect => new RectangleF(MapWindowWorldPosition.X, MapWindowWorldPosition.Y, mapWindowWidth, mapWindowHeight);

	public bool IsScrolling => isScrolling;

	public float FogOfWarFadeRate => fogOfWarFadeRate;

	public float FogOfWarTint => fogOfWarTint;

	public float CloudSharpness => cloudSharpness;

	public float CloudOpacity => cloudOpacity;

	public event Action<Vector3> LeftMouseDownInMap;

	public event Action<Vector3> LeftMouseReleasedInMap;

	public event Action<Vector3> LeftMouseWorldPosDragInMap;

	public event Action<SubtilePos> LeftMouseSubtileDragInMap;

	public event Action<TilePos> LeftMouseTileDragInMap;

	public MapClient()
	{
		mapWindowWidth = The.Sim.Controller.DrawArea.Width;
		mapWindowHeight = The.Sim.Controller.DrawArea.Height;
		inputData = The.Sim.Controller.InputData;
		noOfTilesToDisplayHorizontally = (int)Math.Ceiling((decimal)mapWindowWidth / 48m);
		noOfTilesToDisplayVertically = (int)Math.Ceiling((decimal)mapWindowHeight / 48m);
		bullets = new List<Bullet>();
		VisitorMarkers = new Dictionary<string, Marker>();
	}

	public void SetSize()
	{
		noOfTilesToDisplayHorizontally = Common.ClampTop(noOfTilesToDisplayHorizontally, The.Map.mapTileWidth);
		noOfTilesToDisplayVertically = Common.ClampTop(noOfTilesToDisplayVertically, The.Map.mapTileHeight);
	}

	public void Destroy()
	{
		inputData = null;
	}

	public void TryMapScrolling()
	{
		if (isRightScrolling && inputData.RightButtonDown)
		{
			TryMouseDragForMapScrolling(inputData);
		}
		else
		{
			isRightScrolling = false;
		}
		previousMouseScreenPosition.X = inputData.mouseX;
		previousMouseScreenPosition.Y = inputData.mouseY;
	}

	private void TryMouseDragForMapScrolling(InputData inputData)
	{
		if (rightMouseButtonStateInMap != ButtonState.Pressed)
		{
			rightMouseButtonStateInMap = ButtonState.Pressed;
			lastRightDragMousePosition = new Microsoft.Xna.Framework.Point(inputData.mouseX, inputData.mouseY);
		}
		else if (inputData.mouseX != previousMouseScreenPosition.X || inputData.mouseY != previousMouseScreenPosition.Y)
		{
			isRightScrolling = true;
			HandleRightMouseDrag();
		}
	}

	public void UpdateMouseInMap()
	{
		if (The.Client.IsModal)
		{
			return;
		}
		if (!inputData.LeftButtonDown)
		{
			allowLeftDraggingToStart = true;
		}
		if (The.InGameUI.gui.IsMouseInInterface(inputData.mouseX, inputData.mouseY))
		{
			if (inputData.LeftButtonDown)
			{
				allowLeftDraggingToStart = false;
			}
			if (!startLeftDragMouseScreenPosition.HasValue)
			{
				return;
			}
		}
		if (IsMouseInsideMap())
		{
			isRightScrolling = false;
			MouseWorldLocation = new Vector3((float)inputData.mouseX + MapWindowWorldPosition.X, (float)inputData.mouseY + MapWindowWorldPosition.Y, 0f);
			MouseWorldLocation = The.Map.ClampWorldPosition(MouseWorldLocation);
			MouseTilePosition = MapManager.WorldPosToTilePos(MouseWorldLocation);
			MouseSubtilePosition = MapManager.WorldPosToSubtilePos(MouseWorldLocation);
			if (The.Client != null)
			{
				The.Client.Renderer.UpdatePicking();
			}
			if (inputData.LeftButtonDown && The.Map.TileIsOnMap(MouseTilePosition.ToPoint()))
			{
				if (leftMouseButtonStateInMap != ButtonState.Pressed)
				{
					leftMouseButtonStateInMap = ButtonState.Pressed;
					EvaluateMouseDirection();
					if ((The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.None || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorTool) && allowLeftDraggingToStart)
					{
						startLeftDragMouseSubtilePosition = MouseSubtilePosition;
						startLeftDragMouseTilePosition = MouseTilePosition;
						startLeftDragMouseScreenPosition = new Microsoft.Xna.Framework.Point(inputData.mouseX, inputData.mouseY);
					}
					if (this.LeftMouseDownInMap != null)
					{
						this.LeftMouseDownInMap(MouseWorldLocation);
					}
				}
				else if (startLeftDragMouseScreenPosition.HasValue && (inputData.mouseX != previousMouseScreenPosition.X || inputData.mouseY != previousMouseScreenPosition.Y))
				{
					if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.None)
					{
						HandleMouseDragCaptureRectangle();
						The.InGameUI.ShowOverlaysAndMarkerWindows = true;
					}
					TilePos mouseTilePosition = MouseTilePosition;
					TilePos? tilePos = previousMouseTilePosition;
					if (mouseTilePosition != tilePos && this.LeftMouseTileDragInMap != null)
					{
						this.LeftMouseTileDragInMap(MouseTilePosition);
					}
					SubtilePos mouseSubtilePosition = MouseSubtilePosition;
					SubtilePos? subtilePos = previousMouseSubtilePosition;
					if (mouseSubtilePosition != subtilePos && this.LeftMouseSubtileDragInMap != null)
					{
						this.LeftMouseSubtileDragInMap(MouseSubtilePosition);
					}
					Vector3 mouseWorldLocation = MouseWorldLocation;
					Vector3? vector = previousMouseWorldLocation;
					if (mouseWorldLocation != vector && this.LeftMouseWorldPosDragInMap != null)
					{
						this.LeftMouseWorldPosDragInMap(MouseWorldLocation);
					}
				}
			}
			else if (inputData.RightButtonDown)
			{
				TryMouseDragForMapScrolling(inputData);
			}
			else
			{
				HandleMouseHoverInMap();
				if (leftMouseButtonStateInMap == ButtonState.Pressed)
				{
					HandleLeftMouseRelease(MouseTilePosition);
				}
				if (rightMouseButtonStateInMap == ButtonState.Pressed)
				{
					HandleRightMouseRelease(inputData);
				}
			}
		}
		else
		{
			MouseTilePosition.X = (MouseTilePosition.Y = -1);
		}
		previousMouseWorldLocation = MouseWorldLocation;
		previousMouseTilePosition = MouseTilePosition;
		previousMouseSubtilePosition = MouseSubtilePosition;
		previousMouseScreenPosition = new Microsoft.Xna.Framework.Point(inputData.mouseX, inputData.mouseY);
	}

	private void HandleRightMouseRelease(InputData inputData)
	{
		The.Sim.Controller.Game.IsMouseVisible = true;
		if (!hasRightDragged && inputData.mouseX == lastRightDragMousePosition.Value.X && inputData.mouseY == lastRightDragMousePosition.Value.Y)
		{
			The.InGameUI.HideMapInterface();
		}
		rightMouseButtonStateInMap = ButtonState.Released;
		hasRightDragged = false;
	}

	private void UpdateScrolling(GameTime time)
	{
		_ = time.ElapsedGameTime.TotalSeconds;
		float num = 0f;
		float num2 = 0f;
		Options options = The.Client.Controller.Options;
		float keyScrollSpeedPerSecond = options.KeyScrollSpeedPerSecond;
		bool flag = false;
		if (The.Client.EnableKeyboardShortcuts)
		{
			if (inputData.IsKeyDown(options.KeyScrollUp))
			{
				num = 0f - keyScrollSpeedPerSecond;
				flag = true;
			}
			if (inputData.IsKeyDown(options.KeyScrollDown))
			{
				num = keyScrollSpeedPerSecond;
				flag = true;
			}
			if (inputData.IsKeyDown(options.KeyScrollLeft))
			{
				num2 = 0f - keyScrollSpeedPerSecond;
				flag = true;
			}
			if (inputData.IsKeyDown(options.KeyScrollRight))
			{
				num2 = keyScrollSpeedPerSecond;
				flag = true;
			}
		}
		if (flag)
		{
			float deltaX = (float)((double)num2 * time.ElapsedGameTime.TotalSeconds);
			float deltaY = (float)((double)num * time.ElapsedGameTime.TotalSeconds);
			ChangeMapWindowWorldPosition(deltaX, deltaY);
		}
		if (The.InGameUI.TrackSelectedEntity && The.InGameUI.SelectedEntity.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out var data);
			if (data != null)
			{
				The.InGameUI.ZoomToEntity(data);
			}
		}
	}

	private void ClampTileMapPosition()
	{
		if (mapWindowTilePosClamped.X < 0)
		{
			mapWindowTilePosClamped.X = 0;
		}
		int num = The.Map.mapTileWidth - noOfTilesToDisplayHorizontally;
		if (num > 0 && mapWindowTilePosClamped.X > num)
		{
			mapWindowTilePosClamped.X = num;
		}
		if (mapWindowTilePosClamped.Y < 0)
		{
			mapWindowTilePosClamped.Y = 0;
		}
		int num2 = The.Map.mapTileHeight - noOfTilesToDisplayVertically;
		if (num2 > 0 && mapWindowTilePosClamped.Y > num2)
		{
			mapWindowTilePosClamped.Y = num2;
		}
	}

	public void ZoomToMapPosition(Microsoft.Xna.Framework.Point pos)
	{
		ZoomToMapPosition(pos.X, pos.Y);
	}

	public void ZoomToMapPosition(int x, int y)
	{
		ZoomToMapPosition(new Vector3(x * 48, y * 48, 0f));
	}

	public void ZoomToMapPosition(Vector3 worldLocation)
	{
		float num = Math.Max(mapWindowWidth, noOfTilesToDisplayHorizontally * 48);
		float num2 = Math.Max(mapWindowHeight, noOfTilesToDisplayVertically * 48);
		float x = worldLocation.X - num / 2f;
		float y = worldLocation.Y - num2 / 2f;
		MapWindowWorldPosition = new Vector2(x, y);
	}

	public void HandleLeftMouseRelease(TilePos tile)
	{
		leftMouseButtonStateInMap = ButtonState.Released;
		InGameInterface inGameUI = The.InGameUI;
		switch (The.InGameUI.InterfaceMode)
		{
		case InGameInterface.InterfaceState.Launch:
			if (The.InGameUI.SelectedEntity.HasValue)
			{
				Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
				if (entity != null && entity.Intelligence == null)
				{
					entity.Locomotor.StartMoving(Locomotor.Mode.Ballistic, MouseWorldLocation, 680f, 0f, null, null, GameData.Instance.AllAttackTypes["throwSpear"], null, null);
				}
			}
			break;
		case InGameInterface.InterfaceState.Build:
		{
			Common.Direction? direction = null;
			Entity entity4 = inGameUI.EntitiesBeingPlaced[0].Entity;
			bool flag;
			if (entity4.DirectionalLayout != null)
			{
				direction = entity4.DirectionalLayout.EdgePosition;
				flag = entity4.Structure.IsPlacementValid(entity4.TopLeftMapPosition.Value, direction);
			}
			else
			{
				flag = entity4.Structure.IsPlacementValid(entity4.PlaySiteLocation);
			}
			if (flag)
			{
				Expedition closestExpedition = The.Map.GetClosestExpedition(entity4.TopLeftMapPosition.Value);
				UWGame.Control.Commands.Command command = new Build(entity4.EntityType, entity4.PlaySiteLocation, giveClientFeedback: true, closestExpedition.OwnedEntities.ID);
				The.Client.Controller.StoreAndExecuteCommand(command);
				entity4.Destroy();
			}
			break;
		}
		case InGameInterface.InterfaceState.PlaceExpeditionCenter:
		{
			Vector3 mouseWorldLocation = MouseWorldLocation;
			Expedition expedition = The.InGameUI.GetExpedition();
			if (expedition != null)
			{
				UWGame.Control.Commands.Command command2 = new PlaceExpedition(expedition.ID, mouseWorldLocation, giveClientFeedback: true);
				The.Client.Controller.StoreAndExecuteCommand(command2);
			}
			break;
		}
		case InGameInterface.InterfaceState.EditorPlaceEntity:
		{
			foreach (InGameInterface.EntityPosition item in inGameUI.EntitiesBeingPlaced)
			{
				Vector3 position = MouseWorldLocation + item.Position.ToVector3();
				position = The.Map.ClampWorldPosition(position);
				item.Entity.PlaceEntityOnPlaySite(position, null, null, null);
			}
			Entity entity3 = inGameUI.EntitiesBeingPlaced[0].Entity;
			inGameUI.EntitiesBeingPlaced.Clear();
			inGameUI.SidePanelEditorEntity.CreateEntityForPlacement(entity3.EntityType);
			The.InGameUI.UpdateEntitiesBeingPlacedPositions();
			break;
		}
		case InGameInterface.InterfaceState.EditorDeleteEntities:
		{
			if (!The.InGameUI.IsShowingMapEditor() || The.InGameUI.SidePanelEditorEntity.SelectedEntityType == null)
			{
				break;
			}
			List<Entity> entitiesOnTile = The.Map.GetTile(MouseTilePosition).EntitiesOnTile;
			if (entitiesOnTile == null)
			{
				break;
			}
			string prefix = SidePanelEditorEntity.GetPrefix(The.InGameUI.SidePanelEditorEntity.SelectedEntityType.KeyName);
			for (int num = entitiesOnTile.Count - 1; num >= 0; num--)
			{
				Entity entity2 = entitiesOnTile[num];
				if (entity2.EntityType == The.InGameUI.SidePanelEditorEntity.SelectedEntityType || SidePanelEditorEntity.MatchesPrefix(entity2.EntityType, prefix))
				{
					entity2.Destroy();
				}
			}
			break;
		}
		case InGameInterface.InterfaceState.EditorClearTile:
			if (The.Map.TileIsOnMap(MouseTilePosition))
			{
				The.Map.ClearTile(The.Map.TileMap[MouseTilePosition.X][MouseTilePosition.Y]);
			}
			break;
		case InGameInterface.InterfaceState.Threat:
			inGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
			break;
		case InGameInterface.InterfaceState.BlockSubtile:
			The.Map.SetSubtileTerrainCost(MouseWorldLocation, 0);
			break;
		case InGameInterface.InterfaceState.UnblockSubtile:
			The.Map.SetSubtileTerrainCost(MouseWorldLocation, 3);
			break;
		case InGameInterface.InterfaceState.None:
			HandleLeftClickOrStopDragging();
			break;
		}
		if (this.LeftMouseReleasedInMap != null)
		{
			this.LeftMouseReleasedInMap(MouseWorldLocation);
		}
	}

	public void OnPlaceExpedition()
	{
		The.InGameUI.SetExpeditionMarkerPositions();
		The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
	}

	private EntityID? GetPickedEntity()
	{
		EntityID? result = null;
		if (The.Client != null && The.Client.Renderer.PickedModel.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.Client.Renderer.PickedModel.Value, out var data);
			if (data != null)
			{
				if (CanBeSelected(data))
				{
					return The.Client.Renderer.PickedModel;
				}
			}
			else
			{
				The.Client.Renderer.PickedModel = null;
			}
		}
		return result;
	}

	private static bool CanBeSelected(IKnownEntityData entity)
	{
		if (entity.EntityType.IsSelectable.HasValue)
		{
			return entity.EntityType.IsSelectable.Value;
		}
		if (entity.EntityType.RockType == null && entity.EntityType.ItemType == null && entity.EntityType.TreeType == null)
		{
			return entity.EntityType.TerrainType == null;
		}
		return false;
	}

	public void EvaluateMouseDirection()
	{
		int x = MouseTilePosition.X;
		int y = MouseTilePosition.Y;
		int f = x / 12;
		int f2 = y / 12;
		MouseMapDirection = mouseToDirection[Common.Clamp(f, 0, 3), Common.Clamp(f2, 0, 3)];
	}

	private void HandleLeftClickOrStopDragging()
	{
		if (startLeftDragMouseTilePosition.HasValue && startLeftDragMouseTilePosition.Value != MouseTilePosition)
		{
			The.MapUI.SelectBoundedRectangleTiles(startLeftDragMouseTilePosition.Value, MouseTilePosition, The.InGameUI.SelectedTiles);
			if (inputData.IsKeyDown(limitSelectionKey))
			{
				The.MapUI.CropTilesToConnectedArea(The.InGameUI.SelectedTiles);
			}
			The.InGameUI.SelectedZone = null;
			Microsoft.Xna.Framework.Point contextMenuOpenerPosFromMouse = GetContextMenuOpenerPosFromMouse();
			ShowContextMenuOpener(contextMenuOpenerPosFromMouse.X, contextMenuOpenerPosFromMouse.Y);
			The.InGameUI.ShowSelectedMapAreaPanel();
		}
		else
		{
			HandleMouseClickInTile();
		}
		The.InGameUI.SelectRectangle.Visible = false;
		The.InGameUI.SelectedTilesPreview.Clear();
		ResetDragging();
	}

	public void ResetDragging()
	{
		startLeftDragMouseTilePosition = null;
		startLeftDragMouseScreenPosition = null;
		startLeftDragMouseSubtilePosition = null;
	}

	public void CropTilesToConnectedArea(MapArea mapArea)
	{
		List<TerrainTile> connectedTiles = null;
		mapArea.CheckConnectivity(createList: true, ref connectedTiles);
		mapArea.Clear();
		foreach (TerrainTile item in connectedTiles)
		{
			mapArea.Add(item);
		}
	}

	public void SelectBoundedRectangleTiles(TilePos from, TilePos to, MapArea mapArea)
	{
		mapArea.Clear();
		int x = Common.Min(from.X, to.X);
		int x2 = Common.Max(from.X, to.X);
		int y = Common.Min(from.Y, to.Y);
		int y2 = Common.Max(from.Y, to.Y);
		mapArea.StartDragTile = from.ToPoint();
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		Microsoft.Xna.Framework.Point point = The.Map.ClampTileMapPosition(new Microsoft.Xna.Framework.Point(x, y));
		Microsoft.Xna.Framework.Point point2 = The.Map.ClampTileMapPosition(new Microsoft.Xna.Framework.Point(x2, y2));
		for (int i = point.X; i <= point2.X; i++)
		{
			for (int j = point.Y; j <= point2.Y; j++)
			{
				Microsoft.Xna.Framework.Point point3 = new Microsoft.Xna.Framework.Point(i, j);
				if (!The.Map.TileIsCompletelyBlocked(mapCosts, point3))
				{
					mapArea.Add(The.Map.GetTile(point3));
					if (mapArea.Count > 1600)
					{
						mapArea.RecomputeBoundingRectangleAndEdges();
						return;
					}
				}
			}
		}
		mapArea.RecomputeBoundingRectangleAndEdges();
	}

	private void HandleMouseHoverInMap()
	{
		if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.None)
		{
			HandleHoverOverEntity();
		}
	}

	private void HandleHoverOverEntity(Microsoft.Xna.Framework.Color? hoverTintingColor = null)
	{
		The.InGameUI.HoverTile = null;
		EntityID? entityID = null;
		entityID = GetPickedEntity();
		The.InGameUI.SetHoverEntity(entityID, hoverTintingColor);
	}

	public bool IsMouseInsideMap()
	{
		if (The.InGameUI == null)
		{
			return false;
		}
		if (inputData.mouseX >= 0 && inputData.mouseX < mapWindowWidth && inputData.mouseY >= 0)
		{
			return inputData.mouseY < mapWindowHeight;
		}
		return false;
	}

	private void HandleHoverOverTile(Microsoft.Xna.Framework.Color? hoverTintingColor = null)
	{
		The.InGameUI.SetHoverEntity(null);
		if (IsMouseInsideMap())
		{
			TerrainTile tile = The.Map.GetTile(MouseTilePosition);
			if (The.InGameUI.HoverTile != tile || The.InGameUI.Selection.HoverTintingColor != hoverTintingColor)
			{
				The.InGameUI.HoverTile = tile;
				The.InGameUI.Selection.StartHoverOverTile(hoverTintingColor);
			}
		}
		else
		{
			The.InGameUI.HoverTile = null;
		}
	}

	private void HandleMouseClickInTile()
	{
		EntityID? pickedEntity = GetPickedEntity();
		bool flag = false;
		if (pickedEntity.HasValue)
		{
			The.InGameUI.SelectEntity(pickedEntity.Value);
			flag = true;
		}
		if (MouseTilePosition.X < 0 || MouseTilePosition.X >= The.Map.mapTileWidth || MouseTilePosition.Y < 0 || MouseTilePosition.Y >= The.Map.mapTileHeight)
		{
			return;
		}
		TerrainTile tile = The.Map.GetTile(MouseTilePosition);
		if (!flag && tile.GeoLayoutEntitiesOnTile != null)
		{
			for (int i = 0; i < tile.GeoLayoutEntitiesOnTile.Count; i++)
			{
				Entity entity = Entity.FindByID(tile.GeoLayoutEntitiesOnTile[i]);
				if (entity != null && CanBeSelected(entity) && MouseIsInEntitySelectionArea(entity))
				{
					pickedEntity = entity.ID;
					The.InGameUI.SelectEntity(pickedEntity.Value);
					flag = true;
					The.InGameUI.ShowSelectedEntityPanel();
					return;
				}
			}
		}
		if (!flag && tile.EntitiesOnTile != null)
		{
			foreach (Entity item in tile.EntitiesOnTile)
			{
				if (CanBeSelected(item) && (item.Renderable == null || item.Renderable.RenderAsModel == null) && MouseIsInEntitySelectionArea(item))
				{
					The.InGameUI.SelectEntity(item);
					The.InGameUI.ShowSelectedEntityPanel();
					return;
				}
			}
		}
		if (!flag)
		{
			The.InGameUI.SelectTile(tile);
			Microsoft.Xna.Framework.Point contextMenuOpenerPosFromMouse = GetContextMenuOpenerPosFromMouse();
			ShowContextMenuOpener(contextMenuOpenerPosFromMouse.X, contextMenuOpenerPosFromMouse.Y);
			The.InGameUI.ShowSelectedMapAreaPanel();
			The.InGameUI.ShowOverlaysAndMarkerWindows = true;
		}
	}

	private bool MouseIsInEntitySelectionArea(Entity entity)
	{
		Vector2 point = MouseWorldLocation.ToVector2();
		if (entity.SelectionShape != null && entity.SelectionShape.ContainsPoint(point))
		{
			return true;
		}
		if (entity.Collidable != null && entity.Collidable.ContainsPoint(point))
		{
			return true;
		}
		return false;
	}

	private Microsoft.Xna.Framework.Point GetContextMenuOpenerPosFromMouse()
	{
		Vector2 vector = new Microsoft.Xna.Framework.Point(inputData.mouseX, inputData.mouseY).ToVector2();
		Vector2 vector2 = vector - previousMouseScreenPosition.ToVector2();
		Vector2 vector3 = vector;
		vector3.Y -= 16f;
		vector3.X -= 2f;
		if (vector2.LengthSquared() > 25f)
		{
			vector2.Normalize();
			vector3 += vector2 * 8f;
		}
		else if (startLeftDragMouseScreenPosition.HasValue && vector != startLeftDragMouseScreenPosition.Value.ToVector2())
		{
			vector2 = vector - startLeftDragMouseScreenPosition.Value.ToVector2();
			vector2.Normalize();
			vector3 += vector2 * 32f;
		}
		else
		{
			vector3.X += 6f;
		}
		return vector3.ToPoint();
	}

	private void HandleMouseDragCaptureRectangle()
	{
		int mouseX = inputData.mouseX;
		int mouseY = inputData.mouseY;
		int num = Math.Abs(mouseX - startLeftDragMouseScreenPosition.Value.X);
		int num2 = Math.Abs(mouseY - startLeftDragMouseScreenPosition.Value.Y);
		The.InGameUI.SelectRectangle.SetSize(Math.Min(startLeftDragMouseScreenPosition.Value.X, mouseX), Math.Min(startLeftDragMouseScreenPosition.Value.Y, mouseY), num, num2);
		if (num > 12 || num2 > 12)
		{
			The.InGameUI.SelectRectangle.Visible = true;
		}
		else
		{
			The.InGameUI.SelectRectangle.Visible = false;
		}
		TilePos mouseTilePosition = MouseTilePosition;
		TilePos? tilePos = previousMouseTilePosition;
		if (mouseTilePosition != tilePos)
		{
			UpdateTileSelectionPreview();
		}
	}

	private void UpdateTileSelectionPreview()
	{
		The.InGameUI.SelectedTilesPreview.Clear();
		SelectBoundedRectangleTiles(startLeftDragMouseTilePosition.Value, MouseTilePosition, The.InGameUI.SelectedTilesPreview);
		if (inputData.IsKeyDown(limitSelectionKey))
		{
			CropTilesToConnectedArea(The.InGameUI.SelectedTilesPreview);
		}
	}

	private void HandleRightMouseDrag()
	{
		hasRightDragged = true;
		int mouseX = inputData.mouseX;
		int mouseY = inputData.mouseY;
		float deltaX = lastRightDragMousePosition.Value.X - mouseX;
		float deltaY = lastRightDragMousePosition.Value.Y - mouseY;
		ChangeMapWindowWorldPosition(deltaX, deltaY);
		lastRightDragMousePosition = new Microsoft.Xna.Framework.Point(mouseX, mouseY);
	}

	private void ChangeMapWindowWorldPosition(float deltaX, float deltaY)
	{
		Vector2 vector = MapWindowWorldPosition + new Vector2(deltaX, deltaY);
		// MOD: hold the view inside the map instead of allowing half a screen of overscroll at
		// every edge, which is where the empty grid behind the world shows.
		if (UWGame.Mods.MapEdgeMod.Enabled)
		{
			MapWindowWorldPosition = UWGame.Mods.MapEdgeMod.Clamp(vector, The.Sim.Controller.DrawArea,
				The.Map.MapWorldWidth, The.Map.MapWorldHeight);
			return;
		}
		int num = The.Sim.Controller.DrawArea.Width / 2;
		int num2 = The.Sim.Controller.DrawArea.Height / 2;
		MapWindowWorldPosition = new Vector2(Common.Clamp(vector.X, -num, The.Map.MapWorldWidth - (float)num), Common.Clamp(vector.Y, -num2, The.Map.MapWorldHeight - (float)num2));
	}

	public void ShowContextMenuOpener(int screenPosX, int screenPosY)
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			The.InGameUI.ContextMenuOpener.Hide();
			The.InGameUI.ContextMenuOpener.ShowOnPlayfield(screenPosX, screenPosY);
			The.InGameUI.ContextMenuOpener.Populate();
			return;
		}
		SetTileResourcesWindow setTileResources = The.InGameUI.SetTileResources;
		setTileResources.Hide();
		setTileResources.DisplayWindow.Show();
		setTileResources.WorldPosition = MouseWorldLocation.ToVector2();
		setTileResources.DisplayWindow.X = inputData.mouseX;
		setTileResources.DisplayWindow.Y = inputData.mouseY;
		setTileResources.Populate();
	}

	public void DrawNoiseMap(Microsoft.Xna.Framework.Color color, Tuple<NoiseParams, byte[]> noise)
	{
	}

	private void PrintResourceAmountOnTile(TerrainTile terrainTile, ResourceType resourceType, Microsoft.Xna.Framework.Color color)
	{
		string text = null;
		int num = 0;
		if (resourceType.TileResourceType != null && terrainTile.TileResources != null)
		{
			if (terrainTile.TileResources.TryGetValue(resourceType, out var value))
			{
				num = value.NoOfHarvestableItems;
			}
		}
		else if (resourceType.CropType != null && terrainTile.TreesOnTile != null)
		{
			Entity entity = terrainTile.TreesOnTile.FirstOrDefault((Entity t) => t.EntityType.TreeType.CropTypes != null && t.EntityType.TreeType.CropTypes.Contains(resourceType));
			if (entity != null)
			{
				entity.Find<Tree>(out var c);
				if (c.Crops.TryGetValue(resourceType, out var value2))
				{
					num = value2.NoOfHarvestableItems;
				}
			}
		}
		if (num > 0)
		{
			Vector2 position = The.MapUI.TileEdgeToScreen(terrainTile.X, terrainTile.Y);
			text = num.ToString();
			DevText.Print(position, text, color);
		}
	}

	private void DrawResourceNoiseOnTile(Microsoft.Xna.Framework.Point tilePos, Microsoft.Xna.Framework.Color color, Tuple<NoiseParams, SimplexNoise> noise)
	{
		float resourceNoiseValue = ChangeResourcesAction.GetResourceNoiseValue(noise.Item2, noise.Item1, tilePos);
		DrawTileOverlay(tilePos, color, resourceNoiseValue);
	}

	private void DrawTileOverlay(Microsoft.Xna.Framework.Point tilePos, Microsoft.Xna.Framework.Color color, float value)
	{
		int num = 48;
		Vector2 topLeft = TileEdgeToScreen(tilePos.X, tilePos.Y);
		if (value > 0f)
		{
			float val = value / 3f;
			val = Math.Min(1f, val);
			Microsoft.Xna.Framework.Color colour = color * val;
			Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
		}
	}

	private void IterateOnScreenTiles(Action<TerrainTile> tileFunction)
	{
		int num = mapWindowTileY + noOfTilesToDisplayVertically;
		int num2 = mapWindowTileX + noOfTilesToDisplayHorizontally;
		int num3 = mapWindowTileY;
		int num4 = mapWindowTileX;
		for (int i = num3; i < num; i++)
		{
			for (int j = num4; j < num2; j++)
			{
				tileFunction(The.Map.GetTile(j, i));
			}
		}
	}

	public void DrawCrops(Allegiance allegiance, ResourceType resourcetype)
	{
		Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Black * 0.25f;
		ResourceMap cropsMap = allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(resourcetype);
		byte[][] outValues = null;
		if (cropsMap.GetMap(ref outValues) == ResourceMap.Result.OK)
		{
			DrawByteMap(ref color, outValues);
		}
	}

	private void DrawByteMap(ref Microsoft.Xna.Framework.Color color, byte[][] byteMap)
	{
		int num = mapWindowTileY + noOfTilesToDisplayVertically;
		int num2 = mapWindowTileX + noOfTilesToDisplayHorizontally;
		int num3 = mapWindowTileY;
		int num4 = mapWindowTileX;
		int num5 = 48;
		for (int i = num3; i < num; i++)
		{
			for (int j = num4; j < num2; j++)
			{
				Vector2 topLeft = TileEdgeToScreen(j, i);
				byte b = byteMap[j][i];
				if (b > 0)
				{
					float val = (float)(int)b / 4f;
					val = Math.Min(1f, val);
					color = Microsoft.Xna.Framework.Color.Crimson * val;
					Shape.Box(topLeft, new Vector2(topLeft.X + (float)num5, topLeft.Y + (float)num5), color, solid: true);
				}
			}
		}
	}

	public void Draw()
	{
		if (oldMapWindowWorldPosition != MapWindowWorldPosition)
		{
			isScrolling = true;
			The.Client.MapHasMoved = true;
		}
		else
		{
			isScrolling = false;
		}
		oldMapWindowWorldPosition = MapWindowWorldPosition;
		UpdateHUD();
	}

	public void DrawPathSearch()
	{
		Entity entity = null;
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		}
		else if (The.InGameUI.UIAllegiance != null && The.InGameUI.UIAllegiance.MembersList.Count > 0)
		{
			entity = The.InGameUI.UIAllegiance.MembersList[0];
		}
		if (entity != null && entity.Intelligence != null && entity.Intelligence.PathPlanner != null)
		{
			AStarSearch search = entity.Intelligence.PathPlanner.search;
			if (search != null && !search.Stopped)
			{
				search.Draw();
			}
		}
	}

	public void Update(GameTime time)
	{
		if (!The.Client.IsModal)
		{
			UpdateScrolling(time);
		}
	}

	private void UpdateHUD()
	{
		if (isScrolling)
		{
			The.InGameUI.MoveHUDWindows();
			The.InGameUI.MoveFogMap();
		}
	}

	public void DrawCollisionGeometries()
	{
		if (The.CollisionManager == null)
		{
			return;
		}
		List<Collidable<Entity>> collidablesList = new List<Collidable<Entity>>();
		CollideShape2D bounds = new CollideShape2D(mapWindowWorldPosition, mapWindowWorldPosition + new Vector2(mapWindowWidth, mapWindowHeight));
		The.CollisionManager.GetCollidablesIntersectingBounds(bounds, ref collidablesList);
		foreach (Collidable<Entity> item in collidablesList)
		{
			Entity parent = item.Parent;
			Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Orange;
			if (parent.EntityType.Person != null)
			{
				color = Microsoft.Xna.Framework.Color.DarkCyan;
			}
			else if (parent.EntityType.ItemType != null)
			{
				color = Microsoft.Xna.Framework.Color.Cornsilk;
			}
			if (parent.EntityType.LocomotorType != null && parent.Locomotor.IsColliding)
			{
				color = Microsoft.Xna.Framework.Color.Yellow;
			}
			DrawCollisionShape(item.Parent.Collidable, color);
		}
	}

	public void DrawSelectionShapes()
	{
		foreach (EntityID allStructure in The.Sim.AllStructures)
		{
			Entity entity = Entity.FindByID(allStructure);
			if (entity != null)
			{
				DrawCollisionShape(entity.SelectionShape, Microsoft.Xna.Framework.Color.LightYellow, onlyEnabled: false);
			}
		}
		foreach (EntityID allTerrainEntity in The.Sim.AllTerrainEntities)
		{
			Entity entity2 = Entity.FindByID(allTerrainEntity);
			if (entity2 != null)
			{
				DrawCollisionShape(entity2.SelectionShape, Microsoft.Xna.Framework.Color.LightGoldenrodYellow, onlyEnabled: false);
			}
		}
	}

	public void DrawTerrainGeometries()
	{
		foreach (EntityID allStructure in The.Sim.AllStructures)
		{
			Entity entity = Entity.FindByID(allStructure);
			if (entity != null)
			{
				DrawCollisionShape(entity.Collidable, Microsoft.Xna.Framework.Color.LightPink, onlyEnabled: false);
			}
		}
		foreach (EntityID allTerrainEntity in The.Sim.AllTerrainEntities)
		{
			Entity entity2 = Entity.FindByID(allTerrainEntity);
			if (entity2 != null)
			{
				DrawCollisionShape(entity2.Collidable, Microsoft.Xna.Framework.Color.LightPink, onlyEnabled: false);
			}
		}
	}

	private void DrawCollisionShape(Collidable<Entity> shape, Microsoft.Xna.Framework.Color color, bool onlyEnabled = true)
	{
		if (shape == null || (onlyEnabled && !shape.Enabled))
		{
			return;
		}
		if (shape.IsComposite)
		{
			bool isSelected = The.InGameUI.SelectedEntity == shape.Parent.EntityID;
			int selectedShapeIndex = 0;
			if (isSelected)
			{
				if ((strobe++ & 8) != 0)
				{
					color = Microsoft.Xna.Framework.Color.Black;
				}
				if (inputData.IsKeyDown(Keys.F1))
				{
					selectedShapeIndex = 1;
				}
				if (inputData.IsKeyDown(Keys.F2))
				{
					selectedShapeIndex = 2;
				}
				if (inputData.IsKeyDown(Keys.F3))
				{
					selectedShapeIndex = 3;
				}
				if (inputData.IsKeyDown(Keys.F4))
				{
					selectedShapeIndex = 4;
				}
				if (inputData.IsKeyDown(Keys.NumPad7))
				{
					shape.NudgeChildShape(selectedShapeIndex, -1, -1, 0);
				}
				else if (inputData.IsKeyDown(Keys.NumPad9))
				{
					shape.NudgeChildShape(selectedShapeIndex, 1, -1, 0);
				}
				else if (inputData.IsKeyDown(Keys.NumPad1))
				{
					shape.NudgeChildShape(selectedShapeIndex, -1, 1, 0);
				}
				else if (inputData.IsKeyDown(Keys.NumPad3))
				{
					shape.NudgeChildShape(selectedShapeIndex, 1, 1, 0);
				}
				else
				{
					if (inputData.IsKeyDown(Keys.NumPad8))
					{
						shape.NudgeChildShape(selectedShapeIndex, 0, -1, 0);
					}
					else if (inputData.IsKeyDown(Keys.NumPad2) || inputData.IsKeyDown(Keys.NumPad5))
					{
						shape.NudgeChildShape(selectedShapeIndex, 0, 1, 0);
					}
					if (inputData.IsKeyDown(Keys.NumPad4))
					{
						shape.NudgeChildShape(selectedShapeIndex, -1, 0, 0);
					}
					else if (inputData.IsKeyDown(Keys.NumPad6))
					{
						shape.NudgeChildShape(selectedShapeIndex, 1, 0, 0);
					}
				}
				if (inputData.IsKeyDown(Keys.Subtract))
				{
					shape.NudgeChildShape(selectedShapeIndex, 0, 0, -1);
				}
				else if (inputData.IsKeyDown(Keys.Add))
				{
					shape.NudgeChildShape(selectedShapeIndex, 0, 0, 1);
				}
				if (inputData.IsKeyDown(Keys.Divide))
				{
					shape.NudgeChildShape(selectedShapeIndex, 0, 0, 0, -2);
				}
				else if (inputData.IsKeyDown(Keys.Multiply))
				{
					shape.NudgeChildShape(selectedShapeIndex, 0, 0, 0, 2);
				}
			}
			shape.IterateChildShapes(delegate(CollideShape2D s, int idx)
			{
				DrawShapeDelegate(s, color, isSelected, idx == selectedShapeIndex);
			});
			if (shape.Parent.EntityType.TerrainType == null)
			{
				Microsoft.Xna.Framework.Color colour = Microsoft.Xna.Framework.Color.White;
				if (isSelected && shape.FlipHorizontally)
				{
					colour = Microsoft.Xna.Framework.Color.Chartreuse;
					DevText.Print(WorldPosToScreen(shape.Bounds.BoundsLowerLeft), "FLIPPED HORIZ.", colour);
				}
				Shape.Box(WorldPosToScreen(shape.Bounds.BoundsUpperLeft), WorldPosToScreen(shape.Bounds.BoundsLowerRight), colour, solid: false);
			}
		}
		else
		{
			DrawShapeDelegate(shape.Bounds, color);
		}
	}

	private static bool DrawShapeDelegate(CollideShape2D shape, Microsoft.Xna.Framework.Color c, bool isSelected = false, bool drawCoords = true)
	{
		Microsoft.Xna.Framework.Color color = c;
		if (shape.HFlipped && isSelected && color != Microsoft.Xna.Framework.Color.Black)
		{
			color = Microsoft.Xna.Framework.Color.Chartreuse;
		}
		Vector2 vector = Vector2.Zero;
		if (shape.Parent != null)
		{
			vector = shape.Parent.Center;
		}
		float radius = shape.Radius;
		Vector2 worldPos = shape.Center + shape.Offset;
		worldPos += vector;
		worldPos = The.MapUI.WorldPosToScreen(worldPos);
		if (shape.PrimitiveType == CollidePrim.Circle)
		{
			Shape.Circle(worldPos, radius, color);
		}
		if (isSelected && drawCoords)
		{
			if (color == Microsoft.Xna.Framework.Color.Black)
			{
				color = Microsoft.Xna.Framework.Color.LightGray;
			}
			string text = "X: " + shape.Offset.X + ", Y: " + shape.Offset.Y;
			if (shape.PrimitiveType == CollidePrim.Circle)
			{
				text = text + ", R: " + radius;
			}
			if (shape.PrimitiveType == CollidePrim.Rectangle)
			{
				text = text + ", W: " + shape.GetWidth() + ", H: " + shape.GetHeight();
			}
			DevText.Print(worldPos - Vector2.One, text, Microsoft.Xna.Framework.Color.Black);
			DevText.Print(worldPos + Vector2.One, text, Microsoft.Xna.Framework.Color.Black);
			DevText.Print(worldPos, text, color);
			DevText.Print(worldPos, text, color);
		}
		vector = The.MapUI.WorldPosToScreen(vector);
		Shape.Box(shape.BoundsUpperLeft + shape.Offset + vector, shape.BoundsLowerRight + shape.Offset + vector, Microsoft.Xna.Framework.Color.Brown, solid: false);
		return true;
	}

	public void DrawExpeditionScoutingRadius()
	{
		foreach (Allegiance allegiance in The.Sim.PlaySite.Allegiances)
		{
			foreach (Expedition expedition in allegiance.Expeditions)
			{
				Vector2 center = WorldPosToScreen(expedition.Location.Value);
				float radius = allegiance.GetForageAndHuntingRadius();
				if (allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction == 0f)
				{
					continue;
				}
				if (The.InGameUI.SelectedEntity.HasValue)
				{
					Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
					if (entity != null && entity.AllegianceID.HasValue && entity.AllegianceID == expedition.Allegiance.ID)
					{
						Shape.Circle(center, radius, Microsoft.Xna.Framework.Color.Chartreuse);
						continue;
					}
				}
				Shape.Circle(center, radius, Microsoft.Xna.Framework.Color.Green);
			}
		}
	}

	public void DrawRanges()
	{
		List<Pair<Entity, Vector2>> resultsList = null;
		The.AgentQuadTree.GetEntitiesInRange(GetCenterOfScreenWorldLocation(), 2f / 3f * (float)mapWindowWidth, null, ref resultsList);
		if (resultsList == null)
		{
			return;
		}
		foreach (Pair<Entity, Vector2> item in resultsList)
		{
			Vector2 vector = WorldPosToScreen(item.First.PlaySiteLocation);
			float? coneWidth;
			float? coneLength;
			float attackRange = item.First.GetAttackRange(out coneWidth, out coneLength);
			if (attackRange > 0f)
			{
				Shape.Circle(vector, attackRange, Microsoft.Xna.Framework.Color.Yellow, dashed: true);
			}
			if (coneWidth.HasValue && coneLength.HasValue)
			{
				Vector2 vector2 = Common.AngleToVector(item.First.Rotation - 0.5f * MathHelper.ToRadians(coneWidth.Value));
				vector2.Normalize();
				Vector2 vector3 = Common.AngleToVector(item.First.Rotation + 0.5f * MathHelper.ToRadians(coneWidth.Value));
				vector3.Normalize();
				Shape.Line(vector, vector + coneLength.Value * vector2, Microsoft.Xna.Framework.Color.Orange);
				Shape.Line(vector, vector + coneLength.Value * vector3, Microsoft.Xna.Framework.Color.Orange);
			}
			float? aggroRange = item.First.GetAggroRange();
			if (aggroRange.HasValue && aggroRange.Value > 0f)
			{
				Shape.Circle(vector, aggroRange.Value, Microsoft.Xna.Framework.Color.Red);
			}
			float? assistanceRange = item.First.GetAssistanceRange();
			if (assistanceRange.HasValue && assistanceRange.Value > 0f)
			{
				Shape.Circle(vector, assistanceRange.Value, Microsoft.Xna.Framework.Color.Green);
			}
			float visionRange = item.First.GetVisionRange(The.Sim.DateAndTime.LightLevel);
			Shape.Circle(vector, visionRange, Microsoft.Xna.Framework.Color.White);
		}
	}

	public void DrawInterest()
	{
		foreach (Entity person in The.InGameUI.UIAllegiance.Persons)
		{
			Vector2? vector = null;
			if (person.Intelligence.EntityIDToLookAt != EntityID.Invalid)
			{
				Entity entity = Entity.FindByID(person.Intelligence.EntityIDToLookAt);
				if (entity != null)
				{
					vector = entity.PlaySiteLocation.ToVector2();
				}
			}
			if (person.Intelligence.LocationToLookAt.HasValue)
			{
				vector = person.Intelligence.LocationToLookAt.Value.ToVector2();
			}
			if (vector.HasValue)
			{
				Shape.Line(WorldPosToScreen(person.PlaySiteLocation), WorldPosToScreen(vector.Value), Microsoft.Xna.Framework.Color.Aqua);
			}
		}
	}

	public void DrawBullets()
	{
		for (int i = 0; i < bullets.Count; i++)
		{
			Vector2 start = WorldPosToScreen(bullets[i].Muzzle);
			start.Y -= 20f;
			Vector2 end = WorldPosToScreen(bullets[i].Impact);
			Shape.Line(start, end, bullets[i].Color1, bullets[i].Color2);
			bullets[i].Fade();
		}
	}

	public static void AddBullet(Vector3 muzzleLocation, Vector3 impactLocation, Microsoft.Xna.Framework.Color startColor, Microsoft.Xna.Framework.Color endColor)
	{
		bullets.Add(new Bullet(muzzleLocation, impactLocation, startColor, endColor));
	}

	public static void StartBulletEffect(BulletEffect bulletEffect, float? maxRange, Entity targetAsEntity, Entity attacker, bool hitTarget)
	{
		if (bulletEffect != null)
		{
			Vector3 vector2;
			Vector3 impactLocation;
			if (hitTarget)
			{
				Vector3 vector = targetAsEntity.PlaySiteLocation - attacker.PlaySiteLocation;
				vector.Normalize();
				vector2 = attacker.PlaySiteLocation + bulletEffect.MuzzleDistance * vector;
				impactLocation = targetAsEntity.PlaySiteLocation;
			}
			else
			{
				float num = maxRange ?? 200f;
				Vector3 vector3 = targetAsEntity.PlaySiteLocation - attacker.PlaySiteLocation;
				vector3.Normalize();
				vector3.X += (float)The.Client.ClientRandomGenerator.RandomNormalDistribution(0.0, 0.04);
				vector3.Y += (float)The.Client.ClientRandomGenerator.RandomNormalDistribution(0.0, 0.04);
				float num2 = The.Client.ClientRandomGenerator.RandomBetween(0.5f * num, 2f * num);
				vector2 = attacker.PlaySiteLocation + bulletEffect.MuzzleDistance * vector3;
				impactLocation = vector2 + vector3 * num2;
			}
			AddBullet(vector2, impactLocation, bulletEffect.StartColor, bulletEffect.EndColor);
		}
	}

	public void DrawMarkers()
	{
		if (inputData.IsKeyDown(Keys.S))
		{
			return;
		}
		foreach (KeyValuePair<string, Marker> visitorMarker in VisitorMarkers)
		{
			Marker value = visitorMarker.Value;
			DrawMarker(value);
		}
		foreach (KeyValuePair<object, List<Marker>> renderedObjectMarker in RenderedObjectMarkers)
		{
			foreach (Marker item in renderedObjectMarker.Value)
			{
				DrawMarker(item);
			}
		}
		ClearAllDebugMarkers();
	}

	private void DrawMarker(Marker m)
	{
		Vector2 vector = WorldPosToScreen(m.loc);
		Vector2 bottomRight = vector;
		float num = m.size / 2f;
		vector.X -= num;
		vector.Y -= num;
		bottomRight.X += num;
		bottomRight.Y += num;
		Shape.Box(vector, bottomRight, m.color, solid: true);
	}

	public static void AddVisitorMarker(Vector3 l, Microsoft.Xna.Framework.Color c, object key, float size = 5f)
	{
		string key2 = key.GetHashCode().ToString() + hash++;
		VisitorMarkers.Add(key2, new Marker(l, c, size));
	}

	public void AddDebugMarker(Vector3 l, Microsoft.Xna.Framework.Color c, object key, float size = 5f)
	{
		if (!RenderedObjectMarkers.TryGetValue(key, out var value))
		{
			value = new List<Marker>();
			RenderedObjectMarkers.Add(key, value);
		}
		value.Add(new Marker(l, c, size));
	}

	public static void ClearAllVisitorMarkers(object key)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Marker> visitorMarker in VisitorMarkers)
		{
			if (visitorMarker.Key.StartsWith(key.GetHashCode().ToString()))
			{
				list.Add(visitorMarker.Key);
			}
		}
		foreach (string item in list)
		{
			VisitorMarkers.Remove(item);
		}
	}

	public void ClearAllDebugMarkers()
	{
		RenderedObjectMarkers.Clear();
	}

	public void DrawJobs()
	{
		Microsoft.Xna.Framework.Color startColor = Microsoft.Xna.Framework.Color.White;
		Microsoft.Xna.Framework.Color endColor;
		HaulingJob haulingJob;
		Vector2 from;
		Vector2 to;
		LookUp<EntityGroup, EntityGroupID>.IterateMembers(delegate(EntityGroup owner)
		{
			if (!owner.Parent.Allegiance.Site.IsPlaySite || !owner.Parent.Location.HasValue)
			{
				return;
			}
			if (owner.Parent is Person)
			{
				startColor = new Microsoft.Xna.Framework.Color(255, 0, The.Client.ClientRandomGenerator.Next(100, "MapClient", saveMessage: false));
				if (!((Person)owner.Parent).Parent.Location.HasValue)
				{
					return;
				}
			}
			else if (owner.Parent is Household)
			{
				startColor = new Microsoft.Xna.Framework.Color(0, 255, The.Client.ClientRandomGenerator.Next(100, "MapClient", saveMessage: false));
			}
			else if (owner.Parent is Expedition)
			{
				startColor = new Microsoft.Xna.Framework.Color(0, The.Client.ClientRandomGenerator.Next(50, "MapClient", saveMessage: false), 255);
			}
			Shape.Box(WorldPosToScreen(owner.Parent.Location.Value) - new Vector2(5f, 5f), WorldPosToScreen(owner.Parent.Location.Value) + new Vector2(5f, 5f), startColor, solid: true);
			endColor = startColor;
			endColor *= 0.6f;
			foreach (Job haulingJob2 in owner.HaulingJobs)
			{
				haulingJob = haulingJob2 as HaulingJobSpecificItem;
				if (haulingJob != null)
				{
					Entity entity = Entity.FindByID(haulingJob.Item.Value);
					if (entity != null)
					{
						from = WorldPosToScreen(entity.PlaySiteLocation);
						Vector3 worldPos = (haulingJob.ToLocation.HasValue ? haulingJob.ToLocation.Value : (Entity.FindByID(haulingJob.ToStorage.Value.StorageEntity)?.AccessPoint.Value ?? entity.PlaySiteLocation));
						to = WorldPosToScreen(worldPos);
						Shape.Line(from, to, startColor, endColor);
						if (haulingJob.TakenBy.Count > 0)
						{
							to = WorldPosToScreen(haulingJob.TakenBy.Get(0).PlaySiteLocation);
							Shape.Line(from, to, Microsoft.Xna.Framework.Color.White);
						}
					}
				}
			}
			foreach (Job scoutingJob2 in owner.ScoutingJobs)
			{
				if (scoutingJob2 is ScoutingJob scoutingJob)
				{
					if (scoutingJob.Location.HasValue)
					{
						from = WorldPosToScreen(scoutingJob.Location.Value);
					}
					else
					{
						TerrainTile bottomLeftTile = scoutingJob.Zone.MapArea.BottomLeftTile;
						from = WorldPosToScreen(new Vector3(bottomLeftTile.X * 48, bottomLeftTile.Y * 48, 0f));
					}
					DrawJob(ref from, scoutingJob2, Microsoft.Xna.Framework.Color.Aqua);
				}
			}
			foreach (Job otherJob in owner.OtherJobs)
			{
				Vector3? circaLocation = otherJob.GetCircaLocation();
				if (circaLocation.HasValue)
				{
					Vector2 from2 = circaLocation.Value.ToVector2();
					DrawJob(ref from2, otherJob, Microsoft.Xna.Framework.Color.Orange);
				}
			}
			foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in owner.ProductionJobs)
			{
				foreach (ProcessJob item in productionJob.Value)
				{
					ProcessJob processJob = item;
					if (processJob != null)
					{
						if (processJob.GetCurrentJobLocation(out var location) && location.HasValue)
						{
							from = WorldPosToScreen(location.Value);
							Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), Microsoft.Xna.Framework.Color.Yellow, solid: true);
							for (int i = 0; i < item.TakenBy.Count; i++)
							{
								to = WorldPosToScreen(item.TakenBy.Get(i).PlaySiteLocation);
								Shape.Line(from, to, Microsoft.Xna.Framework.Color.White);
							}
						}
						if (processJob.HarvestJob != null)
						{
							from = WorldPosToScreen(processJob.HarvestJob.Item.Container.AccessPoint);
							Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), Microsoft.Xna.Framework.Color.Yellow, solid: true);
							for (int j = 0; j < item.TakenBy.Count; j++)
							{
								to = WorldPosToScreen(item.TakenBy.Get(j).PlaySiteLocation);
								Shape.Line(from, to, Microsoft.Xna.Framework.Color.White);
							}
						}
					}
				}
			}
		});
		foreach (Allegiance allegiance in The.Sim.PlaySite.Allegiances)
		{
			if (allegiance.AllegianceType == AllegianceType.Player)
			{
				startColor = new Microsoft.Xna.Framework.Color(255, 255, 0);
			}
			else
			{
				startColor = new Microsoft.Xna.Framework.Color(155, 155, 0);
			}
			foreach (Job threatJob in allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs)
			{
				ThreatJob attackJob = threatJob as ThreatJob;
				DrawAttackJob(ref startColor, allegiance, attackJob);
			}
			foreach (Job assetThreatJob in allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobs)
			{
				ThreatJob attackJob = assetThreatJob as ThreatJob;
				DrawAttackJob(ref startColor, allegiance, attackJob);
			}
		}
		int y = mapWindowTileY + noOfTilesToDisplayVertically + 1;
		int x = mapWindowTileX + noOfTilesToDisplayHorizontally + 1;
		y = The.Map.ClampTileMapYPosition(y);
		x = The.Map.ClampTileMapXPosition(x);
		int y2 = mapWindowTileY - 1;
		int x2 = mapWindowTileX - 1;
		x2 = The.Map.ClampTileMapXPosition(x2);
		y2 = The.Map.ClampTileMapYPosition(y2);
		for (int num = y2; num <= y; num++)
		{
			for (int num2 = x2; num2 <= x; num2++)
			{
				TerrainTile tile = The.Map.GetTile(num2, num);
				if (tile.HarvestJobs == null)
				{
					continue;
				}
				int num3 = tile.HarvestJobs.Sum((KeyValuePair<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>> o) => o.Value.Sum((KeyValuePair<ResourceType, List<ProcessJob>> r) => r.Value.Count));
				if (num3 > 0)
				{
					DevText.Print(TileEdgeToScreen(num2, num), num3.ToString(), Microsoft.Xna.Framework.Color.White);
				}
			}
		}
	}

	private void DrawAttackJob(ref Microsoft.Xna.Framework.Color startColor, Allegiance allegiance, AttackJob attackJob)
	{
		if (attackJob.Target.HasValue)
		{
			allegiance.SharedKnowledge.GetKnownData(attackJob.Target.Value, out var data);
			if (data != null)
			{
				Vector2 from = WorldPosToScreen(data.PlaySiteLocation);
				DrawJob(ref from, attackJob, startColor);
			}
		}
	}

	private void DrawJob(ref Vector2 from, Job job, Microsoft.Xna.Framework.Color color)
	{
		Shape.Box(from - new Vector2(5f, 5f), from + new Vector2(5f, 5f), color, solid: true);
		for (int i = 0; i < job.TakenBy.Count; i++)
		{
			Vector2 end = WorldPosToScreen(job.TakenBy.Get(i).PlaySiteLocation);
			Shape.Line(from, end, Microsoft.Xna.Framework.Color.White);
		}
	}

	public void DrawSectorLines()
	{
		int x = mapWindowTileX + noOfTilesToDisplayHorizontally;
		int y = mapWindowTileY + noOfTilesToDisplayVertically;
		MapManager.TileEdgeToSubtile(new Microsoft.Xna.Framework.Point(mapWindowTileX, mapWindowTileY));
		MapManager.TileEdgeToSubtile(new Microsoft.Xna.Framework.Point(x, y));
		RectangleF screenRect = ScreenRect;
		int num = 768;
		for (int i = 0; (float)i < The.Map.MapWorldHeight; i += num)
		{
			for (int j = 0; (float)j < The.Map.MapWorldWidth; j += num)
			{
				RectangleF rectangleF = new RectangleF(j, i, num, num);
				if (rectangleF.IntersectsWith(screenRect))
				{
					Shape.Box(WorldPosToScreen(new Vector2(rectangleF.X, rectangleF.Y)), WorldPosToScreen(new Vector2(rectangleF.Right, rectangleF.Bottom)), Microsoft.Xna.Framework.Color.White, solid: false);
					Shape.Box(WorldPosToScreen(new Vector2(rectangleF.X - 1f, rectangleF.Y - 1f)), WorldPosToScreen(new Vector2(rectangleF.Right - 1f, rectangleF.Bottom - 1f)), Microsoft.Xna.Framework.Color.White, solid: false);
				}
			}
		}
	}

	public void DrawRegionMapOverlay(RegionMap regionMap)
	{
		bool flag = false;
		if (Kensei.Dev.Options.GetOption("Overlays.Render region maps in progress"))
		{
			flag = true;
		}
		int num = 16;
		byte a = 100;
		Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
		black.A = a;
		byte b = 70;
		byte b2 = 100;
		int x = mapWindowTileX + noOfTilesToDisplayHorizontally;
		int y = mapWindowTileY + noOfTilesToDisplayVertically;
		Microsoft.Xna.Framework.Point point = MapManager.TileEdgeToSubtile(new Microsoft.Xna.Framework.Point(mapWindowTileX, mapWindowTileY));
		Microsoft.Xna.Framework.Point point2 = MapManager.TileEdgeToSubtile(new Microsoft.Xna.Framework.Point(x, y));
		a = ((point.Y % 2 != 0) ? b : b2);
		HashSet<UWGame.SimSide.Maps.Region> hashSet = new HashSet<UWGame.SimSide.Maps.Region>();
		for (int i = point.Y; i < point2.Y; i++)
		{
			for (int j = point.X; j < point2.X; j++)
			{
				Vector2 topLeft = SubtileEdgeToScreen(j, i);
				ushort num2 = (flag ? regionMap.GetRegionColorInProgress(j, i) : regionMap.GetRegionColor(j, i));
				_ = 63;
				if (num2 > 0)
				{
					UWGame.SimSide.Maps.Region region = (flag ? regionMap.GetRegionInProgress(num2) : regionMap.GetRegion(num2));
					hashSet.Add(region);
					black = region.DebugColor;
					if (num2 < 40000 && regionMap is DependentRegionMap)
					{
						black = GetFadedColor(black);
					}
					black.A = a;
				}
				else
				{
					black = Microsoft.Xna.Framework.Color.Transparent;
				}
				Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), black, solid: true);
				a = ((a == b2) ? b : b2);
			}
			a = ((a == b2) ? b : b2);
		}
		foreach (UWGame.SimSide.Maps.Region item in hashSet)
		{
			_ = item.Color;
			_ = 63;
			Vector2 topLeft = WorldPosToScreen(item.CenterLocation);
			DevText.Print(topLeft, item.Color.ToString());
			Dictionary<ushort, RegionEdge> value;
			if (!flag)
			{
				regionMap.RegionGraph.TryGetValue(item.Color, out value);
			}
			else
			{
				regionMap.newRegionGraph.TryGetValue(item.Color, out value);
			}
			if (value == null)
			{
				continue;
			}
			foreach (KeyValuePair<ushort, RegionEdge> item2 in value)
			{
				UWGame.SimSide.Maps.Region region2 = (flag ? regionMap.GetRegionInProgress(item2.Key) : regionMap.GetRegion(item2.Key));
				Vector2 end = WorldPosToScreen(region2.CenterLocation);
				Shape.Line(topLeft, end, Microsoft.Xna.Framework.Color.Blue);
			}
		}
	}

	private static Microsoft.Xna.Framework.Color GetFadedColor(Microsoft.Xna.Framework.Color color)
	{
		color *= 0.2f;
		return color;
	}

	public void DrawTerrainCosts(SurfaceType.TransportType transport)
	{
		int num = 16;
		byte a = 100;
		byte b = 70;
		byte b2 = 100;
		Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
		black.A = a;
		int num2 = 3 * (mapWindowTileY + noOfTilesToDisplayVertically);
		int num3 = 3 * (mapWindowTileX + noOfTilesToDisplayHorizontally);
		SubtileLayers subtileLayers = The.Map.TerrainCosts[transport];
		int num4 = mapWindowTileY * 3;
		int num5 = mapWindowTileX * 3;
		a = ((num4 % 2 != 0) ? b : b2);
		for (int i = num4; i < num2; i++)
		{
			for (int j = num5; j < num3; j++)
			{
				Vector2 topLeft = SubtileEdgeToScreen(j, i);
				MapManager.SubtileValue value = subtileLayers.GetValue(j, i);
				switch (MapManager.GetCost(value))
				{
				case 0:
					black = Microsoft.Xna.Framework.Color.Maroon;
					black.A = a;
					break;
				case 1:
					black = Microsoft.Xna.Framework.Color.LightBlue;
					black.A = a;
					break;
				case 2:
					black = Microsoft.Xna.Framework.Color.Purple;
					black.A = a;
					break;
				case 3:
				{
					bool flag = MapManager.TestForFlag(value, MapManager.SubtileValue.Pad);
					bool flag2 = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);
					if (flag && flag2)
					{
						black = Microsoft.Xna.Framework.Color.LightYellow;
						black.A = a;
						break;
					}
					if (flag)
					{
						black = Microsoft.Xna.Framework.Color.Yellow;
						black.A = a;
						break;
					}
					if (!flag2)
					{
						continue;
					}
					black = Microsoft.Xna.Framework.Color.White;
					black.A = a;
					break;
				}
				case 4:
					black = Microsoft.Xna.Framework.Color.Magenta;
					black.A = 200;
					break;
				case 5:
				case 6:
				case 8:
					black = Microsoft.Xna.Framework.Color.DarkBlue;
					black.A = 11;
					break;
				default:
					black = Microsoft.Xna.Framework.Color.Black;
					black.A = a;
					break;
				}
				Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), black, solid: true);
				a = ((a == b2) ? b : b2);
			}
			a = ((a == b2) ? b : b2);
		}
		for (int k = mapWindowTileY; k < mapWindowTileY + noOfTilesToDisplayVertically; k++)
		{
			Vector2 topLeft = TileEdgeToScreen(mapWindowTileX, k);
			Vector2 end = TileEdgeToScreen(mapWindowTileX + noOfTilesToDisplayHorizontally, k);
			Shape.Line(topLeft, end, Microsoft.Xna.Framework.Color.Black, Microsoft.Xna.Framework.Color.Black);
			DevText.Print(topLeft, k.ToString(), Microsoft.Xna.Framework.Color.White);
		}
		for (int l = mapWindowTileX; l < mapWindowTileX + noOfTilesToDisplayHorizontally; l++)
		{
			Vector2 topLeft = TileEdgeToScreen(l, mapWindowTileY);
			Vector2 end = TileEdgeToScreen(l, mapWindowTileY + noOfTilesToDisplayVertically);
			Shape.Line(topLeft, end, Microsoft.Xna.Framework.Color.Black, Microsoft.Xna.Framework.Color.Black);
			DevText.Print(topLeft, l.ToString(), Microsoft.Xna.Framework.Color.White);
		}
	}

	public void SetFogOfWarFadeRate(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue)
		{
			fogOfWarFadeRate = newFloat.Value;
		}
	}

	public void SetFogOfWarTint(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue)
		{
			fogOfWarTint = newFloat.Value;
		}
	}

	public void SetCloudEdgeHardness(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue)
		{
			cloudSharpness = newFloat.Value;
		}
	}

	public void SetCloudOpacity(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue)
		{
			cloudOpacity = newFloat.Value;
		}
	}

	public void ShowFootRegionMap_OnPress(string option, bool? newBool, float? newFloat)
	{
	}

	public void ShowFoot_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", boolToSet: false);
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", boolToSet: false);
		}
	}

	public void ShowATV_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Foot)", boolToSet: false);
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", boolToSet: false);
		}
	}

	public void ShowCar_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", boolToSet: false);
			Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Foot)", boolToSet: false);
		}
	}

	public void RegionsFoot_OnPress(string option, bool? newBool, float? newFloat)
	{
	}

	public void DrawResourceOverlays()
	{
		if (The.Sim.ResourceNoiseSeeds == null)
		{
			return;
		}
		foreach (ResourceType item in ResourceOverlays)
		{
			Microsoft.Xna.Framework.Color color = item.Color ?? Microsoft.Xna.Framework.Color.Red;
			if (The.Sim.ResourceNoiseSeeds.TryGetValue(item, out var noise))
			{
				IterateOnScreenTiles(delegate(TerrainTile tile)
				{
					DrawResourceNoiseOnTile(new Microsoft.Xna.Framework.Point(tile.X, tile.Y), color, noise);
				});
			}
			IterateOnScreenTiles(delegate(TerrainTile tile)
			{
				PrintResourceAmountOnTile(tile, item, color);
			});
		}
	}

	public void DrawDebugInfo()
	{
		bool flag = false;
		if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (Foot)"))
		{
			DrawTerrainCosts(SurfaceType.TransportType.Foot);
			flag = true;
		}
		else if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (ATV)"))
		{
			DrawTerrainCosts(SurfaceType.TransportType.OffRoad);
			flag = true;
		}
		else if (Kensei.Dev.Options.GetOption("Overlays.Terrain costs (Car)"))
		{
			DrawTerrainCosts(SurfaceType.TransportType.Car);
			flag = true;
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Region map (Terrain/Foot)"))
		{
			DrawRegionMapOverlay(The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap);
			flag = true;
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Crops"))
		{
			string key = "item:blackpulp";
			if (GameData.Instance.AllItemTypes.TryGetValue(key, out var value) && GameData.Instance.ItemHarvestSource.TryGetValue(value, out var value2))
			{
				DrawCrops(The.InGameUI.UIAllegiance, value2);
			}
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Allegiances"))
		{
			DrawAllegiances();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Jobs"))
		{
			DrawJobs();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Path search"))
		{
			DrawPathSearch();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Ranges"))
		{
			DrawRanges();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Expedition Scouting Radius"))
		{
			DrawExpeditionScoutingRadius();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Interest"))
		{
			DrawInterest();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Markers"))
		{
			DrawMarkers();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.CollisionGeometries"))
		{
			DrawCollisionGeometries();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.TerrainGeometries"))
		{
			DrawTerrainGeometries();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.SelectionShapes"))
		{
			DrawSelectionShapes();
		}
		if (The.MapUI.DrawCostOverlays() || flag)
		{
			DrawSectorLines();
		}
		if (Kensei.Dev.Options.GetOption("Overlays.Sensor tiles"))
		{
			DrawSensorTiles();
		}
		DrawResourceOverlays();
	}

	private void DrawSensorTiles()
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity);
		Microsoft.Xna.Framework.Color cyan = Microsoft.Xna.Framework.Color.Cyan;
		if (entity == null || !entity.Find<Sensor>(out var c))
		{
			return;
		}
		foreach (TerrainTile item in c.tilesCurrentlySeen)
		{
			DrawTileOverlay(new Microsoft.Xna.Framework.Point(item.X, item.Y), cyan, 1f);
		}
	}

	public bool DrawCostOverlays()
	{
		Entity entity = null;
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		}
		bool result = false;
		foreach (object overlay in Overlays)
		{
			if (overlay is RegionMap regionMap)
			{
				DrawRegionMapOverlay(regionMap);
				result = true;
			}
			if (overlay is InfluenceMap influenceMap)
			{
				if (overlay is ThreatMap threatMap)
				{
					threatMapLimits.Clear();
					if (threatMap.Approach != ThreatStance.Bold && entity != null)
					{
						GetThreatLimits(threatMapLimits, entity.Intelligence.PanicLevel);
					}
					DrawDiscomfortOrInfluenceMapOverlay(threatMap.Map, threatMapLimits);
				}
				else
				{
					DrawDiscomfortOrInfluenceMapOverlay(influenceMap.Map);
				}
				result = false;
			}
			if (overlay is DiscomfortMap discomfortMap)
			{
				DrawDiscomfortOrInfluenceMapOverlay(discomfortMap.Map);
				result = false;
			}
			if (overlay is MovementMap moveMap)
			{
				DrawMoveMapOverlay(moveMap);
				result = true;
			}
		}
		return result;
	}

	public static void GetThreatLimits(List<OverlayLimit> threatMapLimits, int panicLevel)
	{
		threatMapLimits.Add(new OverlayLimit
		{
			Color = Microsoft.Xna.Framework.Color.Blue,
			Edge = (int)GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart
		});
		threatMapLimits.Add(new OverlayLimit
		{
			Color = Microsoft.Xna.Framework.Color.Violet,
			Edge = (int)GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue
		});
		threatMapLimits.Add(new OverlayLimit
		{
			Color = Microsoft.Xna.Framework.Color.Orange,
			Edge = panicLevel
		});
		threatMapLimits.Add(new OverlayLimit
		{
			Color = Microsoft.Xna.Framework.Color.Yellow,
			Edge = 1000f
		});
	}

	private void DrawMoveMapOverlay(MovementMap moveMap)
	{
		int num = 16;
		byte b = 100;
		byte b2 = 70;
		byte b3 = 100;
		Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Black;
		color.A = b;
		int num2 = 3 * (mapWindowTileY + noOfTilesToDisplayVertically);
		int num3 = 3 * (mapWindowTileX + noOfTilesToDisplayHorizontally);
		SubtileLayers subtileLayers = moveMap.Layers[SurfaceType.TransportType.Foot];
		int num4 = mapWindowTileY * 3;
		int num5 = mapWindowTileX * 3;
		for (int i = num4; i < num2; i++)
		{
			for (int j = num5; j < num3; j++)
			{
				Vector2 topLeft = SubtileEdgeToScreen(j, i);
				int layerIndex;
				byte cost = MapManager.GetCost(subtileLayers.GetValue(j, i, out layerIndex));
				if (cost == 0)
				{
					color = Microsoft.Xna.Framework.Color.Red;
				}
				else
				{
					color.R = 0;
					color.B = (byte)Common.Clamp(cost * 50, 0, 255);
				}
				if (layerIndex == 0)
				{
					color = GetFadedColor(color);
				}
				color.A = b;
				Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), color, solid: true);
				b = ((b == b3) ? b2 : b3);
			}
			b = ((b == b3) ? b2 : b3);
		}
		TileSector[][] sectors = moveMap.Children[0].Child.Map.Sectors;
		for (int k = 0; k < Common.GetJaggedArrayWidth(sectors); k++)
		{
			for (int l = 0; l < Common.GetJaggedArrayHeight(sectors); l++)
			{
				TileSector tileSector = sectors[k][l];
				if (tileSector != null)
				{
					Vector2 start = TileEdgeToScreen(tileSector.TileArea.Left, tileSector.TileArea.Top);
					Vector2 end = TileEdgeToScreen(tileSector.TileArea.Right, tileSector.TileArea.Top);
					Shape.Line(start, end, Microsoft.Xna.Framework.Color.White, Microsoft.Xna.Framework.Color.White);
					Vector2 start2 = TileEdgeToScreen(tileSector.TileArea.Left, tileSector.TileArea.Bottom);
					end = TileEdgeToScreen(tileSector.TileArea.Right, tileSector.TileArea.Bottom);
					Shape.Line(start2, end, Microsoft.Xna.Framework.Color.White, Microsoft.Xna.Framework.Color.White);
					Vector2 start3 = TileEdgeToScreen(tileSector.TileArea.Left, tileSector.TileArea.Top);
					end = TileEdgeToScreen(tileSector.TileArea.Left, tileSector.TileArea.Bottom);
					Shape.Line(start3, end, Microsoft.Xna.Framework.Color.White, Microsoft.Xna.Framework.Color.White);
					Vector2 start4 = TileEdgeToScreen(tileSector.TileArea.Right, tileSector.TileArea.Top);
					end = TileEdgeToScreen(tileSector.TileArea.Right, tileSector.TileArea.Bottom);
					Shape.Line(start4, end, Microsoft.Xna.Framework.Color.White, Microsoft.Xna.Framework.Color.White);
				}
			}
		}
	}

	private void DrawDiscomfortOrInfluenceMapOverlay(TileLayer map, List<OverlayLimit> limits = null)
	{
		int num = 48;
		OverlayLimit overlayLimit = null;
		for (int i = 0; i < map.SectorsAcrossHeight; i++)
		{
			for (int j = 0; j < map.SectorsAcrossWidth; j++)
			{
				TileSector tileSector = map.Sectors[j][i];
				if (tileSector == null || !TileAreaIsOnScreen(tileSector.TileArea))
				{
					continue;
				}
				for (int k = 0; k < tileSector.TileArea.Height; k++)
				{
					for (int l = 0; l < tileSector.TileArea.Width; l++)
					{
						byte value = tileSector.GetValue((ushort)l, (ushort)k);
						if (limits != null && limits.Count > 0)
						{
							overlayLimit = Common.GetStairStepIndex((int)value, limits, out var _);
						}
						Microsoft.Xna.Framework.Color color = overlayLimit?.Color ?? Microsoft.Xna.Framework.Color.Blue;
						Microsoft.Xna.Framework.Color colour = color * Common.Clamp((float)(int)value / 50f, 0f, 1f);
						Vector2 topLeft = TileEdgeToScreen(l + tileSector.TileArea.Left, k + tileSector.TileArea.Top);
						Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
					}
				}
				Shape.Box(TileEdgeToScreen(tileSector.TileArea.Location), TileEdgeToScreen(tileSector.TileArea.Right, tileSector.TileArea.Bottom), Microsoft.Xna.Framework.Color.White, solid: false);
			}
		}
	}

	public void DrawAllegiances()
	{
		int num = 48;
		int num2 = mapWindowTileY + noOfTilesToDisplayVertically;
		int num3 = mapWindowTileX + noOfTilesToDisplayHorizontally;
		int num4 = mapWindowTileY;
		int num5 = mapWindowTileX;
		float num6 = 0.25f;
		for (int i = num4; i < num2; i++)
		{
			for (int j = num5; j < num3; j++)
			{
				TerrainTile tile = The.Map.GetTile(j, i);
				Vector2 topLeft = TileEdgeToScreen(j, i);
				if (tile.AllegiancesThatSeeThisTile == null)
				{
					continue;
				}
				foreach (Allegiance item in tile.AllegiancesThatSeeThisTile)
				{
					_ = item.ID;
					Microsoft.Xna.Framework.Color colour = item.DebugColor * num6;
					Shape.Box(topLeft, new Vector2(topLeft.X + (float)num, topLeft.Y + (float)num), colour, solid: true);
				}
			}
		}
	}

	public Vector2 SubtileEdgeToScreen(int x, int y)
	{
		return SubtileEdgeToScreen(new Microsoft.Xna.Framework.Point(x, y));
	}

	public Vector2 SubtileEdgeToScreen(Microsoft.Xna.Framework.Point subtile)
	{
		Vector3 worldPos = MapManager.SubTileEdgeToWorldPos3(subtile);
		return WorldPosToScreen(worldPos);
	}

	public Vector2 TileEdgeToScreen(Microsoft.Xna.Framework.Point tilePos)
	{
		return TileEdgeToScreen(tilePos.X, tilePos.Y);
	}

	public Vector2 TileEdgeToScreen(int x, int y)
	{
		Vector3 worldPos = MapManager.TileEdgeToWorldPos(new Microsoft.Xna.Framework.Point(x, y));
		return WorldPosToScreen(worldPos);
	}

	public Vector2 ScreenToWorldPos(int x, int y)
	{
		return new Vector2((float)x + MapWindowWorldPosition.X, (float)y + MapWindowWorldPosition.Y);
	}

	public Vector2 WorldPosToScreen(Vector3 worldPos)
	{
		return new Vector2(worldPos.X - MapWindowWorldPosition.X, worldPos.Y - MapWindowWorldPosition.Y);
	}

	public Vector2 WorldPosToScreen(Vector2 worldPos)
	{
		return worldPos - MapWindowWorldPosition;
	}

	public Microsoft.Xna.Framework.Point WorldPosToScreenPoint(Vector2 worldPos)
	{
		return new Microsoft.Xna.Framework.Point((int)(worldPos.X - MapWindowWorldPosition.X), (int)(worldPos.Y - MapWindowWorldPosition.Y));
	}

	public void TileCenterToScreen(int x, int y, out int xScreen, out int yScreen)
	{
		Vector3 location = MapManager.TileToWorldPos(new Microsoft.Xna.Framework.Point(x, y));
		Microsoft.Xna.Framework.Point point = WorldPosToScreenPoint(location.ToVector2());
		xScreen = point.X;
		yScreen = point.Y;
	}

	public void TileToScreen(int relativeTileX, int relativeTileY, out int xScreen, out int yScreen)
	{
		Vector3 vector = MapManager.TileToWorldPos(new Microsoft.Xna.Framework.Point(relativeTileX, relativeTileY));
		Vector2 vector2 = WorldPosToScreen(new Vector2(vector.X, vector.Y));
		xScreen = (int)vector2.X;
		yScreen = (int)vector2.Y;
	}

	public Vector2 TilePosToScreen(Microsoft.Xna.Framework.Point tilePos)
	{
		return MapManager.TileToWorldPosVector2(tilePos) - MapWindowWorldPosition;
	}

	public Vector2 GetCenterOfScreenWorldLocation()
	{
		Vector2 pos = new Vector2(MapWindowWorldPosition.X + (float)(mapWindowWidth / 2), MapWindowWorldPosition.Y + (float)(mapWindowHeight / 2));
		return The.Map.ClampWorldPosition(pos);
	}

	public bool WorldPositionIsOnScreen(Vector2 pos, float border)
	{
		if (pos.X >= MapWindowWorldPosition.X - border && pos.X <= MapWindowWorldPosition.X + (float)mapWindowWidth + border && pos.Y >= MapWindowWorldPosition.Y - border)
		{
			return pos.Y <= MapWindowWorldPosition.Y + (float)mapWindowHeight + border;
		}
		return false;
	}

	public bool WorldPositionIsOnScreen(Vector3 pos, float border)
	{
		if (pos.X >= MapWindowWorldPosition.X - border && pos.X <= MapWindowWorldPosition.X + (float)mapWindowWidth + border && pos.Y >= MapWindowWorldPosition.Y - border)
		{
			return pos.Y <= MapWindowWorldPosition.Y + (float)mapWindowHeight + border;
		}
		return false;
	}

	public bool ScreenPositionIsVisible(Vector2 screenPos, int border)
	{
		if (screenPos.X >= (float)(-border) && screenPos.X < (float)(mapWindowWidth + border) && screenPos.Y >= (float)(-border) && screenPos.Y < (float)(mapWindowHeight + border))
		{
			return true;
		}
		return false;
	}

	public bool TileIsOnScreen(int x, int y, int gutterSize = 0)
	{
		if (x >= mapWindowTileX - gutterSize && y >= mapWindowTileY - gutterSize && x <= mapWindowTileX + noOfTilesToDisplayHorizontally + gutterSize)
		{
			return y <= mapWindowTileY + noOfTilesToDisplayVertically + gutterSize;
		}
		return false;
	}

	public bool TileAreaIsOnScreen(Microsoft.Xna.Framework.Rectangle area)
	{
		return new Microsoft.Xna.Framework.Rectangle(mapWindowTileX, mapWindowTileY, noOfTilesToDisplayHorizontally, noOfTilesToDisplayVertically).Intersects(area);
	}
}
