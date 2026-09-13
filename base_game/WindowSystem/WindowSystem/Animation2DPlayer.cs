using System;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Animation2DPlayer
{
	public delegate void AnimationEnded();

	public delegate void FrameChanged();

	private Animation2D animation;

	private bool isEnded = true;

	private int frameIndex;

	private float time;

	private float totalTimeElapsed;

	public Animation2D Animation => animation;

	public bool IsFinished => isEnded;

	public int FrameIndex => frameIndex;

	public bool RequiresUpdate
	{
		get
		{
			if (Animation != null)
			{
				return !isEnded;
			}
			return false;
		}
	}

	public event AnimationEnded AnimationEndedEvent;

	public event FrameChanged FrameChangedEvent;

	public Animation2DPlayer()
	{
	}

	public Animation2DPlayer(Animation2DPlayer original)
	{
		isEnded = original.isEnded;
		time = original.time;
		totalTimeElapsed = original.totalTimeElapsed;
		frameIndex = original.frameIndex;
		animation = original.animation;
	}

	public void StartAnimation(Animation2D animation = null)
	{
		if (animation != null)
		{
			this.animation = animation;
		}
		frameIndex = 0;
		time = 0f;
		totalTimeElapsed = 0f;
		isEnded = false;
	}

	public void StopAnimation()
	{
		isEnded = true;
	}

	public float GetProgress()
	{
		float num = totalTimeElapsed / (animation.FrameTime * (float)animation.FrameCount);
		if (!(num <= 1f))
		{
			return 1f;
		}
		return num;
	}

	public void Update(GameTime gameTime)
	{
		if (!RequiresUpdate)
		{
			return;
		}
		time += (float)gameTime.ElapsedGameTime.TotalSeconds;
		totalTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
		while (time > Animation.FrameTime)
		{
			time -= Animation.FrameTime;
			if (this.FrameChangedEvent != null)
			{
				this.FrameChangedEvent();
			}
			if (Animation.IsLooping)
			{
				frameIndex = (frameIndex + 1) % Animation.FrameCount;
				continue;
			}
			frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
			if (frameIndex == Animation.FrameCount - 1)
			{
				isEnded = true;
				if (this.AnimationEndedEvent != null)
				{
					this.AnimationEndedEvent();
				}
			}
		}
	}

	public Color GetCurrentColor(Color? tintingColor)
	{
		Color color = animation.Cells[frameIndex].Color;
		if (animation.DoColorInterpolation && animation.Cells.Count > frameIndex + 1)
		{
			color = Color.Lerp(color, animation.Cells[frameIndex + 1].Color, time / Animation.FrameTime);
		}
		if (tintingColor.HasValue)
		{
			color = new Color(color.ToVector4() * tintingColor.Value.ToVector4());
		}
		return color;
	}

	public Rectangle GetFrame()
	{
		return animation.Cells[frameIndex].Frame.Value;
	}
}
