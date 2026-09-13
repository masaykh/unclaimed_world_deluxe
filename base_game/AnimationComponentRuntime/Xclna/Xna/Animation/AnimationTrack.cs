using System;
using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

public class AnimationTrack
{
	public enum TrackType
	{
		Main,
		ExtraGait,
		Additional
	}

	public static float BlendPeriodInMilliseconds = 500f;

	public static float GaitBlendPeriodInMilliseconds = 1000f;

	private ModelAnimator modelAnimator;

	public AnimationController currentController;

	public AnimationController controllerBeingBlendedTo;

	public float blendingProgress;

	private bool blendToNull;

	private float? weightFactor;

	private float finalWeightFactor;

	public TrackType TrackTypeValue;

	public int Index;

	public float? WeightFactor
	{
		get
		{
			return weightFactor;
		}
		set
		{
			weightFactor = value;
		}
	}

	public float FinalWeightFactor => finalWeightFactor;

	public string CurrentAnimKey
	{
		get
		{
			if (currentController != null)
			{
				return currentController.AnimationInfo.Name;
			}
			return null;
		}
	}

	public long? CurrentAnimElapsed
	{
		get
		{
			if (currentController != null)
			{
				return currentController.ElapsedTime;
			}
			return null;
		}
	}

	public string AnimKeyBeingBlendedTo
	{
		get
		{
			if (controllerBeingBlendedTo != null)
			{
				return controllerBeingBlendedTo.AnimationInfo.Name;
			}
			return null;
		}
	}

	public AnimationTrack(ModelAnimator animatedModel, TrackType trackType)
	{
		TrackTypeValue = trackType;
		modelAnimator = animatedModel;
		foreach (BonePose bonePose in modelAnimator.BonePoses)
		{
			bonePose.SetAnimationTrack(this);
		}
	}

	public void FinishBlending()
	{
		if (currentController != null)
		{
			currentController.SpeedFactor = 0.0;
		}
		currentController = controllerBeingBlendedTo;
		controllerBeingBlendedTo = null;
		blendingProgress = 0f;
		blendToNull = false;
	}

	public void Update(GameTime gameTime)
	{
		if (currentController != null)
		{
			currentController.Update(gameTime);
		}
		if (controllerBeingBlendedTo != null)
		{
			controllerBeingBlendedTo.Update(gameTime);
		}
		AnimationController animationController = currentController;
		AnimationController animationController2 = controllerBeingBlendedTo;
		if (TrackTypeValue == TrackType.Main)
		{
			if (controllerBeingBlendedTo != null)
			{
				float blendPeriodToUse = GetBlendPeriodToUse();
				blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / blendPeriodToUse;
				if (blendingProgress >= 1f)
				{
					FinishBlending();
				}
			}
		}
		else
		{
			if ((blendToNull && currentController != null) || controllerBeingBlendedTo != null)
			{
				float blendPeriodToUse2 = GetBlendPeriodToUse();
				blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / blendPeriodToUse2;
				if (blendingProgress >= 1f)
				{
					FinishBlending();
				}
			}
			if (blendToNull || (currentController == null && controllerBeingBlendedTo != null))
			{
				finalWeightFactor = blendingProgress * (WeightFactor.HasValue ? WeightFactor.Value : 0f);
			}
			else
			{
				finalWeightFactor = (WeightFactor.HasValue ? WeightFactor.Value : 0f);
			}
		}
		if (currentController != animationController || controllerBeingBlendedTo != animationController2)
		{
			modelAnimator.UpdateModelBones(this);
		}
	}

	private float GetBlendPeriodToUse()
	{
		if (currentController.AnimationInfo.IsGaitAnim && controllerBeingBlendedTo.AnimationInfo.IsGaitAnim)
		{
			return GaitBlendPeriodInMilliseconds;
		}
		return BlendPeriodInMilliseconds;
	}

	public void RunWithoutBlending(AnimationController anim)
	{
		controllerBeingBlendedTo = null;
		currentController = anim;
		blendingProgress = 0f;
		modelAnimator.UpdateModelBones(this);
	}

	public void UpdateAnimationManuallyByTimeScalar(GameTime gameTime, double scalar)
	{
		if (currentController != null && controllerBeingBlendedTo == null)
		{
			if (!CanBeScaled(currentController))
			{
				return;
			}
			currentController.UpdateAnimationTimeScalar(scalar);
		}
		if (controllerBeingBlendedTo != null)
		{
			if (CanBeScaled(controllerBeingBlendedTo))
			{
				controllerBeingBlendedTo.UpdateAnimationTimeScalar(scalar);
			}
			blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / GetBlendPeriodToUse();
			if (blendingProgress >= 1f)
			{
				FinishBlending();
				modelAnimator.UpdateModelBones(this);
			}
		}
	}

	private bool CanBeScaled(AnimationController controller)
	{
		if (controller.AnimationInfo.IsGaitAnim || controller.IsLooping)
		{
			return false;
		}
		return true;
	}

	public void StartAnimation(AnimationController animController, Playback playback, StartingPoint startingPoint, BlendMode mode, out bool replacedAnim, float speedFactor = 1f, Looping looping = Looping.No, long? startOffsetToAdd = null, bool? setCallback = null, EventHandler pickNewRandomAnim = null, Random random = null)
	{
		replacedAnim = false;
		if (animController == null)
		{
			if (!blendToNull)
			{
				blendToNull = true;
				controllerBeingBlendedTo = null;
				blendingProgress = 0f;
				modelAnimator.UpdateModelBones(this);
			}
			return;
		}
		if (currentController != null && currentController.AnimationInfo.IsGaitAnim)
		{
			currentController.SpeedFactor = speedFactor;
		}
		long? num = null;
		switch (playback)
		{
		case Playback.Backwards:
			animController.SpeedFactor = -1f * speedFactor;
			if (startingPoint == StartingPoint.FromBeginning)
			{
				num = animController.AnimationInfo.Duration;
			}
			else if (animController.IsAnimationAtStart())
			{
				if (looping != Looping.Yes)
				{
					return;
				}
				num = animController.AnimationInfo.Duration;
			}
			break;
		case Playback.Forwards:
			animController.SpeedFactor = speedFactor;
			switch (startingPoint)
			{
			case StartingPoint.FromBeginning:
				num = animController.GetStartOfAnimation(startOffsetToAdd);
				break;
			case StartingPoint.Current:
				if (animController.ElapsedTime == animController.AnimationInfo.Duration && looping == Looping.Yes)
				{
					throw new Exception("you can't use manual playback on a looping anim like: " + animController.AnimationInfo.Name);
				}
				break;
			case StartingPoint.Specified:
				num = animController.GetStartOfAnimation(startOffsetToAdd);
				break;
			case StartingPoint.Random:
				num = (long)((double)animController.AnimationInfo.StartOffset + random.NextDouble() * (double)(animController.AnimationInfo.Duration - animController.AnimationInfo.StartOffset));
				break;
			}
			break;
		case Playback.Manual:
			animController.SpeedFactor = 0.0;
			num = animController.GetStartOfAnimation(startOffsetToAdd);
			animController.IsManual = true;
			break;
		default:
			throw new Exception("Animation playback type of undefined type: " + playback);
		}
		if (setCallback == true)
		{
			animController.AnimationEnded += pickNewRandomAnim;
		}
		else if (pickNewRandomAnim != null)
		{
			animController.AnimationEnded -= pickNewRandomAnim;
		}
		if (playback != Playback.Manual && looping == Looping.Yes)
		{
			animController.IsLooping = true;
		}
		else
		{
			animController.IsLooping = false;
		}
		switch (mode)
		{
		case BlendMode.Normal:
			if (currentController == null)
			{
				currentController = animController;
				currentController.ElapsedTime = num.Value;
				modelAnimator.UpdateModelBones(this);
			}
			else
			{
				if (controllerBeingBlendedTo == animController)
				{
					break;
				}
				if (currentController == animController)
				{
					if (controllerBeingBlendedTo != null)
					{
						blendingProgress = 0f;
						if (pickNewRandomAnim != null)
						{
							controllerBeingBlendedTo.AnimationEnded -= pickNewRandomAnim;
						}
						controllerBeingBlendedTo.SpeedFactor = 0.0;
						controllerBeingBlendedTo = null;
						modelAnimator.UpdateModelBones(this);
						replacedAnim = true;
						if (!currentController.IsLooping)
						{
							currentController.ElapsedTime = num.Value;
						}
					}
					else if (blendToNull)
					{
						blendingProgress = 0f;
						blendToNull = false;
						modelAnimator.UpdateModelBones(this);
					}
					else
					{
						currentController.ElapsedTime = num.Value;
					}
				}
				else if (controllerBeingBlendedTo != null)
				{
					if (pickNewRandomAnim != null)
					{
						controllerBeingBlendedTo.AnimationEnded -= pickNewRandomAnim;
					}
					if (blendingProgress > 0.25f)
					{
						FinishBlending();
					}
					else
					{
						blendingProgress = 0f;
						controllerBeingBlendedTo.SpeedFactor = 0.0;
					}
					controllerBeingBlendedTo = animController;
					controllerBeingBlendedTo.ElapsedTime = num.Value;
					modelAnimator.UpdateModelBones(this);
				}
				else if (blendToNull)
				{
					blendingProgress = 0f;
					blendToNull = false;
					controllerBeingBlendedTo = animController;
					controllerBeingBlendedTo.ElapsedTime = num.Value;
					modelAnimator.UpdateModelBones(this);
				}
				else
				{
					blendingProgress = 0f;
					controllerBeingBlendedTo = animController;
					controllerBeingBlendedTo.ElapsedTime = num.Value;
					modelAnimator.UpdateModelBones(this);
				}
			}
			break;
		case BlendMode.NoBlending:
			RunWithoutBlending(animController);
			break;
		}
		if (controllerBeingBlendedTo != null && controllerBeingBlendedTo.AnimationInfo.IsGaitAnim)
		{
			controllerBeingBlendedTo.SpeedFactor = speedFactor;
		}
	}

	public void StopAnimation()
	{
		if (currentController != null)
		{
			currentController.SpeedFactor = 0.0;
			currentController = null;
		}
		if (controllerBeingBlendedTo != null)
		{
			controllerBeingBlendedTo.SpeedFactor = 0.0;
			controllerBeingBlendedTo = null;
		}
		blendToNull = false;
		modelAnimator.UpdateModelBones(this);
	}

	public bool RunsAnimation()
	{
		if (currentController == null)
		{
			return controllerBeingBlendedTo != null;
		}
		return true;
	}

	public Matrix GetCurrentTransform(BonePose bonePose, bool doesAnimContainChannel, bool doesBlendContainChannel)
	{
		Matrix result;
		if (currentController == null || !doesAnimContainChannel)
		{
			if (!(controllerBeingBlendedTo != null && doesBlendContainChannel))
			{
				return bonePose.DefaultTransform;
			}
			Matrix end = controllerBeingBlendedTo.GetCurrentBoneTransform(bonePose);
			Util.SlerpMatrix(ref bonePose.DefaultTransform, ref end, blendingProgress, out result);
		}
		else
		{
			Matrix start = currentController.GetCurrentBoneTransform(bonePose);
			if (!(controllerBeingBlendedTo != null && doesBlendContainChannel))
			{
				return start;
			}
			Matrix end = controllerBeingBlendedTo.GetCurrentBoneTransform(bonePose);
			Util.SlerpMatrix(ref start, ref end, blendingProgress, out result);
		}
		return result;
	}
}
