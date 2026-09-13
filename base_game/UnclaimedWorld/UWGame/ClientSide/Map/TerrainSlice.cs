using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Map;

public class TerrainSlice
{
	private RenderTarget2D terrainRenderTargetTexture;

	public bool NeedsRedraw;

	public bool HasRedrawn;

	private Vector2 screenPos;

	private Vector2 position;

	private GameWorldRenderer renderer;

	private List<TerrainBatch> mySliceBatches;

	public RenderTarget2D clipRenderTarget;

	private Plane clipPlane;

	public TerrainSlice(GameWorldRenderer renderer, Vector2 pos, int width, int height)
	{
		this.renderer = renderer;
		pos.X -= 0.5f;
		pos.Y -= 0.5f;
		position = pos;
		PresentationParameters presentationParameters = The.Client.GraphicsDevice.PresentationParameters;
		terrainRenderTargetTexture = new RenderTarget2D(The.Client.GraphicsDevice, width, height, mipMap: false, The.Client.GraphicsDevice.DisplayMode.Format, presentationParameters.DepthStencilFormat);
		NeedsRedraw = true;
		clipPlane = GameWorldRenderer.CreatePlane(renderer.Water.WaterHeight + 1.5f, new Vector3(0f, 0f, -1f), clipSide: true);
		clipRenderTarget = new RenderTarget2D(The.Client.GraphicsDevice, width, height, mipMap: false, The.Client.GraphicsDevice.DisplayMode.Format, presentationParameters.DepthStencilFormat);
	}

	public void Destroy()
	{
		terrainRenderTargetTexture.Dispose();
		clipRenderTarget.Dispose();
	}

	public void DiscardTexture()
	{
		terrainRenderTargetTexture = null;
	}

	private void Redraw()
	{
		The.Client.GraphicsDevice.SetRenderTarget(terrainRenderTargetTexture);
		The.Client.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
		if (mySliceBatches == null)
		{
			renderer.SetUpTerrainVerticesAndIndicesInCurrentView(The.Client.Renderer.TerrainSliceSize, position, out mySliceBatches);
		}
		UWGame.Port.RenderTrace.Log("slice redraw at " + position.ToString() + ": batches=" +
			(mySliceBatches == null ? "NULL" : mySliceBatches.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)) +
			", verts in first batch=" + (mySliceBatches != null && mySliceBatches.Count > 0 && mySliceBatches[0].terrainVerticesList != null
				? mySliceBatches[0].terrainVerticesList.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) : "n/a"));
		UWGame.Port.RenderTrace.Submit("terrain slice redraw");
		mySliceBatches[0].terrainVerticesList.Min((VertexMultitextured v) => v.Position.X);
		mySliceBatches[0].terrainVerticesList.Max((VertexMultitextured v) => v.Position.X);
		mySliceBatches[0].terrainVerticesList.Min((VertexMultitextured v) => v.Position.Y);
		mySliceBatches[0].terrainVerticesList.Max((VertexMultitextured v) => v.Position.Y);
		Vector2 renderTargetSize = new Vector2(terrainRenderTargetTexture.Width, terrainRenderTargetTexture.Height);
		renderer.DrawTerrainUserVertices(renderer.terrainBatches, null, position, renderTargetSize, mySliceBatches);
		The.Client.Controller.SetZoomRenderTaget();
		The.Client.GraphicsDevice.SetRenderTarget(clipRenderTarget);
		The.Client.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
		renderer.DrawTerrainUserVertices(renderer.terrainBatches, clipPlane, position, renderTargetSize, mySliceBatches);
		The.Client.Controller.SetZoomRenderTaget();
		mySliceBatches = null;
		NeedsRedraw = false;
	}

	public void DrawRefractionMap()
	{
		if (terrainRenderTargetTexture != null && The.Client.spriteBatch != null)
		{
			screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			The.Client.spriteBatch.Draw(terrainRenderTargetTexture, screenPos, Color.White);
			The.Client.spriteBatch.End();
		}
	}

	public void DrawClipMap()
	{
		if (clipRenderTarget != null && The.Client.spriteBatch != null)
		{
			screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			The.Client.spriteBatch.Draw(clipRenderTarget, screenPos, Color.White);
			The.Client.spriteBatch.End();
		}
	}

	public void Draw(RenderTarget2D oldRenderTarget)
	{
		// PORT DIAGNOSTIC (port.renderTrace). This is the path the terrain actually takes: drawn
		// into a per-slice target once, then blitted from it every frame. Recording whether the
		// contents were reported lost, whether a redraw happened, and what the cached target holds
		// is what separates "the terrain was never drawn" from "it was drawn and then thrown away".
		bool contentLost = terrainRenderTargetTexture.IsContentLost;
		if (contentLost)
		{
			NeedsRedraw = true;
		}
		if (UWGame.Port.RenderTrace.Recording)
		{
			UWGame.Port.RenderTrace.Log("slice at " + position.ToString() +
				": contentLost=" + contentLost + ", needsRedraw=" + NeedsRedraw +
				", target=" + (terrainRenderTargetTexture == null ? "NULL" :
					terrainRenderTargetTexture.Width + "x" + terrainRenderTargetTexture.Height + " " +
					terrainRenderTargetTexture.Format + " usage " + terrainRenderTargetTexture.RenderTargetUsage));
		}
		if (NeedsRedraw)
		{
			Redraw();
			The.Client.GraphicsDevice.SetRenderTarget(oldRenderTarget);
		}
		if (terrainRenderTargetTexture != null && The.Client.spriteBatch != null)
		{
			screenPos = The.MapUI.WorldPosToScreen(new Vector2(position.X - 0.5f, position.Y - 0.5f));
			The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			The.Client.spriteBatch.Draw(terrainRenderTargetTexture, screenPos, Color.White);
			The.Client.spriteBatch.End();
			UWGame.Port.RenderTrace.Submit("terrain slice blit", 2);
			// One PNG PER SLICE, named by position. A single dump answered "does a slice hold a
			// picture", which was the right question while none of the terrain drew; now that some
			// of it does, the question is WHICH slices are blank, and one sample cannot say.
			UWGame.Port.RenderTrace.DumpTarget(
				"6-terrainSlice-" + ((int)position.X).ToString(System.Globalization.CultureInfo.InvariantCulture) +
				"x" + ((int)position.Y).ToString(System.Globalization.CultureInfo.InvariantCulture),
				terrainRenderTargetTexture);
		}
	}
}
