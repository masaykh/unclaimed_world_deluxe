using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class RequiresFuelType
{
	public float MaxFuel = 1f;

	public bool HasFlames;

	public bool ProducesSmoke;

	public float BurnRatePerDay;

	public string FuelTypeTag;

	public string FuelTypeKeyName;

	[XmlIgnore]
	public List<EntityType> FuelEntityTypes;

	[XmlIgnore]
	public string FuelClientString { get; private set; }

	public float GetNeededFuel(float durationInDays)
	{
		return BurnRatePerDay * durationInDays;
	}

	public void PostLoadContentInitialize()
	{
		if (!string.IsNullOrEmpty(FuelTypeTag))
		{
			FuelEntityTypes = GameData.Instance.FuelByTag[FuelTypeTag];
		}
		if (!string.IsNullOrEmpty(FuelTypeKeyName))
		{
			EntityType item = GameData.Instance.AllEntityTypes[FuelTypeKeyName];
			if (FuelEntityTypes == null)
			{
				FuelEntityTypes = new List<EntityType>();
			}
			if (!FuelEntityTypes.Contains(item))
			{
				FuelEntityTypes.Add(item);
			}
		}
		if (FuelEntityTypes != null)
		{
			string text = "";
			string text2 = "";
			int num = 0;
			while (text.Length < 20 && num < FuelEntityTypes.Count)
			{
				text += text2;
				text += FuelEntityTypes[num].Name;
				text2 = ", ";
				num++;
			}
			if (num < FuelEntityTypes.Count)
			{
				text += "...";
			}
			FuelClientString = text;
		}
	}
}
