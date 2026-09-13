using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class FogMap
{
	public Window DisplayWindow;

	private Image fogmap;

	private Texture2D mapTexture;

	private Color[] mapTextureColors;

	private int mapWidth = The.Map.mapTileWidth;

	private int mapHeight = The.Map.mapTileHeight;

	public FogMap()
	{
		DisplayWindow = new Window(The.InGameUI.gui);
		DisplayWindow.Level = Level.Min;
		DisplayWindow.Position = new Point(0, 0);
		DisplayWindow.WindowSize = new Vector2(1f, 1f);
		DisplayWindow.CornerSize = 1;
		DisplayWindow.Margin = 0;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = false;
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.HasOverlayComponents = true;
		DisplayWindow.CanHaveFocus = false;
		DisplayWindow.Show();
		DisplayWindow.Destroyed += DisplayWindow_Destroyed;
		CreateMap();
		fogmap = new Image(The.InGameUI.gui);
		fogmap.Texture = mapTexture;
		DisplayWindow.Add(fogmap);
		fogmap.Position = new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y);
		fogmap.Width = mapWidth * 48;
		fogmap.Height = mapHeight * 48;
		fogmap.ScaleImageToSizeOfControl = true;
		fogmap.RenderType = RenderType.Overlay;
		fogmap.DebugTag = "fogmap";
		fogmap.ClipThis = false;
		DrawMapTexture();
	}

	private void DisplayWindow_Destroyed()
	{
		Destroy();
	}

	private void Destroy()
	{
		mapTexture.Dispose();
	}

	public void MoveToPosition(Point pos)
	{
		fogmap.Position = pos;
	}

	public void CreateMap()
	{
		mapTexture = new Texture2D(The.Client.GraphicsDevice, mapWidth, mapHeight, false, SurfaceFormat.Color);
		mapTextureColors = new Color[mapWidth * mapHeight];
		for (int i = 0; i < mapTextureColors.Length; i++)
		{
			mapTextureColors[i] = Color.Black;
		}
		mapTexture.SetData(mapTextureColors);
		if (fogmap != null)
		{
			fogmap.Texture = mapTexture;
		}
	}

	public void Update(GameTime gameTime)
	{
		DrawMapTexture();
	}

	private void DrawMapTexture()
	{
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		TerrainTile[][] tileMap = The.Map.TileMap;
		Color color = Color.Black * The.MapUI.FogOfWarTint;
		float fogOfWarFadeRate = The.MapUI.FogOfWarFadeRate;
		for (int i = 0; i < mapHeight; i++)
		{
			int num = i * mapWidth;
			for (int j = 0; j < mapWidth; j++)
			{
				TerrainTile terrainTile = tileMap[j][i];
				if (j == 9)
				{
					_ = 9;
				}
				Color value = (terrainTile.HasEverBeenSeenByPlayer ? ((!terrainTile.AllegiancesThatSeeThisTile.Contains(uIAllegiance)) ? color : Color.Transparent) : Color.Black);
				mapTextureColors[num + j] = Color.Lerp(mapTextureColors[num + j], value, fogOfWarFadeRate);
			}
		}
		mapTexture.SetData(mapTextureColors);
	}
}
