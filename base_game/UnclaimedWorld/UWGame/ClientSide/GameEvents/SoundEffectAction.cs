using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.Client.Audio;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.GameEvents;

public class SoundEffectAction : EventActionType, IXmlSerializable, IGameData
{
	public string Sound;

	private SoundData soundEffect;

	public WorldLocation? Location;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SoundEffectAction))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

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

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
