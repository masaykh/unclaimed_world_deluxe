using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

public class AnimatedHead
{
	private Renderable parent;

	private bool isLerpingBackSpine;

	private float lerpBackProgress;

	public AnimatedHead(Renderable parent)
	{
		this.parent = parent;
	}

	public void TurnToLook(Vector2 lookTarget, float lookAngle, float lookTargetAngle)
	{
		float num = parent.RenderableType.AnimatedHeadType.SpineBones.Length;
		float num2 = lookAngle / num;
		new Vector3(0f, 1f, 0f);
		Vector3 axis = new Vector3(1f, 0f, 0f);
		Vector3 axis2 = new Vector3(0f, 0f, 1f);
		Matrix identity = Matrix.Identity;
		identity *= Matrix.CreateFromAxisAngle(axis, 0f - num2);
		identity *= Matrix.CreateFromAxisAngle(axis2, parent.RenderableType.AnimatedHeadType.PitchForward);
		for (int i = 0; (float)i < num; i++)
		{
			string boneName = parent.RenderableType.AnimatedHeadType.SpineBones[i];
			BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];
			Matrix twist = identity;
			if (num > 2f)
			{
				if (i == 0)
				{
					twist = Matrix.Identity;
					twist *= Matrix.CreateFromAxisAngle(axis, -0.5f * num2);
					twist *= Matrix.CreateFromAxisAngle(axis2, 1f * parent.RenderableType.AnimatedHeadType.PitchForward);
				}
				else if ((float)i == num - 1f)
				{
					twist = Matrix.Identity;
					twist *= Matrix.CreateFromAxisAngle(axis, -1.5f * num2);
					twist *= Matrix.CreateFromAxisAngle(axis2, 1f * parent.RenderableType.AnimatedHeadType.PitchForward);
				}
			}
			TwistBone(bonePose, ref twist);
		}
	}

	public void Update()
	{
	}

	public void StartLerpingBackSpine()
	{
		isLerpingBackSpine = true;
		lerpBackProgress = 0f;
	}

	public void UpdateLerpingBackSpine(GameTime gameTime)
	{
		if (isLerpingBackSpine)
		{
			lerpBackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds * 1f;
			if (lerpBackProgress >= 6f)
			{
				isLerpingBackSpine = false;
			}
			string[] spineBones = parent.RenderableType.AnimatedHeadType.SpineBones;
			foreach (string boneName in spineBones)
			{
				LerpSpineBoneToNormal(boneName, isLerpingBackSpine);
			}
		}
	}

	private void TwistBone(BonePose bonePose, ref Matrix twist, bool resetAnimator = false)
	{
		Matrix defaultTransform = twist * Matrix.CreateTranslation(bonePose.DefaultTransform.Translation);
		_ = parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor;
		bonePose.DefaultTransform = defaultTransform;
		bonePose.UseSpecialTransform = true;
	}

	private void SwitchSpineBoneToNormal(string boneName)
	{
		BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];
		Matrix transform = parent.RenderAsModel.AnimatedModel.ModelAnimator.Model.Bones[bonePose.Index].Transform;
		bonePose.DefaultTransform = transform;
		bonePose.UseSpecialTransform = false;
	}

	private void LerpSpineBoneToNormal(string boneName, bool isStillLerping)
	{
		BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];
		Matrix transform = parent.RenderAsModel.AnimatedModel.ModelAnimator.Model.Bones[bonePose.Index].Transform;
		bonePose.UseSpecialTransform = isStillLerping;
		if (isStillLerping)
		{
			float turnToLookLerpFactor = parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor;
			Matrix defaultTransform = Common.EaseInValueTowardsTarget(bonePose.DefaultTransform, transform, turnToLookLerpFactor);
			bonePose.DefaultTransform = defaultTransform;
		}
		else
		{
			bonePose.DefaultTransform = transform;
		}
	}
}
