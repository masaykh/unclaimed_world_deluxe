using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological;

public class CasteType : IXmlSerializable, IEdge
{
	public string KeyName;

	public string Name;

	public List<AgeGroupType> AgeGroupTypes;

	[XmlIgnore]
	public float MaxAge;

	public string ModelName;

	public float? ModelScale;

	public string ModelBasicTextureName;

	public Vector3? PrimaryColor;

	public Vector3? SecondaryColor;

	public Vector3? TertiaryColor;

	public Vector3? QuaternaryColor;

	public float? Size;

	public Reproduction Reproduction;

	public float WeightMean;

	public float WeightStandardDeviation;

	public float HeightMean;

	public float HeightStandardDeviation;

	public SerializableDictionary<string, BioProperty> BioProperties;

	private static int noOfAgeGroupTypes = Enum.GetValues(typeof(AIAgeGroup)).Length;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(CasteType))
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

	[XmlElement("ProbabilityEdge")]
	public float Edge { get; set; }

	public void Initialize(int casteNo)
	{
		MaxAge = 0f;
		foreach (AgeGroupType ageGroupType in AgeGroupTypes)
		{
			ageGroupType.Initialize();
			if (ageGroupType.Edge > MaxAge)
			{
				MaxAge = ageGroupType.Edge;
			}
		}
		if (string.IsNullOrEmpty(Name))
		{
			Name = "Caste #" + casteNo;
		}
	}

	public void PostDataCompleteInitialize()
	{
		foreach (AgeGroupType ageGroupType in AgeGroupTypes)
		{
			ageGroupType.PostDataCompleteInitialize();
		}
	}

	public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
	{
		return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
	}

	public void Validate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "Caste KeyName", KeyName != null);
		if (AgeGroupTypes.Count != noOfAgeGroupTypes)
		{
			EntityType.CreateValidationError(ref errors, KeyName + ": All age group types must be defined.");
		}
		int num = -1;
		foreach (AgeGroupType ageGroupType in AgeGroupTypes)
		{
			if ((int)ageGroupType.AIAgeGroup <= num)
			{
				EntityType.CreateValidationError(ref errors, KeyName + ": The age group AI types are not in the correct sequence.");
			}
			num = (int)ageGroupType.AIAgeGroup;
			ageGroupType.Validate(ref errors);
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
