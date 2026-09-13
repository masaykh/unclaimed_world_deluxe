using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide;

public class BloomComponent
{
	public enum IntermediateBuffer
	{
		PreBloom,
		BlurredHorizontally,
		BlurredBothWays,
		FinalResult
	}

	private SpriteBatch spriteBatch;

	private Effect bloomExtractEffect;

	private Effect bloomCombineEffect;

	private Effect gaussianBlurEffect;

	private RenderTarget2D renderTarget1;

	private RenderTarget2D renderTarget2;

	private BloomSettings settings = BloomSettings.PresetSettings[0];

	public BloomSettings BaseSettings;

	private IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

	private Game game;

	public BloomSettings Settings
	{
		get
		{
			return settings;
		}
		set
		{
			settings = value;
		}
	}

	public IntermediateBuffer ShowBuffer
	{
		get
		{
			return showBuffer;
		}
		set
		{
			showBuffer = value;
		}
	}

	public BloomComponent(Game game)
	{
		this.game = game;
		if (game == null)
		{
			throw new ArgumentNullException("game");
		}
		LoadContent();
	}

	protected void LoadContent()
	{
		spriteBatch = new SpriteBatch(game.GraphicsDevice);
		bloomExtractEffect = game.Content.Load<Effect>("BloomExtract");
		bloomCombineEffect = game.Content.Load<Effect>("BloomCombine");
		gaussianBlurEffect = game.Content.Load<Effect>("GaussianBlur");
		PresentationParameters presentationParameters = game.GraphicsDevice.PresentationParameters;
		int width = The.Client.Controller.DrawArea.Width;
		int height = The.Client.Controller.DrawArea.Height;
		SurfaceFormat backBufferFormat = presentationParameters.BackBufferFormat;
		width /= 2;
		height /= 2;
		renderTarget1 = new RenderTarget2D(game.GraphicsDevice, width, height, mipMap: false, backBufferFormat, DepthFormat.None);
		renderTarget2 = new RenderTarget2D(game.GraphicsDevice, width, height, mipMap: false, backBufferFormat, DepthFormat.None);
	}

	public void UnloadContent()
	{
	}

	public void Destroy()
	{
		renderTarget1.Dispose();
		renderTarget2.Dispose();
	}

	public void Draw(Texture2D sceneTexture)
	{
		bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Settings.BloomThreshold);
		game.GraphicsDevice.SetRenderTarget(renderTarget1);
		DrawFullscreenQuad(sceneTexture, renderTarget1.Width, renderTarget1.Height, bloomExtractEffect, IntermediateBuffer.PreBloom);
		SetBlurEffectParameters(1f / (float)renderTarget1.Width, 0f);
		game.GraphicsDevice.SetRenderTarget(renderTarget2);
		DrawFullscreenQuad(renderTarget1, renderTarget2.Width, renderTarget2.Height, gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);
		SetBlurEffectParameters(0f, 1f / (float)renderTarget1.Height);
		game.GraphicsDevice.SetRenderTarget(renderTarget1);
		DrawFullscreenQuad(renderTarget2, renderTarget1.Width, renderTarget1.Height, gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);
		The.Client.Controller.SetZoomRenderTaget();
		EffectParameterCollection parameters = bloomCombineEffect.Parameters;
		parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
		parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
		parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
		parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);
		parameters["BaseTexture"].SetValue(sceneTexture);
		Dimension drawArea = The.Client.Controller.DrawArea;
		DrawFullscreenQuad(renderTarget1, drawArea.Width, drawArea.Height, bloomCombineEffect, IntermediateBuffer.FinalResult);
	}

	private void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect, IntermediateBuffer currentBuffer)
	{
		if (showBuffer >= currentBuffer)
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
		}
		else
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
		}
		spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
		spriteBatch.End();
	}

	private void SetBlurEffectParameters(float dx, float dy)
	{
		EffectParameter effectParameter = gaussianBlurEffect.Parameters["SampleWeights"];
		EffectParameter effectParameter2 = gaussianBlurEffect.Parameters["SampleOffsets"];
		int count = effectParameter.Elements.Count;
		float[] array = new float[count];
		Vector2[] array2 = new Vector2[count];
		array[0] = ComputeGaussian(0f);
		array2[0] = new Vector2(0f);
		float num = array[0];
		for (int i = 0; i < count / 2; i++)
		{
			num += (array[i * 2 + 2] = (array[i * 2 + 1] = ComputeGaussian(i + 1))) * 2f;
			float num2 = (float)(i * 2) + 1.5f;
			array2[i * 2 + 2] = -(array2[i * 2 + 1] = new Vector2(dx, dy) * num2);
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] /= num;
		}
		effectParameter.SetValue(array);
		effectParameter2.SetValue(array2);
	}

	private float ComputeGaussian(float n)
	{
		float blurAmount = Settings.BlurAmount;
		return (float)(1.0 / Math.Sqrt(Math.PI * 2.0 * (double)blurAmount) * Math.Exp((0f - n * n) / (2f * blurAmount * blurAmount)));
	}
}
