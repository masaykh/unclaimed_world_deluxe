using System;
using System.Collections.ObjectModel;

namespace Xclna.Xna.Animation;

public class AnimationInfo
{
	private uint duration;

	private double durationInSeconds;

	private string animationName;

	private long startOffset;

	public bool OKToBlendAnimation = true;

	public bool HideHandAttachments;

	private double startOffsetInSeconds;

	private AnimationChannelCollection boneAnimations;

	private bool isGaitAnim;

	public long StartOffset
	{
		get
		{
			return startOffset;
		}
		set
		{
			startOffset = value;
			startOffsetInSeconds = new TimeSpan(startOffset).TotalSeconds;
		}
	}

	public bool IsGaitAnim
	{
		get
		{
			return isGaitAnim;
		}
		set
		{
			isGaitAnim = value;
		}
	}

	public AnimationChannelCollection AnimationChannels => boneAnimations;

	public ReadOnlyCollection<string> AffectedBones => boneAnimations.AffectedBones;

	public uint Duration => duration;

	public double DurationInSeconds => durationInSeconds;

	public string Name => animationName;

	public float ActionPointInSeconds { get; set; }

	internal AnimationInfo(string animationName, AnimationChannelCollection anims)
	{
		this.animationName = animationName;
		boneAnimations = anims;
		foreach (BoneKeyFrameCollection anim in anims)
		{
			if (anim.Duration > duration)
			{
				duration = anim.Duration;
				durationInSeconds = new TimeSpan(duration).TotalSeconds;
			}
		}
	}

	public bool AffectsBone(string boneName)
	{
		return boneAnimations.AffectsBone(boneName);
	}
}
