using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Skills;

namespace UWGame.SimSide.Entities;

[XmlRoot("Skill")]
[DebuggerDisplay("{KeyName}")]
public class SkillType : IXmlSerializable, IGameData, IHasCategory<SkillCategory>
{
	public string Description;

	public string ProfessionKey;

	[XmlIgnore]
	public ProfessionType ProfessionType;

	public bool SuppressDisplayForPersons;

	public bool SuppressDisplayForBiologicals;

	public int SortOrder;

	public bool GiveExpertSkillBonus;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SkillType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<SkillCategory, string>
			{
				GetterMethod = (SkillCategory t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllSkillCategories[s] : null
			}
		}
	};

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public SkillCategory Category { get; set; }

	public SkillType()
	{
	}

	public SkillType(string key, string name)
	{
		KeyName = key;
		Name = name;
	}

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
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
		if (ProfessionKey != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProfessionKey, GameData.Instance.AllProfessionTypes, out ProfessionType);
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
