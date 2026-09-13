using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors.Stances;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

public class RenderAsModel : RenderAsBase, IAttachable, IUpdatable
{
	public enum AppliedAttachableTransforms
	{
		Attachor,
		Attachee,
		Animation
	}

	private Matrix localTransform = Matrix.Identity;

	public ModelData ModelData;

	public float FinalModelScale = 1f;

	public string FinalModelName;

	public string FinalModelBasicTextureName;

	public Texture FinalModelBasicTexture;

	public AnimatedModel AnimatedModel;

	private bool canAnimate = true;

	private AnimationTrack mainAnimation;

	private AnimationTrack secondGaitAnimation;

	private List<AnimationTrack> additionalAnimationTracks;

	public AnimConditionInfo SelectedAnimInfo;

	private bool animationFlagsAreDirty = true;

	public Vector3 CustomColor0;

	public Vector3 CustomColor1;

	public Vector3 CustomColor2;

	public Vector3 CustomColor3;

	public static float LocationLerpLimit = 0.5f;

	public static float RotationLerpLimit = 0.005f;

	private float previousGaitSpeed;

	private string previousMainGait;

	private string previousSecondGait;

	private float previousAnim1Weight;

	private Vector3 location = Vector3.Zero;

	private Vector3? previousLocation;

	private Vector3 facingNormal = Vector3.UnitX;

	private Vector3? previousFacingNormal;

	public static float LerpFactor = 0.87f;

	public static float LerpRotationFactor = 0.85f;

	public static float DistanceToStopLerping = 24f;

	private const float distanceFactorToReduceLerping = 8f;

	private List<Pair<Entity, Vector2>> nearByEntitiesOfSameType = new List<Pair<Entity, Vector2>>();

	private const long elapsedDistanceToConsiderInSync = 6000000L;

	private int noOfFramesWeHaveBeenDirty;

	public Matrix LocalTransform
	{
		get
		{
			return localTransform;
		}
		set
		{
			localTransform = value;
		}
	}

	public Matrix CombinedTransform { get; set; }

	public ModelAnimator ModelAnimator { get; set; }

	public AttacheePoint? AttacheePointValue { get; set; }

	public BonePose AttacheeBone { get; set; }

	public BonePose AttachorBone { get; set; }

	public AttachPoint AttachorPoint { get; set; }

	public object AttachedTo { get; set; }

	public float Scale => FinalModelScale;

	public string CurrentBaseAnimKey => mainAnimation.CurrentAnimKey;

	public string CurrentAdditionalAnim1Key => additionalAnimationTracks[0].CurrentAnimKey;

	public string CurrentAdditionalAnim2Key => additionalAnimationTracks[1].CurrentAnimKey;

	private RenderAsModelType RenderAsModelType => base.Parent.EntityType.RenderableTypeMode.RenderAsModelType;

	private bool IsAnimated
	{
		get
		{
			if (AnimatedModel != null)
			{
				return canAnimate;
			}
			return false;
		}
	}

	public Vector3 Location
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	public Vector3 FacingNormal
	{
		get
		{
			return facingNormal;
		}
		set
		{
			facingNormal = value;
		}
	}

	private void SetAnimationActionStateFlag(AnimAction state)
	{
		Renderable.SetAnimationActionStateFlag(state);
	}

	private void SetAnimationStateFlag(AnimModifier state)
	{
		Renderable.SetAnimationStateFlag(state);
	}

	private void ClearAnimationActionStateFlag(AnimAction state)
	{
		Renderable.ClearAnimationActionStateFlag(state);
	}

	private void ClearAnimationStateFlag(AnimModifier state)
	{
		Renderable.ClearAnimationStateFlag(state);
	}

	public RenderAsModel(Entity parent, Renderable renderable)
		: base(parent, renderable)
	{
	}

	public RenderAsModel(Vector3 customColor0, Vector3 customColor1, Vector3 customColor2, Vector3 customColor3, Matrix localTransform, Vector3 location, Vector3 facingNormal, string finalModelName, string finalModelTextureName, float finalModelScale, AnimatedModel originalAnimatedModel, Renderable newRenderable, bool setAnimationFlagsDirty, IKnownEntityData parent)
		: base(null, newRenderable)
	{
		FinalModelScale = finalModelScale;
		FinalModelBasicTextureName = finalModelTextureName;
		if (!string.IsNullOrEmpty(FinalModelBasicTextureName))
		{
			FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[FinalModelBasicTextureName];
		}
		FinalModelName = finalModelName;
		ModelData = GameData.Instance.AllModels[FinalModelName];
		this.facingNormal = facingNormal;
		this.location = location;
		this.localTransform = localTransform;
		CustomColor0 = customColor0;
		CustomColor1 = customColor1;
		CustomColor2 = customColor2;
		CustomColor3 = customColor3;
		base.Parent = parent;
		canAnimate = setAnimationFlagsDirty;
		if (originalAnimatedModel != null)
		{
			switch (ModelData.ModelType)
			{
			case ModelType.Skinned:
				AnimatedModel = new SkinnedAnimatedModel((SkinnedAnimatedModel)originalAnimatedModel, canAnimate);
				break;
			case ModelType.Stiff:
				AnimatedModel = new StiffAnimatedModel((StiffAnimatedModel)originalAnimatedModel, canAnimate);
				break;
			}
			ModelAnimator = AnimatedModel.ModelAnimator;
			animationFlagsAreDirty = setAnimationFlagsDirty;
			if (animationFlagsAreDirty)
			{
				CreateAnimationTracks();
			}
		}
		else
		{
			InitializeAnimatedModel(createAnimationTracks: true);
		}
	}

	public void SetAnimFlagsDirty()
	{
		animationFlagsAreDirty = true;
	}

	public void SetToParentLocation()
	{
		location = base.Parent.PlaySiteLocation;
		facingNormal = base.Parent.FacingNormal;
	}

	public void Initialize()
	{
		ModelData = GameData.Instance.AllModels[FinalModelName];
		if (base.Parent is Entity)
		{
			((Entity)base.Parent).BoundingRadius3D = ModelData.BoundingSphereRadius * FinalModelScale;
		}
		if (!string.IsNullOrEmpty(FinalModelBasicTextureName))
		{
			FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[FinalModelBasicTextureName];
		}
		InitializeAnimatedModel(createAnimationTracks: true);
		_ = Matrix.Identity;
		Matrix identity = Matrix.Identity;
		Matrix identity2 = Matrix.Identity;
		Matrix matrix = Matrix.CreateScale(FinalModelScale);
		localTransform = matrix * identity * identity2;
	}

	private void InitializeAnimatedModel(bool createAnimationTracks)
	{
		switch (ModelData.ModelType)
		{
		case ModelType.Skinned:
			AnimatedModel = new SkinnedAnimatedModel();
			break;
		case ModelType.Stiff:
			AnimatedModel = new StiffAnimatedModel();
			break;
		}
		if (AnimatedModel != null)
		{
			AnimatedModel.Initialize(ModelData.Model);
			ModelAnimator = AnimatedModel.ModelAnimator;
			if (createAnimationTracks)
			{
				CreateAnimationTracks();
			}
		}
	}

	private void CreateAnimationTracks()
	{
		mainAnimation = new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.Main);
		secondGaitAnimation = new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.ExtraGait);
		additionalAnimationTracks = new List<AnimationTrack>();
		for (int i = 0; i < 2; i++)
		{
			additionalAnimationTracks.Add(new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.Additional)
			{
				Index = i
			});
		}
	}

	public void DrawShadow()
	{
		AnimatedModel.DrawShadow(new Vector3(0f, 0f, 0f));
	}

	public double? GetUpdateInterval()
	{
		bool flag = false;
		if (AnimatedModel != null && canAnimate)
		{
			flag = true;
		}
		if (flag)
		{
			return 0.0;
		}
		return null;
	}

	public void Update(GameTime gameTime)
	{
		if (mainAnimation != null)
		{
			mainAnimation.Update(gameTime);
		}
		if (secondGaitAnimation != null)
		{
			secondGaitAnimation.Update(gameTime);
		}
		if (additionalAnimationTracks != null)
		{
			foreach (AnimationTrack additionalAnimationTrack in additionalAnimationTracks)
			{
				additionalAnimationTrack.Update(gameTime);
			}
		}
		// PORT DIAGNOSTIC (port.renderTrace). THIS is the per-frame route to the bone palette:
		// AnimatedModel.Update -> ModelAnimator.Update -> CopyAbsoluteTransforms, which is the
		// only thing that ever writes to it. (The public CopyAbsoluteTransforms below is a
		// LOAD-TIME entry point, reached from Entity.LoadPostProcess; counting that one reported
		// zero and said nothing about whether the palette is maintained.)
		//
		// The palette is allocated as `new Matrix[skinInfo[i].Count]` - all zeros - so a model
		// drawn with a skinned technique whose update is skipped is posed by zero matrices:
		// every vertex lands at w=0 and the mesh explodes through the perspective divide.
		if (IsAnimated)
		{
			UWGame.Port.RenderTrace.Submit("model animated (palette maintained)");
			AnimatedModel.Update(gameTime);
		}
		else
		{
			UWGame.Port.RenderTrace.Submit("model NOT animated (palette left zero)");
		}
	}

	public void CopyAbsoluteTransforms()
	{
		// PORT DIAGNOSTIC (port.renderTrace). This is the ONLY thing that fills the bone palette:
		// ModelAnimator allocates it as `new Matrix[skinInfo[i].Count]`, which is all-zero, and
		// nothing else writes to it. A model drawn with a skinned technique that never reaches
		// here is posed by zero matrices - every vertex lands at w=0 and the mesh explodes
		// through the perspective divide. By eye that is indistinguishable from a dozen other
		// faults, so the two branches are counted separately.
		if (IsAnimated)
		{
			UWGame.Port.RenderTrace.Submit("bone palette updated");
			AnimatedModel.ModelAnimator.CopyAbsoluteTransforms();
		}
		else
		{
			UWGame.Port.RenderTrace.Submit("bone palette SKIPPED (not animated)");
		}
	}

	public void ComputeMatricesForDrawing(AnimatedModel.Transformations transformations, float scale)
	{
		bool flag = false;
		if (base.ParentEntity != null && base.ParentEntity.IsStarted() == false)
		{
			flag = true;
		}
		else if (!The.Sim.IsPaused)
		{
			flag = true;
		}
		if (flag)
		{
			if (true)
			{
				if (Renderable.CanLerpLocation())
				{
					if (Renderable.LerpableWasRenderedLastFrame)
					{
						LerpLocationTowardsEntity();
						SetMoveAnimationStates();
					}
					else
					{
						location = base.Parent.PlaySiteLocation;
						facingNormal = base.Parent.FacingNormal;
					}
					Renderable.LerpableWasRenderedLastFrame = true;
				}
				else
				{
					location = base.Parent.PlaySiteLocation;
					facingNormal = base.Parent.FacingNormal;
				}
			}
			previousLocation = location;
			previousFacingNormal = facingNormal;
		}
		ComputeMatricesForDrawing(transformations, scale, location, facingNormal, base.Parent.Rotation, null, null);
	}

	private void LerpLocationTowardsEntity()
	{
		bool flag = true;
		float num = 1f;
		if ((0u | (flag ? 1u : 0u)) != 0)
		{
			float num2 = ((base.ParentEntity.Locomotor == null) ? 0f : Common.DistanceOctile(Location, base.ParentEntity.Locomotor.CurrentMoveTarget));
			num = num2 - DistanceToStopLerping;
			num /= 8f * DistanceToStopLerping;
			num = Common.Clamp(num, 0f, 1f);
			num = (float)Math.Pow((double)num - 1.0, 5.0) + 1f;
		}
		if (false && !Common.IsZero(num) && !Common.IsLocationEqual(location, base.Parent.Location.Value))
		{
			float num3 = MathHelper.Lerp(0f, LerpFactor, num);
			location = location * num3 + base.Parent.Location.Value * (1f - num3);
		}
		else
		{
			location = base.Parent.Location.Value;
		}
		if (flag && !Common.IsZero(num))
		{
			float num4 = MathHelper.Lerp(0f, LerpRotationFactor, num);
			facingNormal = facingNormal * num4 + base.Parent.FacingNormal * (1f - num4);
			facingNormal.Normalize();
		}
		else
		{
			facingNormal = base.Parent.FacingNormal;
		}
	}

	private void SetMoveAnimationStates()
	{
		if (!previousLocation.HasValue || !Common.IsLocationEqual(previousLocation.Value, location) || !previousFacingNormal.HasValue || !Common.IsDirectionEqual(previousFacingNormal.Value, facingNormal) || base.ParentEntity.Locomotor.IsMoving())
		{
			if (base.ParentEntity.IsHaulingNothingMounted())
			{
				SetHaulingAnimStates();
			}
			else
			{
				ClearHaulingAnimStates();
				Renderable.SetAnimationActionStateFlag(AnimAction.Moving);
			}
			UpdateGaitAnimation();
		}
		else
		{
			ClearHaulingAnimStates();
			ClearAnimationActionStateFlag(AnimAction.Moving);
		}
	}

	private void UpdateGaitAnimation()
	{
		if (SelectedAnimInfo == null || SelectedAnimInfo.GaitSetKey == null)
		{
			return;
		}
		float num = 0f;
		if (previousLocation.HasValue)
		{
			double totalSeconds = The.Client.ElapsedTimeBetweenDraws.TotalSeconds;
			if (totalSeconds == 0.0)
			{
				return;
			}
			num = (float)((double)Vector3.Distance(Location, previousLocation.Value) / totalSeconds);
			if (num < 3f)
			{
				if (previousFacingNormal.HasValue)
				{
					double angleBetweenVectors = Common.GetAngleBetweenVectors(previousFacingNormal.Value.ToVector2(), facingNormal.ToVector2());
					float num2 = (float)(6.0 * angleBetweenVectors / totalSeconds);
					num += num2;
					num = Common.ClampTop(num, 32f);
				}
			}
			else if (base.ParentEntity.Locomotor.MoveSpeed > 0f)
			{
				num = Common.Clamp(num, 0.001f, base.ParentEntity.Locomotor.MoveSpeed * 1.4f);
			}
		}
		GetMoveAnimationsToUse(num, SelectedAnimInfo.GaitSetKey, out var anim, out var anim2, out var anim1Weight);
		float num3;
		float num4;
		if (anim2 != null)
		{
			num3 = MathHelper.Lerp(anim2.StrideLength, anim.StrideLength, anim1Weight);
			num4 = MathHelper.Lerp(anim2.StrideDuration, anim.StrideDuration, anim1Weight);
		}
		else
		{
			num3 = anim.StrideLength;
			num4 = anim.StrideDuration;
		}
		float speedFactor = num * num4 / num3;
		StartGaitAnimations(anim, anim2, speedFactor, anim1Weight);
	}

	private void StartGaitAnimations(GaitAnimationBracket animBracket1, GaitAnimationBracket animBracket2, float speedFactor, float anim1Weight)
	{
		if (!(animBracket1?.AnimationKey != previousMainGait) && !(animBracket2?.AnimationKey != previousSecondGait) && !((double)Math.Abs(speedFactor - previousGaitSpeed) > 0.001) && !((double)Math.Abs(anim1Weight - previousAnim1Weight) > 0.001))
		{
			return;
		}
		previousMainGait = animBracket1?.AnimationKey;
		previousSecondGait = animBracket2?.AnimationKey;
		previousAnim1Weight = anim1Weight;
		previousGaitSpeed = speedFactor;
		AnimationController animationController = null;
		AnimationController animationController2 = null;
		animationController = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animBracket1.AnimationKey);
		if (animBracket2 != null)
		{
			animationController2 = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animBracket2.AnimationKey);
		}
		long? startOffsetToAdd = null;
		StartingPoint startingPoint = StartingPoint.Current;
		if (mainAnimation.currentController == null)
		{
			if (animationController2 != null)
			{
				startingPoint = StartingPoint.FromBeginning;
			}
			startingPoint = StartingPoint.FromBeginning;
		}
		else if (mainAnimation.currentController == animationController)
		{
			startOffsetToAdd = animationController.ElapsedTimeMinusStartOffset;
			startingPoint = StartingPoint.Specified;
		}
		else if (mainAnimation.currentController.AnimationInfo.IsGaitAnim)
		{
			startOffsetToAdd = mainAnimation.currentController.ElapsedTimeMinusStartOffset;
			startingPoint = StartingPoint.Specified;
		}
		else
		{
			startingPoint = StartingPoint.FromBeginning;
		}
		bool replacedAnim = false;
		mainAnimation.StartAnimation(animationController, Playback.Forwards, startingPoint, BlendMode.Normal, out replacedAnim, speedFactor, Looping.Yes, startOffsetToAdd, null, null, The.Client.ClientRandomGenerator.Random);
		startOffsetToAdd = animationController.ElapsedTimeMinusStartOffset;
		secondGaitAnimation.WeightFactor = 1f - anim1Weight;
	}

	private void GetMoveAnimationsToUse(float moveSpeed, string gaitAnimsKey, out GaitAnimationBracket anim1, out GaitAnimationBracket anim2, out float anim1Weight)
	{
		GaitAnimationBracket[] array = RenderAsModelType.GaitAnimations[gaitAnimsKey];
		anim1 = null;
		anim2 = null;
		anim1Weight = 1f;
		float num = 0f;
		float num2 = 0f;
		foreach (GaitAnimationBracket gaitAnimationBracket in array)
		{
			if (moveSpeed >= gaitAnimationBracket.MinimumSpeed && moveSpeed <= gaitAnimationBracket.MaximumSpeed)
			{
				if (anim1 != null)
				{
					anim2 = gaitAnimationBracket;
					float num3 = num2 - gaitAnimationBracket.MinimumSpeed;
					anim1Weight = num / num3;
					anim1Weight = Common.Clamp(anim1Weight, 0f, 1f);
					break;
				}
				anim1 = gaitAnimationBracket;
				num = gaitAnimationBracket.MaximumSpeed - moveSpeed;
				num2 = gaitAnimationBracket.MaximumSpeed;
				anim1Weight = 1f;
			}
		}
		if (anim1 == null)
		{
			anim1Weight = 1f;
			anim1 = array[array.Length - 1];
		}
	}

	private void SetHaulingAnimStates()
	{
		ClearAnimationActionStateFlag(AnimAction.Moving);
		SetAnimationActionStateFlag(AnimAction.Hauling);
		AnimModifier? burdenAnimStateFlag = Renderable.GetBurdenAnimStateFlag(base.ParentEntity.AgentStorage.GetHaulingPercentageOfCapacity());
		if (burdenAnimStateFlag == AnimModifier.Heavy)
		{
			SetAnimationStateFlag(AnimModifier.HaulHeavy);
		}
		else
		{
			ClearAnimationStateFlag(AnimModifier.HaulHeavy);
		}
		BoxHandlingWhenHauling boxHandlingWhenHauling = Renderable.RenderableType.BoxHandlingWhenHauling;
		if (!burdenAnimStateFlag.HasValue || boxHandlingWhenHauling == null || boxHandlingWhenHauling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysInHand)
		{
			return;
		}
		if (boxHandlingWhenHauling.BoxHandling != BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack)
		{
			if (boxHandlingWhenHauling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar)
			{
				AnimModifier? animModifier = burdenAnimStateFlag;
				AnimModifier animModifier2 = AnimModifier.Heavy;
				if (animModifier.GetValueOrDefault() == animModifier2 && animModifier.HasValue && Renderable.AnimConditions.Modifiers.Test(AnimModifier.Far))
				{
					goto IL_00c3;
				}
			}
			Renderable.AttachBoxModelToHand(burdenAnimStateFlag);
			return;
		}
		goto IL_00c3;
		IL_00c3:
		if (boxHandlingWhenHauling.UseHeavyBackpack)
		{
			Renderable.AttachModelToBack("backpackHeavy", AttacheePoint.Back);
		}
		else
		{
			Renderable.AttachModelToBack("box", AttacheePoint.Bottom);
		}
	}

	public static void GetAttachTransformations(Renderable attachable, AttachPoint attachor, AttacheePoint? attacheePointName, AnimConditionInfo animCondition, out AttachPoint attacheePoint, out Vector3 translation, out Vector3 rotation)
	{
		GetAttachTransformations(attachable, attachor, attacheePointName, animCondition, out attacheePoint, out translation, out rotation, out var _, out var _);
	}

	public static void GetAttachTransformations(Renderable attachable, AttachPoint attachor, AttacheePoint? attacheePointName, AnimConditionInfo animCondition, out AttachPoint attacheePoint, out Vector3 translation, out Vector3 rotation, out AppliedAttachableTransforms appliedRotationTransform, out AppliedAttachableTransforms appliedTranslationTransform)
	{
		string keyName = attachable.RenderableType.KeyName;
		translation = Vector3.Zero;
		rotation = Vector3.Zero;
		ModelData modelData = attachable.RenderAsModel.ModelData;
		attacheePoint = modelData.GetAttacheePoint(attacheePointName);
		if (animCondition != null && animCondition.AttachPoints != null)
		{
			appliedTranslationTransform = AppliedAttachableTransforms.Animation;
			appliedRotationTransform = AppliedAttachableTransforms.Animation;
			AnimConditionInfo.AttachPointData[] attachPoints = animCondition.AttachPoints;
			foreach (AnimConditionInfo.AttachPointData attachPointData in attachPoints)
			{
				AttacheePoint? attacheePoint2 = attachPointData.AttacheePoint;
				AttacheePoint? attacheePoint3 = attacheePointName;
				if (attacheePoint2.GetValueOrDefault() == attacheePoint3.GetValueOrDefault() && attacheePoint2.HasValue == attacheePoint3.HasValue && attachPointData.RenderableTypeKey == keyName)
				{
					translation = attachPointData.Translation;
					rotation = attachPointData.Rotation;
					return;
				}
			}
			attachPoints = animCondition.AttachPoints;
			foreach (AnimConditionInfo.AttachPointData attachPointData2 in attachPoints)
			{
				if (attachPointData2.AttacheePoint == attacheePointName)
				{
					translation = attachPointData2.Translation;
					rotation = attachPointData2.Rotation;
					return;
				}
			}
			attachPoints = animCondition.AttachPoints;
			foreach (AnimConditionInfo.AttachPointData attachPointData3 in attachPoints)
			{
				if (attachPointData3.RenderableTypeKey == keyName)
				{
					translation = attachPointData3.Translation;
					rotation = attachPointData3.Rotation;
					return;
				}
			}
			translation = animCondition.AttachPoints[0].Translation;
			rotation = animCondition.AttachPoints[0].Rotation;
		}
		else
		{
			if (attacheePoint != null && attacheePoint.Translation.HasValue)
			{
				translation = attacheePoint.Translation.Value;
				appliedTranslationTransform = AppliedAttachableTransforms.Attachee;
			}
			else if (attachor.Translation.HasValue)
			{
				translation = attachor.Translation.Value;
				appliedTranslationTransform = AppliedAttachableTransforms.Attachor;
			}
			else
			{
				appliedTranslationTransform = AppliedAttachableTransforms.Attachor;
			}
			if (attacheePoint != null && attacheePoint.Rotation != new Vector3(0f, 0f, 0f))
			{
				rotation = attacheePoint.Rotation;
				appliedRotationTransform = AppliedAttachableTransforms.Attachee;
			}
			else if (attachor.Rotation != new Vector3(0f, 0f, 0f))
			{
				rotation = attachor.Rotation;
				appliedRotationTransform = AppliedAttachableTransforms.Attachor;
			}
			else
			{
				appliedRotationTransform = AppliedAttachableTransforms.Attachor;
			}
		}
	}

	private void ClearHaulingAnimStates()
	{
		ClearAnimationActionStateFlag(AnimAction.Hauling);
		ClearAnimationStateFlag(AnimModifier.HaulHeavy);
	}

	public void ComputeMatricesForDrawing(AnimatedModel.Transformations transformations, float scale, Vector3 _LOC_, Vector3 _DIR_, float _ROT_, float? _ROLL_, float? _PITCH_)
	{
		if (!_ROLL_.HasValue)
		{
			Vector3 vector = -new Vector3(_DIR_.X, _DIR_.Y, 0f);
			Vector3 vector2 = -Vector3.UnitZ;
			Vector3 right = Vector3.Cross(vector, vector2);
			AnimatedModel.ComputeMatricesForDrawing(vector, vector2, right, scale, _LOC_, ModelData.ModelOffset, transformations);
		}
		else
		{
			Matrix identity = Matrix.Identity;
			identity *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, -(float)Math.PI / 2f);
			identity *= Matrix.CreateFromAxisAngle(identity.Right, -(float)Math.PI / 2f);
			float angle = (_ROLL_.HasValue ? _ROLL_.Value : 0f);
			float angle2 = (_PITCH_.HasValue ? _PITCH_.Value : 0f);
			identity *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, _ROT_);
			identity *= Matrix.CreateFromAxisAngle(identity.Backward, angle);
			identity *= Matrix.CreateFromAxisAngle(identity.Right, angle2);
			AnimatedModel.ComputeMatricesForDrawing(identity.Forward, identity.Up, identity.Right, scale, _LOC_, ModelData.ModelOffset, transformations);
		}
		AnimatedModel.ModelAnimator.ComputeTransformsForAttachedObjects(scale);
	}

	private bool ReplaceAnimationConditionState()
	{
		if (RenderAsModelType.AnimConditions == null && RenderAsModelType.DefaultInfo == null)
		{
			return true;
		}
		RenderAsModelType.FindBestAnimInfo(Renderable.AnimConditions, out var bestMatch);
		if (WaitWithFillerAnim())
		{
			return false;
		}
		if (IsUnnecessaryIdleChange(bestMatch))
		{
			return true;
		}
		if (IsUnnecessaryStanceChange(bestMatch))
		{
			return true;
		}
		if (bestMatch != null)
		{
			if (bestMatch.SoundAndAnimationSet != null && bestMatch.SoundAndAnimationSet.BaseAnimations != null)
			{
				bestMatch.SoundAndAnimationSet.BaseAnimations.Contains("idleToSleep");
			}
			AdoptAnimInfo(bestMatch);
		}
		return true;
	}

	private bool WaitWithFillerAnim()
	{
		if (!Renderable.AnimConditions.Action.HasValue && noOfFramesWeHaveBeenDirty < 4 && SelectedAnimInfo != null && SelectedAnimInfo.Looping == Looping.Yes)
		{
			return true;
		}
		return false;
	}

	private bool IsUnnecessaryIdleChange(AnimConditionInfo info)
	{
		if ((info == RenderAsModelType.DefaultInfo || (RenderAsModelType.DefaultStances != null && RenderAsModelType.DefaultStances.Contains(info))) && SelectedAnimInfo != null && SelectedAnimInfo.ConditionSet != null && SelectedAnimInfo.ConditionSet.Action == AnimAction.Idle)
		{
			return true;
		}
		return false;
	}

	private AnimModifier? GetEndStance(AnimConditionInfo info)
	{
		if (info.ConditionSet.Modifiers == null)
		{
			return null;
		}
		BitMask64 modifiers = info.ConditionSet.Modifiers;
		bool flag = modifiers.Test(AnimModifier.Kneeling);
		bool flag2 = modifiers.Test(AnimModifier.Sitting);
		bool flag3 = modifiers.Test(AnimModifier.Lying);
		if (modifiers.Test(AnimModifier.Reverse))
		{
			if (flag3)
			{
				return AnimModifier.Lying;
			}
			if (flag2)
			{
				return AnimModifier.Sitting;
			}
			if (flag)
			{
				return AnimModifier.Kneeling;
			}
			return null;
		}
		if (flag3)
		{
			if (flag2)
			{
				return AnimModifier.Sitting;
			}
			if (flag)
			{
				return AnimModifier.Kneeling;
			}
			return null;
		}
		if (flag2)
		{
			if (flag)
			{
				return AnimModifier.Kneeling;
			}
			return null;
		}
		return null;
	}

	private bool IsUnnecessaryStanceChange(AnimConditionInfo info)
	{
		if (info == null || info.ConditionSet == null || info.ConditionSet.Action != AnimAction.ChangingStance)
		{
			return false;
		}
		StancesType stancesType = base.Parent.EntityType.LocomotorType.StancesType;
		AnimModifier? endStance = stancesType.GetEndStance(info);
		if (endStance.HasValue)
		{
			if (SelectedAnimInfo != null && SelectedAnimInfo.ConditionSet != null && SelectedAnimInfo.ConditionSet.Modifiers != null && SelectedAnimInfo.ConditionSet.Modifiers.Test(endStance.Value))
			{
				return true;
			}
		}
		else if (SelectedAnimInfo != null && stancesType.IsDefaultStance(SelectedAnimInfo.ConditionSet))
		{
			return true;
		}
		return false;
	}

	private bool IsStanding(AnimConditions conditionSet)
	{
		if (conditionSet != null && conditionSet.Modifiers != null)
		{
			if (!conditionSet.Modifiers.Test(AnimModifier.Sitting) && !conditionSet.Modifiers.Test(AnimModifier.Kneeling))
			{
				return !conditionSet.Modifiers.Test(AnimModifier.Lying);
			}
			return false;
		}
		return true;
	}

	private bool AdoptAnimInfo(AnimConditionInfo newInfo)
	{
		if (newInfo == null)
		{
			newInfo = RenderAsModelType.DefaultInfo;
		}
		string previousAnimKey = ((SelectedAnimInfo != null) ? CurrentBaseAnimKey : null);
		Looping? looping = ((SelectedAnimInfo != null) ? new Looping?(SelectedAnimInfo.Looping) : ((Looping?)null));
		string blendingToAnimKey = ((SelectedAnimInfo != null) ? mainAnimation.AnimKeyBeingBlendedTo : null);
		string previousAdditionalAnimKey = ((SelectedAnimInfo != null) ? CurrentAdditionalAnim1Key : null);
		string previousAdditionalBlendAnimKey = ((SelectedAnimInfo != null) ? additionalAnimationTracks[0].AnimKeyBeingBlendedTo : null);
		string previousAdditionalAnimKey2 = ((SelectedAnimInfo != null) ? CurrentAdditionalAnim2Key : null);
		string previousAdditionalBlendAnimKey2 = ((SelectedAnimInfo != null) ? additionalAnimationTracks[1].AnimKeyBeingBlendedTo : null);
		AnimConditionInfo selectedAnimInfo = SelectedAnimInfo;
		SelectedAnimInfo = newInfo;
		SoundData oldSound = null;
		SoundData newSound = null;
		if (SelectedAnimInfo.SoundAndAnimationSet != null)
		{
			if (SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations != null)
			{
				int? newSelectedIndex;
				string randomAnimToSwitchTo = GetRandomAnimToSwitchTo(mainAnimation, SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations, out newSelectedIndex);
				if (newSelectedIndex.HasValue && randomAnimToSwitchTo != null && OKToStartAnimation(previousAnimKey, randomAnimToSwitchTo, blendingToAnimKey, looping, SelectedAnimInfo.Looping))
				{
					bool value = false;
					if (SelectedAnimInfo.Looping == Looping.Yes && SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations.Length > 1)
					{
						value = true;
					}
					float num = 0f;
					if (base.Parent.EntityType.IsFlyer)
					{
						num = The.Client.ClientRandomGenerator.RandomBetween(-0.01f, 0.01f);
					}
					StartAnimation(randomAnimToSwitchTo, mainAnimation, SelectedAnimInfo.Playback, SelectedAnimInfo.StartingPoint, SelectedAnimInfo.BlendMode, SelectedAnimInfo.SpeedFactor + num, SelectedAnimInfo.Looping, SelectedAnimInfo.StartingPointInSeconds, value, PickNewRandomAnim);
					if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null)
					{
						newSound = SelectedAnimInfo.SoundAndAnimationSet.SoundData[newSelectedIndex.Value];
					}
					secondGaitAnimation.StartAnimation(null, Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal, out var _, 1f, Looping.No, null, null, null, The.Client.ClientRandomGenerator.Random);
				}
			}
			else if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null)
			{
				newSound = Common.GetRandomListMember(SelectedAnimInfo.SoundAndAnimationSet.SoundData, The.Client.ClientRandomGenerator);
			}
			int? newSelectedIndex2;
			string randomAnimToSwitchTo2 = GetRandomAnimToSwitchTo(additionalAnimationTracks[0], SelectedAnimInfo.SoundAndAnimationSet.AdditionalAnimations1, out newSelectedIndex2);
			string randomAnimToSwitchTo3 = GetRandomAnimToSwitchTo(additionalAnimationTracks[1], SelectedAnimInfo.SoundAndAnimationSet.AdditionalAnimations2, out newSelectedIndex2);
			AdoptAdditionalAnim(looping, previousAdditionalAnimKey, previousAdditionalBlendAnimKey, randomAnimToSwitchTo2, additionalAnimationTracks[0]);
			AdoptAdditionalAnim(looping, previousAdditionalAnimKey2, previousAdditionalBlendAnimKey2, randomAnimToSwitchTo3, additionalAnimationTracks[1]);
		}
		Renderable.AdoptStateSound(newSound, oldSound, SelectedAnimInfo.Looping);
		if (selectedAnimInfo != SelectedAnimInfo)
		{
			Renderable.AdoptParticleEffects(SelectedAnimInfo.ParticleEmitters, selectedAnimInfo?.ParticleEmitters);
			AdoptAttachedRenderables(selectedAnimInfo);
			AdoptAttachedTransformationChanges(selectedAnimInfo);
		}
		return true;
	}

	public void StartAnimation(string animKey, AnimationTrack track, Playback playback, StartingPoint startingPoint, BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null, bool? setCallback = false, EventHandler pickNewRandomAnim = null)
	{
		_ = animKey == "sleepToIdle";
		AnimationController animationController = null;
		if (animKey != null)
		{
			animationController = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animKey);
			Renderable.HideHandAttachments = animationController.AnimationInfo.HideHandAttachments;
		}
		long? startOffsetToAdd2 = null;
		if (startOffsetToAdd.HasValue)
		{
			startOffsetToAdd2 = new TimeSpan(0, 0, 0, 0, (int)(1000f * startOffsetToAdd).Value).Ticks;
		}
		track.StartAnimation(animationController, playback, startingPoint, mode, out var _, speedFactor, looping, startOffsetToAdd2, setCallback, pickNewRandomAnim, The.Client.ClientRandomGenerator.Random);
	}

	public void PickNewRandomAnim(object sender, EventArgs e)
	{
		if (The.Client == null)
		{
			return;
		}
		((AnimationController)sender).AnimationEnded -= PickNewRandomAnim;
		if (SelectedAnimInfo.SoundAndAnimationSet == null || SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations == null || !SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations.Contains(((AnimationController)sender).AnimationInfo.Name))
		{
			return;
		}
		int? newSelectedIndex = null;
		string randomAnimToSwitchTo = GetRandomAnimToSwitchTo(mainAnimation, SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations, out newSelectedIndex);
		StartAnimation(randomAnimToSwitchTo, mainAnimation, SelectedAnimInfo.Playback, SelectedAnimInfo.StartingPoint, SelectedAnimInfo.BlendMode, SelectedAnimInfo.SpeedFactor, SelectedAnimInfo.Looping, SelectedAnimInfo.StartingPointInSeconds, true, PickNewRandomAnim);
		if (newSelectedIndex.HasValue)
		{
			SoundData newSound = null;
			if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null)
			{
				newSound = SelectedAnimInfo.SoundAndAnimationSet.SoundData[newSelectedIndex.Value];
			}
			Renderable.AdoptStateSound(newSound, null, SelectedAnimInfo.Looping);
		}
	}

	private string GetRandomAnimToSwitchTo(AnimationTrack track, string[] animationsToChooseFrom, out int? newSelectedIndex)
	{
		newSelectedIndex = null;
		if (animationsToChooseFrom == null)
		{
			return null;
		}
		if (AllowAnimToContinue(track.controllerBeingBlendedTo, animationsToChooseFrom))
		{
			return track.controllerBeingBlendedTo.AnimationInfo.Name;
		}
		if (AllowAnimToContinue(track.currentController, animationsToChooseFrom))
		{
			newSelectedIndex = Array.IndexOf(animationsToChooseFrom, track.currentController.AnimationInfo.Name);
			return track.currentController.AnimationInfo.Name;
		}
		return GetRandomAnim(animationsToChooseFrom, track, out newSelectedIndex);
	}

	private bool AllowAnimToContinue(AnimationController controller, string[] animationsToChooseFrom)
	{
		if (controller != null && animationsToChooseFrom.Contains(controller.AnimationInfo.Name) && controller.SpeedFactor > 0.0 && controller.ElapsedTimeMinusStartOffset > 0 && controller.ElapsedTime < controller.AnimationInfo.Duration)
		{
			return true;
		}
		return false;
	}

	private string GetRandomAnim(string[] anims, AnimationTrack track, out int? selectedIndex)
	{
		selectedIndex = null;
		if (anims != null)
		{
			if (anims.Length > 1)
			{
				GetNearbyAgentsOfSameType(ref nearByEntitiesOfSameType);
				string currentAnimKey = track.CurrentAnimKey;
				long? currentAnimElapsed = track.CurrentAnimElapsed;
				float num = 0f;
				int? num2 = null;
				for (int i = 0; i < anims.Length; i++)
				{
					string animKey = anims[i];
					float num3 = ScoreRandomAnim(animKey, currentAnimElapsed, currentAnimKey, nearByEntitiesOfSameType);
					if (num3 > 0f && num3 > num)
					{
						num2 = i;
						num = num3;
					}
				}
				if (!num2.HasValue)
				{
					num2 = Common.GetRandomListMemberIndex(anims, The.Client.ClientRandomGenerator);
				}
				selectedIndex = num2;
				return anims[selectedIndex.Value];
			}
			selectedIndex = 0;
			return anims[0];
		}
		return null;
	}

	private float ScoreRandomAnim(string animKey, long? currentAnimElapsed, string currentAnimKey, List<Pair<Entity, Vector2>> nearByEntitiesOfSameType)
	{
		float num = 1f;
		foreach (Pair<Entity, Vector2> item in nearByEntitiesOfSameType)
		{
			if (item.First.Renderable != null && item.First.Renderable.RenderAsModel.IsRunningAnim(animKey, 0L, out var animIsInSync))
			{
				if (animIsInSync)
				{
					return 0f;
				}
				num -= 0.1f;
				num = Common.ClampBottom(num, 0.1f);
			}
		}
		if (animKey == currentAnimKey)
		{
			num -= 0.1f;
		}
		return num + The.Client.ClientRandomGenerator.RandomBetween(0f, 0.05f);
	}

	private void GetNearbyAgentsOfSameType(ref List<Pair<Entity, Vector2>> nearByEntitiesOfSameType)
	{
		nearByEntitiesOfSameType.Clear();
		The.AgentQuadTree.GetEntitiesInRange(base.Parent.Location.Value.ToVector2(), 300f, (Entity e) => e.EntityType == base.ParentEntity.EntityType && e != base.ParentEntity, ref nearByEntitiesOfSameType);
	}

	private bool AnimIsPlayedByNearbyAgent(string animKey, long animProgress)
	{
		if (base.ParentEntity != null)
		{
			nearByEntitiesOfSameType.Clear();
			The.AgentQuadTree.GetEntitiesInRange(base.Parent.PlaySiteLocation.ToVector2(), 300f, (Entity e) => e.EntityType == base.ParentEntity.EntityType, ref nearByEntitiesOfSameType);
			foreach (Pair<Entity, Vector2> item in nearByEntitiesOfSameType)
			{
				_ = item;
				IsRunningAnim(animKey, animProgress, out var _);
			}
		}
		return false;
	}

	private void AdoptAdditionalAnim(Looping? previousAnimIsLooping, string previousAdditionalAnimKey, string previousAdditionalBlendAnimKey, string currentAdditionalAnimKey, AnimationTrack track)
	{
		if (currentAdditionalAnimKey != null)
		{
			if (OKToStartAnimation(previousAdditionalAnimKey, currentAdditionalAnimKey, previousAdditionalBlendAnimKey, previousAnimIsLooping, SelectedAnimInfo.Looping))
			{
				StartAnimation(currentAdditionalAnimKey, track, SelectedAnimInfo.Playback, SelectedAnimInfo.StartingPoint, BlendMode.Normal, SelectedAnimInfo.SpeedFactor, SelectedAnimInfo.Looping, null, false);
			}
		}
		else if (track.RunsAnimation())
		{
			track.StopAnimation();
		}
	}

	private void AdoptAttachedTransformationChanges(AnimConditionInfo oldInfo)
	{
		if (oldInfo == null || oldInfo.AttachPoints != SelectedAnimInfo.AttachPoints)
		{
			ModelAnimator.IterateAttachedEntities(RecomputeLocalTransformForAttachable);
		}
	}

	private void AdoptAttachedRenderables(AnimConditionInfo oldInfo)
	{
		bool flag = false;
		if (oldInfo != null)
		{
			flag = TemporaryRenderablesToAttachAreEquals(oldInfo.TemporaryRenderablesToAttach, SelectedAnimInfo.TemporaryRenderablesToAttach);
		}
		if (oldInfo != null && oldInfo.TemporaryRenderablesToAttach != null && !flag)
		{
			AnimConditionInfo.TemporaryAttachable[] temporaryRenderablesToAttach = oldInfo.TemporaryRenderablesToAttach;
			foreach (AnimConditionInfo.TemporaryAttachable temporaryAttachable in temporaryRenderablesToAttach)
			{
				Renderable.RemoveAttachables(temporaryAttachable.RenderableTypeKey, temporaryAttachable.AttachorTag);
			}
		}
		if ((oldInfo == null || !flag) && SelectedAnimInfo.TemporaryRenderablesToAttach != null)
		{
			AnimConditionInfo.TemporaryAttachable[] temporaryRenderablesToAttach = SelectedAnimInfo.TemporaryRenderablesToAttach;
			foreach (AnimConditionInfo.TemporaryAttachable temporaryAttachable2 in temporaryRenderablesToAttach)
			{
				Renderable.AttachPooledObjectIfPossible(temporaryAttachable2.RenderableTypeKey, temporaryAttachable2.AttachorTag, temporaryAttachable2.AttacheePoint ?? AttacheePoint.RightHand);
			}
		}
	}

	private bool TemporaryRenderablesToAttachAreEquals(AnimConditionInfo.TemporaryAttachable[] renderablesToAttach1, AnimConditionInfo.TemporaryAttachable[] renderablesToAttach2)
	{
		if (renderablesToAttach1 == null)
		{
			if (renderablesToAttach2 == null)
			{
				return true;
			}
			return false;
		}
		if (renderablesToAttach2 == null)
		{
			return false;
		}
		if (renderablesToAttach1.Length != renderablesToAttach2.Length)
		{
			return false;
		}
		for (int i = 0; i < renderablesToAttach1.Length; i++)
		{
			if (!renderablesToAttach1[i].Equals(renderablesToAttach2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static void RecomputeLocalTransformForAttachable(IAttachable attachable)
	{
		Renderable renderable = ((RenderAsModel)attachable).Renderable;
		GetAttachTransformations(animCondition: ((Entity)attachable.AttachedTo).Renderable.RenderAsModel.SelectedAnimInfo, attachable: renderable, attachor: attachable.AttachorPoint, attacheePointName: attachable.AttacheePointValue, attacheePoint: out var attacheePoint, translation: out var translation, rotation: out var rotation);
		Matrix matrix = Renderable.CreateTransformForAttachedEntity(attachable.Scale, attachable.AttachorPoint, attacheePoint, translation, rotation);
		renderable.RenderAsModel.LocalTransform = matrix;
	}

	private bool OKToStartAnimation(string previousAnimKey, string newAnimKey, string blendingToAnimKey, Looping? previousLoopingState, Looping newLoopingState)
	{
		if (previousAnimKey == newAnimKey && (blendingToAnimKey == null || blendingToAnimKey == newAnimKey))
		{
			Looping? looping = previousLoopingState;
			Looping looping2 = Looping.Yes;
			if (looping.GetValueOrDefault() == looping2 && looping.HasValue && newLoopingState == Looping.Yes)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsRunningAnim(string animKey, long animProgress, out bool animIsInSync)
	{
		animIsInSync = false;
		bool animIsInSync2;
		bool num = IsRunningAnim(animKey, animProgress, mainAnimation.currentController, out animIsInSync2);
		bool animIsInSync3 = false;
		bool flag = false;
		if (!num || !animIsInSync2)
		{
			flag = IsRunningAnim(animKey, animProgress, mainAnimation.controllerBeingBlendedTo, out animIsInSync3);
		}
		animIsInSync = animIsInSync2 || animIsInSync3;
		return num || flag;
	}

	private static bool IsRunningAnim(string animKey, long animProgress, AnimationController animController, out bool animIsInSync)
	{
		animIsInSync = false;
		if (animController != null && animController.AnimationInfo != null && animController.AnimationInfo.Name == animKey)
		{
			long num2;
			if (animController.IsLooping)
			{
				long num = animController.AnimationInfo.Duration;
				num2 = Math.Abs(animProgress - animController.ElapsedTime);
				if (num2 > num / 2)
				{
					num2 = num - num2;
				}
			}
			else
			{
				num2 = Math.Abs(animController.ElapsedTime - animProgress);
			}
			if (num2 < 6000000)
			{
				animIsInSync = true;
			}
			return true;
		}
		return false;
	}

	public string GetCurrentMainAnimation()
	{
		if (mainAnimation.currentController != null && mainAnimation.currentController.AnimationInfo != null)
		{
			return mainAnimation.currentController.AnimationInfo.Name;
		}
		return null;
	}

	internal void AppendAnimDebugInfo(StringBuilder states)
	{
		states.Append("    current main:  " + mainAnimation.CurrentAnimKey);
		states.Append("\n");
		states.Append("    blend main:    " + mainAnimation.AnimKeyBeingBlendedTo);
		states.Append("\n");
		states.Append("\n");
		if (secondGaitAnimation.FinalWeightFactor > 0f && (secondGaitAnimation.currentController != null || secondGaitAnimation.controllerBeingBlendedTo != null))
		{
			states.Append("    current 2nd gait:  " + secondGaitAnimation.CurrentAnimKey);
			states.Append("\n");
			states.Append("    blend 2nd gait:    " + secondGaitAnimation.AnimKeyBeingBlendedTo);
			states.Append("\n");
			states.Append("\n");
			states.Append("    weight:  " + secondGaitAnimation.FinalWeightFactor);
			states.Append("\n");
		}
		int num = 1;
		foreach (AnimationTrack additionalAnimationTrack in additionalAnimationTracks)
		{
			if (additionalAnimationTrack.currentController != null || additionalAnimationTrack.controllerBeingBlendedTo != null)
			{
				states.Append("    additional track #" + num + ", current: " + additionalAnimationTrack.CurrentAnimKey);
				states.Append("\n");
				states.Append("    additional track #" + num + ", blend: " + additionalAnimationTrack.AnimKeyBeingBlendedTo);
				states.Append("\n");
				states.Append("\n");
			}
		}
	}

	public void UpdateAnimationManuallyByTimeScalar(double scalar)
	{
		if (!animationFlagsAreDirty)
		{
			mainAnimation.UpdateAnimationManuallyByTimeScalar(The.Client.GameTime, scalar);
		}
	}

	public void StartMainAnimation(string animKey, Playback playback, StartingPoint startingPoint, BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
	{
		StartAnimation(animKey, mainAnimation, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd, false);
	}

	public void StartAdditionalAnimation(string animKey, Playback playback, StartingPoint startingPoint, BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
	{
		foreach (AnimationTrack additionalAnimationTrack in additionalAnimationTracks)
		{
			if (additionalAnimationTrack.currentController == null)
			{
				StartAnimation(animKey, additionalAnimationTrack, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd, false);
			}
		}
	}

	public void StopMainAnimation()
	{
		mainAnimation.StopAnimation();
	}

	public void UpdateAnimationConditionState()
	{
		if (animationFlagsAreDirty)
		{
			if (ReplaceAnimationConditionState())
			{
				animationFlagsAreDirty = false;
				noOfFramesWeHaveBeenDirty = 0;
				previousMainGait = null;
			}
			else
			{
				noOfFramesWeHaveBeenDirty++;
			}
		}
	}
}
