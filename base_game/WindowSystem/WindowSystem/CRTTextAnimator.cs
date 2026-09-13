using System;
using Microsoft.Xna.Framework.Audio;

namespace WindowSystem;

public abstract class CRTTextAnimator : UIComponent
{
	public float TimeBetweenUpdates = 0.02f;

	protected double timePassed;

	protected bool isStarted;

	protected int currentIndex;

	protected static SoundEffectInstance sound;

	protected static int noOfSoundPlays;

	public bool IsStarted => isStarted;

	public CRTTextAnimator(GUIManager gui)
		: base(gui)
	{
	}

	public abstract void Add(Label label);

	public abstract void Clear();

	public override int Add(UIComponent control)
	{
		throw new Exception("Can only add labels...");
	}

	public abstract void StartAnimating();

	protected void Stop()
	{
		isStarted = false;
		noOfSoundPlays--;
		if (noOfSoundPlays <= 0)
		{
			if (sound != null)
			{
				sound.Stop(immediate: true);
			}
			noOfSoundPlays = 0;
		}
	}

	protected void StartIt()
	{
		isStarted = true;
		timePassed = 0.0;
		currentIndex = 0;
		noOfSoundPlays++;
	}
}
