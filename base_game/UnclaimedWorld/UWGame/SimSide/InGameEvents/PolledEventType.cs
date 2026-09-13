using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents;

public class PolledEventType : IGameData, IXmlSerializable
{
	public string Comment;

	public bool PlaySiteOnly = true;

	public Condition Condition;

	public const double DefaultPollInterval = 4.0;

	public bool UseDefaultPollInterval;

	public EvalNode PollInterval;

	public bool AllowRandomTimeOffset;

	public TimePoint StartTimePoint;

	public bool StartAfterInterval;

	public ActionSets ActionSets;

	public string ActionSetsKey;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(PolledEventType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
		if (Condition != null)
		{
			Condition.PreInitValidate(ref errors);
		}
		if (ActionSets != null)
		{
			ActionSets.PreInitValidate(ref errors);
		}
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

	public void Initialize()
	{
		if (Condition != null)
		{
			Condition.Initialize();
		}
		if (ActionSets != null)
		{
			ActionSets.Initialize();
		}
		if (ActionSetsKey != null)
		{
			ActionSets = GameData.Instance.AllActionSets[ActionSetsKey];
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
		if (ActionSets != null)
		{
			ActionSets.PostInitValidate(ref errors);
		}
	}

	public double? GetPollIntervalUpdateInterval()
	{
		if (PollInterval != null)
		{
			PropertyResult? propertyResult = PollInterval.Evaluate(null, null, null, null);
			if (propertyResult.HasValue && propertyResult.Value.NumberResult.HasValue)
			{
				return propertyResult.Value.NumberResult.Value;
			}
		}
		return null;
	}

	public void Fire(Entity triggeringEntity, IHasExposedProperties polledEventSource, out bool isExpired)
	{
		ActionSets.Fire(triggeringEntity, null, polledEventSource, out isExpired);
	}

	public void LoadContent(ContentManager content)
	{
		if (ActionSets != null)
		{
			ActionSets.LoadContent(content);
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
