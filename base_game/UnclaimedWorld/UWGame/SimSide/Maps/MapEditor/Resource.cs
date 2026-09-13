using System.Xml.Serialization;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Maps.MapEditor;

public class Resource
{
	public string KeyName;

	[XmlIgnore]
	public ResourceType ResourceType;

	public int? Modifier;

	public int? MinResourceItems;

	public int? MaxResourceItems;

	[XmlIgnore]
	public float? AbsoluteMeanItems;

	[XmlIgnore]
	public float? AbsoluteStandardDeviation;

	public Resource()
	{
	}

	public Resource(ResourceType resourceType)
	{
		ResourceType = resourceType;
		KeyName = resourceType.KeyName;
	}

	public void PostDataCompleteInitialize()
	{
		if (MinResourceItems.HasValue && MaxResourceItems.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(MinResourceItems.Value, MaxResourceItems.Value, out AbsoluteMeanItems, out AbsoluteStandardDeviation);
		}
		if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			ResourceType = GameData.Instance.AllResourceTypes[KeyName];
		}
	}

	public bool ShouldSerializeModifier()
	{
		return Modifier.HasValue;
	}

	public bool ShouldSerializeMinResourceItems()
	{
		return MinResourceItems.HasValue;
	}

	public bool ShouldSerializeMaxResourceItems()
	{
		return MaxResourceItems.HasValue;
	}

	public override string ToString()
	{
		return KeyName;
	}
}
