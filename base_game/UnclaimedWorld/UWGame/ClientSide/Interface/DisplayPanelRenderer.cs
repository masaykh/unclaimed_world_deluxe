using System.Collections.Generic;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;
using UWGame.Control;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class DisplayPanelRenderer
{
	public Texture2D scanlines;

	public Texture2D dropshadow;

	public Texture2D fingerprint;

	public Texture2D dustOnEdges;

	public Texture2D cleanedLCD;

	public Texture2D cleanedMoreLCD;

	public Texture2D dust;

	public Effect CRTEffect;

	public Effect LCDEffect;

	private VertexCRTQuad[] crtDisplayVertices;

	private short[] allCRTIndices;

	private int noOfCRTQuads = 6;

	private int DisplayQuadIndex;

	private VertexLCDQuad[] lcdDisplayVertices;

	private short[] lcdIndices;

	private int noOfLCDQuads = 6;

	private int LCDQuadIndex;

	public Dictionary<Level, List<CRTScreen>> CRTPanels = new Dictionary<Level, List<CRTScreen>>();

	public Dictionary<Level, List<LCDScreen>> LCDPanels = new Dictionary<Level, List<LCDScreen>>();

	public RenderTarget2D DisplayPanelContentRenderTarget;

	private int scanlinesWidth;

	private int scanlinesHeight;

	private GraphicsDevice device;

	private UnclaimedWorld game;

	public DisplayPanelRenderer(UnclaimedWorld game)
	{
		this.game = game;
		LCDPanels.Add(Level.Min, new List<LCDScreen>());
		LCDPanels.Add(Level.RockBottom, new List<LCDScreen>());
		LCDPanels.Add(Level.Bottom, new List<LCDScreen>());
		LCDPanels.Add(Level.BelowBelowBelowMiddle, new List<LCDScreen>());
		LCDPanels.Add(Level.BelowBelowMiddle, new List<LCDScreen>());
		LCDPanels.Add(Level.BelowMiddle, new List<LCDScreen>());
		LCDPanels.Add(Level.Middle, new List<LCDScreen>());
		LCDPanels.Add(Level.Dialogs, new List<LCDScreen>());
		LCDPanels.Add(Level.StackedDialogs, new List<LCDScreen>());
		LCDPanels.Add(Level.EventDialog, new List<LCDScreen>());
		LCDPanels.Add(Level.Menu, new List<LCDScreen>());
		LCDPanels.Add(Level.MessageBox, new List<LCDScreen>());
		CRTPanels.Add(Level.Min, new List<CRTScreen>());
		CRTPanels.Add(Level.RockBottom, new List<CRTScreen>());
		CRTPanels.Add(Level.Bottom, new List<CRTScreen>());
		CRTPanels.Add(Level.BelowBelowBelowMiddle, new List<CRTScreen>());
		CRTPanels.Add(Level.BelowBelowMiddle, new List<CRTScreen>());
		CRTPanels.Add(Level.BelowMiddle, new List<CRTScreen>());
		CRTPanels.Add(Level.Middle, new List<CRTScreen>());
		CRTPanels.Add(Level.Dialogs, new List<CRTScreen>());
		CRTPanels.Add(Level.StackedDialogs, new List<CRTScreen>());
		CRTPanels.Add(Level.EventDialog, new List<CRTScreen>());
		CRTPanels.Add(Level.Menu, new List<CRTScreen>());
		CRTPanels.Add(Level.MessageBox, new List<CRTScreen>());
		device = game.GraphicsDevice;
		PresentationParameters presentationParameters = device.PresentationParameters;
		DisplayPanelContentRenderTarget = new RenderTarget2D(device, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat);
	}

	public void Destroy()
	{
		DisplayPanelContentRenderTarget.Dispose();
	}

	public void Initialize()
	{
		crtDisplayVertices = new VertexCRTQuad[noOfCRTQuads * 4];
		allCRTIndices = new short[noOfCRTQuads * 6];
		GameWorldRenderer.SetUpIndices(noOfCRTQuads, allCRTIndices);
		lcdDisplayVertices = new VertexLCDQuad[noOfLCDQuads * 4];
		lcdIndices = new short[noOfLCDQuads * 6];
		GameWorldRenderer.SetUpIndices(noOfLCDQuads, lcdIndices);
	}

	public void LoadContent()
	{
		device = game.GraphicsDevice;
		if (game.Controller.GraphicsLevelSetting == Controller.GraphicsLevel.High)
		{
			scanlines = game.Content.Load<Texture2D>("GUI\\CRT_ScanLines");
			cleanedLCD = game.Content.Load<Texture2D>("GUI\\LCD_Cleaned");
			cleanedMoreLCD = game.Content.Load<Texture2D>("GUI\\LCD_Cleaned_more");
			dustOnEdges = game.Content.Load<Texture2D>("GUI\\CRT_Grunge_v2");
			dust = game.Content.Load<Texture2D>("GUI\\Comm\\event_dust");
			CRTEffect = game.Content.Load<Effect>("GUI\\CRT");
			LCDEffect = game.Content.Load<Effect>("GUI\\LCD");
			scanlinesWidth = scanlines.Width;
			scanlinesHeight = scanlines.Height;
		}
	}

	public CRTScreen AddCRT(UIComponent displayBox, Point destPos, int width, int height, Level level, Window window, ReflectionToUse toUse, bool isMonochrome, float alpha = 1f)
	{
		return AddCRT(displayBox, new Rectangle(destPos.X, destPos.Y, width, height), new Rectangle(destPos.X, destPos.Y, width, height), level, window, toUse, isMonochrome, alpha);
	}

	public CRTScreen AddCRT(UIComponent displayBox, Rectangle source, Rectangle destination, Level level, Window window, ReflectionToUse toUse, bool isMonochrome, float alpha = 1f)
	{
		CRTScreen cRTScreen = new CRTScreen(displayBox, source, destination, DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height, scanlinesWidth, scanlinesHeight, isMonochrome, toUse, window, alpha);
		CRTPanels[level].Add(cRTScreen);
		return cRTScreen;
	}

	public LCDScreen AddLCD(UIComponent displayBox, Level level, Window window, bool drawDust)
	{
		Rectangle rectangle = new Rectangle(displayBox.X, displayBox.Y, displayBox.Width, displayBox.Height);
		LCDScreen lCDScreen = new LCDScreen(displayBox, rectangle, rectangle, DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height, DisplayPanelContentRenderTarget.Width, DisplayPanelContentRenderTarget.Height, dust.Width, dust.Height, window, drawDust);
		LCDPanels[level].Add(lCDScreen);
		return lCDScreen;
	}

	public void Update(GameTime gameTime)
	{
	}

	public void StartPanelRendering()
	{
		device.SetRenderTarget(DisplayPanelContentRenderTarget);
		Color color = new Color(0, 0, 0, 0);
		device.Clear(ClearOptions.Target, color, 1f, 0);
	}

	public void Draw(Level level)
	{
		game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		game.Controller.SetZoomRenderTaget();
		DrawCRTsAtThisLevel(level);
		DrawLCDsAtThisLevel(level);
	}

	private void DrawCRTsAtThisLevel(Level level)
	{
		if (!CRTPanels.TryGetValue(level, out var value) || value.Count == 0)
		{
			return;
		}
		DisplayQuadIndex = 0;
		foreach (CRTScreen item in CRTPanels[level])
		{
			if (item.ParentWindow == null || item.ParentWindow.Visible)
			{
				item.CopyQuadToVertexBuffer(crtDisplayVertices, DisplayQuadIndex);
				DisplayQuadIndex++;
			}
		}
		if (DisplayQuadIndex == 0)
		{
			return;
		}
		CRTEffect.CurrentTechnique = CRTEffect.Techniques["CRT_HighQuality"];
		CRTEffect.Parameters["ScanlinesTexture"].SetValue(scanlines);
		CRTEffect.Parameters["GrungeTexture"].SetValue(dustOnEdges);
		CRTEffect.Parameters["PanelContentTexture"].SetValue(DisplayPanelContentRenderTarget);
		Dimension drawArea = game.Controller.DrawArea;
		Vector2 value2 = new Vector2(drawArea.Width, drawArea.Height);
		CRTEffect.Parameters["ViewportSize"].SetValue(value2);
		foreach (EffectPass pass in CRTEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, crtDisplayVertices, 0, DisplayQuadIndex * 4, allCRTIndices, 0, DisplayQuadIndex * 2);
		}
	}

	private void DrawLCDsAtThisLevel(Level level)
	{
		if (!LCDPanels.TryGetValue(level, out var value) || value.Count == 0)
		{
			return;
		}
		DisplayQuadIndex = 0;
		foreach (LCDScreen item in LCDPanels[level])
		{
			if (item.ParentWindow == null || item.ParentWindow.Visible)
			{
				item.CopyQuadToVertexBuffer(lcdDisplayVertices, DisplayQuadIndex);
				DisplayQuadIndex++;
			}
		}
		if (DisplayQuadIndex == 0)
		{
			return;
		}
		_ = ref lcdDisplayVertices[0];
		LCDEffect.CurrentTechnique = LCDEffect.Techniques["LCD"];
		LCDEffect.Parameters["GrungeTexture"].SetValue(dust);
		LCDEffect.Parameters["CleanedTexture"].SetValue(cleanedMoreLCD);
		LCDEffect.Parameters["PanelContentTexture"].SetValue(DisplayPanelContentRenderTarget);
		Dimension drawArea = game.Controller.DrawArea;
		Vector2 value2 = new Vector2(drawArea.Width, drawArea.Height);
		LCDEffect.Parameters["ViewportSize"].SetValue(value2);
		foreach (EffectPass pass in LCDEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, lcdDisplayVertices, 0, DisplayQuadIndex * 4, lcdIndices, 0, DisplayQuadIndex * 2);
		}
	}
}
