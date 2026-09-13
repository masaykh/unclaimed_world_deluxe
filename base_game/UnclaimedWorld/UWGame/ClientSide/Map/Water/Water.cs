using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Map.Water;

public class Water
{
	private Texture2D waterBumpMap;

	private Texture2D waterBumpMapLarge;

	private Model skyDome;

	private float waterHeight = 1470f;

	public const float WaterDepthForDeepestBlue = 255f;

	public RenderTarget2D refractionRenderTarget;

	private RenderTarget2D reflectionRenderTarget;

	private int waterQuadWidth;

	private int waterQuadHeight;

	private Matrix reflectionViewMatrix;

	private VertexBuffer waterVertexBuffer;

	private VertexWater[] waterVertices;

	private int currentWaterVertex;

	private short[] waterIndices;

	private float totalElapsedUnpausedTime;

	private VertexPositionNormalTexture[] skyQuad = new VertexPositionNormalTexture[6];

	private Texture2D cloudMap;

	private BasicEffect skyQuadEffect;

	private GraphicsDevice device;

	private Effect waterEffect;

	private Sim game;

	private GameWorldRenderer renderer;

	private Vector3 sunPositionSpecularityHack = new Vector3(0f, 0f, 1.5f);

	private Vector3 camPositionSpecularityHack = new Vector3(0f, -3600f, 0f);

	private float specularIntensity = 12f;

	public float WaterHeight
	{
		get
		{
			return waterHeight;
		}
		set
		{
			waterHeight = value;
		}
	}

	public Water(GameWorldRenderer renderer)
	{
		this.renderer = renderer;
	}

	public void LoadContent()
	{
		device = The.Client.GraphicsDevice;
		game = The.Sim;
		Dimension drawArea = The.Client.Controller.DrawArea;
		waterQuadWidth = drawArea.Width;
		waterQuadHeight = drawArea.Height;
		waterEffect = The.Client.Content.Load<Effect>("water");
		cloudMap = The.Client.Content.Load<Texture2D>("cloudMap");
		waterBumpMap = The.Client.Content.Load<Texture2D>("waterbump");
		waterBumpMapLarge = The.Client.Content.Load<Texture2D>("waterbumpFlipped");
		UpdateReflectedViewMatrix();
		PresentationParameters presentationParameters = device.PresentationParameters;
		Dimension drawArea2 = The.Client.Controller.DrawArea;
		refractionRenderTarget = new RenderTarget2D(device, drawArea2.Width, drawArea2.Height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat);
		reflectionRenderTarget = new RenderTarget2D(device, drawArea2.Width, drawArea2.Height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat);
		SetupSkyVertices();
		skyQuadEffect = new BasicEffect(device);
		skyQuadEffect.EnableDefaultLighting();
		skyQuadEffect.World = Matrix.Identity;
		skyQuadEffect.View = reflectionViewMatrix;
		skyQuadEffect.Projection = The.Client.Projection;
		skyQuadEffect.TextureEnabled = true;
		skyQuadEffect.Texture = cloudMap;
	}

	public void UnloadContent()
	{
	}

	public void Destroy()
	{
		reflectionRenderTarget.Dispose();
		refractionRenderTarget.Dispose();
	}

	public void Initialize()
	{
		waterVertices = new VertexWater[(renderer.noOfVerticesHorizontal + 2) * renderer.noOfVerticesVertical];
		waterIndices = new short[(renderer.noOfVerticesHorizontal + 1) * (renderer.noOfVerticesVertical - 1) * 6];
		SetUpWaterIndices();
	}

	private void UpdateWaterVertices()
	{
		Vector2 mapWindowWorldPosition = The.MapUI.MapWindowWorldPosition;
		float num = (float)waterQuadWidth / (float)waterBumpMap.Width;
		float num2 = (float)waterQuadHeight / (float)waterBumpMap.Height;
		float num3 = mapWindowWorldPosition.X / (float)waterBumpMap.Width;
		float num4 = mapWindowWorldPosition.Y / (float)waterBumpMap.Height;
		waterVertices[0].BumpTextureCoordinate = new Vector2(num3, num4);
		waterVertices[2].BumpTextureCoordinate = new Vector2(num3, num4 + num2);
		waterVertices[1].BumpTextureCoordinate = new Vector2(num3 + num, num4);
		waterVertices[3].BumpTextureCoordinate = new Vector2(num3, num4 + num2);
		waterVertices[5].BumpTextureCoordinate = new Vector2(num3 + num, num4 + num2);
		waterVertices[4].BumpTextureCoordinate = new Vector2(num3 + num, num4);
	}

	private void SetUpWaterIndices()
	{
		int noOfVerticesVertical = renderer.noOfVerticesVertical;
		int num = renderer.noOfVerticesHorizontal + 2;
		int num2 = 0;
		for (int i = 0; i < noOfVerticesVertical - 1; i++)
		{
			for (int j = 0; j < num - 1; j++)
			{
				short num3 = (short)(j + i * num);
				short num4 = (short)(j + 1 + i * num);
				short num5 = (short)(j + 1 + (i + 1) * num);
				short num6 = (short)(j + (i + 1) * num);
				waterIndices[num2++] = num3;
				waterIndices[num2++] = num5;
				waterIndices[num2++] = num6;
				waterIndices[num2++] = num3;
				waterIndices[num2++] = num4;
				waterIndices[num2++] = num5;
			}
		}
	}

	private void SetupSkyVertices()
	{
		Dimension drawArea = The.Client.Controller.DrawArea;
		float x = drawArea.Width;
		float y = drawArea.Height;
		float z = -1000f;
		skyQuad[0] = default(VertexPositionNormalTexture);
		skyQuad[0].Position = new Vector3(0f, 0f, z);
		skyQuad[0].TextureCoordinate = new Vector2(0f, 0f);
		skyQuad[2] = default(VertexPositionNormalTexture);
		skyQuad[2].Position = new Vector3(0f, y, z);
		skyQuad[2].TextureCoordinate = new Vector2(0f, 1f);
		skyQuad[1] = default(VertexPositionNormalTexture);
		skyQuad[1].Position = new Vector3(x, 0f, z);
		skyQuad[1].TextureCoordinate = new Vector2(1f, 0f);
		skyQuad[3] = default(VertexPositionNormalTexture);
		skyQuad[3].Position = new Vector3(0f, y, z);
		skyQuad[3].TextureCoordinate = new Vector2(0f, 1f);
		skyQuad[5] = default(VertexPositionNormalTexture);
		skyQuad[5].Position = new Vector3(x, y, z);
		skyQuad[5].TextureCoordinate = new Vector2(1f, 1f);
		skyQuad[4] = default(VertexPositionNormalTexture);
		skyQuad[4].Position = new Vector3(x, 0f, z);
		skyQuad[4].TextureCoordinate = new Vector2(1f, 0f);
		for (int i = 0; i < skyQuad.Length; i++)
		{
			skyQuad[i].Normal = Vector3.UnitZ;
		}
	}

	public void SetUpWaterVerticesAndIndicesInCurrentView()
	{
		int num = The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally + 4;
		int num2 = The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically + 2;
		currentWaterVertex = 0;
		for (int i = The.MapUI.mapWindowTileY - 2; i < num2; i++)
		{
			float num3 = i * 48 + 24 + 10;
			for (int j = The.MapUI.mapWindowTileX - 2; j < num; j++)
			{
				float num4 = j * 48 + 24;
				int num5 = Common.Clamp(j, 0, The.Map.mapTileWidth - 1);
				int num6 = Common.Clamp(i, 0, The.Map.mapTileHeight - 1);
				TerrainTile terrainTile = The.Map.TileMap[num5][num6];
				Terrain centerTerrain = terrainTile.GetCenterTerrain();
				waterVertices[currentWaterVertex].WaterDepth = centerTerrain.LevelBelowWater;
				waterVertices[currentWaterVertex].Position = new Vector3(num4, num3, WaterHeight);
				waterVertices[currentWaterVertex].TextureCoordinate.X = num4 / (float)waterQuadWidth;
				waterVertices[currentWaterVertex].TextureCoordinate.Y = num3 / (float)waterQuadHeight;
				waterVertices[currentWaterVertex].BumpTextureCoordinate = new Vector2(num4 / (float)waterBumpMap.Width, num3 / (float)waterBumpMap.Height);
				waterVertices[currentWaterVertex].WaterColor = terrainTile.ColorOfWater;
				waterVertices[currentWaterVertex].BottomTint = terrainTile.WaterBottomTint;
				currentWaterVertex++;
			}
		}
	}

	private void DrawSkyQuad()
	{
		skyQuadEffect.View = reflectionViewMatrix;
		skyQuadEffect.World = Matrix.CreateTranslation(new Vector3(The.MapUI.MapWindowWorldPosition, 0f));
		foreach (EffectPass pass in skyQuadEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserPrimitives(PrimitiveType.TriangleList, skyQuad, 0, 2);
		}
	}

	public void UpdateReflectedViewMatrix()
	{
		Vector3 terrainCameraPosition = renderer.TerrainCameraPosition;
		terrainCameraPosition.Z = WaterHeight;
		Vector3 vector = terrainCameraPosition;
		vector.Z = -1000f;
		Vector3 cameraUpVector = Vector3.Cross(Vector3.Left, vector - terrainCameraPosition);
		reflectionViewMatrix = Matrix.CreateLookAt(terrainCameraPosition, vector, cameraUpVector);
	}

	public void DrawRefractionMap(List<TerrainBatch> terrainBatches)
	{
		if (refractionRenderTarget.IsContentLost)
		{
			PresentationParameters presentationParameters = device.PresentationParameters;
			Dimension drawArea = The.Client.Controller.DrawArea;
			refractionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat);
		}
		refractionRenderTarget = renderer.terrainSlicedMap.refractionMap;
	}

	public void DrawReflectionMap()
	{
		if (reflectionRenderTarget.IsContentLost)
		{
			PresentationParameters presentationParameters = device.PresentationParameters;
			Dimension drawArea = The.Client.Controller.DrawArea;
			reflectionRenderTarget = new RenderTarget2D(device, drawArea.Width, drawArea.Height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat);
		}
		device.SetRenderTarget(reflectionRenderTarget);
		device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);
		DrawSkyQuad();
	}

	public void DrawWater(GameTime gameTime)
	{
		totalElapsedUnpausedTime = (float)(The.Sim.TotalUnPausedGameTime.TotalMilliseconds / 100.0);
		SetUpWaterVerticesAndIndicesInCurrentView();
		waterEffect.CurrentTechnique = waterEffect.Techniques["Water"];
		_ = The.MapUI.MapWindowWorldPosition;
		Matrix identity = Matrix.Identity;
		waterEffect.Parameters["xWorld"].SetValue(identity);
		waterEffect.Parameters["reflectWorldViewProjection"].SetValue(reflectionViewMatrix * The.Client.Projection);
		waterEffect.Parameters["refractWorldViewProjection"].SetValue(renderer.TerrainViewMatrix * The.Client.Projection);
		waterEffect.Parameters["xReflectionMap"].SetValue(reflectionRenderTarget);
		waterEffect.Parameters["xRefractionMap"].SetValue(renderer.terrainSlicedMap.refractionMap);
		waterEffect.Parameters["xClipMap"].SetValue(renderer.terrainSlicedMap.clipMap);
		waterEffect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
		waterEffect.Parameters["xWaterBumpMapLarge"].SetValue(waterBumpMapLarge);
		waterEffect.Parameters["xWaveLength"].SetValue(0.8f);
		waterEffect.Parameters["xWaveHeight"].SetValue(0.1f);
		Vector3 terrainCameraPosition = renderer.TerrainCameraPosition;
		waterEffect.Parameters["xCamPos"].SetValue(terrainCameraPosition);
		waterEffect.Parameters["camPositionSpecularityHack"].SetValue(camPositionSpecularityHack);
		waterEffect.Parameters["specularIntensity"].SetValue(Common.ClampBottom(The.Sim.PlaySite.PlaySite.Weather.SunIntensity, 0.5f) * specularIntensity);
		Vector3 value = new Vector3(0f, -0.81016f, 0.5862f);
		value += sunPositionSpecularityHack;
		waterEffect.Parameters["xLightDirection"].SetValue(value);
		waterEffect.Parameters["xTime"].SetValue(totalElapsedUnpausedTime);
		waterEffect.Parameters["xWindForce"].SetValue(0.001f);
		waterEffect.Parameters["xWindDirection"].SetValue(new Vector3(1f, 0.15f, 0f));
		foreach (EffectPass pass in waterEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, waterVertices, 0, currentWaterVertex, waterIndices, 0, waterIndices.Length / 3);
		}
	}
}
