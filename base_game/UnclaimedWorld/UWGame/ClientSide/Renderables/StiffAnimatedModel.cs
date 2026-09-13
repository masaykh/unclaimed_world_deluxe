using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables;

public class StiffAnimatedModel : AnimatedModel
{
	private float mainLightTweakFactor = 0.8f;

	private float fillLightTweakFactor = 1f;

	private float backLightTweakFactor = 1f;

	public StiffAnimatedModel()
	{
	}

	public StiffAnimatedModel(StiffAnimatedModel original, bool canAnimate)
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
		float reflectivityFactor = 1f;
		graphicsDevice.RasterizerState = RasterizerState.CullNone;
		graphicsDevice.DepthStencilState = DepthStencilState.Default;
		if (technique == GameWorldRenderer.RenderTechnique.Standard || technique == GameWorldRenderer.RenderTechnique.DrawModelEmitters || technique == GameWorldRenderer.RenderTechnique.StandardOverlay || technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
		{
			CreateLights(technique, lightIntensity, ref reflectivityFactor, tintColor);
		}
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		if (technique == GameWorldRenderer.RenderTechnique.Standard || technique == GameWorldRenderer.RenderTechnique.StandardMonochrome || technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
		{
			Vector2 scanLinesDimensions = GetScanLinesDimensions();
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
				if (transforms == null)
				{
					break;
				}
				Matrix value2 = transforms[mesh.ParentBone.Index] * world;
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
						effect.CurrentTechnique = effect.Techniques["StandardRender"];
						break;
					}
					if (basicTexture != null)
					{
						effect.Parameters["BasicTexture"].SetValue(basicTexture);
					}
					effect.Parameters["AlphaFactor"].SetValue(alphaFactor);
					effect.Parameters["World"].SetValue(value2);
					effect.Parameters["View"].SetValue(view);
					effect.Parameters["Projection"].SetValue(projection);
					effect.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);
					effect.Parameters["EyePosition"].SetValue(The.Client.Renderer.CameraPosition);
					effect.Parameters["ReflectivityFactor"].SetValue(reflectivityFactor);
					effect.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
					effect.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
					effect.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);
					effect.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
					effect.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
					effect.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);
					effect.Parameters["DirLight2Direction"].SetValue(backLightDirection);
					effect.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
					// Dropped by the OpenGL rebuild of Vehicle - the shader never reads it. See Port/EffectCompat.
					UWGame.Port.EffectCompat.SetIfDeclared(effect, "DirLight2SpecularColor", backLightColor);
					effect.Parameters["ReplaceColor0"].SetValue(replaceColor0.Value);
					effect.Parameters["ReplaceColor1"].SetValue(replaceColor1.Value);
					effect.Parameters["ReplaceColor2"].SetValue(replaceColor2.Value);
					effect.Parameters["ReplaceColor3"].SetValue(replaceColor3.Value);
					effect.Parameters["DirtLevel"].SetValue(dirtLevel);
				}
			}
			ModelAnimator.Draw(The.Sim.GameTime);
			// PORT DIAGNOSTIC (port.renderTrace). AFTER the draw, deliberately: ModelAnimator.Draw is
			// what uploads the bone palette, so reading it beforehand reports the PREVIOUS frame - or
			// zeros on the first one. That cost a round trip: the palette read back all-zero and looked
			// like the bug, when it was only being read too early.
			UWGame.Port.RenderTrace.ModelDraw("stiff " + technique + " " + (ModelAnimator?.Model?.Meshes?.Count.ToString() ?? "?") + "m", ModelAnimator?.Model, ModelAnimator?.Model?.Meshes?[0]?.Effects?[0], skinned: false);

			return;
		}
		foreach (ModelMesh mesh2 in ModelAnimator.Model.Meshes)
		{
			if (transforms == null)
			{
				break;
			}
			Matrix value3 = transforms[mesh2.ParentBone.Index] * world;
			foreach (Effect effect2 in mesh2.Effects)
			{
				switch (technique)
				{
				case GameWorldRenderer.RenderTechnique.NoLighting:
					effect2.CurrentTechnique = effect2.Techniques["RenderNoLighting"];
					break;
				case GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha:
					effect2.CurrentTechnique = effect2.Techniques["DepthHeightBillboardAlpha"];
					break;
				case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
					effect2.CurrentTechnique = effect2.Techniques["NormalDepth"];
					break;
				case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
					effect2.CurrentTechnique = effect2.Techniques["EmittersOnly"];
					effect2.Parameters["DirLight0Direction"].SetValue(mainLightDirection);
					effect2.Parameters["DirLight0DiffuseColor"].SetValue(mainLightColor);
					effect2.Parameters["DirLight0SpecularColor"].SetValue(mainLightColor);
					effect2.Parameters["DirLight1Direction"].SetValue(fillLightDirection);
					effect2.Parameters["DirLight1DiffuseColor"].SetValue(fillLightColor);
					effect2.Parameters["DirLight1SpecularColor"].SetValue(fillLightColor);
					effect2.Parameters["DirLight2Direction"].SetValue(backLightDirection);
					effect2.Parameters["DirLight2DiffuseColor"].SetValue(backLightColor);
					UWGame.Port.EffectCompat.SetIfDeclared(effect2, "DirLight2SpecularColor", backLightColor);
					break;
				}
				effect2.Parameters["ViewportSize"].SetValue(value);
				effect2.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
				effect2.Parameters["World"].SetValue(value3);
				effect2.Parameters["View"].SetValue(view);
				effect2.Parameters["Projection"].SetValue(projection);
				effect2.Parameters["UncorrectedWorld"].SetValue(uncorrectedStandardDrawingWorldTransformation);
			}
		}
		// PORT DIAGNOSTIC (port.renderTrace). Recorded HERE, at the only place a model reaches the
		// GPU, so a model that is never offered and one whose parts are all skipped read differently.
		ModelAnimator.Draw(The.Sim.GameTime);
		UWGame.Port.RenderTrace.ModelDraw("stiff " + technique + " " + (ModelAnimator?.Model?.Meshes?.Count.ToString() ?? "?") + "m", ModelAnimator?.Model, ModelAnimator?.Model?.Meshes?[0]?.Effects?[0], skinned: false);
	}

	private void CreateLights(GameWorldRenderer.RenderTechnique technique, float lightIntensity, ref float reflectivityFactor, Color? tintColor = null)
	{
		if (technique == GameWorldRenderer.RenderTechnique.StandardMonochrome)
		{
			mainLightColor = 0.5f * lightIntensity * Vector3.One;
			fillLightColor = 0.2f * lightIntensity * Vector3.One;
			backLightColor = 0.4f * lightIntensity * Vector3.One;
			reflectivityFactor = 0.3f;
		}
		else if (tintColor.HasValue)
		{
			Vector3 vector = tintColor.Value.ToVector3();
			mainLightColor = mainLightTweakFactor * lightIntensity * vector;
			fillLightColor = fillLightTweakFactor * 0.6f * lightIntensity * vector;
			backLightColor = backLightTweakFactor * lightIntensity * vector;
		}
		else
		{
			mainLightColor = mainLightTweakFactor * lightIntensity * AnimatedModel.mainLightColorConstant;
			fillLightColor = fillLightTweakFactor * 0.6f * lightIntensity * AnimatedModel.fillLightColorConstant;
			backLightColor = backLightTweakFactor * lightIntensity * AnimatedModel.backLightColorConstant;
		}
		SetLightDirections();
	}
}
