using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Scenarios;

public class ScenarioData
{
	public string SpawnWorldAction;

	public string SpawnSiteAction;

	public string[] Actions;

	public string[] ConditionalEvents;

	public Difficulty[] MainDifficultySettings;

	public bool CustomEnabled = true;

	public OptionSet[] OptionSets;

	public bool EnableMissions = true;

	public bool EnableGraphs = true;

	public bool EnableContacts = true;

	public bool EnablePersonell = true;

	public bool EnableWorldMap = true;

	public bool EnablePolicy = true;

	public bool EnableLedger = true;

	public string LoadingBackgroundImage;

	public string LoadingDialogImage;

	public string LoadingDialogHeading;

	public string LoadingDialogText;

	public string WorldMapImage;

	[XmlIgnore]
	public Texture2D WorldMapTexture;

	public void Initialize()
	{
		if (OptionSets != null)
		{
			OptionSet[] optionSets = OptionSets;
			for (int i = 0; i < optionSets.Length; i++)
			{
				optionSets[i].Initialize();
			}
			IOrderedEnumerable<OptionSet> source = OptionSets.OrderBy((OptionSet o) => o.DisplayGroup);
			OptionSets = source.ToArray();
		}
	}

	public void LoadContent(ContentManager content)
	{
		if (WorldMapImage != null)
		{
			WorldMapTexture = content.Load<Texture2D>(WorldMapImage);
		}
	}

	public void PostInitValidate(List<string> listOfErrors)
	{
		if (MainDifficultySettings == null)
		{
			EntityType.ValidateRequiredValue(ref listOfErrors, "MainDifficultySettings", hasValue: false);
		}
		else
		{
			Difficulty[] mainDifficultySettings = MainDifficultySettings;
			for (int i = 0; i < mainDifficultySettings.Length; i++)
			{
				mainDifficultySettings[i].PostInitValidate(this, listOfErrors);
			}
		}
		if (OptionSets != null)
		{
			OptionSet[] optionSets = OptionSets;
			for (int i = 0; i < optionSets.Length; i++)
			{
				foreach (IGrouping<string, Option> item in optionSets[i].OptionsGroupedByDifficulty)
				{
					string difficultyKey = item.First().Difficulty.KeyName;
					if (MainDifficultySettings.FirstOrDefault((Difficulty d) => d.KeyName == difficultyKey) == null)
					{
						EntityType.CreateValidationError(ref listOfErrors, "The custom difficulty key: " + difficultyKey + " was not found in the main difficulty list.");
					}
				}
			}
		}
		else
		{
			EntityType.ValidateRequiredValue(ref listOfErrors, "OptionSets", hasValue: false);
		}
	}
}
