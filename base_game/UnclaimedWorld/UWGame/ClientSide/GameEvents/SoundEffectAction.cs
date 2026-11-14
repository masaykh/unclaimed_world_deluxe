using System.Collections.Generic;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.GameEvents;

// PORT FIX. The studio's class was also IXmlSerializable, writing itself through a
// CustomXmlSerializer proxy. XmlSerializer cannot write or read an IXmlSerializable type through
// its base class's [XmlInclude] ("may not be used in this context"), and this is the one event
// action that is: EventActionType lists it, and scenarios hold it in ActionSets. Every exported
// scenario that plays a sound - Making Headway, Muckroot Mining Site, the three Fields of Tau Ceti
// - got an actionSets.xml cut off where the first one was reached, and could not be loaded back
// (Kastuk, 2026-11-14). Its fields are a sound key and an optional location, which XmlSerializer
// handles on its own like the other thirty actions'. The game never serialized one: scenario
// actions are built in code.
public class SoundEffectAction : EventActionType, IGameData
{
	public string Sound;

	private SoundData soundEffect;

	public WorldLocation? Location;

	public SoundEffectAction(string keyName)
		: base(keyName)
	{
	}

	public SoundEffectAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		if (Location.HasValue)
		{
			The.Client.AudioManager.PlayWorldSound(soundEffect, Location.Value);
		}
		else
		{
			The.Client.AudioManager.PlaySound(soundEffect, 1f);
		}
		return true;
	}

	public override void Initialize()
	{
		base.Initialize();
		soundEffect = GameData.Instance.AllSoundData[Sound];
	}

	public override void PostInitValidate(ref List<string> listOfErrors)
	{
		base.PostInitValidate(ref listOfErrors);
		if (soundEffect == null)
		{
			EntityType.CreateValidationError(ref listOfErrors, string.Format("soundEffect was not filled out!", base.KeyName));
		}
	}
}
