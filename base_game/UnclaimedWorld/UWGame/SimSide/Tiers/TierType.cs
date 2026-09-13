using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.Tiers;

public class TierType : IGameData, IEdge
{
	public string Description;

	public string Icon;

	public float UpperEdge;

	[XmlIgnore]
	public int Index;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public float Edge
	{
		get
		{
			return UpperEdge;
		}
		set
		{
		}
	}

	public bool DeleteRecord { get; set; }

	public static void GetTierBelow(int tierIndex, out TierType previousTier, out float lowerTierEdge)
	{
		previousTier = null;
		if (tierIndex > 0)
		{
			previousTier = GameData.Instance.Tiers[tierIndex - 1];
			lowerTierEdge = previousTier.UpperEdge;
		}
		else
		{
			lowerTierEdge = 0f;
		}
	}

	public float GetTierEdgeBelow()
	{
		GetTierBelow(Index, out var _, out var lowerTierEdge);
		return lowerTierEdge;
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void Initialize()
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
	}
}
