using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation;

public class CustomDataPresentation : IGameDataObject
{
	public string[] PresentationTypeCategoryKeys;

	public PresentationTypeCategory[] PresentationTypeCategories;

	[XmlIgnore]
	public List<PresentationTypeCategory> FinalPresentationTypeCategories;

	public void Initialize()
	{
		FinalPresentationTypeCategories = new List<PresentationTypeCategory>();
		if (PresentationTypeCategoryKeys != null)
		{
			string[] presentationTypeCategoryKeys = PresentationTypeCategoryKeys;
			foreach (string key in presentationTypeCategoryKeys)
			{
				FinalPresentationTypeCategories.Add(GameData.Instance.AllPresentationTypeCategories[key]);
			}
		}
		else
		{
			PresentationTypeCategory[] presentationTypeCategories = PresentationTypeCategories;
			foreach (PresentationTypeCategory presentationTypeCategory in presentationTypeCategories)
			{
				presentationTypeCategory.Initialize();
				FinalPresentationTypeCategories.Add(presentationTypeCategory);
			}
		}
		foreach (PresentationTypeCategory finalPresentationTypeCategory in FinalPresentationTypeCategories)
		{
			_ = finalPresentationTypeCategory;
		}
	}

	public void PostDataCompleteInitialize()
	{
	}
}
