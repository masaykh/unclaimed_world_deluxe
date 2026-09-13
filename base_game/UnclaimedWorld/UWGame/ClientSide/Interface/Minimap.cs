using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Map;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Trees;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class Minimap
{
	private Image jack;

	private Image grungeBottomLeft;

	private Image grungeTopLeft;

	private Image grungeTopRight;

	private Image displayDust;

	private Image buttonDust;

	private Box frame;

	public Window DisplayWindow;

	private Image minimap;

	private Image minimapLocationFrame;

	private CRTScreen crtScreen;

	private Regulator mapRegulator = new Regulator(The.Client.ClientRandomGenerator, 0.5, "Minimap");

	private Texture2D mapTexture;

	private Color[] mapTextureColors;

	private Color[] mapBackBuffer;

	private Texture2D frameTexture;

	private Color[] frameTextureColors;

	private Color deepWaterColor = new Color(36, 63, 52);

	private Color shallowWaterColor = new Color(65, 94, 83);

	private Color groundColor = new Color(161, 182, 174);

	private Color structureColor = Color.DarkOrchid;

	private Color unitColor = Color.Tomato;

	private Color fovColor = new Color(130, 148, 141);

	private int mapWidth;

	private int mapHeight;

	private float minimapScaleFactorX;

	private float minimapScaleFactorY;

	private bool draggingInMap;

	private int screenWidth;

	private int screenHeight;

	private bool frameIsDirty;

	private bool settingsAreDirty;

	private int currentMapTileRow;

	public Minimap(int screenWidth, int screenHeight, int screenX)
	{
		int num = screenHeight + StatusScreen.GetPlasticFrameHeight();
		int width = GetWidth(screenWidth);
		this.screenWidth = screenWidth;
		this.screenHeight = screenHeight;
		GUIManager gui = The.InGameUI.gui;
		_ = The.Sim.Controller.Game;
		int cornerSize = 30;
		DisplayWindow = new Window(gui);
		DisplayWindow.CornerSize = cornerSize;
		DisplayWindow.Margin = 0;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = false;
		DisplayWindow.Position = new Point(screenX, The.Client.Controller.DrawArea.Height - num);
		DisplayWindow.WindowSize = new Vector2(width, num);
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.HasCRTOrLCDComponents = true;
		DisplayWindow.HasOverlayComponents = true;
		DisplayWindow.Destroyed += DisplayWindow_Destroyed;
		displayDust = new Image(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		displayDust.SetSkinLocation(SkinState.Normal, sourceRectangle);
		displayDust.Alpha = 0.04f;
		DisplayWindow.Add(displayDust);
		displayDust.Position = new Point(0, 0);
		displayDust.ResizeControlToFitImage();
		CreateMap();
		int num2 = 7;
		int num3 = 7;
		minimap = new Image(gui);
		minimap.Texture = mapTexture;
		DisplayWindow.Add(minimap);
		minimap.Position = new Point(num2 + 7, num3 + 7);
		minimap.Width = screenWidth;
		minimap.Height = screenHeight;
		minimap.ScaleImageToSizeOfControl = true;
		minimap.RenderType = RenderType.CRTAndLCD;
		minimap.DebugTag = "minimap";
		minimapLocationFrame = new Image(gui);
		minimapLocationFrame.Texture = frameTexture;
		DisplayWindow.Add(minimapLocationFrame);
		minimapLocationFrame.Position = minimap.Position;
		minimapLocationFrame.Width = screenWidth;
		minimapLocationFrame.Height = screenHeight;
		minimapLocationFrame.ScaleImageToSizeOfControl = true;
		minimapLocationFrame.RenderType = RenderType.CRTAndLCD;
		minimapLocationFrame.MouseDown += minimap_MouseDown;
		minimapLocationFrame.MouseUp += minimap_MouseUp;
		minimapLocationFrame.MouseMove += minimap_MouseMove;
		ComputeScaleFactor();
		if (The.Sim.Controller.GraphicsLevelSetting == Controller.GraphicsLevel.High)
		{
			crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(minimap, new Point(minimap.AbsolutePosition.X, minimap.AbsolutePosition.Y), minimap.Width, minimap.Height, Level.Bottom, DisplayWindow, ReflectionToUse.Small, isMonochrome: false);
		}
		StatusScreen.AddCRTPlasticFrame(gui, DisplayWindow, minimap.Position, minimap.Width, minimap.Height, out frame);
		DrawMapTexture();
		Show();
	}

	private void DisplayWindow_Destroyed()
	{
		Destroy();
	}

	public static int GetWidth(int screenWidth)
	{
		return screenWidth + StatusScreen.GetPlasticFrameWidth();
	}

	private void Destroy()
	{
		frameTexture.Dispose();
		mapTexture.Dispose();
	}

	private void ComputeScaleFactor()
	{
		minimapScaleFactorX = 1f / ((float)minimap.Width / (float)mapWidth);
		minimapScaleFactorY = 1f / ((float)minimap.Height / (float)mapHeight);
	}

	public void CreateMap()
	{
		mapWidth = The.Map.mapTileWidth;
		mapHeight = The.Map.mapTileHeight;
		mapTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);
		frameTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);
		mapTextureColors = new Color[mapWidth * mapHeight];
		mapBackBuffer = new Color[mapWidth * mapHeight];
		frameTextureColors = new Color[mapWidth * mapHeight];
		mapTexture.SetData(mapTextureColors);
		frameTexture.SetData(frameTextureColors);
		if (minimap != null)
		{
			minimap.Texture = mapTexture;
			minimap.SetSkinLocation(SkinState.Normal, new Rectangle(0, 0, mapTexture.Width, mapTexture.Height));
			minimapLocationFrame.Texture = frameTexture;
			minimapLocationFrame.SetSkinLocation(SkinState.Normal, new Rectangle(0, 0, frameTexture.Width, frameTexture.Height));
			ComputeScaleFactor();
		}
	}

	public void Hide()
	{
		DisplayWindow.Hide();
		The.InGameUI.OverlayPanel.btAccessMinimap.IsChecked = false;
	}

	public void Show()
	{
		DisplayWindow.Show();
		if (The.InGameUI.OverlayPanel.btAccessMinimap != null)
		{
			The.InGameUI.OverlayPanel.btAccessMinimap.IsChecked = true;
		}
	}

	private void minimap_MouseMove(MouseEventArgs args)
	{
		if (draggingInMap)
		{
			MoveMap(args);
		}
	}

	private void minimap_MouseUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			draggingInMap = false;
		}
	}

	private void minimap_MouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			draggingInMap = true;
			The.InGameUI.EnableTracking(enable: false);
			MoveMap(args);
		}
	}

	private void MoveMap(MouseEventArgs args)
	{
		int num = args.Position.Y - minimap.AbsolutePosition.Y;
		int x = (int)((float)(args.Position.X - minimap.AbsolutePosition.X) * minimapScaleFactorX);
		x = The.Map.ClampTileMapXPosition(x);
		int y = (int)((float)num * minimapScaleFactorY);
		y = The.Map.ClampTileMapYPosition(y);
		The.MapUI.ZoomToMapPosition(x, y);
		frameIsDirty = true;
	}

	public void SetFrameDirty()
	{
		frameIsDirty = true;
	}

	public void SetSettingsDirty()
	{
		settingsAreDirty = true;
		currentMapTileRow = 0;
	}

	public void Update(GameTime gameTime)
	{
		if (settingsAreDirty || mapRegulator.IsReady() || currentMapTileRow > 0)
		{
			DrawMapTexture();
			settingsAreDirty = false;
		}
		if (frameIsDirty)
		{
			SetMinimapLocationFrameTexture();
			frameIsDirty = false;
		}
	}

	private Color? GetTreeResourceColorEditor(TerrainTile tile, bool isInGodMode)
	{
		if (tile.TreesOnTile != null)
		{
			foreach (Entity item in tile.TreesOnTile)
			{
				if (!item.Find<EditorData>(out var c) || c.Resources == null)
				{
					continue;
				}
				Resource[] resources = c.Resources;
				foreach (Resource resource in resources)
				{
					if (The.InGameUI.OverlaySettings.DisplayResourceType(resource.ResourceType))
					{
						return resource.ResourceType.Category.Color ?? Color.White;
					}
				}
			}
		}
		return null;
	}

	private Color? GetTreeResourceColor(TerrainTile tile, bool isInGodMode)
	{
		if (tile.TreesOnTile != null)
		{
			_ = The.InGameUI.UIAllegiance.SharedKnowledge;
			foreach (Entity item in tile.TreesOnTile)
			{
				if (item.EntityType.TreeType.CropTypes == null)
				{
					continue;
				}
				item.Find<UWGame.SimSide.Trees.Tree>(out var c);
				foreach (KeyValuePair<ResourceType, Crop> crop in c.Crops)
				{
					if (The.InGameUI.OverlaySettings.DrawResource(crop.Value, isInGodMode))
					{
						return crop.Value.ResourceType.Category.Color ?? Color.White;
					}
				}
			}
		}
		return null;
	}

	private Color? GetTileResourceColorEditor(TerrainTile tile, bool isInGodMode)
	{
		if (tile.DesignerPlacedResources != null)
		{
			Resource[] designerPlacedResources = tile.DesignerPlacedResources;
			foreach (Resource resource in designerPlacedResources)
			{
				if (The.InGameUI.OverlaySettings.DisplayResourceType(resource.ResourceType))
				{
					return resource.ResourceType.Category.Color ?? Color.White;
				}
			}
		}
		return null;
	}

	private Color? GetTileResourceColor(TerrainTile tile, bool isInGodMode)
	{
		if (tile.TileResources != null)
		{
			foreach (KeyValuePair<ResourceType, TileResourceContainer> tileResource in tile.TileResources)
			{
				if (The.InGameUI.OverlaySettings.DrawResource(tileResource.Value, isInGodMode))
				{
					return tileResource.Value.ResourceType.Category.Color ?? Color.White;
				}
			}
		}
		return null;
	}

	private Color? GetTileEntityColor(TerrainTile tile)
	{
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		foreach (Entity item in tile.EntitiesOnTile)
		{
			if (item.EntityType.Person != null && The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers])
			{
				return OverlaySettings.personsColor;
			}
			if (The.InGameUI.OverlaySettings.DisplayEntityType(item.EntityType) && sharedKnowledge.AllDetectedEntities.Contains(item.DetectableID))
			{
				return OverlaySettings.GetGroupingColorFromEntity(item, faded: false);
			}
		}
		if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out var value))
		{
			foreach (MemoryFact item2 in value)
			{
				if (The.InGameUI.OverlaySettings.DisplayEntityType(item2.EntityType))
				{
					return OverlaySettings.GetGroupingColorFromEntity(item2, faded: true);
				}
			}
		}
		return null;
	}

	private void DrawMapTexture()
	{
		int mapTileWidth = The.Map.mapTileWidth;
		int mapTileHeight = The.Map.mapTileHeight;
		TerrainTile[][] tileMap = The.Map.TileMap;
		bool isInGodMode = GameWorldRenderer.GetIsInGodMode();
		int num = 16;
		int num2 = Math.Min(mapTileHeight, currentMapTileRow + num);
		bool flag = The.InGameUI.OverlaySettings.DisplayAnyResources();
		bool flag2 = The.InGameUI.OverlaySettings.DisplayAnyEntities();
		bool flag3 = The.Sim.Mode == Sim.EngineMode.Game;
		for (int i = currentMapTileRow; i < num2; i++)
		{
			int num3 = i * mapTileWidth;
			for (int j = 0; j < mapTileWidth; j++)
			{
				TerrainTile terrainTile = tileMap[j][i];
				Color color;
				if (!isInGodMode && !terrainTile.HasEverBeenSeenByPlayer)
				{
					color = Color.Black;
				}
				else
				{
					SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
					if (flag2 && terrainTile.EntitiesOnTile != null)
					{
						Color? tileEntityColor = GetTileEntityColor(terrainTile);
						if (tileEntityColor.HasValue)
						{
							color = tileEntityColor.Value;
							mapBackBuffer[num3 + j] = color;
							continue;
						}
					}
					if (flag)
					{
						Color? color2 = null;
						if (flag3)
						{
							color2 = GetTreeResourceColor(terrainTile, isInGodMode);
							if (!color2.HasValue)
							{
								color2 = GetTileResourceColor(terrainTile, isInGodMode);
							}
						}
						else
						{
							color2 = GetTreeResourceColorEditor(terrainTile, isInGodMode);
							if (!color2.HasValue)
							{
								color2 = GetTileResourceColorEditor(terrainTile, isInGodMode);
							}
						}
						if (color2.HasValue)
						{
							color = color2.Value;
							mapBackBuffer[num3 + j] = color;
							continue;
						}
					}
					color = ((terrainTile.GetCenterTerrain().LevelBelowWater > 0f) ? Color.Lerp(shallowWaterColor, deepWaterColor, terrainTile.GetCenterTerrain().LevelBelowWater / 255f) : ((!terrainTile.TileIsInFogOfWar(sharedKnowledge.Allegiance)) ? groundColor : fovColor));
				}
				mapBackBuffer[num3 + j] = color;
			}
			currentMapTileRow = i + 1;
		}
		if (num2 == mapTileHeight)
		{
			currentMapTileRow = 0;
			mapTexture.SetData(mapBackBuffer);
		}
	}

	private bool IsThereAnyEntityToDisplay()
	{
		foreach (KeyValuePair<EntityType, bool> item in The.InGameUI.OverlaySettings.EntityTypesToDisplay)
		{
			if (item.Value)
			{
				return true;
			}
		}
		return false;
	}

	private void SetMinimapLocationFrameTexture()
	{
		int mapTileWidth = The.Map.mapTileWidth;
		int mapTileHeight = The.Map.mapTileHeight;
		Color gray = Color.Gray;
		frameTextureColors = new Color[mapWidth * mapHeight];
		int num = The.MapUI.mapWindowTileY * mapTileWidth;
		int num2 = Common.ClampTop(The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally, mapWidth - 1);
		for (int i = The.MapUI.mapWindowTileX; i <= num2; i++)
		{
			frameTextureColors[num + i] = gray;
		}
		num = Common.ClampTop(The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically, mapTileHeight - 1) * mapTileWidth;
		for (int j = The.MapUI.mapWindowTileX; j <= num2; j++)
		{
			frameTextureColors[num + j] = gray;
		}
		int num3 = Common.ClampTop(The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically, mapTileHeight - 1);
		int mapWindowTileX = The.MapUI.mapWindowTileX;
		for (int k = The.MapUI.mapWindowTileY; k < num3; k++)
		{
			frameTextureColors[k * mapTileWidth + mapWindowTileX] = gray;
		}
		for (int l = The.MapUI.mapWindowTileY; l < num3; l++)
		{
			frameTextureColors[l * mapTileWidth + num2] = gray;
		}
		frameTexture.SetData(frameTextureColors);
	}
}
