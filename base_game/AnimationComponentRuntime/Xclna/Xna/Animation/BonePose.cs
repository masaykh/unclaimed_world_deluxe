using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public class BonePose
{
	private static Matrix currentMatrixBuffer;

	private int index;

	private string name;

	private BonePose parent;

	private AnimationTrack mainAnimationTrack;

	private AnimationTrack secondGaitTrack;

	private BonePoseCollection children;

	private bool? blendToSpecialTransform;

	private bool useSpecialTransform;

	private bool doesMainAnimAffectBone;

	private bool doesMainBlendAnimAffectBone;

	private bool doesSecondGaitAnimAffectBone;

	private bool doesSecondGaitBlendAnimAffectBone;

	private List<AnimationTrack> additionalAnimationTracks = new List<AnimationTrack>();

	private List<bool> additionalAnimAffectsBone = new List<bool>();

	private List<bool> additionalBlendAnimAffectsBone = new List<bool>();

	public Matrix DefaultTransform;

	private float? specialTransformBlendingProgress;

	public AnimationTrack MainAnimationTrack => mainAnimationTrack;

	public bool UseSpecialTransform
	{
		get
		{
			return useSpecialTransform;
		}
		set
		{
			if (value == useSpecialTransform)
			{
				return;
			}
			if (value)
			{
				blendToSpecialTransform = true;
				if (!specialTransformBlendingProgress.HasValue)
				{
					specialTransformBlendingProgress = 0f;
				}
			}
			else
			{
				blendToSpecialTransform = false;
				if (!specialTransformBlendingProgress.HasValue)
				{
					specialTransformBlendingProgress = 1f;
				}
			}
			useSpecialTransform = value;
		}
	}

	public BonePoseCollection Children => children;

	public BonePose Parent => parent;

	public int Index => index;

	public string Name => name;

	internal BonePose(ModelBone bone, ModelBoneCollection bones, BonePose[] anims)
	{
		index = bone.Index;
		name = bone.Name;
		DefaultTransform = bone.Transform;
		if (bone.Parent != null)
		{
			parent = anims[bone.Parent.Index];
		}
		anims[index] = this;
		List<BonePose> list = new List<BonePose>();
		foreach (ModelBone child in bone.Children)
		{
			BonePose item = new BonePose(bones[child.Index], bones, anims);
			list.Add(item);
		}
		children = new BonePoseCollection(list);
	}

	private void FindHierarchy(List<BonePose> poses)
	{
		poses.Add(this);
		foreach (BonePose child in children)
		{
			child.FindHierarchy(poses);
		}
	}

	public BonePoseCollection GetHierarchy()
	{
		List<BonePose> list = new List<BonePose>();
		FindHierarchy(list);
		return new BonePoseCollection(list);
	}

	public void SetAnimationTrack(AnimationTrack track)
	{
		switch (track.TrackTypeValue)
		{
		case AnimationTrack.TrackType.Main:
			mainAnimationTrack = track;
			break;
		case AnimationTrack.TrackType.ExtraGait:
			secondGaitTrack = track;
			break;
		case AnimationTrack.TrackType.Additional:
			additionalAnimationTracks.Add(track);
			additionalAnimAffectsBone.Add(item: false);
			additionalBlendAnimAffectsBone.Add(item: false);
			break;
		}
	}

	public void UpdateAnimControllerStatus(AnimationTrack track)
	{
		switch (track.TrackTypeValue)
		{
		case AnimationTrack.TrackType.Main:
			if (track.currentController != null)
			{
				doesMainAnimAffectBone = track.currentController.AnimationInfo.AffectsBone(name);
			}
			else
			{
				doesMainAnimAffectBone = false;
			}
			if (track.controllerBeingBlendedTo != null)
			{
				doesMainBlendAnimAffectBone = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(name);
			}
			else
			{
				doesMainBlendAnimAffectBone = false;
			}
			break;
		case AnimationTrack.TrackType.ExtraGait:
			if (track.currentController != null)
			{
				doesSecondGaitAnimAffectBone = track.currentController.AnimationInfo.AffectsBone(name);
			}
			else
			{
				doesSecondGaitAnimAffectBone = false;
			}
			if (track.controllerBeingBlendedTo != null)
			{
				doesSecondGaitBlendAnimAffectBone = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(name);
			}
			else
			{
				doesSecondGaitBlendAnimAffectBone = false;
			}
			break;
		case AnimationTrack.TrackType.Additional:
			if (track.currentController != null)
			{
				additionalAnimAffectsBone[track.Index] = track.currentController.AnimationInfo.AffectsBone(name);
			}
			else
			{
				additionalAnimAffectsBone[track.Index] = false;
			}
			if (track.controllerBeingBlendedTo != null)
			{
				additionalBlendAnimAffectsBone[track.Index] = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(name);
			}
			else
			{
				additionalBlendAnimAffectsBone[track.Index] = false;
			}
			break;
		}
	}

	public Matrix GetCurrentTransform()
	{
		if (useSpecialTransform && !blendToSpecialTransform.HasValue)
		{
			return DefaultTransform;
		}
		foreach (AnimationTrack additionalAnimationTrack in additionalAnimationTracks)
		{
			bool flag = additionalAnimAffectsBone[additionalAnimationTrack.Index];
			bool flag2 = additionalBlendAnimAffectsBone[additionalAnimationTrack.Index];
			if (flag || flag2)
			{
				return additionalAnimationTrack.GetCurrentTransform(this, flag, flag2);
			}
		}
		Matrix start = mainAnimationTrack.GetCurrentTransform(this, doesMainAnimAffectBone, doesMainBlendAnimAffectBone);
		if (blendToSpecialTransform.HasValue)
		{
			Util.SlerpMatrix(ref start, ref DefaultTransform, specialTransformBlendingProgress.Value, out start);
			if (blendToSpecialTransform == true)
			{
				specialTransformBlendingProgress += 0.05f;
				if (specialTransformBlendingProgress > 1f)
				{
					specialTransformBlendingProgress = null;
					blendToSpecialTransform = null;
				}
			}
			else
			{
				specialTransformBlendingProgress -= 0.05f;
				if (specialTransformBlendingProgress < 0f)
				{
					specialTransformBlendingProgress = null;
					blendToSpecialTransform = null;
				}
			}
		}
		return start;
	}
}
