using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Xclna.Xna.Animation;

[DebuggerDisplay("{animation.animationName}|{uniqueID}")]
public abstract class AnimationController : IAnimationController
{
	private int uniqueID;

	private static int idCounter;

	private AnimationInfo animation;

	private double speedFactor = 1.0;

	private long elapsedTime;

	private long elapsed;

	private bool isLooping = true;

	private bool isManual;

	public bool IsLooping
	{
		get
		{
			return isLooping;
		}
		set
		{
			isManual = !value;
			isLooping = value;
		}
	}

	public bool IsManual
	{
		get
		{
			return isManual;
		}
		set
		{
			isLooping = !value;
			isManual = value;
			if (isManual)
			{
				speedFactor = 0.0;
			}
		}
	}

	public AnimationInfo AnimationInfo => animation;

	public long ElapsedTime
	{
		get
		{
			return elapsedTime;
		}
		set
		{
			if (animation.Name == "idleToSleep")
			{
				_ = 1000000;
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("ElapsedTime", "When setting the ElapsedTime for an animation, the value  must be between 0 and the animation duration.");
			}
			elapsedTime = value;
		}
	}

	public long ElapsedTimeMinusStartOffset => ElapsedTime - animation.StartOffset;

	public double SpeedFactor
	{
		get
		{
			return speedFactor;
		}
		set
		{
			if (double.IsNaN(speedFactor))
			{
				throw new Exception("error when setting speed factor (divide by zero?)");
			}
			speedFactor = value;
		}
	}

	public event EventHandler AnimationEnded;

	public event EventHandler AnimationTracksChanged;

	public AnimationController(AnimationInfo sourceAnimation)
	{
		animation = sourceAnimation;
		uniqueID = ++idCounter;
	}

	public void SetAnimationAtStart(long? startOffsetTicksToAdd = null)
	{
		ElapsedTime = GetStartOfAnimation(startOffsetTicksToAdd);
	}

	public long GetStartOfAnimation(long? startOffsetTicksToAdd = null)
	{
		long num = animation.StartOffset;
		if (startOffsetTicksToAdd.HasValue)
		{
			num += startOffsetTicksToAdd.Value;
		}
		return num;
	}

	public void SynchronizeAnim(AnimationController animToSynchronizeWith)
	{
		ElapsedTime = animToSynchronizeWith.ElapsedTime;
	}

	public bool IsAnimationAtStart()
	{
		return ElapsedTime <= animation.StartOffset;
	}

	protected virtual void OnAnimationEnded(EventArgs args)
	{
		if (this.AnimationEnded != null)
		{
			this.AnimationEnded(this, args);
		}
	}

	public void UpdateAnimationTimeScalar(double scalar)
	{
		IsManual = true;
		long num = animation.StartOffset + (long)(((double)animation.Duration - (double)animation.StartOffset) * scalar);
		if (animation.Name == "idleToSleep")
		{
			_ = 1000000;
		}
		ElapsedTime = num;
		if (ElapsedTime > animation.Duration)
		{
			ElapsedTime = animation.Duration;
		}
		if (ElapsedTime < 0)
		{
			ElapsedTime = 0L;
		}
	}

	public void Update(GameTime gameTime)
	{
		if (speedFactor == 0.0)
		{
			return;
		}
		elapsed = (long)(speedFactor * (double)gameTime.ElapsedGameTime.Ticks);
		if (isLooping)
		{
			ElapsedTime += elapsed;
			if (ElapsedTime > animation.Duration)
			{
				OnAnimationEnded(null);
				ElapsedTime = animation.StartOffset;
			}
		}
		else if (speedFactor > 0.0)
		{
			if (ElapsedTime != animation.Duration && elapsed != 0L)
			{
				ElapsedTime += elapsed;
				if (ElapsedTime >= animation.Duration || ElapsedTime < 0)
				{
					ElapsedTime = animation.Duration;
					OnAnimationEnded(null);
				}
			}
		}
		else if (ElapsedTime != 0L && elapsed != 0L)
		{
			ElapsedTime += elapsed;
			if (ElapsedTime >= animation.Duration || ElapsedTime < 0)
			{
				ElapsedTime = animation.StartOffset;
				OnAnimationEnded(null);
			}
		}
	}

	public abstract Matrix GetCurrentBoneTransform(BonePose pose);

	public bool ContainsAnimationTrack(BonePose pose)
	{
		return animation.AnimationChannels.AffectsBone(pose.Name);
	}

	protected virtual void OnAnimationTracksChanged(EventArgs e)
	{
		if (this.AnimationTracksChanged != null)
		{
			this.AnimationTracksChanged(this, e);
		}
	}
}
