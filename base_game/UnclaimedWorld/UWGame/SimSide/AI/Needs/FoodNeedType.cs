using System.Xml.Serialization;
using UWGame.SimSide.Items;

namespace UWGame.SimSide.AI.Needs;

public class FoodNeedType
{
	public string FoodNutrient;

	[XmlIgnore]
	public FoodNutrientType FoodNutrientType;

	public float RequiredNutrientsAsFractionOfEntityBulk;

	public bool IsEssential = true;

	public void PostDataCompleteInitialize()
	{
		if (FoodNutrient != null)
		{
			FoodNutrientType = GameData.Instance.AllFoodNutrientTypes[FoodNutrient];
		}
	}
}
