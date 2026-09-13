using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide;

public class RandomSoundAndAnimationSet
{
	public string[] BaseAnimations;

	public string[] AdditionalAnimations1;

	public string[] AdditionalAnimations2;

	public string[] Sounds;

	[XmlIgnore]
	public SoundData[] SoundData { get; private set; }

	public void Initialize()
	{
		if (Sounds == null)
		{
			return;
		}
		SoundData = new SoundData[Sounds.Length];
		for (int i = 0; i < Sounds.Length; i++)
		{
			string text = Sounds[i];
			if (!string.IsNullOrEmpty(text))
			{
				SoundData[i] = GameData.Instance.AllSoundData[text];
			}
			else
			{
				SoundData[i] = null;
			}
		}
	}

	public void PostLoadContentValidate(ref List<string> listOfErrors)
	{
		if (BaseAnimations != null && SoundData != null && BaseAnimations.Length != SoundData.Length)
		{
			EntityType.CreateValidationError(ref listOfErrors, "There must be the same number of animation and sound entries.");
		}
	}
}
