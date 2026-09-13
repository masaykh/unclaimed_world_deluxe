using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

public abstract class AnimatedModel
{
	public enum Transformations
	{
		All,
		OnlyRotation,
		OnlyYCorrection
	}

	protected enum EmitterRenderMode
	{
		NoEmitters,
		EmittersOnly,
		All
	}

	protected static BlendState overlayBlendState;

	public ModelAnimator ModelAnimator;

	protected Matrix translation;

	protected Matrix shadowTransformation;

	protected Matrix world;

	protected Matrix scaling;

	public Matrix StandardDrawingWorldTransformation;

	protected Matrix uncorrectedStandardDrawingWorldTransformation;

	protected float worldPositionY;

	private Matrix shadowMatrix;

	public Matrix[] transforms;

	protected static Vector3 mainLightColorConstant;

	protected static Vector3 fillLightColorConstant;

	protected static Vector3 backLightColorConstant;

	protected Vector3 mainLightColor;

	protected Vector3 fillLightColor;

	protected Vector3 backLightColor;

	protected Vector3 mainLightDirection;

	protected Vector3 fillLightDirection;

	protected Vector3 backLightDirection;

	static AnimatedModel()
	{
		mainLightColorConstant = new Vector3(1f, 1f, 0.85f);
		fillLightColorConstant = new Vector3(0.8f, 0.8f, 1f);
		backLightColorConstant = new Vector3(0.8f, 0.8f, 1f);
		overlayBlendState = new BlendState();
		overlayBlendState.ColorSourceBlend = Blend.SourceAlpha;
		overlayBlendState.ColorDestinationBlend = Blend.One;
	}

	public AnimatedModel()
	{
	}

	public AnimatedModel(AnimatedModel original, bool canAnimate)
	{
		translation = original.translation;
		shadowTransformation = original.shadowTransformation;
		world = original.world;
		scaling = original.scaling;
		StandardDrawingWorldTransformation = original.StandardDrawingWorldTransformation;
		uncorrectedStandardDrawingWorldTransformation = original.uncorrectedStandardDrawingWorldTransformation;
		worldPositionY = original.worldPositionY;
		shadowMatrix = original.shadowMatrix;
		transforms = original.transforms;
		ModelAnimator = new ModelAnimator(original.ModelAnimator, canAnimate);
	}

	public void Initialize(Model model)
	{
		ModelAnimator = new ModelAnimator(model);
	}

	public void Update(GameTime gameTime)
	{
		ModelAnimator.Update(gameTime);
	}

	protected Vector2 GetScanLinesDimensions()
	{
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 vector = new Vector2(drawArea.Width, drawArea.Height);
		return new Vector2(vector.X / (float)The.Client.Renderer.Scanlines.Width, vector.Y / (float)The.Client.Renderer.Scanlines.Height);
	}

	public void ComputeMatricesForDrawing(Vector3 forward, Vector3 up, Vector3 right, float scale, Vector3 location, Vector3 offset, Transformations transformations)
	{
		if (The.Client != null)
		{
			CreateBoneTransformMatrixArray();
			world = Matrix.Identity;
			world.Forward = forward;
			world.Up = up;
			world.Right = right;
			scaling = Matrix.CreateScale(scale);
			if (transformations == Transformations.All)
			{
				float y = The.Client.Renderer.CorrectModelYPositionForDrawing(location.Y);
				translation = Matrix.CreateTranslation(new Vector3(location.X, y, 0f - location.Z) + offset);
				shadowMatrix = Matrix.CreateShadow(plane: new Plane(-Vector3.UnitZ, location.Z), lightDirection: The.Sim.DateAndTime.SunPosition);
				StandardDrawingWorldTransformation = scaling * world * translation;
				worldPositionY = location.Y;
				Matrix matrix = Matrix.CreateTranslation(new Vector3(location.X, location.Y, 0f - location.Z) + offset);
				uncorrectedStandardDrawingWorldTransformation = scaling * world * matrix;
			}
			else
			{
				StandardDrawingWorldTransformation = scaling * world;
				uncorrectedStandardDrawingWorldTransformation = scaling * world;
			}
			ModelAnimator.World = StandardDrawingWorldTransformation;
			ModelAnimator.UncorrectedWorld = uncorrectedStandardDrawingWorldTransformation;
		}
	}

	public void CreateBoneTransformMatrixArray()
	{
		transforms = new Matrix[ModelAnimator.Model.Bones.Count];
		ModelAnimator.Model.CopyAbsoluteBoneTransformsTo(transforms);
	}

	public void DrawStandard(GameWorldRenderer.RenderTechnique technique, Matrix view, Matrix projection, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, float drawWithAlpha, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null)
	{
		DrawModel(ref StandardDrawingWorldTransformation, ref view, ref projection, technique, replaceColor0, replaceColor1, replaceColor2, replaceColor3, drawWithAlpha, lightIntensity, dirtLevel, basicTexture, tintColor);
	}

	public void DrawShadow(Vector3 displacement)
	{
		world.Translation = displacement;
		shadowTransformation = scaling * world * shadowMatrix * translation;
		DrawModel(ref shadowTransformation, ref The.Client.Renderer.View, ref The.Client.Projection, GameWorldRenderer.RenderTechnique.NoLighting, null, null, null, null, 1f, 1f, 0f);
	}

	public virtual void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection, GameWorldRenderer.RenderTechnique technique, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, float alphaFactor, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null)
	{
	}

	public static bool HasAttachorModel(Entity entity, string attachorBoneName)
	{
		if (entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out var value))
		{
			foreach (RenderAsModel item in value)
			{
				if (item.Parent.EntityType.Name == "Box")
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasAttachedModel(Entity entity, string attachedEntityKey, string attachorBoneName, string attacheeBoneName)
	{
		if (entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out var value))
		{
			foreach (IAttachable item in value)
			{
				if (((RenderAsModel)item).Renderable.RenderableType.KeyName == attachedEntityKey && item.AttacheeBone != null && item.AttacheeBone.Name == attacheeBoneName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OrientAttachedModel(Entity entity, string attachorBoneName, Vector3 euler, Vector3 trans)
	{
		if (!entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i] is RenderAsModel renderAsModel)
			{
				Matrix matrix = Matrix.CreateTranslation(trans);
				Matrix identity = Matrix.Identity;
				identity *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(euler.X));
				identity *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(euler.Y));
				identity *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(euler.Z));
				renderAsModel.LocalTransform = identity * matrix;
				RenderAsModel renderAsModel2 = null;
			}
		}
	}

	protected void CreateLights(float lightIntensity, Color? tintColor = null)
	{
		if (tintColor.HasValue)
		{
			Vector3 vector = tintColor.Value.ToVector3();
			mainLightColor = lightIntensity * vector;
			fillLightColor = 0.6f * lightIntensity * vector;
			backLightColor = lightIntensity * vector;
		}
		else
		{
			mainLightColor = lightIntensity * mainLightColorConstant;
			fillLightColor = 0.6f * lightIntensity * fillLightColorConstant;
			backLightColor = lightIntensity * backLightColorConstant;
		}
		SetLightDirections();
	}

	protected void SetLightDirections()
	{
		mainLightDirection = -The.Sim.DateAndTime.SunPosition;
		fillLightDirection = Vector3.Cross(mainLightDirection, Vector3.UnitY);
		if (mainLightDirection.X != 0f)
		{
			fillLightDirection.X *= Math.Sign(mainLightDirection.X);
		}
		fillLightDirection.Z = Math.Abs(fillLightDirection.Z);
		backLightDirection = -The.Client.Renderer.CameraDirection;
		backLightDirection.Normalize();
	}

	public void Destroy()
	{
		if (The.Sim != null)
		{
			ModelAnimator = null;
		}
	}

	protected void HandleEmitterModelParts(GameWorldRenderer.RenderTechnique technique)
	{
		switch (technique)
		{
		case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
			TurnonEmitterModelParts(EmitterRenderMode.All);
			break;
		case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
			TurnonEmitterModelParts(EmitterRenderMode.EmittersOnly);
			break;
		case GameWorldRenderer.RenderTechnique.Standard:
			TurnonEmitterModelParts(EmitterRenderMode.All);
			break;
		default:
			TurnonEmitterModelParts(EmitterRenderMode.All);
			break;
		}
	}

	private void TurnonEmitterModelParts(EmitterRenderMode renderMode)
	{
		foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				if (renderMode == EmitterRenderMode.All)
				{
					meshPart.Tag = null;
					continue;
				}
				Vector3 valueVector = meshPart.Effect.Parameters["EmissiveColor"].GetValueVector3();
				switch (renderMode)
				{
				case EmitterRenderMode.NoEmitters:
					if (valueVector != Vector3.Zero)
					{
						meshPart.Tag = "skip";
					}
					else
					{
						meshPart.Tag = null;
					}
					break;
				case EmitterRenderMode.EmittersOnly:
					if (valueVector != Vector3.Zero)
					{
						meshPart.Tag = null;
					}
					else
					{
						meshPart.Tag = "skip";
					}
					break;
				}
			}
		}
	}
}
