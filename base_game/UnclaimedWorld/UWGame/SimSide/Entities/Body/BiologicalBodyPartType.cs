using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Body;

public class BiologicalBodyPartType : BodyPartType
{
	public OrganType[] OrganTypes;

	public string TissueLayer;

	[XmlIgnore]
	public BodyLayerType TissueLayerType;

	[XmlIgnore]
	public bool HasVitalOrgans;

	public override bool IsVital()
	{
		return HasVitalOrgans;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (OrganTypes != null)
		{
			OrganType[] organTypes = OrganTypes;
			for (int i = 0; i < organTypes.Length; i++)
			{
				if (organTypes[i].IsVital)
				{
					HasVitalOrgans = true;
					break;
				}
			}
		}
		if (TissueLayer != null)
		{
			TissueLayerType = GameData.Instance.AllBodyLayerTypes[TissueLayer];
		}
	}

	public override void PostLoadContentInitialize()
	{
		base.PostLoadContentInitialize();
		if (TissueLayerType != null)
		{
			TissueLayerType.PostLoadContentInitialize();
		}
	}
}
