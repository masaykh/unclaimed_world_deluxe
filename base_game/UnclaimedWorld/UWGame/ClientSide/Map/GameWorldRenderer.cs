using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputEventSystem;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using UWGame.Client.MapRender;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Map.Water;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Vegetation;
using UWGame.Port;

namespace UWGame.ClientSide.Map;

public class GameWorldRenderer
{
	private class TerrainTilePosition
	{
		public TerrainTile TerrainTile;

		public int X;

		public int Y;

		public TerrainPosition TerrainPosition;

		public TerrainPosition[][] TerrainSubtilePositions;

		public TerrainTilePosition(int x, int y, TerrainTile closestTile)
		{
			X = x;
			Y = y;
			TerrainTile = closestTile;
		}

		public void RecomputeTerrainTile()
		{
			if (TerrainTile.Terrain != null)
			{
				TerrainPosition terrainPosition = new TerrainPosition(this, TerrainTile.Terrain);
				TerrainPosition = terrainPosition;
				return;
			}
			Common.InitJaggedArray(ref TerrainSubtilePositions, 3, 3);
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					TerrainPosition terrainPosition2 = new TerrainPosition(this, TerrainTile.TerrainSubtiles[i][j], i, j);
					TerrainSubtilePositions[i][j] = terrainPosition2;
				}
			}
		}
	}

	private class TerrainPosition
	{
		public Terrain Terrain;

		public Vector3 RenderPosition;

		public Vector2 RenderTextureCoordinate;

		public TerrainTilePosition Parent;

		private const float oneOverTextureSize = 0.001953125f;

		public TerrainPosition(TerrainTilePosition parent, Terrain terrain, int? sx = null, int? sy = null)
		{
			Parent = parent;
			Terrain = terrain;
			ComputeTerrainPosition(sx, sy);
		}

		private void ComputeTerrainPosition(int? sx = null, int? sy = null)
		{
			int x = Parent.X;
			int y = Parent.Y;
			float xPos;
			float yPos;
			if (!Terrain.IsSubtileTerrain())
			{
				MapManager.TileToWorldPos(x, y, out xPos, out yPos);
			}
			else
			{
				xPos = x * 48 + sx.Value * 16 + 8;
				yPos = y * 48 + sy.Value * 16 + 8;
			}
			RenderPosition = new Vector3(xPos, yPos, 1400f + Terrain.TerrainDepth);
			RenderTextureCoordinate.X = xPos * 0.001953125f;
			RenderTextureCoordinate.Y = yPos * 0.001953125f;
		}
	}

	public enum RenderTechnique
	{
		Standard,
		StandardMonochrome,
		StandardOverlay,
		NoLighting,
		NormalsAndDepth,
		LightSources,
		DepthHeightBillboardAlpha,
		DrawModelEmitters,
		Outline
	}

	private enum DrawLightsTechnique
	{
		TwoDeeLightSources,
		ModelEmittedLight
	}

	private Sim sim;

	public Dictionary<string, LightSourceType> LightSourceTypes;

	public UWGame.ClientSide.Map.Water.Water Water;

	private DayAndNightEffects DayAndNightEffects;

	public Effect TimeOfDayLightingEffect;

	public Texture2D Scanlines;

	public Texture2D OverlayGradient;

	public Effect CloudShadowsEffect;

	private Dictionary<string, Queue<Renderable>> attachableRenderables = new Dictionary<string, Queue<Renderable>>();

	public List<List<ILocatable>> sortedObjectsToDraw = new List<List<ILocatable>>();

	private List<LightSource> lightSourcesToDraw = new List<LightSource>();

	private List<ILocatable> shadowsToDraw = new List<ILocatable>();

	private MapResourceRenderer mapResourceRenderer = new MapResourceRenderer();

	private Effect GroundFeatureEffect;

	public VertexGroundFeature[] groundFeatureVertices;

	private int groundFeatureQuadIndex;

	private short[] groundFeatureIndices;

	public const int noOfFeatureQuads = 15000;

	private const int noOfRoadQuads = 15000;

	private const int noOfLightSourceQuads = 200;

	private const int noOfOverlayQuads = 2000;

	private const int noOfInfluenceQuads = 10000;

	private List<Renderable> overlayModelEntities = new List<Renderable>();

	private List<Renderable> lightEmittingModels = new List<Renderable>();

	private VertexFeatureQuad[] featureVertices;

	private VertexLightSourceQuad[] lightSourceVertices;

	private VertexOverlayQuad[] overlayVertices;

	private VertexOverlayGroundSpriteQuad[] overlayGroundSpriteVertices;

	private int influenceMapQuadIndex;

	private VertexOverlayGroundSpriteQuad[] influenceMapVertices;

	public short[] featureIndices;

	private int overlayQuadIndex;

	private int overlayGroundSpriteQuadIndex;

	private short[] lightSourceIndices;

	private Plane noClippingPlane;

	private const int terrainsPerBatch = 3;

	public Matrix TerrainViewMatrix;

	public Vector3 TerrainCameraPosition;

	public int noOfVerticesHorizontal;

	public int noOfVerticesVertical;

	private const int xTilesToIncludeInDraw = 2;

	private const int yBottomTilesToIncludeInDraw = 4;

	private const int yTopTilesToIncludeInDraw = 2;

	public SpriteSheet GhostedStructuresSpriteSheet;

	public RenderTarget2D DiffuseMSRenderTarget;

	private RenderTarget2D diffuseRenderTarget;

	private RenderTarget2D diffuseFinalRenderTarget;

	private RenderTarget2D edgeDetectNormalDepthRenderTarget;

	private RenderTarget2D shadowRenderTarget;

	private RenderTarget2D emissiveModelLightRenderTarget;

	private RenderTarget2D emissiveModelLightDistanceRenderTarget;

	public RenderTarget2D DistanceHeightAndBillboardAlphaRenderTarget;

	public Effect terrainEffect;

	public Effect billboardEffect;

	public Effect lightSourceEffect;

	private Effect overlayEffect;

	private Effect overlayGroundSpritesEffect;

	public double CameraViewingAngle;

	public double CosCameraViewingAngle;

	public float modelYCorrectionFactor;

	public Vector3 CameraTarget;

	public Vector3 CameraDirection = new Vector3(0f, -1200f, 1000f);

	public Vector3 CameraPosition;

	public Matrix View;

	private BlendState lightsBlendAdd = new BlendState
	{
		ColorBlendFunction = BlendFunction.Add,
		ColorSourceBlend = Blend.One,
		ColorDestinationBlend = Blend.One
	};

	private BlendState overlayBlendState = new BlendState
	{
		ColorSourceBlend = Blend.SourceAlpha,
		ColorDestinationBlend = Blend.One
	};

	private Dictionary<string, Texture2D> terrainTextures = new Dictionary<string, Texture2D>();

	private List<string> textureParams = new List<string>();

	public int TileStartX;

	public int TileEndX;

	public int TileStartY;

	public int TileEndY;

	public Texture2D perlinTexture;

	public Texture2D perlinBigTexture;

	private Texture2D cloudShadowTexture;

	private Vector4 TimeOfDayLightingFactor;

	private BloomComponent bloom;

	private const float amountToLowerBloomThresholdAtDawn = 0.5f;

	private const float amountToRaiseBloomIntensityAtDawn = 0.8f;

	private const float amountToLowerBloomThresholdAtSunset = 0.4f;

	private const float amountToRaiseBloomIntensityAtSunset = 0.4f;

	private float windTime;

	public List<TerrainBatch> terrainBatches = new List<TerrainBatch>();

	public int TerrainSliceSize = 1024;

	public TerrainSlicedMap terrainSlicedMap;

	public EntityID? PickedModel;

	public RasterizerState rasterizerStateWireframe;

	public List<Renderable> renderablesFadingOut = new List<Renderable>();

	private HashSet<Renderable> previouslyDrawnRenderablesThatCanFade = new HashSet<Renderable>();

	private HashSet<Renderable> currentlyDrawnRenderablesThatCanFade = new HashSet<Renderable>();

	private HashSet<Renderable> previouslyDrawnRenderablesThatCanLerp = new HashSet<Renderable>();

	private HashSet<Renderable> currentlyDrawnRenderablesThatCanLerp = new HashSet<Renderable>();

	public List<RenderAsBillboard> OverlayBillboards = new List<RenderAsBillboard>();

	private int rightRenderEdge;

	private int bottomRenderEdge;

	private TerrainTilePosition[][] terrainTilePositions;

	private Dictionary<TerrainTile, List<TerrainTilePosition>> tilesToPositions = new Dictionary<TerrainTile, List<TerrainTilePosition>>();

	private bool isInGodMode;

	private int tileStartShadowsX;

	private int tileEndShadowsX;

	private int tileStartShadowsY;

	private int tileEndShadowsY;

	private List<IDrawnAsGroundSprite> bottomSprites = new List<IDrawnAsGroundSprite>();

	private List<IDrawnAsGroundSprite> middleSprites = new List<IDrawnAsGroundSprite>();

	private List<IDrawnAsGroundSprite> topSprites = new List<IDrawnAsGroundSprite>();

	private List<IDrawnAsGroundSprite> outlineSprites = new List<IDrawnAsGroundSprite>();

	public const int GutterSize = 2;

	private bool first;

	private static double a;

	private Viewport DrawAreaViewport;

	public GameWorldRenderer()
	{
		sim = The.Sim;
		Water = new UWGame.ClientSide.Map.Water.Water(this);
		DayAndNightEffects = new DayAndNightEffects();
		textureParams.Add("texture0");
		textureParams.Add("texture1");
		textureParams.Add("texture2");
		textureParams.Add("texture3");
		CosCameraViewingAngle = Vector3.Dot(CameraDirection, Vector3.Down) / (CameraDirection.Length() * Vector3.Down.Length());
		CameraViewingAngle = Math.Acos(CosCameraViewingAngle);
		modelYCorrectionFactor = (float)Math.Cos(1.5707963705062866 - CameraViewingAngle);
		UpdateTerrainViewMatrix();
		rasterizerStateWireframe = new RasterizerState
		{
			CullMode = CullMode.None,
			FillMode = FillMode.WireFrame
		};
		Dimension drawArea = The.Client.Controller.DrawArea;
		Viewport viewport = The.Client.GraphicsDevice.Viewport;
		DrawAreaViewport = new Viewport(0, 0, drawArea.Width, drawArea.Height)
		{
			MinDepth = viewport.MinDepth,
			MaxDepth = viewport.MaxDepth
		};
		The.Client.InitializeSpriteBatch();
	}

	public void Init()
	{
		noClippingPlane = CreatePlane(4000f, new Vector3(0f, 0f, -1f), clipSide: true);
	}

	public void LoadContent()
	{
		GraphicsDevice graphicsDevice = The.Client.GraphicsDevice;
		PresentationParameters presentationParameters = graphicsDevice.PresentationParameters;
		int width = The.Client.Controller.DrawArea.Width;
		int height = The.Client.Controller.DrawArea.Height;
		// PORT DEVIATION 15 (see PORTING-NOTES.md) - DesktopGL only.
		//
		// This is the only render target in the pipeline that asks for multisampling (the game
		// requests 4 samples in UnclaimedWorld.cs:74), and on DesktopGL nothing written to it
		// ever landed: a centre-pixel readback taken immediately after Clear(Color.White)
		// returned R0 G0 B0 A0, alpha included, so not even the clear took effect. Every other
		// target here is built identically but with a literal 0 for the sample count, and those
		// all work.
		//
		// WindowsDX keeps the requested count. MonoGame's PlatformResolveRenderTargets resolves
		// a multisampled target into its own texture when it is unbound, so the SpriteBatch blit
		// that follows reads a genuinely antialiased result - the samples ARE consumed, just not
		// by an explicit resolve call. Forcing 0 on both targets silently dropped 4x MSAA from
		// the world pass on the shipping DirectX build, which is not a trade this deviation is
		// entitled to make: it exists to work around a GL backend limitation, not to change how
		// the game looks where it already worked.
		int diffuseMSSamples = presentationParameters.MultiSampleCount;
#if UW_GL
		diffuseMSSamples = 0;
#endif
		DiffuseMSRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, diffuseMSSamples, RenderTargetUsage.PreserveContents);
		edgeDetectNormalDepthRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
		shadowRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		diffuseRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		diffuseFinalRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		emissiveModelLightRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		emissiveModelLightDistanceRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, SurfaceFormat.Rg32, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		DistanceHeightAndBillboardAlphaRenderTarget = new RenderTarget2D(graphicsDevice, width, height, mipMap: false, SurfaceFormat.Rgba1010102, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
		GhostedStructuresSpriteSheet = The.Client.Content.Load<SpriteSheet>("GhostedBuildings");
		Scanlines = The.Client.Content.Load<Texture2D>("GUI\\CRT_ScanLines");
		OverlayGradient = The.Client.Content.Load<Texture2D>("overlayGradient");
		terrainTextures.Add("greengrass", The.Client.Content.Load<Texture2D>("terrain\\t_greengrass_base"));
		terrainTextures.Add("earth", The.Client.Content.Load<Texture2D>("terrain\\t_earth_base"));
		terrainTextures.Add("muckroot", The.Client.Content.Load<Texture2D>("terrain\\muckroot_base"));
		terrainTextures.Add("muckrootthin", The.Client.Content.Load<Texture2D>("terrain\\t_muckrootthin_base"));
		terrainTextures.Add("billowgrass", The.Client.Content.Load<Texture2D>("terrain\\t_billowgrass_base"));
		terrainTextures.Add("firegrass", The.Client.Content.Load<Texture2D>("terrain\\firegrass_base"));
		terrainTextures.Add("sand", The.Client.Content.Load<Texture2D>("terrain\\t_sand_base"));
		terrainTextures.Add("vulcanic", The.Client.Content.Load<Texture2D>("terrain\\t_vulcanic_base"));
		terrainTextures.Add("rocks", The.Client.Content.Load<Texture2D>("terrain\\t_rocks_base"));
		terrainTextures.Add("limestone", The.Client.Content.Load<Texture2D>("terrain\\t_limestone_base"));
		terrainTextures.Add("humus", The.Client.Content.Load<Texture2D>("terrain\\t_humus_base"));
		terrainTextures.Add("seabed", The.Client.Content.Load<Texture2D>("terrain\\t_seabed_base"));
		terrainTextures.Add("deepseabed", The.Client.Content.Load<Texture2D>("terrain\\t_deepseabed_base"));
		terrainTextures.Add("linear gradient normal map", The.Client.Content.Load<Texture2D>("terrain\\t_rocks_base_depthmap"));
		perlinTexture = The.Client.Content.Load<Texture2D>("perlin_2");
		perlinBigTexture = The.Client.Content.Load<Texture2D>("perlinMedium");
		terrainEffect = The.Client.Content.Load<Effect>("multiTex");
		billboardEffect = The.Client.Content.Load<Effect>("billboard");
		lightSourceEffect = The.Client.Content.Load<Effect>("LightSourcesEffect");
		overlayEffect = The.Client.Content.Load<Effect>("OverlayEffect");
		overlayGroundSpritesEffect = The.Client.Content.Load<Effect>("OverlayGroundSpriteEffect");
		Water.LoadContent();
		DayAndNightEffects.LoadContent();
		TimeOfDayLightingEffect = The.Client.Content.Load<Effect>("TimeOfDayAndLightsources");
		cloudShadowTexture = The.Client.Content.Load<Texture2D>("CloudShadowTexture5");
		CloudShadowsEffect = The.Client.Content.Load<Effect>("CloudShadows");
		GroundFeatureEffect = The.Client.Content.Load<Effect>("RoadsAndPaths");
	}

	public void Destroy()
	{
		edgeDetectNormalDepthRenderTarget.Dispose();
		emissiveModelLightDistanceRenderTarget.Dispose();
		emissiveModelLightRenderTarget.Dispose();
		DistanceHeightAndBillboardAlphaRenderTarget.Dispose();
		DiffuseMSRenderTarget.Dispose();
		diffuseRenderTarget.Dispose();
		shadowRenderTarget.Dispose();
		if (bloom != null)
		{
			bloom.Destroy();
		}
		Water.Destroy();
		if (terrainSlicedMap != null)
		{
			terrainSlicedMap.Destroy();
		}
	}

	public void UnloadContent()
	{
		if (bloom != null)
		{
			bloom.UnloadContent();
		}
		Water.UnloadContent();
	}

	public void PostLoadContent()
	{
		if (The.Client.BloomEnabled)
		{
			bloom = new BloomComponent(The.Sim.Controller.Game);
			bloom.BaseSettings = BloomSettings.PresetSettings[6];
			bloom.Settings = new BloomSettings(bloom.BaseSettings.Name, bloom.BaseSettings.BloomThreshold, bloom.BaseSettings.BlurAmount, bloom.BaseSettings.BloomIntensity, bloom.BaseSettings.BaseIntensity, bloom.BaseSettings.BloomSaturation, bloom.BaseSettings.BloomSaturation);
		}
		AssertVertexbufferAndIndexBufferMatch(15000, 15000);
		AssertVertexbufferAndIndexBufferMatch(10000, 15000);
		AssertVertexbufferAndIndexBufferMatch(2000, 15000);
		groundFeatureVertices = new VertexGroundFeature[60000];
		groundFeatureIndices = new short[90000];
		SetUpIndices(15000, groundFeatureIndices);
		featureVertices = new VertexFeatureQuad[60000];
		featureIndices = new short[90000];
		SetUpIndices(15000, featureIndices);
		lightSourceVertices = new VertexLightSourceQuad[800];
		lightSourceIndices = new short[1200];
		SetUpIndices(200, lightSourceIndices);
		overlayVertices = new VertexOverlayQuad[8000];
		overlayGroundSpriteVertices = new VertexOverlayGroundSpriteQuad[8000];
		influenceMapVertices = new VertexOverlayGroundSpriteQuad[40000];
		mapResourceRenderer.PostLoadContent();
		if (terrainSlicedMap == null)
		{
			terrainSlicedMap = new TerrainSlicedMap();
			terrainSlicedMap.Init();
		}
	}

	public void InitAfterMapLoad()
	{
		noOfVerticesHorizontal = The.MapUI.noOfTilesToDisplayHorizontally + 4;
		noOfVerticesVertical = The.MapUI.noOfTilesToDisplayVertically + 4;
		rightRenderEdge = The.Map.mapTileWidth + 2;
		bottomRenderEdge = The.Map.mapTileHeight + 2;
		sortedObjectsToDraw.Clear();
		for (int i = 0; i <= The.MapUI.noOfTilesToDisplayVertically + 4 + 2; i++)
		{
			sortedObjectsToDraw.Add(new List<ILocatable>());
		}
		Water.Initialize();
		CreateTerrainTilePositions();
		mapResourceRenderer.Init();
	}

	public void CreateTerrainTilePositions()
	{
		Common.InitJaggedArray(ref terrainTilePositions, The.Map.mapTileWidth + 4, The.Map.mapTileHeight + 4);
		for (int i = -2; i < The.Map.mapTileWidth + 2; i++)
		{
			for (int j = -2; j < The.Map.mapTileHeight + 2; j++)
			{
				int num = Common.Clamp(i, 0, The.Map.mapTileWidth - 1);
				int num2 = Common.Clamp(j, 0, The.Map.mapTileHeight - 1);
				TerrainTile tile = The.Map.TileMap[num][num2];
				CreateTerrainTilePositions(i, j, tile);
			}
		}
	}

	public void CreateTerrainTilePositions(int x, int y, TerrainTile tile)
	{
		TerrainTilePosition terrainTilePosition = new TerrainTilePosition(x, y, tile);
		terrainTilePosition.X = x;
		terrainTilePosition.Y = y;
		terrainTilePosition.RecomputeTerrainTile();
		terrainTilePositions[x + 2][y + 2] = terrainTilePosition;
		Common.AddToMultiList(tilesToPositions, tile, terrainTilePosition);
	}

	public void RecomputeTerrainTilePositions(TerrainTile tile)
	{
		if (!tilesToPositions.TryGetValue(tile, out var value))
		{
			return;
		}
		foreach (TerrainTilePosition item in value)
		{
			item.RecomputeTerrainTile();
		}
	}

	private TerrainTilePosition GetTerrainTilePosition(int x, int y)
	{
		return terrainTilePositions[x + 2][y + 2];
	}

	private static void AssertVertexbufferAndIndexBufferMatch(int vertextBufferSizeInQuads, int indexBufferSizeInQuads)
	{
		if (vertextBufferSizeInQuads > indexBufferSizeInQuads)
		{
			throw new Exception("Vertex buffer should not be larger than the index buffer.");
		}
	}

	private void CreateTerrainTrianglesToTheRightAndDown(int x, int y, int lastXToDraw, int lastYToDraw, TerrainBatch batch)
	{
		TerrainTilePosition terrainTilePosition = null;
		TerrainTilePosition downRightTile = null;
		TerrainTilePosition terrainTilePosition2 = null;
		TerrainTilePosition terrainTilePosition3 = GetTerrainTilePosition(x, y);
		if (x < lastXToDraw)
		{
			int num = x + 1;
			if (num < rightRenderEdge)
			{
				terrainTilePosition = GetTerrainTilePosition(num, y);
			}
		}
		if (y < lastYToDraw)
		{
			int num2 = y + 1;
			if (num2 < bottomRenderEdge)
			{
				terrainTilePosition2 = GetTerrainTilePosition(x, num2);
				if (terrainTilePosition != null)
				{
					downRightTile = GetTerrainTilePosition(x + 1, num2);
				}
			}
		}
		if (terrainTilePosition3.TerrainTile.Terrain != null)
		{
			if (terrainTilePosition != null)
			{
				ConnectSingleTerrainToTheRight(batch, terrainTilePosition3, terrainTilePosition, downRightTile);
			}
			if (terrainTilePosition2 != null)
			{
				ConnectSingleTerrainDown(batch, terrainTilePosition3, terrainTilePosition2, downRightTile);
			}
			return;
		}
		CreateInnerSubtiles(batch, terrainTilePosition3);
		if (terrainTilePosition != null)
		{
			ConnectSubtileTerrainToTheRight(batch, terrainTilePosition3, terrainTilePosition, downRightTile, terrainTilePosition2);
		}
		if (terrainTilePosition2 != null)
		{
			ConnectSubtileTerrainDown(batch, terrainTilePosition3, terrainTilePosition2);
		}
	}

	private static void ConnectSubtileTerrainToTheRight(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition rightTile, TerrainTilePosition downRightTile, TerrainTilePosition downTile)
	{
		if (downRightTile == null || downTile == null)
		{
			return;
		}
		if (rightTile.TerrainTile.Terrain != null)
		{
			SetupTerrainVertex(tile, 2, 0, batch);
			SetupTerrainVertex(rightTile, null, null, batch);
			SetupTerrainVertex(tile, 2, 1, batch);
			SetupTerrainVertex(tile, 2, 1, batch);
			SetupTerrainVertex(rightTile, null, null, batch);
			SetupTerrainVertex(tile, 2, 2, batch);
			if (downRightTile.TerrainTile.Terrain != null)
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(rightTile, null, null, batch);
				SetupTerrainVertex(downRightTile, null, null, batch);
			}
			else
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(rightTile, null, null, batch);
				SetupTerrainVertex(downRightTile, 0, 0, batch);
			}
		}
		else
		{
			SetupTerrainVertex(tile, 2, 0, batch);
			SetupTerrainVertex(rightTile, 0, 0, batch);
			SetupTerrainVertex(rightTile, 0, 1, batch);
			SetupTerrainVertex(tile, 2, 0, batch);
			SetupTerrainVertex(rightTile, 0, 1, batch);
			SetupTerrainVertex(tile, 2, 1, batch);
			SetupTerrainVertex(tile, 2, 1, batch);
			SetupTerrainVertex(rightTile, 0, 1, batch);
			SetupTerrainVertex(rightTile, 0, 2, batch);
			SetupTerrainVertex(tile, 2, 1, batch);
			SetupTerrainVertex(rightTile, 0, 2, batch);
			SetupTerrainVertex(tile, 2, 2, batch);
			if (downRightTile.TerrainTile.Terrain != null)
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(rightTile, 0, 2, batch);
				SetupTerrainVertex(downRightTile, null, null, batch);
			}
			else
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(rightTile, 0, 2, batch);
				SetupTerrainVertex(downRightTile, 0, 0, batch);
			}
		}
		if (downRightTile.TerrainTile.Terrain != null)
		{
			if (downTile.TerrainTile.Terrain != null)
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(downRightTile, null, null, batch);
				SetupTerrainVertex(downTile, null, null, batch);
			}
			else
			{
				SetupTerrainVertex(tile, 2, 2, batch);
				SetupTerrainVertex(downRightTile, null, null, batch);
				SetupTerrainVertex(downTile, 2, 0, batch);
			}
		}
		else if (downTile.TerrainTile.Terrain != null)
		{
			SetupTerrainVertex(tile, 2, 2, batch);
			SetupTerrainVertex(downRightTile, 0, 0, batch);
			SetupTerrainVertex(downTile, null, null, batch);
		}
		else
		{
			SetupTerrainVertex(tile, 2, 2, batch);
			SetupTerrainVertex(downRightTile, 0, 0, batch);
			SetupTerrainVertex(downTile, 2, 0, batch);
		}
	}

	private static void CreateInnerSubtiles(TerrainBatch batch, TerrainTilePosition tile)
	{
		SetupTerrainVertex(tile, 0, 0, batch);
		SetupTerrainVertex(tile, 1, 0, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 0, 0, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 0, 1, batch);
		SetupTerrainVertex(tile, 1, 0, batch);
		SetupTerrainVertex(tile, 2, 0, batch);
		SetupTerrainVertex(tile, 2, 1, batch);
		SetupTerrainVertex(tile, 1, 0, batch);
		SetupTerrainVertex(tile, 2, 1, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 0, 1, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
		SetupTerrainVertex(tile, 0, 1, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
		SetupTerrainVertex(tile, 0, 2, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 2, 1, batch);
		SetupTerrainVertex(tile, 2, 2, batch);
		SetupTerrainVertex(tile, 1, 1, batch);
		SetupTerrainVertex(tile, 2, 2, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
	}

	private static void ConnectSubtileTerrainDown(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition downTile)
	{
		if (downTile.TerrainTile.Terrain != null)
		{
			SetupTerrainVertex(tile, 2, 2, batch);
			SetupTerrainVertex(downTile, null, null, batch);
			SetupTerrainVertex(tile, 1, 2, batch);
			SetupTerrainVertex(tile, 1, 2, batch);
			SetupTerrainVertex(downTile, null, null, batch);
			SetupTerrainVertex(tile, 0, 2, batch);
			return;
		}
		SetupTerrainVertex(tile, 2, 2, batch);
		SetupTerrainVertex(downTile, 2, 0, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
		SetupTerrainVertex(downTile, 2, 0, batch);
		SetupTerrainVertex(downTile, 1, 0, batch);
		SetupTerrainVertex(tile, 1, 2, batch);
		SetupTerrainVertex(downTile, 1, 0, batch);
		SetupTerrainVertex(tile, 0, 2, batch);
		SetupTerrainVertex(tile, 0, 2, batch);
		SetupTerrainVertex(downTile, 1, 0, batch);
		SetupTerrainVertex(downTile, 0, 0, batch);
	}

	private static void ConnectSingleTerrainDown(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition downTile, TerrainTilePosition downRightTile)
	{
		if (downTile.TerrainTile.Terrain != null)
		{
			if (downRightTile != null)
			{
				SetupTerrainVertex(tile, null, null, batch);
				if (downRightTile.TerrainTile.Terrain != null)
				{
					SetupTerrainVertex(downRightTile, null, null, batch);
				}
				else
				{
					SetupTerrainVertex(downRightTile, 0, 0, batch);
				}
				SetupTerrainVertex(downTile, null, null, batch);
			}
			return;
		}
		SetupTerrainVertex(tile, null, null, batch);
		SetupTerrainVertex(downTile, 1, 0, batch);
		SetupTerrainVertex(downTile, 0, 0, batch);
		SetupTerrainVertex(tile, null, null, batch);
		SetupTerrainVertex(downTile, 2, 0, batch);
		SetupTerrainVertex(downTile, 1, 0, batch);
		if (downRightTile != null)
		{
			if (downRightTile.TerrainTile.Terrain != null)
			{
				SetupTerrainVertex(tile, null, null, batch);
				SetupTerrainVertex(downRightTile, null, null, batch);
				SetupTerrainVertex(downTile, 2, 0, batch);
			}
			else
			{
				SetupTerrainVertex(tile, null, null, batch);
				SetupTerrainVertex(downRightTile, 0, 0, batch);
				SetupTerrainVertex(downTile, 2, 0, batch);
			}
		}
	}

	private static void ConnectSingleTerrainToTheRight(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition rightTile, TerrainTilePosition downRightTile)
	{
		if (downRightTile == null)
		{
			return;
		}
		if (rightTile.TerrainTile.Terrain != null)
		{
			SetupTerrainVertex(tile, null, null, batch);
			SetupTerrainVertex(rightTile, null, null, batch);
			if (downRightTile.TerrainTile.Terrain != null)
			{
				SetupTerrainVertex(downRightTile, null, null, batch);
			}
			else
			{
				SetupTerrainVertex(downRightTile, 0, 0, batch);
			}
			return;
		}
		SetupTerrainVertex(tile, null, null, batch);
		SetupTerrainVertex(rightTile, 0, 0, batch);
		SetupTerrainVertex(rightTile, 0, 1, batch);
		SetupTerrainVertex(tile, null, null, batch);
		SetupTerrainVertex(rightTile, 0, 1, batch);
		SetupTerrainVertex(rightTile, 0, 2, batch);
		if (downRightTile.TerrainTile.Terrain != null)
		{
			SetupTerrainVertex(tile, null, null, batch);
			SetupTerrainVertex(rightTile, 0, 2, batch);
			SetupTerrainVertex(downRightTile, null, null, batch);
		}
		else
		{
			SetupTerrainVertex(tile, null, null, batch);
			SetupTerrainVertex(rightTile, 0, 2, batch);
			SetupTerrainVertex(downRightTile, 0, 0, batch);
		}
	}

	private void SetUpTerrainIndices(short[] indices)
	{
		int num = 0;
		for (int i = 0; i < noOfVerticesVertical - 1; i++)
		{
			for (int j = 0; j < noOfVerticesHorizontal - 1; j++)
			{
				short num2 = (short)(j + i * noOfVerticesHorizontal);
				short num3 = (short)(j + 1 + i * noOfVerticesHorizontal);
				short num4 = (short)(j + 1 + (i + 1) * noOfVerticesHorizontal);
				short num5 = (short)(j + (i + 1) * noOfVerticesHorizontal);
				indices[num++] = num2;
				indices[num++] = num4;
				indices[num++] = num5;
				indices[num++] = num2;
				indices[num++] = num3;
				indices[num++] = num4;
			}
		}
	}

	public static void SetUpIndices(int noOfQuads, short[] indices)
	{
		int num = 0;
		for (int i = 0; i < noOfQuads - 1; i++)
		{
			int num2 = i * 4;
			short num3 = (short)num2;
			short num4 = (short)(num2 + 1);
			short num5 = (short)(num2 + 2);
			short num6 = (short)(num2 + 3);
			indices[num++] = num3;
			indices[num++] = num5;
			indices[num++] = num6;
			indices[num++] = num3;
			indices[num++] = num4;
			indices[num++] = num5;
		}
	}

	private void SortObjectsForDrawingAndComputeMatrices(bool drawModels)
	{
		foreach (List<ILocatable> item in sortedObjectsToDraw)
		{
			item.Clear();
		}
		shadowsToDraw.Clear();
		ComputeDrawingArea();
		bool flag = true;
		bool drawBillboards = true;
		bool flag2 = GetIsInGodMode();
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		for (int i = TileStartY; i <= TileEndY; i++)
		{
			int currentRow = i - TileStartY;
			for (int j = TileStartX; j <= TileEndX; j++)
			{
				TerrainTile terrainTile = The.Map.TileMap[j][i];
				bool flag3 = !terrainTile.AllegiancesThatSeeThisTile.Contains(The.InGameUI.UIAllegiance);
				if (terrainTile.BaseCenterForMultiTileEntities != null && flag)
				{
					foreach (Entity baseCenterForMultiTileEntity in terrainTile.BaseCenterForMultiTileEntities)
					{
						if (baseCenterForMultiTileEntity.Renderable != null && baseCenterForMultiTileEntity.Renderable.LightSources != null)
						{
							lightSourcesToDraw.AddRange(baseCenterForMultiTileEntity.Renderable.LightSources);
						}
					}
				}
				if (terrainTile.RememberedRootEntitiesOnTile != null && terrainTile.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out var value))
				{
					foreach (MemoryFact item2 in value)
					{
						if (!item2.IsAlwaysShown() && !item2.PartOfID.HasValue && !item2.ContainedBy.HasValue)
						{
							AddRenderableToRender(drawModels, drawBillboards, currentRow, item2.Renderable, j, i);
						}
					}
				}
				if (!flag2 && The.InGameUI.UIAllegiance.AllegianceType == AllegianceType.Player && !terrainTile.HasEverBeenSeenByPlayer)
				{
					continue;
				}
				if (terrainTile.GeoLayoutEntitiesOnTile != null)
				{
					for (int num = terrainTile.GeoLayoutEntitiesOnTile.Count - 1; num >= 0; num--)
					{
						Entity entity = Entity.FindByID(terrainTile.GeoLayoutEntitiesOnTile[num]);
						if (entity != null)
						{
							AddRenderableToRender(drawModels, drawBillboards, currentRow, entity.Renderable, j, i);
						}
						else
						{
							terrainTile.GeoLayoutEntitiesOnTile.RemoveAt(num);
						}
					}
				}
				if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.Build || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorPlaceEntity)
				{
					foreach (InGameInterface.EntityPosition item3 in The.InGameUI.EntitiesBeingPlaced)
					{
						AddRenderableToRender(drawModels, drawBillboards, currentRow, item3.Entity.Renderable, j, i);
					}
				}
				if (terrainTile.EntitiesOnTile != null)
				{
					foreach (Entity item4 in terrainTile.EntitiesOnTile)
					{
						if (flag2 || (!flag3 && !item4.RequiresRollToDetect()) || item4.EntityType.GetIsNeverInFogOfWar() || (item4.EntityType.IntelligenceType != null && item4.Intelligence.Allegiance == The.InGameUI.UIAllegiance) || sharedKnowledge.AllDetectedEntities.Contains(item4.DetectableID))
						{
							AddRenderableToRender(drawModels, drawBillboards, currentRow, item4.Renderable, j, i);
						}
					}
				}
				if (terrainTile.TileResources != null)
				{
					foreach (KeyValuePair<ResourceType, TileResourceContainer> tileResource in terrainTile.TileResources)
					{
						if (terrainTile.X == 18)
						{
							_ = terrainTile.Y;
							_ = 10;
						}
						if (flag2 || sharedKnowledge.AllDetectedEntities.Contains(tileResource.Value.DetectableID))
						{
							AddRenderableToRender(drawModels, drawBillboards, currentRow, tileResource.Value.Renderable, j, i);
						}
					}
				}
				if (!(!flag3 || flag2) || terrainTile.RenderablesOnTile == null)
				{
					continue;
				}
				foreach (Renderable item5 in terrainTile.RenderablesOnTile)
				{
					AddRenderableToRender(drawModels, drawBillboards, currentRow, item5, j, i);
				}
			}
		}
		foreach (List<ILocatable> item6 in sortedObjectsToDraw)
		{
			foreach (ILocatable item7 in item6)
			{
				Renderable asRenderable = item7.AsRenderable;
				if (asRenderable != null && asRenderable.RenderAsModel != null)
				{
					asRenderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, asRenderable.RenderAsModel.FinalModelScale);
				}
			}
		}
		if (previouslyDrawnRenderablesThatCanFade != null)
		{
			foreach (Renderable item8 in previouslyDrawnRenderablesThatCanFade)
			{
				if (currentlyDrawnRenderablesThatCanFade == null || !currentlyDrawnRenderablesThatCanFade.Contains(item8))
				{
					renderablesFadingOut.Add(item8);
					item8.FadeOut();
				}
			}
		}
		if (previouslyDrawnRenderablesThatCanLerp != null)
		{
			foreach (Renderable item9 in previouslyDrawnRenderablesThatCanLerp)
			{
				if (currentlyDrawnRenderablesThatCanLerp == null || !currentlyDrawnRenderablesThatCanLerp.Contains(item9))
				{
					item9.LerpableWasRenderedLastFrame = false;
				}
			}
		}
		previouslyDrawnRenderablesThatCanLerp = currentlyDrawnRenderablesThatCanLerp;
		currentlyDrawnRenderablesThatCanLerp = null;
		previouslyDrawnRenderablesThatCanFade = currentlyDrawnRenderablesThatCanFade;
		currentlyDrawnRenderablesThatCanFade = null;
	}

	private void DrawInvisibleEntitiesForDebugOrEditor()
	{
	}

	public static bool GetIsInGodMode()
	{
		bool result = false;
		if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			result = true;
		}
		return result;
	}

	private void ComputeDrawingArea()
	{
		TileStartX = The.MapUI.mapWindowTileX - 2;
		TileEndX = TileStartX + The.MapUI.noOfTilesToDisplayHorizontally + 4;
		TileStartY = The.MapUI.mapWindowTileY - 2;
		TileEndY = TileStartY + The.MapUI.noOfTilesToDisplayVertically + 2 + 4;
		TileStartX = The.Map.ClampTileMapXPosition(TileStartX);
		TileEndX = The.Map.ClampTileMapXPosition(TileEndX);
		TileStartY = The.Map.ClampTileMapYPosition(TileStartY);
		TileEndY = The.Map.ClampTileMapYPosition(TileEndY);
		tileStartShadowsX = The.MapUI.mapWindowTileX - 1;
		tileEndShadowsX = tileStartShadowsX + The.MapUI.noOfTilesToDisplayHorizontally + 2;
		tileStartShadowsY = The.MapUI.mapWindowTileY - 1;
		tileEndShadowsY = tileStartShadowsY + The.MapUI.noOfTilesToDisplayVertically + 2;
		Vector3? dropShadowEstimate = DayAndNightEffects.GetDropShadowEstimate();
		int num = 0;
		int num2 = 0;
		if (dropShadowEstimate.HasValue)
		{
			num = (int)(dropShadowEstimate.Value.X * 50f / 48f);
			num2 = (int)(dropShadowEstimate.Value.Y * 50f / 48f);
		}
		if (num > 0)
		{
			tileStartShadowsX -= num;
		}
		else if (num < 0)
		{
			tileEndShadowsX += Math.Abs(num);
		}
		if (num2 > 0)
		{
			tileStartShadowsY -= num2;
		}
		else if (num2 < 0)
		{
			tileEndShadowsY += Math.Abs(num2);
		}
		tileStartShadowsX = The.Map.ClampTileMapXPosition(tileStartShadowsX);
		tileEndShadowsX = The.Map.ClampTileMapXPosition(tileEndShadowsX);
		tileStartShadowsY = The.Map.ClampTileMapYPosition(tileStartShadowsY);
		tileEndShadowsY = The.Map.ClampTileMapYPosition(tileEndShadowsY);
	}

	private void AddRenderableToRender(bool drawModels, bool drawBillboards, int currentRow, Renderable renderable, int tileX, int tileY)
	{
		if (renderable != null)
		{
			renderable.IsOnScreen = true;
		}
		if (renderable == null || !renderable.IsDrawn)
		{
			return;
		}
		if (renderable.SelectedSpriteInfo != null && renderable.RenderAsGroundSprite != null && PositionIsOnTile(renderable.MapPosition, tileX, tileY))
		{
			if (renderable.Parent != null && renderable.Parent.EntityType.StructureType != null && renderable.Parent.EntityType.StructureType.IsAddon)
			{
				middleSprites.Add(renderable.RenderAsGroundSprite);
			}
			else
			{
				bottomSprites.Add(renderable.RenderAsGroundSprite);
			}
		}
		bool flag = false;
		if (renderable.RenderAsBillboard != null)
		{
			if (drawBillboards)
			{
				foreach (RenderAsBillboard item in renderable.RenderAsBillboard)
				{
					if (PositionIsOnTile(item.MapPosition.Value, tileX, tileY))
					{
						if (renderable.Entity != null)
						{
							renderable.Entity.ToString().Contains("Tipi");
						}
						if (TileEntitiesAreInSight(tileX, tileY))
						{
							sortedObjectsToDraw[currentRow].Add(item);
						}
						if (!renderable.DrawAsNonPhysical && TileShadowsAreInSight(tileX, tileY))
						{
							shadowsToDraw.Add(item);
						}
					}
				}
			}
		}
		else if (renderable.RenderAsModel != null && drawModels && PositionIsOnTile(renderable.MapPosition, tileX, tileY))
		{
			if (TileEntitiesAreInSight(tileX, tileY))
			{
				sortedObjectsToDraw[currentRow].Add(renderable);
				flag = true;
				if (!renderable.DrawAsNonPhysical && renderable.Parent.EntityType.BiologicalType != null && renderable.RenderAsModel.ModelData.HasEmittingParts)
				{
					lightEmittingModels.Add(renderable);
				}
			}
			if (!renderable.DrawAsNonPhysical && renderable.MemoryFact == null && TileShadowsAreInSight(tileX, tileY))
			{
				shadowsToDraw.Add(renderable);
			}
		}
		if (!flag && renderable.ParticleEmitters != null && renderable.MemoryFact == null && PositionIsOnTile(renderable.MapPosition, tileX, tileY) && TileEntitiesAreInSight(tileX, tileY))
		{
			sortedObjectsToDraw[currentRow].Add(renderable);
			flag = true;
		}
		if (!flag && renderable.StateSoundPlaying != null && renderable.MemoryFact == null && PositionIsOnTile(renderable.MapPosition, tileX, tileY) && TileEntitiesAreInSight(tileX, tileY))
		{
			sortedObjectsToDraw[currentRow].Add(renderable);
			flag = true;
		}
		if (flag)
		{
			if (renderable.CanLerpLocation())
			{
				Common.AddToList(ref currentlyDrawnRenderablesThatCanLerp, renderable);
			}
			if (renderable.CanFade())
			{
				Common.AddToList(ref currentlyDrawnRenderablesThatCanFade, renderable);
				renderable.FadeIn();
			}
		}
	}

	public bool PositionIsOnTile(Point positionOfEntity, int tileX, int tileY)
	{
		if (positionOfEntity.X == tileX)
		{
			return positionOfEntity.Y == tileY;
		}
		return false;
	}

	private bool TileShadowsAreInSight(int tileX, int tileY)
	{
		if (tileX >= tileStartShadowsX && tileX <= tileEndShadowsX && tileY >= tileStartShadowsY)
		{
			return tileY <= tileEndShadowsY;
		}
		return false;
	}

	public bool TileEntitiesAreInSight(int tileX, int tileY)
	{
		if (tileX >= TileStartX && tileX <= TileEndX && tileY >= TileStartY)
		{
			return tileY <= TileEndY;
		}
		return false;
	}

	public Renderable GetFreeAttachableRenderable(string renderableTypeKey)
	{
		if (!attachableRenderables.TryGetValue(renderableTypeKey, out var value))
		{
			value = new Queue<Renderable>();
			attachableRenderables.Add(renderableTypeKey, value);
		}
		if (value.Count == 0)
		{
			RenderableType type = GameData.Instance.AttachableRenderableTypes[renderableTypeKey];
			for (int i = 0; i < 1; i++)
			{
				Renderable renderable = RenderableFactory.Produce(null, type);
				renderable.Initialize(updatePropertiesFromEntity: true);
				value.Enqueue(renderable);
			}
		}
		return value.Dequeue();
	}

	public void RetireAttachableRenderable(Renderable renderable)
	{
		attachableRenderables[renderable.RenderableType.KeyName].Enqueue(renderable);
	}

	private void DrawBloomEffect(Texture2D sceneTexture)
	{
		if (The.Client.spriteBatch == null)
		{
			return;
		}
		if (The.Client.BloomEnabled)
		{
			if (DayAndNightEffects.SunAnimation == DayAndNightEffects.SunAnimations.MorningAfterSunrise)
			{
				float num = The.Sim.DateAndTime.SunElevation / 0.104f;
				float num2 = bloom.BaseSettings.BloomThreshold - 0.5f;
				float num3 = bloom.BaseSettings.BloomIntensity + 0.8f;
				float bloomThreshold;
				float bloomIntensity;
				if (num < 0.1f)
				{
					num *= 10f;
					bloomThreshold = MathHelper.SmoothStep(bloom.BaseSettings.BloomThreshold, num2, num);
					bloomIntensity = MathHelper.SmoothStep(bloom.BaseSettings.BaseIntensity, num3, num);
				}
				else if (num < 0.4f)
				{
					bloomThreshold = num2;
					bloomIntensity = num3;
				}
				else
				{
					num = (num - 0.4f) / 0.6f;
					bloomThreshold = MathHelper.SmoothStep(num2, bloom.BaseSettings.BloomThreshold, num);
					bloomIntensity = MathHelper.SmoothStep(num3, bloom.BaseSettings.BloomIntensity, num);
				}
				bloom.Settings.BloomThreshold = bloomThreshold;
				bloom.Settings.BloomIntensity = bloomIntensity;
			}
			else if (DayAndNightEffects.SunAnimation == DayAndNightEffects.SunAnimations.EveningBeforeSunset)
			{
				float num4 = 1f - The.Sim.DateAndTime.SunElevation / 0.104f;
				float num5 = bloom.BaseSettings.BloomThreshold - 0.4f;
				float num6 = bloom.BaseSettings.BloomIntensity + 0.4f;
				float bloomThreshold2;
				float bloomIntensity2;
				if (num4 < 0.5f)
				{
					num4 *= 10f;
					bloomThreshold2 = MathHelper.SmoothStep(bloom.BaseSettings.BloomThreshold, num5, num4);
					bloomIntensity2 = MathHelper.SmoothStep(bloom.BaseSettings.BaseIntensity, num6, num4);
				}
				else if (num4 < 0.7f)
				{
					bloomThreshold2 = num5;
					bloomIntensity2 = num6;
				}
				else
				{
					num4 = (num4 - 0.7f) / 0.3f;
					bloomThreshold2 = MathHelper.SmoothStep(num5, bloom.BaseSettings.BloomThreshold, num4);
					bloomIntensity2 = MathHelper.SmoothStep(num6, bloom.BaseSettings.BloomIntensity, num4);
				}
				bloom.Settings.BloomThreshold = bloomThreshold2;
				bloom.Settings.BloomIntensity = bloomIntensity2;
			}
			bloom.Draw(sceneTexture);
		}
		else
		{
			The.Client.Controller.SetZoomRenderTaget();
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			The.Client.spriteBatch.Draw(sceneTexture, new Rectangle(0, 0, sceneTexture.Width, sceneTexture.Height), Color.White);
			The.Client.spriteBatch.End();
		}
	}

	public void UpdateModelMatricesWithNewPositions()
	{
		bool drawModels = true;
		if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			isInGodMode = true;
		}
		float x = The.MapUI.MapWindowWorldPosition.X + (float)The.MapUI.mapWindowWidth / 2f;
		float y = The.MapUI.MapWindowWorldPosition.Y + (float)The.MapUI.mapWindowHeight / 2f;
		CameraTarget = new Vector3(x, y, 0f);
		CameraPosition = CameraTarget - CameraDirection;
		Vector3 cameraUpVector = -Vector3.UnitZ;
		View = Matrix.CreateLookAt(CameraPosition, CameraTarget, cameraUpVector);
		SortObjectsForDrawingAndComputeMatrices(drawModels);
	}

	public void UpdateMousePicking()
	{
	}

	private bool IsThereWaterInCurrentView()
	{
		GetEdgesOfTerrainToDraw(out var lastXToDraw, out var lastYToDraw, out var firstXToDraw, out var firstYToDraw);
		for (int i = firstYToDraw; i < lastYToDraw; i++)
		{
			for (int j = firstXToDraw; j < lastXToDraw; j++)
			{
				int num = Common.Clamp(j, 0, The.Map.mapTileWidth - 1);
				int num2 = Common.Clamp(i, 0, The.Map.mapTileHeight - 1);
				if (The.Map.TileMap[num][num2].IsPartlyUnderWater())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Draw()
	{
		// PORT DIAGNOSTIC: one frame of what the world pass drew, when port.renderTrace is on.
		// Everything below is a no-op otherwise. See UWGame.Port.RenderTrace.
		UWGame.Port.RenderTrace.BeginFrame();
		UWGame.Port.RenderTrace.DeviceState("Draw() entry", The.Client.GraphicsDevice);

		bool num = IsThereWaterInCurrentView();
		if (!sim.IsPaused)
		{
			windTime += (float)sim.GameTime.ElapsedGameTime.TotalSeconds * 0.333f;
		}
		DayAndNightEffects.Recompute();
		Color? timeOfDayColor = DayAndNightEffects.GetTimeOfDayColor();
		if (timeOfDayColor.HasValue)
		{
			TimeOfDayLightingFactor = ComputeTimeOfDayLightMultiplier(timeOfDayColor.Value);
		}
		else
		{
			TimeOfDayLightingFactor = Vector4.One;
			TimeOfDayLightingFactor.W = 1f;
		}
		UpdateTerrainViewMatrix();
		if (num)
		{
			Water.UpdateReflectedViewMatrix();
			if (The.MapUI.IsScrolling)
			{
				Water.DrawRefractionMap(terrainBatches);
			}
			Water.DrawReflectionMap();
		}
		The.Client.GraphicsDevice.SetRenderTarget(DiffuseMSRenderTarget);
		The.Client.GraphicsDevice.Clear(Color.White);
		UWGame.Port.RenderTrace.Log("world target bound and cleared to white");
		UWGame.Port.RenderTrace.DeviceState("after binding DiffuseMSRenderTarget", The.Client.GraphicsDevice);
		terrainSlicedMap.Draw(DiffuseMSRenderTarget);
		if (num)
		{
			Water.DrawWater(sim.GameTime);
		}
		DrawGroundFeatureSprites();
		DrawMapEdges();
		bool drawModels = true;
		if (true && The.Sim.DateAndTime.SunIsUp)
		{
			DrawShadows(drawModels);
		}
		DrawSortedObjectsAndParticles();
		if (true && The.Sim.DateAndTime.SunIsUp)
		{
			DrawCloudAndDropShadows();
		}
		DrawBloomEffect(diffuseFinalRenderTarget);
		The.MapUI.DrawBullets();
		Dimension drawArea = The.Client.Controller.DrawArea;
		Manager.Draw(The.Client.GraphicsDevice, Matrix.Identity, drawArea.Width, drawArea.Height);
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		DrawOverlayGroundSprites();
		DrawOverlayBillboards();
		DrawOverlayModels();
		DrawInfluenceMapSprites();
		if (sim.Mode == Sim.EngineMode.Edit)
		{
			bool printCoords = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[EditorOverlayTypes.Coords];
			PrintEditorTileInfo(printCoords);
		}

		// PORT DIAGNOSTIC. Dumped here, at the end of the pass, because a render target that is
		// still bound cannot be read back: this is the first point where all of them are free.
		// The order below is the order the world travels through them, so the first blank PNG in
		// the sequence is where the picture stops existing.
		if (UWGame.Port.RenderTrace.Active)
		{
			The.Client.GraphicsDevice.SetRenderTarget(null);
			UWGame.Port.RenderTrace.DumpTarget("1-world-DiffuseMS", DiffuseMSRenderTarget);
			UWGame.Port.RenderTrace.DumpTarget("2-shadow", shadowRenderTarget);
			UWGame.Port.RenderTrace.DumpTarget("3-edgeDetectNormalDepth", edgeDetectNormalDepthRenderTarget);
			UWGame.Port.RenderTrace.DumpTarget("4-diffuse", diffuseRenderTarget);
			UWGame.Port.RenderTrace.DumpTarget("5-diffuseFinal", diffuseFinalRenderTarget);
			UWGame.Port.RenderTrace.EndFrame();
		}
	}

	public static Plane CreatePlane(float height, Vector3 planeNormalDirection, bool clipSide)
	{
		planeNormalDirection.Normalize();
		Vector4 value = new Vector4(planeNormalDirection, height);
		if (clipSide)
		{
			value *= -1f;
		}
		return new Plane(value);
	}

	private void DrawLightsFromModelEmitters()
	{
		if (lightEmittingModels.Count <= 0)
		{
			return;
		}
		The.Client.GraphicsDevice.SetRenderTargets(emissiveModelLightRenderTarget, emissiveModelLightDistanceRenderTarget);
		The.Client.GraphicsDevice.Clear(Color.Black);
		foreach (Renderable lightEmittingModel in lightEmittingModels)
		{
			lightEmittingModel.Draw(RenderTechnique.DrawModelEmitters, ref View, ref The.Client.Projection);
		}
	}

	private void DrawOverlayModels()
	{
		foreach (Renderable overlayModelEntity in overlayModelEntities)
		{
			overlayModelEntity.Draw(RenderTechnique.StandardOverlay, ref View, ref The.Client.Projection, 0.25f);
		}
	}

	private void CopyContainedBillboardShadowQuads(Renderable renderable, ref int featureQuadIndex)
	{
		foreach (RenderAsBillboard item in renderable.RenderAsBillboard)
		{
			item.CopyShadowQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
		}
	}

	private void DrawShadows(bool drawModels)
	{
		SetShadowDrawing();
		int featureQuadIndex = 0;
		foreach (ILocatable item in shadowsToDraw)
		{
			if (item is Renderable renderable)
			{
				if (renderable.RenderAsModel != null)
				{
					renderable.RenderAsModel.DrawShadow();
				}
			}
			else if (item is RenderAsBillboard renderAsBillboard)
			{
				CopyContainedBillboardShadowQuads(renderAsBillboard.Parent, ref featureQuadIndex);
			}
		}
		if (featureQuadIndex > 0)
		{
			DrawBillboardBatch(featureQuadIndex, RenderTechnique.NoLighting);
		}
	}

	private void SetupInterfaceOnMapQuads()
	{
		overlayGroundSpriteQuadIndex = 0;
		The.InGameUI.Selection.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
		The.InGameUI.SelectedTiles.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
		The.InGameUI.SelectedTilesPreview.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
		The.InGameUI.SelectRectangle.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
		if (!The.InGameUI.ShowOverlaysAndMarkerWindows || !The.InGameUI.UIExpedition.HasValue)
		{
			return;
		}
		Expedition expedition = Expedition.FindByID(The.InGameUI.UIExpedition.Value);
		if (expedition == null)
		{
			return;
		}
		foreach (Zone zone in expedition.OwnedEntities.Zones)
		{
			zone.MapArea.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
		}
	}

	private void SetupInfluenceQuads()
	{
		influenceMapQuadIndex = 0;
		if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.Build || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorPlaceEntity || The.InGameUI.OverlaySettings.OverlayTypeSettings[OverlayTypes.BuildAreas] || The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[EditorOverlayTypes.BuildAreas])
		{
			The.InGameUI.TerrainBlockingRender.SetupQuad(influenceMapVertices, ref influenceMapQuadIndex);
		}
		if (The.InGameUI.OverlaySettings.OverlayTypeSettings[OverlayTypes.Threats])
		{
			The.InGameUI.ThreatRender.SetupQuad(influenceMapVertices, ref influenceMapQuadIndex);
		}
	}

	public float CorrectModelYPositionForDrawing(float yLocation)
	{
		float num = yLocation - CameraTarget.Y;
		return CameraTarget.Y + num / modelYCorrectionFactor;
	}

	private void DrawGroundFeatureSprites()
	{
		groundFeatureQuadIndex = 0;
		bool flag = true;
		_ = The.InGameUI.UIAllegiance.SharedKnowledge;
		for (int i = TileStartX; i <= TileEndX; i++)
		{
			TerrainTile[] array = The.Map.TileMap[i];
			for (int j = TileStartY; j <= TileEndY; j++)
			{
				TerrainTile terrainTile = array[j];
				The.MapUI.TileToScreen(i, j, out var xScreen, out var yScreen);
				new Rectangle(xScreen, yScreen, 48, 48);
				if (!flag)
				{
					continue;
				}
				if (terrainTile.EdgeLayoutEntities != null)
				{
					foreach (Entity edgeLayoutEntity in terrainTile.EdgeLayoutEntities)
					{
						RenderAsGroundSprite renderAsGroundSprite = edgeLayoutEntity.Renderable.RenderAsGroundSprite;
						if (renderAsGroundSprite != null)
						{
							middleSprites.Add(renderAsGroundSprite);
						}
					}
				}
				middleSprites.Add(terrainTile);
			}
		}
		bottomSprites = bottomSprites.Distinct().ToList();
		middleSprites = middleSprites.Distinct().ToList();
		topSprites = topSprites.Distinct().ToList();
		DrawSetOfGroundSprites(bottomSprites);
		DrawSetOfGroundSprites(middleSprites);
		DrawSetOfGroundSprites(topSprites);
		if (groundFeatureQuadIndex > 0)
		{
			DrawGroundFeatureUserVertices(groundFeatureQuadIndex);
		}
		bottomSprites.Clear();
		middleSprites.Clear();
		topSprites.Clear();
	}

	private void DrawSetOfGroundSprites(List<IDrawnAsGroundSprite> list)
	{
		foreach (IDrawnAsGroundSprite item in list)
		{
			item.CopyQuadToVertexBuffer(groundFeatureVertices, ref groundFeatureQuadIndex);
		}
	}

	public void GetEdgesOfTerrainToDraw(out int lastXToDraw, out int lastYToDraw, out int firstXToDraw, out int firstYToDraw)
	{
		lastXToDraw = The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally + 2;
		lastYToDraw = The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically + 2;
		firstXToDraw = The.MapUI.mapWindowTileX - 2;
		firstYToDraw = The.MapUI.mapWindowTileY - 2;
	}

	private void GetEdgesOfTerrainToDraw(float size, Vector2 position, out int lastXToDraw, out int lastYToDraw, out int firstXToDraw, out int firstYToDraw)
	{
		int num = (int)Math.Ceiling((decimal)size / 48m);
		int num2 = (int)Math.Ceiling((decimal)size / 48m);
		lastXToDraw = Common.Min(terrainTilePositions.Length - 2, MapManager.WorldPosToTile(position).X + num + 2);
		lastYToDraw = Common.Min(terrainTilePositions[0].Length - 2, MapManager.WorldPosToTile(position).Y + num2 + 2);
		firstXToDraw = MapManager.WorldPosToTile(position).X - 2;
		firstYToDraw = MapManager.WorldPosToTile(position).Y - 2;
	}

	public void SetUpTerrainVerticesAndIndicesInCurrentView()
	{
		if (!The.MapUI.IsScrolling && terrainBatches != null && terrainBatches.Count > 0 && !The.MapUI.RenderedTerrainIsDirty)
		{
			return;
		}
		The.MapUI.RenderedTerrainIsDirty = false;
		terrainBatches.Clear();
		GetEdgesOfTerrainToDraw(out var lastXToDraw, out var lastYToDraw, out var firstXToDraw, out var firstYToDraw);
		CreateBatchesOfTerrainTypesToDraw(lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw, terrainBatches);
		Parallel.ForEach(terrainBatches, delegate(TerrainBatch batch)
		{
			for (int i = firstYToDraw; i < lastYToDraw; i++)
			{
				for (int j = firstXToDraw; j < lastXToDraw; j++)
				{
					CreateTerrainTrianglesToTheRightAndDown(j, i, lastXToDraw, lastYToDraw, batch);
				}
			}
		});
	}

	public void SetUpTerrainVerticesAndIndicesInCurrentView(float size, Vector2 position, out List<TerrainBatch> terrainBatchList)
	{
		terrainBatchList = new List<TerrainBatch>();
		The.MapUI.RenderedTerrainIsDirty = false;
		GetEdgesOfTerrainToDraw(size, position, out var lastXToDraw, out var lastYToDraw, out var firstXToDraw, out var firstYToDraw);
		CreateBatchesOfTerrainTypesToDraw(lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw, terrainBatchList);
		Parallel.ForEach(terrainBatchList, delegate(TerrainBatch batch)
		{
			for (int i = firstYToDraw; i < lastYToDraw; i++)
			{
				for (int j = firstXToDraw; j < lastXToDraw; j++)
				{
					CreateTerrainTrianglesToTheRightAndDown(j, i, lastXToDraw, lastYToDraw, batch);
				}
			}
		});
	}

	private void CreateBatchesOfTerrainTypesToDraw(int lastXToDraw, int lastYToDraw, int firstXToDraw, int firstYToDraw, List<TerrainBatch> batches)
	{
		List<TerrainBatch> list = new List<TerrainBatch>();
		TerrainBatch terrainBatch = new TerrainBatch();
		terrainBatch.terrainInBatch.Add(GameData.Instance.AllSoilComponentTypes["soil:groundrock"]);
		batches.Add(terrainBatch);
		List<RenderedTerrainType> list2 = new List<RenderedTerrainType>();
		List<string> alreadyBatched = new List<string>();
		for (int i = firstYToDraw; i < lastYToDraw; i++)
		{
			for (int j = firstXToDraw; j < lastXToDraw; j++)
			{
				int num = Common.Clamp(j, 0, The.Map.mapTileWidth - 1);
				int num2 = Common.Clamp(i, 0, The.Map.mapTileHeight - 1);
				TerrainTile terrainTile = The.Map.TileMap[num][num2];
				if (terrainTile.Terrain != null)
				{
					if (terrainTile.Terrain.SoilComponents != null)
					{
						AddSoilTypesToDraw(terrainTile.Terrain, list, list2, alreadyBatched);
					}
					continue;
				}
				for (int k = 0; k < 3; k++)
				{
					for (int l = 0; l < 3; l++)
					{
						AddSoilTypesToDraw(terrainTile.TerrainSubtiles[k][l], list, list2, alreadyBatched);
					}
				}
			}
		}
		for (int m = firstYToDraw; m < lastYToDraw; m++)
		{
			for (int n = firstXToDraw; n < lastXToDraw; n++)
			{
				int num3 = Common.Clamp(n, 0, The.Map.mapTileWidth - 1);
				int num4 = Common.Clamp(m, 0, The.Map.mapTileHeight - 1);
				TerrainTile terrainTile = The.Map.TileMap[num3][num4];
				if (terrainTile.Terrain != null)
				{
					if (terrainTile.Terrain.Vegetation != null)
					{
						AddVegetationTypesToDraw(terrainTile.Terrain, list2, alreadyBatched);
					}
				}
				else
				{
					if (terrainTile.TerrainSubtiles == null)
					{
						continue;
					}
					for (int num5 = 0; num5 < 3; num5++)
					{
						for (int num6 = 0; num6 < 3; num6++)
						{
							AddVegetationTypesToDraw(terrainTile.TerrainSubtiles[num5][num6], list2, alreadyBatched);
						}
					}
				}
			}
		}
		list2.Sort();
		foreach (RenderedTerrainType item in list2)
		{
			if (terrainBatch.terrainInBatch.Count == 3)
			{
				terrainBatch = new TerrainBatch();
				batches.Add(terrainBatch);
			}
			terrainBatch.terrainInBatch.Add(item);
		}
		list.Sort();
		batches.AddRange(list);
	}

	private void AddSoilTypesToDraw(Terrain terrain, List<TerrainBatch> renderAsRockBatches, List<RenderedTerrainType> terrainTypesToSortIntoBatches, List<string> alreadyBatched)
	{
		if (terrain.SoilComponents == null)
		{
			return;
		}
		foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in terrain.SoilComponents)
		{
			if (soilComponent.Value.Amount > 0f && !alreadyBatched.Contains(soilComponent.Value.SoilComponentType.KeyName))
			{
				if (soilComponent.Value.SoilComponentType.RenderAsRocksType != null)
				{
					TerrainBatch terrainBatch = new TerrainBatch();
					terrainBatch.RenderAsRocks = true;
					terrainBatch.terrainInBatch.Add(soilComponent.Value.SoilComponentType);
					renderAsRockBatches.Add(terrainBatch);
				}
				else
				{
					terrainTypesToSortIntoBatches.Add(soilComponent.Value.SoilComponentType);
				}
				alreadyBatched.Add(soilComponent.Value.SoilComponentType.KeyName);
			}
		}
	}

	private static void SetupTerrainVertex(TerrainTilePosition tile, int? sx, int? sy, TerrainBatch batch)
	{
		VertexMultitextured item = default(VertexMultitextured);
		TerrainPosition terrainPosition;
		Terrain terrain;
		if (tile.TerrainTile.Terrain != null)
		{
			terrainPosition = tile.TerrainPosition;
			terrain = terrainPosition.Terrain;
		}
		else
		{
			terrainPosition = tile.TerrainSubtilePositions[sx.Value][sy.Value];
			terrain = terrainPosition.Terrain;
		}
		item.Position = terrainPosition.RenderPosition;
		item.TextureCoordinate = terrainPosition.RenderTextureCoordinate;
		for (int i = 0; i < 3; i++)
		{
			float num = 0f;
			Vector4 vector = Vector4.One;
			float num2 = 1f;
			float num3 = 1f;
			if (i < batch.terrainInBatch.Count)
			{
				RenderedTerrainType renderedTerrainType = batch.terrainInBatch[i];
				LowVegetation value;
				SoilComponent value2;
				if (renderedTerrainType.IsBaseTerrain)
				{
					num = 1f;
					vector = Vector4.One;
				}
				else if (terrain.Vegetation != null && renderedTerrainType is LowVegetationType && terrain.Vegetation.TryGetValue((LowVegetationType)renderedTerrainType, out value))
				{
					num = value.DisplayAmount;
					vector = Vector4.One;
					value.LowVegetationType.GetPerlinNoiseChannel();
					num3 = value.LowVegetationType.RenderPerlinNoiseSharpness;
					num2 = value.LowVegetationType.NoiseScaling;
				}
				else if (terrain.SoilComponents != null && renderedTerrainType is SoilComponentType && terrain.SoilComponents.TryGetValue((SoilComponentType)renderedTerrainType, out value2))
				{
					num = value2.DisplayAmount;
					vector = Vector4.Lerp(value2.SoilComponentType.DryTintAsVector, value2.SoilComponentType.WetTintAsVector, terrain.Parent.Moisture);
					value2.SoilComponentType.GetPerlinNoiseChannel();
					num3 = value2.SoilComponentType.RenderPerlinNoiseSharpness;
					num2 = value2.SoilComponentType.NoiseScaling;
				}
			}
			switch (i)
			{
			case 0:
				item.TexWeights.X = num;
				item.TintColor0 = vector;
				item.AlphaSharpness.X = num3;
				item.NoiseScaling.X = num2;
				break;
			case 1:
				item.TexWeights.Y = num;
				item.TintColor1 = vector;
				item.AlphaSharpness.Y = num3;
				item.NoiseScaling.Y = num2;
				break;
			case 2:
				item.TexWeights.Z = num;
				item.TintColor2 = vector;
				item.AlphaSharpness.Z = num3;
				item.NoiseScaling.Z = num2;
				break;
			}
		}
		batch.terrainVerticesList.Add(item);
		batch.terrainIndicesList.Add((short)batch.terrainIndicesList.Count);
	}

	private static void AddVegetationTypesToDraw(Terrain terrain, List<RenderedTerrainType> terrainTypesToSortIntoBatches, List<string> alreadyBatched)
	{
		if (terrain.Vegetation == null)
		{
			return;
		}
		foreach (KeyValuePair<LowVegetationType, LowVegetation> item in terrain.Vegetation)
		{
			if (item.Value.DisplayAmount > 0f && !alreadyBatched.Contains(item.Value.LowVegetationType.KeyName))
			{
				terrainTypesToSortIntoBatches.Add(item.Value.LowVegetationType);
				alreadyBatched.Add(item.Value.LowVegetationType.KeyName);
			}
		}
	}

	public void SetShadowDrawing()
	{
		The.Client.GraphicsDevice.SetRenderTarget(shadowRenderTarget);
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		Color color = new Color(1f, 1f, 1f, 0f);
		The.Client.GraphicsDevice.Clear(color);
	}

	public void DrawCloudAndDropShadows()
	{
		The.Client.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
		CloudShadowsEffect.CurrentTechnique = CloudShadowsEffect.Techniques["RenderCloudShadows"];
		CloudShadowsEffect.Parameters["BillboardDepthHeightMap"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);
		CloudShadowsEffect.Parameters["GroundDropShadowTexture"].SetValue(shadowRenderTarget);
		CloudShadowsEffect.Parameters["CloudTexture"].SetValue(cloudShadowTexture);
		CloudShadowsEffect.Parameters["CloudEdgeSharpness"].SetValue(The.MapUI.CloudSharpness);
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		CloudShadowsEffect.Parameters["ViewportSize"].SetValue(value);
		CloudShadowsEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		CloudShadowsEffect.Parameters["CloudCoverLimit"].SetValue(1f - The.Sim.PlaySite.PlaySite.Weather.CloudCover);
		CloudShadowsEffect.Parameters["CloudPosition"].SetValue(The.Sim.PlaySite.PlaySite.Weather.CloudPosition);
		CloudShadowsEffect.Parameters["ShadowAlpha"].SetValue(DayAndNightEffects.GetDropShadowAlphaFactor() * The.MapUI.CloudOpacity);
		foreach (EffectPass pass in CloudShadowsEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.quadRenderer.Render(The.Client.GraphicsDevice, -Vector2.One, Vector2.One);
		}
	}

	public void DrawTerrainUserVertices(List<TerrainBatch> batches, Plane? clippingPlane, Vector2 position, Vector2 renderTargetSize, List<TerrainBatch> terrainSliceBatches = null)
	{
		The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		terrainEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		terrainEffect.Parameters["perlinTexture"].SetValue(perlinTexture);
		terrainEffect.Parameters["ViewportSize"].SetValue(renderTargetSize);
		terrainEffect.Parameters["WindowPosition"].SetValue(position);
		terrainEffect.Parameters["NearPlane"].SetValue(The.Client.NearPlane);
		terrainEffect.Parameters["FarPlane"].SetValue(The.Client.FarPlane);
		terrainEffect.Parameters["ZOffset"].SetValue(2500f);
		if (clippingPlane.HasValue)
		{
			terrainEffect.Parameters["ClipPlane0"].SetValue(new Vector4(clippingPlane.Value.Normal, clippingPlane.Value.D));
			terrainEffect.Parameters["DoClipping"].SetValue(value: true);
		}
		else
		{
			terrainEffect.Parameters["DoClipping"].SetValue(value: false);
		}
		bool flag = false;
		if (sim.Mode == Sim.EngineMode.Edit)
		{
			flag = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[EditorOverlayTypes.TerrainDivision];
		}
		if (flag)
		{
			The.Client.GraphicsDevice.RasterizerState = rasterizerStateWireframe;
		}
		else
		{
			The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		}
		if (terrainSliceBatches == null)
		{
			DrawTerrainUsingBatchList(batches);
		}
		else
		{
			DrawTerrainUsingBatchList(terrainSliceBatches);
		}
		The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	}

	private void DrawTerrainUsingBatchList(List<TerrainBatch> batchlist)
	{
		foreach (TerrainBatch item in batchlist)
		{
			if (item.RenderAsRocks)
			{
				terrainEffect.CurrentTechnique = terrainEffect.Techniques["RenderRocksSingleLayer" + item.terrainInBatch[0].GetPerlinNoiseChannel()];
				terrainEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
				terrainEffect.Parameters["LightPosition"].SetValue(new Vector3(The.Sim.DateAndTime.SunPosition.X, The.Sim.DateAndTime.SunPosition.Y, The.Sim.DateAndTime.SunPosition.Z));
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder("MultiTextured");
				stringBuilder.Append(item.terrainInBatch[0].GetPerlinNoiseChannel().ToString());
				if (item.terrainInBatch.Count > 1)
				{
					stringBuilder.Append(item.terrainInBatch[1].GetPerlinNoiseChannel().ToString());
				}
				terrainEffect.CurrentTechnique = terrainEffect.Techniques[stringBuilder.ToString()];
			}
			if (item.terrainInBatch.Count > 2)
			{
				terrainEffect.Parameters["terrain3Noise"].SetValue((float)item.terrainInBatch[2].GetPerlinNoiseChannel());
			}
			for (int i = 0; i < item.terrainInBatch.Count; i++)
			{
				RenderedTerrainType renderedTerrainType = item.terrainInBatch[i];
				_ = renderedTerrainType.TextureName == "greengrass";
				terrainEffect.Parameters[textureParams[i]].SetValue(terrainTextures[renderedTerrainType.TextureName]);
				if (renderedTerrainType is SoilComponentType { RenderAsRocksType: not null } soilComponentType && soilComponentType.RenderAsRocksType.DepthMapTextureName != null)
				{
					UWGame.Port.EffectCompat.SetIfDeclared(terrainEffect, "NormalMap", terrainTextures[soilComponentType.RenderAsRocksType.DepthMapTextureName]);
				}
			}
			UWGame.Port.RenderTrace.Technique("multiTex (terrain)", terrainEffect);
			foreach (EffectPass pass in terrainEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				VertexMultitextured[] terrainVerticesArray = item.TerrainVerticesArray;
				short[] terrainIndicesArray = item.TerrainIndicesArray;
				The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, terrainVerticesArray, 0, terrainVerticesArray.Length, terrainIndicesArray, 0, terrainIndicesArray.Length / 3);
				UWGame.Port.RenderTrace.Submit("terrain", terrainIndicesArray.Length / 3);
			}
		}
	}

	public void DrawGroundOutlineUserVertices(int numberOfQuadsToDraw, bool doubleSpeed = false)
	{
		The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		// Dropped by the OpenGL rebuild - the shader never reads it. See Port/EffectCompat.
		UWGame.Port.EffectCompat.SetIfDeclared(GroundFeatureEffect, "AlphaAdjustment", 1f);
		GroundFeatureEffect.CurrentTechnique = GroundFeatureEffect.Techniques["RenderOutlineGroundSprites"];
		UWGame.Port.EffectCompat.SetIfDeclared(GroundFeatureEffect, "NormalMap", terrainTextures["linear gradient normal map"]);
		GroundFeatureEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		GroundFeatureEffect.Parameters["spriteSheetTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		GroundFeatureEffect.Parameters["ViewportSize"].SetValue(value);
		GroundFeatureEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		foreach (EffectPass pass in GroundFeatureEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, groundFeatureVertices, 0, 4 * numberOfQuadsToDraw, groundFeatureIndices, 0, 2 * numberOfQuadsToDraw);
			UWGame.Port.RenderTrace.Submit("ground feature sprites (RoadsAndPaths)", 2 * numberOfQuadsToDraw);
		}
	}

	public void DrawGroundFeatureUserVertices(int numberOfQuadsToDraw)
	{
		The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		GroundFeatureEffect.CurrentTechnique = GroundFeatureEffect.Techniques["RenderGroundSprites"];
		GroundFeatureEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
		GroundFeatureEffect.Parameters["LightPosition"].SetValue(new Vector3(The.Sim.DateAndTime.SunPosition.X, The.Sim.DateAndTime.SunPosition.Y, The.Sim.DateAndTime.SunPosition.Z));
		UWGame.Port.EffectCompat.SetIfDeclared(GroundFeatureEffect, "NormalMap", terrainTextures["linear gradient normal map"]);
		GroundFeatureEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		GroundFeatureEffect.Parameters["spriteSheetTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		GroundFeatureEffect.Parameters["ViewportSize"].SetValue(value);
		GroundFeatureEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		foreach (EffectPass pass in GroundFeatureEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, groundFeatureVertices, 0, 4 * numberOfQuadsToDraw, groundFeatureIndices, 0, 2 * numberOfQuadsToDraw);
			UWGame.Port.RenderTrace.Submit("ground feature sprites (RoadsAndPaths)", 2 * numberOfQuadsToDraw);
		}
	}

	private void SetLightSourceDrawing()
	{
	}

	public static void InsertionSort<T>(IList<T> list) where T : ILocatable
	{
		int count = list.Count;
		for (int i = 1; i < count; i++)
		{
			T val = list[i];
			int num = i - 1;
			while (num >= 0 && list[num].CompareTo(val) > 0)
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = val;
		}
	}

	public static void SaveRenderTargetToFile(string name, RenderTarget2D renderTarget)
	{
		using Stream stream = File.Create(name + ".png");
		renderTarget.SaveAsPng(stream, renderTarget.Width, renderTarget.Height);
	}

	public static void SaveTextureToFile(string name, Texture2D texture2D)
	{
		using Stream stream = File.Create(name + ".png");
		texture2D.SaveAsPng(stream, texture2D.Width, texture2D.Height);
	}

	private void DrawSortedObjectsAndParticles()
	{
		int featureQuadIndex = 0;
		overlayQuadIndex = 0;
		overlayModelEntities.Clear();
		GraphicsDevice graphicsDevice = The.Client.GraphicsDevice;
		graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		graphicsDevice.DepthStencilState = DepthStencilState.None;
		graphicsDevice.BlendState = BlendState.AlphaBlend;
		graphicsDevice.SetRenderTarget(edgeDetectNormalDepthRenderTarget);
		graphicsDevice.Clear(Color.Black);
		featureQuadIndex = DrawNormalDepthMapForEdgeEnhancement(featureQuadIndex);
		graphicsDevice.SetRenderTarget(DistanceHeightAndBillboardAlphaRenderTarget);
		featureQuadIndex = DrawDepthMapForLighting(featureQuadIndex, graphicsDevice);
		graphicsDevice.BlendState = BlendState.AlphaBlend;
		graphicsDevice.SetRenderTarget(DiffuseMSRenderTarget);
		UWGame.Port.RenderTrace.DeviceState("before sorted objects (models, billboards)", graphicsDevice);
		featureQuadIndex = DrawSortedObjectsMain(featureQuadIndex);
		UWGame.Port.RenderTrace.Log("sorted objects drawn, quad index now " +
			featureQuadIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
		DrawInvisibleEntitiesForDebugOrEditor();
		DrawMapResourceOverlays();
		graphicsDevice.SetRenderTarget(diffuseRenderTarget);
		if (true)
		{
			DrawOutlines();
		}
		else
		{
			UWGame.Port.RenderTrace.DeviceState("before compositing the world onto the back buffer",
				The.Client.GraphicsDevice);
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			The.Client.spriteBatch.Draw(DiffuseMSRenderTarget, Vector2.Zero, Color.White);
			The.Client.spriteBatch.End();
			UWGame.Port.RenderTrace.Submit("composite blit (world -> back buffer)", 2);
		}
		DrawLightsFromModelEmitters();
		graphicsDevice.SetRenderTarget(diffuseFinalRenderTarget);
		DrawTimeOfDayOverlay();
		GetLightsToDrawAndDrawThem();
	}

	/// <summary>
	private void DrawTopAndBottomEdges(Rectangle rect, Rectangle sourcerect)
	{
		_ = DiffuseMSRenderTarget.Width / 2;
		int num = (int)(float)(DiffuseMSRenderTarget.Width / rect.Width + 1);
		for (int i = 0; i < Common.Max(1, DiffuseMSRenderTarget.Height / rect.Height); i++)
		{
			for (int j = 0; (float)j < The.Map.MapWorldWidth / (float)rect.Width + (float)num; j++)
			{
				Vector2 vector = new Vector2(rect.Width * j - num * rect.Width / 2, -rect.Height * (1 + i));
				Point point = The.MapUI.WorldPosToScreenPoint(new Vector2(vector.X, vector.Y));
				rect.X = point.X - 1;
				rect.Y = point.Y - 1;
				The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
				vector.Y = rect.Height * i + (int)The.Map.MapWorldWidth;
				point = The.MapUI.WorldPosToScreenPoint(new Vector2(vector.X, vector.Y));
				rect.X = point.X - 1;
				rect.Y = point.Y - 1;
				The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
			}
		}
	}

	private void DrawLeftAndRightEdges(Rectangle rect, Rectangle sourcerect)
	{
		for (int i = 0; i < Common.Max(1, DiffuseMSRenderTarget.Width / rect.Width); i++)
		{
			for (int j = 0; (float)j < The.Map.MapWorldHeight / (float)rect.Height; j++)
			{
				Vector2 vector = new Vector2(-rect.Width * (1 + i), rect.Height * j);
				Point point = The.MapUI.WorldPosToScreenPoint(new Vector2(vector.X, vector.Y));
				rect.X = point.X - 1;
				rect.Y = point.Y - 1;
				The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
				vector.X = rect.Width * i + (int)The.Map.MapWorldWidth;
				point = The.MapUI.WorldPosToScreenPoint(new Vector2(vector.X, vector.Y));
				rect.X = point.X - 1;
				rect.Y = point.Y - 1;
				The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
			}
		}
	}

	private void DrawMapResourceOverlays()
	{
		if (The.Sim.Mode != Sim.EngineMode.Edit && The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea)
		{
			mapResourceRenderer.Render(The.Map, this);
		}
	}

	private void DrawMapEdges()
	{
		if (The.Client.spriteBatch != null)
		{
			Rectangle rect = default(Rectangle);
			Rectangle sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("mapedge_base");
			rect.Height = sourceRectangle.Height;
			rect.Width = sourceRectangle.Width;
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			DrawTopAndBottomEdges(rect, sourceRectangle);
			DrawLeftAndRightEdges(rect, sourceRectangle);
			The.Client.spriteBatch.End();
		}
	}

	private int DrawSortedObjectsMain(int featureQuadIndex, RenderTechnique renderTechnique = RenderTechnique.Standard)
	{
		bool renderIds = false;
		if (sim.Mode == Sim.EngineMode.Edit)
		{
			renderIds = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[EditorOverlayTypes.EntityIDs];
		}
		// PORT DIAGNOSTIC (port.renderTrace): how much was OFFERED to this pass. Zero here means
		// nothing was visible to draw - a collection or culling problem - while a healthy count
		// with no pixels on screen means the drawing itself is at fault. The two need different
		// answers, and the screen cannot tell them apart.
		if (UWGame.Port.RenderTrace.Recording)
		{
			int offered = 0;
			foreach (List<ILocatable> bucket in sortedObjectsToDraw)
			{
				offered += bucket.Count;
			}
			UWGame.Port.RenderTrace.Log("sorted objects offered: " +
				sortedObjectsToDraw.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) +
				" bucket(s), " + offered.ToString(System.Globalization.CultureInfo.InvariantCulture) + " item(s)");
		}
		foreach (List<ILocatable> item in sortedObjectsToDraw)
		{
			foreach (ILocatable item2 in item)
			{
				Entity entity = null;
				if (item2 is RenderAsBillboard)
				{
					RenderAsBillboard renderAsBillboard = (RenderAsBillboard)item2;
					entity = renderAsBillboard.Parent.Entity;
					if (renderAsBillboard.Parent != null && renderAsBillboard.Parent.DrawAsOverlay)
					{
						renderAsBillboard.CopyOverlayQuadToVertexBuffer(overlayVertices, ref overlayQuadIndex);
					}
					else
					{
						renderAsBillboard.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
					}
				}
				else
				{
					Renderable asRenderable = item2.AsRenderable;
					if (asRenderable != null)
					{
						entity = asRenderable.Entity;
						if (!asRenderable.DrawAsOverlay)
						{
							if (featureQuadIndex > 0)
							{
								DrawBillboardBatch(featureQuadIndex, RenderTechnique.Standard);
								featureQuadIndex = 0;
							}
							asRenderable.Draw(RenderTechnique.Standard, ref View, ref The.Client.Projection);
						}
					}
				}
				if (entity != null && sim.Mode == Sim.EngineMode.Edit)
				{
					PrintEditorData(entity, renderIds);
				}
			}
		}
		if (featureQuadIndex > 0)
		{
			DrawBillboardBatch(featureQuadIndex, renderTechnique);
			featureQuadIndex = 0;
		}
		return featureQuadIndex;
	}

	private static void DrawAccessPointMarkers(Entity objectAsEntity)
	{
		if (objectAsEntity.Contains != null && objectAsEntity.Contains is IExit exit)
		{
			The.MapUI.AddDebugMarker(exit.ComputeAccessPoint(), Color.Azure, objectAsEntity, 3f);
		}
		else
		{
			The.MapUI.AddDebugMarker(objectAsEntity.AccessPoint.Value, Color.LightYellow, objectAsEntity, 3f);
		}
	}

	private static void DrawAgentMarker(Entity objectAsEntity)
	{
		if (objectAsEntity.EntityType.IntelligenceType != null)
		{
			The.MapUI.AddDebugMarker(objectAsEntity.PlaySiteLocation, Color.Orange, objectAsEntity);
		}
	}

	private void PrintEditorTileInfo(bool printCoords)
	{
		for (int i = TileStartX; i <= TileEndX; i++)
		{
			TerrainTile[] array = The.Map.TileMap[i];
			for (int j = TileStartY; j <= TileEndY; j++)
			{
				TerrainTile terrainTile = array[j];
				bool flag = false;
				if (terrainTile.DesignerPlacedResources != null)
				{
					Vector2 printPos = The.MapUI.TileEdgeToScreen(terrainTile.X, terrainTile.Y);
					Resource[] designerPlacedResources = terrainTile.DesignerPlacedResources;
					foreach (Resource resource in designerPlacedResources)
					{
						printPos = PrintEditorResource(printPos, resource, out var wasPrinted);
						if (wasPrinted)
						{
							flag = wasPrinted;
						}
					}
				}
				if (!flag && printCoords)
				{
					PrintCoords(i, j);
				}
			}
		}
	}

	private void PrintEditorData(Entity objectAsEntity, bool renderIds)
	{
		if (objectAsEntity == null)
		{
			return;
		}
		Vector2 vector = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
		vector.X -= 26f;
		if (renderIds)
		{
			DevText.Print(vector, objectAsEntity.EntityID.ToString(), Color.White);
		}
		if (objectAsEntity.Find<EditorData>(out var c) && c.Resources != null)
		{
			vector.Y -= 40f;
			Resource[] resources = c.Resources;
			foreach (Resource resource in resources)
			{
				vector = PrintEditorResource(vector, resource, out var _);
			}
		}
	}

	private static void PrintCoords(int tileX, int tileY)
	{
		Vector2 position;
		Vector2 vector = (position = The.MapUI.TileEdgeToScreen(tileX, tileY));
		position.X += 8f;
		position.Y += 20f;
		DevText.Print(position, tileX + "," + tileY, Color.White);
		Vector3 vector2 = MapManager.TileEdgeToWorldPos(new Point(tileX, tileY));
		DevText.Print(vector, vector2.X.ToString(), Color.Yellow);
		DevText.Print(vector + new Vector2(0f, 12f), vector2.Y.ToString(), Color.Yellow);
	}

	private static Vector2 PrintEditorResource(Vector2 printPos, Resource resource, out bool wasPrinted)
	{
		wasPrinted = false;
		if (!The.InGameUI.OverlaySettings.DisplayResourceType(resource.ResourceType))
		{
			return printPos;
		}
		ResourceType resourceType = resource.ResourceType;
		Color colour = resourceType.Color ?? resourceType.Category.Color ?? Color.White;
		StringBuilder stringBuilder = new StringBuilder();
		string value = "";
		if (resource.MinResourceItems.HasValue)
		{
			stringBuilder.Append(resource.MinResourceItems.Value + "-" + resource.MaxResourceItems.Value);
			value = "|";
		}
		if (resource.Modifier.HasValue && resource.Modifier.Value != 100)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(resource.Modifier.Value + " %");
		}
		string text = stringBuilder.ToString();
		if (!string.IsNullOrEmpty(text))
		{
			DevText.Print(printPos, text, colour);
			wasPrinted = true;
			printPos.Y += 12f;
		}
		return printPos;
	}

	private void DrawEntityDebugText(Entity objectAsEntity, bool drawInfo)
	{
		if (drawInfo && objectAsEntity != null)
		{
			if (objectAsEntity.Find<BodyComponent>(out var c))
			{
				Vector2 position = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
				position.Y -= 32f;
				position.X -= 14f;
				DevText.Print(position, ((int)c.Body.GlobalHitpoints).ToString(), Color.LightGreen);
			}
			if (objectAsEntity.Find<Intelligence>(out var c2))
			{
				Vector2 position2 = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
				position2.Y -= 22f;
				position2.X -= 14f;
				DevText.Print(position2, ((int)(100f * c2.Morale)).ToString(), Color.LightBlue);
			}
			if (objectAsEntity.Find<NonLivingEntity>(out var c3))
			{
				Vector2 position3 = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
				position3.Y -= 32f;
				position3.X -= 14f;
				DevText.Print(position3, ((int)(100f * c3.Condition)).ToString(), Color.LightCyan);
			}
		}
	}

	private int DrawDepthMapForLighting(int featureQuadIndex, GraphicsDevice device)
	{
		Color color = new Color(1f, 0f, 0f, 0f);
		device.Clear(color);
		device.BlendState = BlendState.NonPremultiplied;
		foreach (List<ILocatable> item in sortedObjectsToDraw)
		{
			foreach (ILocatable item2 in item)
			{
				if (item2 is RenderAsBillboard)
				{
					RenderAsBillboard renderAsBillboard = (RenderAsBillboard)item2;
					if (renderAsBillboard.Parent != null && !renderAsBillboard.Parent.DrawAsNonPhysical)
					{
						renderAsBillboard.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
					}
					continue;
				}
				Renderable asRenderable = item2.AsRenderable;
				if (!asRenderable.DrawAsNonPhysical)
				{
					if (featureQuadIndex > 0)
					{
						DrawBillboardBatch(featureQuadIndex, RenderTechnique.DepthHeightBillboardAlpha);
						featureQuadIndex = 0;
					}
					asRenderable?.Draw(RenderTechnique.DepthHeightBillboardAlpha, ref View, ref The.Client.Projection);
				}
			}
		}
		if (featureQuadIndex > 0)
		{
			DrawBillboardBatch(featureQuadIndex, RenderTechnique.DepthHeightBillboardAlpha);
			featureQuadIndex = 0;
		}
		return featureQuadIndex;
	}

	private int DrawNormalDepthMapForEdgeEnhancement(int featureQuadIndex)
	{
		foreach (List<ILocatable> item in sortedObjectsToDraw)
		{
			InsertionSort(item);
			foreach (ILocatable item2 in item)
			{
				if (item2 is LightSource)
				{
					continue;
				}
				if (item2.AsRenderAsBillboard != null)
				{
					if (item2.AsRenderAsBillboard.Parent != null && !item2.AsRenderAsBillboard.Parent.DrawAsNonPhysical)
					{
						item2.AsRenderAsBillboard.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
					}
					continue;
				}
				Renderable asRenderable = item2.AsRenderable;
				if (asRenderable == null)
				{
					continue;
				}
				if (asRenderable.DrawAsNonPhysical)
				{
					overlayModelEntities.Add(asRenderable);
					continue;
				}
				if (featureQuadIndex > 0)
				{
					DrawBillboardBatch(featureQuadIndex, RenderTechnique.NormalsAndDepth);
					featureQuadIndex = 0;
				}
				asRenderable.Draw(RenderTechnique.NormalsAndDepth, ref View, ref The.Client.Projection);
			}
		}
		if (featureQuadIndex > 0)
		{
			DrawBillboardBatch(featureQuadIndex, RenderTechnique.NormalsAndDepth);
			featureQuadIndex = 0;
		}
		return featureQuadIndex;
	}

	private void DrawOverlayBillboards()
	{
		if (overlayQuadIndex == 0)
		{
			return;
		}
		The.Client.GraphicsDevice.BlendState = overlayBlendState;
		overlayEffect.CurrentTechnique = overlayEffect.Techniques["DrawOverlay"];
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		overlayEffect.Parameters["ViewportSize"].SetValue(value);
		overlayEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		overlayEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);
		overlayEffect.Parameters["OverlayTexture"].SetValue(GhostedStructuresSpriteSheet.Texture);
		Texture2D distanceHeightAndBillboardAlphaRenderTarget = DistanceHeightAndBillboardAlphaRenderTarget;
		overlayEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(distanceHeightAndBillboardAlphaRenderTarget);
		foreach (EffectPass pass in overlayEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, overlayVertices, 0, overlayQuadIndex * 4, featureIndices, 0, overlayQuadIndex * 2);
		}
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
	}

	private void DrawOverlayGroundSprites()
	{
		SetupInterfaceOnMapQuads();
		UWGame.Port.RenderTrace.Log("overlay ground sprites offered: " + overlayGroundSpriteQuadIndex + " quad(s)");
		if (overlayGroundSpriteQuadIndex == 0)
		{
			return;
		}
		The.Client.GraphicsDevice.BlendState = overlayBlendState;
		overlayGroundSpritesEffect.CurrentTechnique = overlayGroundSpritesEffect.Techniques["DrawOverlayGroundSprite"];
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		overlayGroundSpritesEffect.Parameters["ViewportSize"].SetValue(value);
		// Dropped by the OpenGL rebuild - the shader never reads it. See Port/EffectCompat.
		UWGame.Port.EffectCompat.SetIfDeclared(overlayGroundSpritesEffect, "WindowPosition", The.MapUI.MapWindowWorldPosition);
		overlayGroundSpritesEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);
		overlayGroundSpritesEffect.Parameters["OverlayTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);
		overlayGroundSpritesEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);
		foreach (EffectPass pass in overlayGroundSpritesEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, overlayGroundSpriteVertices, 0, overlayGroundSpriteQuadIndex * 4, featureIndices, 0, overlayGroundSpriteQuadIndex * 2);
			UWGame.Port.RenderTrace.Submit("overlay ground sprites", overlayGroundSpriteQuadIndex * 2);
		}
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
	}

	private void DrawInfluenceMapSprites()
	{
		SetupInfluenceQuads();
		UWGame.Port.RenderTrace.Log("influence map sprites offered: " + influenceMapQuadIndex + " quad(s)");
		if (influenceMapQuadIndex == 0)
		{
			return;
		}
		The.Client.GraphicsDevice.BlendState = overlayBlendState;
		overlayGroundSpritesEffect.CurrentTechnique = overlayGroundSpritesEffect.Techniques["DrawInfluenceOverlay"];
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		overlayGroundSpritesEffect.Parameters["ViewportSize"].SetValue(value);
		UWGame.Port.EffectCompat.SetIfDeclared(overlayGroundSpritesEffect, "WindowPosition", The.MapUI.MapWindowWorldPosition);
		overlayGroundSpritesEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);
		overlayGroundSpritesEffect.Parameters["OverlayTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);
		foreach (EffectPass pass in overlayGroundSpritesEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, influenceMapVertices, 0, influenceMapQuadIndex * 4, featureIndices, 0, influenceMapQuadIndex * 2);
			UWGame.Port.RenderTrace.Submit("influence map sprites", influenceMapQuadIndex * 2);
		}
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
	}

	private void GetLightsToDrawAndDrawThem()
	{
		int num = 0;
		foreach (LightSource item in lightSourcesToDraw)
		{
			item.CopyQuadToVertexBuffer(lightSourceVertices, num);
			num++;
		}
		int num2;
		if (num <= 0)
		{
			num2 = ((lightEmittingModels.Count > 0) ? 1 : 0);
			if (num2 == 0)
			{
				goto IL_006d;
			}
		}
		else
		{
			num2 = 1;
		}
		The.Client.GraphicsDevice.BlendState = lightsBlendAdd;
		goto IL_006d;
		IL_006d:
		if (num > 0)
		{
			DrawLightSources(num, DrawLightsTechnique.TwoDeeLightSources);
			num = 0;
		}
		if (lightEmittingModels.Count > 0)
		{
			LightSource lightSource = new LightSource(Vector3.Zero);
			lightSource.SetupQuadVertices(Vector3.Zero, Vector2.Zero, 0f, emissiveModelLightRenderTarget.Bounds, emissiveModelLightRenderTarget);
			num = 0;
			lightSource.CopyQuadToVertexBuffer(lightSourceVertices, num);
			num = 1;
			DrawLightSources(num, DrawLightsTechnique.ModelEmittedLight);
		}
		if (num2 != 0)
		{
			The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		}
		lightEmittingModels.Clear();
		lightSourcesToDraw.Clear();
	}

	private void DrawOutlines()
	{
		if (The.Client.spriteBatch != null)
		{
			Effect edgeDetectEffect = The.Client.EdgeDetectEffect;
			EffectParameterCollection parameters = edgeDetectEffect.Parameters;
			parameters["EdgeWidth"].SetValue(0.4f);
			parameters["EdgeIntensity"].SetValue(0.4f);
			Vector2 value = new Vector2(DiffuseMSRenderTarget.Width, DiffuseMSRenderTarget.Height);
			parameters["ScreenResolution"].SetValue(value);
			parameters["NormalDepthTexture"].SetValue(edgeDetectNormalDepthRenderTarget);
			edgeDetectEffect.CurrentTechnique = edgeDetectEffect.Techniques["EdgeDetect"];
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, edgeDetectEffect);
			The.Client.spriteBatch.Draw(DiffuseMSRenderTarget, Vector2.Zero, Color.White);
			The.Client.spriteBatch.End();
		}
	}

	private void DrawLightSources(int lightSourceIndex, DrawLightsTechnique tech)
	{
		float num = new Vector3(1f - TimeOfDayLightingFactor.X, 1f - TimeOfDayLightingFactor.Y, 1f - TimeOfDayLightingFactor.Z).Length();
		num = MathHelper.Clamp(num / 0.7f, 0f, 1f);
		if (tech == DrawLightsTechnique.TwoDeeLightSources)
		{
			lightSourceEffect.CurrentTechnique = lightSourceEffect.Techniques["DrawLightSources"];
			lightSourceEffect.Parameters["LightSourceTexture"].SetValue(GameData.Instance.LightSourcesSpriteSheet.Texture);
		}
		else
		{
			lightSourceEffect.CurrentTechnique = lightSourceEffect.Techniques["DrawModelEmitterLights"];
			lightSourceEffect.Parameters["EmitterLightSourceDistance"].SetValue(emissiveModelLightDistanceRenderTarget);
			lightSourceEffect.Parameters["LightSourceTexture"].SetValue(emissiveModelLightRenderTarget);
		}
		lightSourceEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		lightSourceEffect.Parameters["DarknessLevel"].SetValue(num);
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		lightSourceEffect.Parameters["ViewportSize"].SetValue(value);
		lightSourceEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		lightSourceEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);
		lightSourceEffect.Parameters["DiffuseSceneTexture"].SetValue(diffuseRenderTarget);
		foreach (EffectPass pass in lightSourceEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, lightSourceVertices, 0, lightSourceIndex * 4, lightSourceIndices, 0, lightSourceIndex * 2);
		}
	}

	private void DrawBillboardBatch(int featureQuadIndex, RenderTechnique technique)
	{
		billboardEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		switch (technique)
		{
		case RenderTechnique.Standard:
		{
			The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			if (DayAndNightEffects.SunAnimation != DayAndNightEffects.SunAnimations.Night)
			{
				billboardEffect.CurrentTechnique = billboardEffect.Techniques["Standard"];
				billboardEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
			}
			else
			{
				billboardEffect.CurrentTechnique = billboardEffect.Techniques["StandardAtNight"];
			}
			Dimension drawArea = The.Client.Controller.DrawArea;
			Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
			billboardEffect.Parameters["ViewportSize"].SetValue(value);
			billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
			billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);
			billboardEffect.Parameters["NormalTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.NormalTexture);
			billboardEffect.Parameters["LightPosition"].SetValue(new Vector3(The.Sim.DateAndTime.SunPosition.X, The.Sim.DateAndTime.SunPosition.Y, The.Sim.DateAndTime.SunPosition.Z));
			billboardEffect.Parameters["WindTime"].SetValue(windTime);
			billboardEffect.Parameters["ShadowXAlignment"].SetValue(DayAndNightEffects.ShadowXAlignment);
			{
				foreach (EffectPass pass in billboardEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, featureVertices, 0, featureQuadIndex * 4, featureIndices, 0, featureQuadIndex * 2);
				}
				return;
			}
		}
		case RenderTechnique.NormalsAndDepth:
			The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			The.Client.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
			billboardEffect.CurrentTechnique = billboardEffect.Techniques["NormalsAndDepthMap"];
			break;
		case RenderTechnique.DepthHeightBillboardAlpha:
			billboardEffect.CurrentTechnique = billboardEffect.Techniques["DepthHeightBillboardAlpha"];
			break;
		case RenderTechnique.NoLighting:
			billboardEffect.CurrentTechnique = billboardEffect.Techniques["Shadow"];
			billboardEffect.Parameters["Rotation"].SetValue(DayAndNightEffects.SunShadowRotationMatrix);
			billboardEffect.Parameters["ShadowScaling"].SetValue(DayAndNightEffects.ShadowScaling);
			break;
		}
		Dimension drawArea2 = The.Client.Controller.DrawArea;
		Vector2 value2 = new Vector2(drawArea2.Width, drawArea2.Height);
		billboardEffect.Parameters["ViewportSize"].SetValue(value2);
		billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);
		billboardEffect.Parameters["WindTime"].SetValue(windTime);
		billboardEffect.Parameters["ShadowXAlignment"].SetValue(DayAndNightEffects.ShadowXAlignment);
		foreach (EffectPass pass2 in billboardEffect.CurrentTechnique.Passes)
		{
			pass2.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, featureVertices, 0, featureQuadIndex * 4, featureIndices, 0, featureQuadIndex * 2);
		}
	}

	public void UpdateTerrainViewMatrix()
	{
		TerrainCameraPosition = CameraTarget;
		Vector3 cameraTarget = CameraTarget;
		TerrainCameraPosition.Z = -1000f;
		float num = 150f;
		TerrainCameraPosition.X += num;
		cameraTarget.X = TerrainCameraPosition.X;
		Vector3 cameraUpVector = Vector3.Cross(CameraTarget - TerrainCameraPosition, Vector3.Left);
		cameraUpVector.Normalize();
		TerrainViewMatrix = Matrix.CreateLookAt(TerrainCameraPosition, cameraTarget, cameraUpVector);
	}

	private Vector4 ComputeTimeOfDayLightMultiplier(Color tint)
	{
		Vector4 result = tint.ToVector4();
		result *= result.W;
		float x = result.X;
		float y = result.Y;
		float z = result.Z;
		result.X = 1f - z - y;
		result.Y = 1f - x - z;
		result.Z = 1f - x - y;
		result.W = 1f;
		return result;
	}

	private void DrawTimeOfDayOverlay()
	{
		GraphicsDevice graphicsDevice = The.Client.GraphicsDevice;
		TimeOfDayLightingEffect.Parameters["baseTexture"].SetValue(diffuseRenderTarget);
		TimeOfDayLightingEffect.Parameters["AmbientColorForLightSources"].SetValue(TimeOfDayLightingFactor);
		TimeOfDayLightingEffect.CurrentTechnique = TimeOfDayLightingEffect.Techniques["AmbientLight"];
		foreach (EffectPass pass in TimeOfDayLightingEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.quadRenderer.Render(graphicsDevice, -Vector2.One, Vector2.One);
		}
	}

	public void UpdatePicking()
	{
		Ray ray = CalculateCursorRay(The.Client.Projection, View);
		PickedModel = null;
		float num = float.MaxValue;
		foreach (List<ILocatable> item in sortedObjectsToDraw)
		{
			foreach (ILocatable item2 in item)
			{
				Renderable asRenderable = item2.AsRenderable;
				if (asRenderable == null || asRenderable.RenderAsModel == null)
				{
					continue;
				}
				IKnownEntityData parent = asRenderable.Parent;
				if (parent == null)
				{
					continue;
				}
				bool insideBoundingSphere;
				Vector3 vertex;
				Vector3 vertex2;
				Vector3 vertex3;
				float? num2 = RayIntersectsModel(ray, asRenderable.RenderAsModel.AnimatedModel.ModelAnimator.Model, asRenderable.RenderAsModel.AnimatedModel.StandardDrawingWorldTransformation, out insideBoundingSphere, out vertex, out vertex2, out vertex3);
				if (num2.HasValue)
				{
					if (num2 < num)
					{
						num = num2.Value;
						PickedModel = parent.EntityID;
					}
				}
				else if (insideBoundingSphere && PickedModel.HasValue)
				{
				}
			}
		}
	}

	public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
	{
		InputData inputData = The.Client.Controller.InputData;
		Vector3 source = new Vector3(inputData.mouseX, inputData.mouseY, 0f);
		Vector3 source2 = new Vector3(inputData.mouseX, inputData.mouseY, 1f);
		Vector3 vector = DrawAreaViewport.Unproject(source, projectionMatrix, viewMatrix, Matrix.Identity);
		Vector3 direction = DrawAreaViewport.Unproject(source2, projectionMatrix, viewMatrix, Matrix.Identity) - vector;
		direction.Normalize();
		return new Ray(vector, direction);
	}

	private static float? RayIntersectsModel(Ray ray, Model model, Matrix modelTransform, out bool insideBoundingSphere, out Vector3 vertex1, out Vector3 vertex2, out Vector3 vertex3)
	{
		vertex1 = (vertex2 = (vertex3 = Vector3.Zero));
		Matrix matrix = Matrix.Invert(modelTransform);
		Dictionary<string, object> dictionary = (Dictionary<string, object>)model.Tag;
		if (dictionary == null)
		{
			throw new InvalidOperationException("Model.Tag is not set correctly. Make sure your model was built using the custom TrianglePickingProcessor.");
		}
		float? num = ((BoundingSphere)dictionary["BoundingSphere"]).Transform(modelTransform).Intersects(ray);
		ray.Position = Vector3.Transform(ray.Position, matrix);
		ray.Direction = Vector3.TransformNormal(ray.Direction, matrix);
		if (!num.HasValue)
		{
			insideBoundingSphere = false;
			return null;
		}
		insideBoundingSphere = true;
		float? result = null;
		Vector3[] array = (Vector3[])dictionary["Vertices"];
		for (int i = 0; i < array.Length; i += 3)
		{
			RayIntersectsTriangle(ref ray, ref array[i], ref array[i + 1], ref array[i + 2], out var result2);
			if (result2.HasValue)
			{
				return result2;
			}
		}
		return result;
	}

	private static void RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float? result)
	{
		Vector3.Subtract(ref vertex2, ref vertex1, out var result2);
		Vector3.Subtract(ref vertex3, ref vertex1, out var result3);
		Vector3.Cross(ref ray.Direction, ref result3, out var result4);
		Vector3.Dot(ref result2, ref result4, out var result5);
		if (result5 > -1E-45f && result5 < float.Epsilon)
		{
			result = null;
			return;
		}
		float num = 1f / result5;
		Vector3.Subtract(ref ray.Position, ref vertex1, out var result6);
		Vector3.Dot(ref result6, ref result4, out var result7);
		result7 *= num;
		if (result7 < 0f || result7 > 1f)
		{
			result = null;
			return;
		}
		Vector3.Cross(ref result6, ref result2, out var result8);
		Vector3.Dot(ref ray.Direction, ref result8, out var result9);
		result9 *= num;
		if (result9 < 0f || result7 + result9 > 1f)
		{
			result = null;
			return;
		}
		Vector3.Dot(ref result3, ref result8, out var result10);
		result10 *= num;
		if (result10 < 0f)
		{
			result = null;
		}
		else
		{
			result = result10;
		}
	}
}
