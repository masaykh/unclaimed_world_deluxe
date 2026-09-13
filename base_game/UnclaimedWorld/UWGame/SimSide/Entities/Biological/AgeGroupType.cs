using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological;

public class AgeGroupType : IXmlSerializable, IEdge
{
	public string Name;

	public AIAgeGroup AIAgeGroup;

	public NeedType[] NeedTypes;

	public string ModelName;

	public float? ModelScaleFraction;

	public string ModelBasicTextureName;

	public Vector3? PrimaryColor;

	public Vector3? SecondaryColor;

	public Vector3? TertiaryColor;

	public Vector3? QuaternaryColor;

	public float? Size;

	public bool CanReproduce;

	public float HeightTargetModifier;

	public float WeightTargetModifier;

	public SerializableDictionary<string, BioProperty> BioProperties;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AgeGroupType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<Vector3?, string>
			{
				GetterMethod = (Vector3? t) => t.HasValue ? PersonType.Vector3ToHexString(t.Value) : null,
				SetterMethod = (string s) => (s != null) ? new Vector3?(PersonType.HexStringToVector3(s)) : ((Vector3?)null)
			}
		}
	};

	[XmlElement("AgeUpperEnd")]
	public float Edge { get; set; }

	public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
	{
		return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
	}

	public void Validate(ref List<string> errors)
	{
		if (NeedTypes != null)
		{
			NeedType[] needTypes = NeedTypes;
			for (int i = 0; i < needTypes.Length; i++)
			{
				needTypes[i].Validate(ref errors);
			}
		}
	}

	public void Initialize()
	{
		if (NeedTypes != null)
		{
			NeedType[] needTypes = NeedTypes;
			for (int i = 0; i < needTypes.Length; i++)
			{
				needTypes[i].Initialize();
			}
		}
		ComputeNeedsNormalizedWeight();
	}

	public void PostDataCompleteInitialize()
	{
		if (NeedTypes != null)
		{
			NeedType[] needTypes = NeedTypes;
			for (int i = 0; i < needTypes.Length; i++)
			{
				needTypes[i].PostDataCompleteInitialize();
			}
		}
	}

	private void ComputeNeedsNormalizedWeight()
	{
		float num = 0f;
		if (NeedTypes == null)
		{
			return;
		}
		NeedType[] needTypes = NeedTypes;
		foreach (NeedType needType in needTypes)
		{
			if (needType.DecreasedEnergyWeight > 0f)
			{
				num += needType.DecreasedEnergyWeight;
			}
		}
		float num2 = 1f / num;
		needTypes = NeedTypes;
		foreach (NeedType needType2 in needTypes)
		{
			if (needType2.DecreasedEnergyWeight > 0f)
			{
				needType2.NormalizedDecreasedEnergyWeight = needType2.DecreasedEnergyWeight * num2;
			}
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
