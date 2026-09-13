using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace UWGame.Client.Audio;

public class PlayedSound
{
	public SoundEffectInstance SoundEffectInstance;

	public SoundData SoundData;

	public List<Tuple<Vector3, float>> Sources;

	private Action<PlayedSound> soundEndedEvent;

	public event Action<PlayedSound> SoundEndedEvent
	{
		add
		{
			if (soundEndedEvent == null || !soundEndedEvent.GetInvocationList().Contains(value))
			{
				soundEndedEvent = (Action<PlayedSound>)Delegate.Combine(soundEndedEvent, value);
			}
		}
		remove
		{
			soundEndedEvent = (Action<PlayedSound>)Delegate.Remove(soundEndedEvent, value);
		}
	}

	public override string ToString()
	{
		string text = SoundData.Sound;
		if (SoundEffectInstance != null)
		{
			text = string.Concat(text, " - ", SoundEffectInstance.State, ", vol. ", SoundEffectInstance.Volume.ToString("N2"), ", pan: ", SoundEffectInstance.Pan.ToString("N2"));
		}
		return text;
	}

	internal void SoundEnded()
	{
		if (soundEndedEvent != null)
		{
			soundEndedEvent(this);
		}
	}
}
