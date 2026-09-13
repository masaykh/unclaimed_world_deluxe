using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;

namespace UWGame.Client.MapRender;

public class TerrainSlicedMap
{
	private TerrainSlice[][] terrainSlices;

	private bool redrawMap;

	public RenderTarget2D refractionMap;

	public RenderTarget2D clipMap;

	public void Init()
	{
		PresentationParameters presentationParameters = The.Client.GraphicsDevice.PresentationParameters;
		refractionMap = new RenderTarget2D(The.Client.GraphicsDevice, The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight, mipMap: false, The.Client.GraphicsDevice.DisplayMode.Format, presentationParameters.DepthStencilFormat);
		clipMap = new RenderTarget2D(The.Client.GraphicsDevice, The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight, mipMap: false, The.Client.GraphicsDevice.DisplayMode.Format, presentationParameters.DepthStencilFormat);
		int num = (int)Math.Ceiling(The.Map.MapWorldWidth / (float)The.Client.Renderer.TerrainSliceSize);
		int num2 = (int)Math.Ceiling(The.Map.MapWorldHeight / (float)The.Client.Renderer.TerrainSliceSize);
		terrainSlices = new TerrainSlice[num][];
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < num; i++)
		{
			int num5 = The.Client.Renderer.TerrainSliceSize;
			float num6 = num3 + (float)num5;
			if (num6 > The.Map.MapWorldWidth)
			{
				num5 -= (int)(num6 - The.Map.MapWorldWidth);
			}
			terrainSlices[i] = new TerrainSlice[num2];
			for (int j = 0; j < num2; j++)
			{
				int num7 = The.Client.Renderer.TerrainSliceSize;
				float num8 = num4 + (float)num7;
				if (num8 > The.Map.MapWorldHeight)
				{
					num7 -= (int)(num8 - The.Map.MapWorldHeight);
				}
				terrainSlices[i][j] = new TerrainSlice(The.Client.Renderer, new Vector2(num3, num4), num5, num7);
				num4 += (float)num7;
				if (j >= num2 - 1)
				{
					num4 = 0f;
				}
			}
			num3 += (float)num5;
		}
		redrawMap = true;
	}

	public void Destroy()
	{
		refractionMap.Dispose();
		clipMap.Dispose();
		for (int i = 0; i < Common.GetJaggedArrayWidth(terrainSlices); i++)
		{
			TerrainSlice[] array = terrainSlices[i];
			for (int j = 0; j < Common.GetJaggedArrayHeight(terrainSlices); j++)
			{
				array[j].Destroy();
			}
		}
	}

	public void Redraw(Vector3 location)
	{
		int num = (int)(location.X / (float)The.Client.Renderer.TerrainSliceSize);
		int num2 = (int)(location.Y / (float)The.Client.Renderer.TerrainSliceSize);
		if (num < Common.GetJaggedArrayWidth(terrainSlices) && num2 < Common.GetJaggedArrayHeight(terrainSlices))
		{
			terrainSlices[num][num2].NeedsRedraw = true;
		}
	}

	public void RedrawMap(RenderTarget2D target)
	{
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(terrainSlices);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(terrainSlices);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				terrainSlices[i][j].NeedsRedraw = true;
				terrainSlices[i][j].Draw(target);
			}
		}
	}

	public void Draw(RenderTarget2D target)
	{
		if (redrawMap)
		{
			redrawMap = false;
			RedrawMap(target);
		}
		GetTerrainSliceIndexesToDraw(out var minX, out var maxX, out var minY, out var maxY);
		for (int i = minX; i <= maxX; i++)
		{
			for (int j = minY; j <= maxY; j++)
			{
				terrainSlices[i][j].Draw(target);
			}
		}
		The.Client.GraphicsDevice.SetRenderTarget(refractionMap);
		for (int k = minX; k <= maxX; k++)
		{
			for (int l = minY; l <= maxY; l++)
			{
				terrainSlices[k][l].DrawRefractionMap();
			}
		}
		The.Client.GraphicsDevice.SetRenderTarget(clipMap);
		for (int m = minX; m <= maxX; m++)
		{
			for (int n = minY; n <= maxY; n++)
			{
				terrainSlices[m][n].DrawClipMap();
			}
		}
		The.Client.GraphicsDevice.SetRenderTarget(target);
	}

	private void GetTerrainSliceIndexesToDraw(out int minX, out int maxX, out int minY, out int maxY)
	{
		minX = (int)(The.MapUI.MapWindowWorldPosition.X / (float)The.Client.Renderer.TerrainSliceSize);
		minX = Common.ClampBottom(minX, 0);
		maxX = (int)Math.Ceiling((The.MapUI.MapWindowWorldPosition.X + (float)The.MapUI.mapWindowWidth) / (float)The.Client.Renderer.TerrainSliceSize);
		maxX = Common.ClampTop(maxX, Common.GetJaggedArrayWidth(terrainSlices) - 1);
		minY = (int)(The.MapUI.MapWindowWorldPosition.Y / (float)The.Client.Renderer.TerrainSliceSize);
		minY = Common.ClampBottom(minY, 0);
		maxY = (int)Math.Ceiling((The.MapUI.MapWindowWorldPosition.Y + (float)The.MapUI.mapWindowHeight) / (float)The.Client.Renderer.TerrainSliceSize);
		maxY = Common.ClampTop(maxY, Common.GetJaggedArrayHeight(terrainSlices) - 1);
	}
}
