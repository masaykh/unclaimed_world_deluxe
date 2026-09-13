using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables;

public class SkinnedAnimatedModel : AnimatedModel
{
	static SkinnedAnimatedModel()
	{
		AnimatedModel.overlayBlendState = new BlendState();
		AnimatedModel.overlayBlendState.ColorSourceBlend = Blend.SourceAlpha;
		AnimatedModel.overlayBlendState.ColorDestinationBlend = Blend.One;
	}

	public SkinnedAnimatedModel()
	{
	}

	public SkinnedAnimatedModel(SkinnedAnimatedModel original, bool canAnimate)
		: base(original, canAnimate)
	{
	}

	public override void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection, GameWorldRenderer.RenderTechnique technique, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, float alphaFactor, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null)
	{
		if (The.Client == null)
		{
			return;
		}
		ModelAnimator.World = world;
		GraphicsDevice graphicsDevice = The.Client.GraphicsDevice;
		HandleEmitterModelParts(technique);
		if (technique == GameWorldRenderer.RenderTechnique.Standard || technique == GameWorldRenderer.RenderTechnique.DrawModelEmitters || technique == GameWorldRenderer.RenderTechnique.StandardOverlay || technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
		{
			CreateLights(lightIntensity, tintColor);
		}
		if (technique == GameWorldRenderer.RenderTechnique.Standard || technique == GameWorldRenderer.RenderTechnique.StandardMonochrome || technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
		{
			Dimension drawArea = The.Client.Controller.DrawArea;
			Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
			Vector2 scanLinesDimensions = GetScanLinesDimensions();
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			if (technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
			{
				graphicsDevice.BlendState = AnimatedModel.overlayBlendState;
			}
			else
			{
				graphicsDevice.BlendState = BlendState.AlphaBlend;
			}
			foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
			{
				foreach (Effect effect in mesh.Effects)
				{
					switch (technique)
					{
					case GameWorldRenderer.RenderTechnique.StandardMonochrome:
						effect.CurrentTechnique = effect.Techniques["StandardRenderMonochrome"];
						break;
					case GameWorldRenderer.RenderTechnique.StandardOverlay:
					{
						effect.CurrentTechnique = effect.Techniques["StandardOverlay"];
						effect.Parameters["ViewportSize"].SetValue(value);
						effect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
						Texture2D distanceHeightAndBillboardAlphaRenderTarget = The.Client.Renderer.DistanceHeightAndBillboardAlphaRenderTarget;
						effect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(distanceHeightAndBillboardAlphaRenderTarget);
						effect.Parameters["ScanlinesTexture"].SetValue(The.Client.Renderer.Scanlines);
						effect.Parameters["ScanlinesTextureDimensions"].SetValue(scanLinesDimensions);
						effect.Parameters["OverlayGradient"].SetValue(The.Client.Renderer.OverlayGradient);
						break;
					}
					default:
						effect.CurrentTechnique = effect.Techniques["SkinnedRender"];
						break;
					}
					if (basicTexture != null)
					{
						effect.Parameters["BasicTexture"].SetValue(basicTexture);
					}
					effect.Parameters["View"].SetValue(view);
					effect.Parameters["World"].SetValue(world);
					effect.Parameters["Projection"].SetValue(projection);
					effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);
					effect.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);
					effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
					effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
					effect.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);
					effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
					effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
					effect.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);
					effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
					effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
					effect.Parameters["ReplaceColor0"].SetValue(replaceColor0.Value);
					effect.Parameters["ReplaceColor1"].SetValue(replaceColor1.Value);
					effect.Parameters["ReplaceColor2"].SetValue(replaceColor2.Value);
					effect.Parameters["ReplaceColor3"].SetValue(replaceColor3.Value);
					effect.Parameters["AlphaFactor"].SetValue(alphaFactor);
				}
			}
			ModelAnimator.Draw(The.Sim.GameTime);
			// PORT DIAGNOSTIC (port.renderTrace). AFTER the draw, deliberately: ModelAnimator.Draw is
			// what uploads the bone palette, so reading it beforehand reports the PREVIOUS frame - or
			// zeros on the first one. That cost a round trip: the palette read back all-zero and looked
			// like the bug, when it was only being read too early.
			UWGame.Port.RenderTrace.ModelDraw("skinned " + technique + " " + (ModelAnimator?.Model?.Meshes?.Count.ToString() ?? "?") + "m", ModelAnimator?.Model, ModelAnimator?.Model?.Meshes?[0]?.Effects?[0], skinned: true);

			graphicsDevice.DepthStencilState = DepthStencilState.None;
			return;
		}
		foreach (ModelMesh mesh2 in ModelAnimator.Model.Meshes)
		{
			foreach (Effect effect2 in mesh2.Effects)
			{
				switch (technique)
				{
				case GameWorldRenderer.RenderTechnique.NoLighting:
					effect2.CurrentTechnique = effect2.Techniques["SkinnedRenderNoLighting"];
					break;
				case GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha:
					effect2.CurrentTechnique = effect2.Techniques["DepthHeightBillboardAlpha"];
					break;
				case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
					effect2.CurrentTechnique = effect2.Techniques["EmittersOnly"];
					effect2.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
					effect2.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
					effect2.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
					effect2.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
					effect2.Parameters["DirLight2Direction"].SetValue(backLightDirection);
					effect2.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
					break;
				case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
					effect2.CurrentTechnique = effect2.Techniques["NormalDepth"];
					graphicsDevice.BlendState = BlendState.NonPremultiplied;
					graphicsDevice.DepthStencilState = DepthStencilState.Default;
					break;
				}
				effect2.Parameters["View"].SetValue(view);
				effect2.Parameters["World"].SetValue(world);
				effect2.Parameters["Projection"].SetValue(projection);
				effect2.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);
				effect2.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);
				Dimension drawArea2 = The.Client.Controller.DrawArea;
				Vector2 value2 = new Vector2(drawArea2.Width, drawArea2.Height);
				effect2.Parameters["ViewportSize"].SetValue(value2);
				effect2.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
				effect2.Parameters["AlphaFactor"].SetValue(alphaFactor);
			}
		}
		// PORT DIAGNOSTIC (port.renderTrace). Recorded HERE, at the only place a model reaches the
		// GPU, so a model that is never offered and one whose parts are all skipped read differently.
		ModelAnimator.Draw(The.Sim.GameTime);
		UWGame.Port.RenderTrace.ModelDraw("skinned " + technique + " " + (ModelAnimator?.Model?.Meshes?.Count.ToString() ?? "?") + "m", ModelAnimator?.Model, ModelAnimator?.Model?.Meshes?[0]?.Effects?[0], skinned: true);
	}
}
