using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Systems.Triggers;

[DebuggerDisplay("{KeyName}")]
public class TriggerType : IGameData, IXmlSerializable
{
	public float? Range;

	public Vector2? AreaDimensions;

	public float? LifetimeInSeconds;

	public string ActionSetsKey;

	public double DurationBetweenTriggerUpdatesInSeconds;

	public TriggerPriority Priority;

	public bool IsPrey;

	public bool IsDrivenVehicle;

	public bool IsEntityDied;

	public int? MaxTimesToTriggerBeforeExpiring;

	public float? CooldownInSeconds;

	[XmlIgnore]
	public long? CooldownInTicks;

	public Interest Interest;

	public bool CanTriggerWhenUndetected;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(TriggerType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
		if (CooldownInSeconds.HasValue)
		{
			CooldownInTicks = TimeSpan.FromSeconds(CooldownInSeconds.Value).Ticks;
		}
		if (Interest != null)
		{
			Interest.Initialize();
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
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
