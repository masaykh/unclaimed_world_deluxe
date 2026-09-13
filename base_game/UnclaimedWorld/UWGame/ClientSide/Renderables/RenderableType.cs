using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide.Particles;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Renderables;

public class RenderableType : IXmlSerializable, IGameData
{
	public RenderAsIconType RenderAsIconType;

	public RenderAsModelType RenderAsModelType;

	public RenderAsConnectedGroundSpriteType RenderAsConnectedGroundSpriteType;

	public ClientStateInfo[] ClientStateConditions;

	public ClientStateInfo DefaultClientState;

	public ParticleEmitterType[] ParticleEmitterTypes;

	public AnimatedHeadType AnimatedHeadType;

	public BoxHandlingWhenHauling BoxHandlingWhenHauling;

	public bool FadeOutWhenDestroyed;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public RenderKindOfType RenderKindOfType { get; set; }

	static RenderableType()
	{
		_proxyData = new CustomXmlSerializer.XmlProxyData(typeof(RenderableType))
		{
			TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
		};
	}

	public RenderableType()
	{
	}

	public RenderableType(string keyName = null)
	{
		KeyName = keyName;
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

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
		if (RenderAsModelType != null && DefaultClientState != null && DefaultClientState.RenderAsBillboardType != null)
		{
			CreateValidationError(ref listOfErrors, "Both the RenderAsModel and RenderAsBillboard types are defined. This is not allowed.");
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

	public void LoadContent(ContentManager content)
	{
		if (RenderAsModelType != null)
		{
			RenderAsModelType.LoadContent(content);
		}
	}

	public void PostLoadContentValidate(ref List<string> listOfErrors, EntityType parent)
	{
		if (RenderAsModelType != null)
		{
			RenderAsModelType.PostLoadContentValidate(ref listOfErrors, parent);
		}
		if (DefaultClientState != null)
		{
			DefaultClientState.PostLoadContentValidate(parent, ref listOfErrors);
		}
		if (ClientStateConditions != null)
		{
			ClientStateInfo[] clientStateConditions = ClientStateConditions;
			for (int i = 0; i < clientStateConditions.Length; i++)
			{
				clientStateConditions[i].PostLoadContentValidate(parent, ref listOfErrors);
			}
		}
	}

	public static void CreateValidationError(ref List<string> listOfErrors, string errorMessage)
	{
		if (listOfErrors == null)
		{
			listOfErrors = new List<string>();
		}
		listOfErrors.Add(errorMessage);
	}

	public void Initialize()
	{
		if (DefaultClientState == null)
		{
			FindBestStaticInfo(new BitMask64(typeof(StateModifier)), ClientStateConditions, DefaultClientState, out var bestMatch);
			DefaultClientState = (ClientStateInfo)bestMatch;
		}
		if (DefaultClientState != null)
		{
			DefaultClientState.Initialize();
		}
		if (ClientStateConditions != null)
		{
			ClientStateInfo[] clientStateConditions = ClientStateConditions;
			for (int i = 0; i < clientStateConditions.Length; i++)
			{
				clientStateConditions[i].Initialize();
			}
		}
		if (RenderAsModelType != null)
		{
			RenderAsModelType.Initialize();
		}
	}

	public bool CanFade()
	{
		if (RenderAsModelType == null)
		{
			return ParticleEmitterTypes != null;
		}
		return true;
	}

	public static void FindBestStaticInfo(BitMask64 condition, IStateInfo[] SpriteConditions, IStateInfo Default, out IStateInfo bestMatch)
	{
		bestMatch = Default;
		if (SpriteConditions == null)
		{
			return;
		}
		int num = 0;
		foreach (IStateInfo stateInfo in SpriteConditions)
		{
			if (stateInfo.Conditions == null || !stateInfo.Conditions.Any())
			{
				continue;
			}
			if (condition.Equals(stateInfo.Conditions))
			{
				bestMatch = stateInfo;
				break;
			}
			int num2 = 0;
			if (condition.Bits != 0L)
			{
				if (stateInfo.Conditions != null)
				{
					num2 = (int)condition.CountIntersection(stateInfo.Conditions);
				}
				if (((stateInfo.Forbiddens != null && condition.CountIntersection(stateInfo.Forbiddens) != 0) ? 1 : 0) > (false ? 1 : 0))
				{
					continue;
				}
			}
			int num3 = num2;
			if (num3 > num)
			{
				num = num3;
				bestMatch = stateInfo;
			}
		}
	}

	public static StateModifier GetRandomFlavour(int maxFlavour)
	{
		return The.Client.ClientRandomGenerator.RandomBetween(1, maxFlavour) switch
		{
			1 => StateModifier.Flavour1, 
			2 => StateModifier.Flavour2, 
			3 => StateModifier.Flavour3, 
			4 => StateModifier.Flavour4, 
			5 => StateModifier.Flavour5, 
			6 => StateModifier.Flavour6, 
			7 => StateModifier.Flavour7, 
			_ => StateModifier.Flavour1, 
		};
	}
}
