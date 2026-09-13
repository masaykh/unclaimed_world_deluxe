using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Entities.Templates;

public class CultureTemplate : IGameData
{
	public string CasteKey;

	public string RaceKey;

	public NormalDistribution AgeInYears;

	public int NoOfPortraitFlavours = 1;

	public string[] UncommonFirstNames;

	public string[] CommonFirstNames;

	public string[] UncommonLastNames;

	public string[] CommonLastNames;

	[XmlIgnore]
	private List<StringChance> firstNames;

	private List<StringChance> lastNames;

	private const float commonChance = 0.2f;

	private const float uncommonChance = 0.1f;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void FillEntity(Entity entity)
	{
		string text = null;
		string lastName = null;
		Intelligence intelligence = entity.Intelligence;
		if (intelligence != null)
		{
			text = Common.GetStairStepIndex(firstNames, out var stairstep, The.Sim.GameplayRandomGenerator).String;
			if (lastNames != null)
			{
				lastName = Common.GetStairStepIndex(lastNames, out stairstep, The.Sim.GameplayRandomGenerator).String;
			}
			intelligence.SetName(text, lastName);
		}
		entity.Find<UWGame.SimSide.Entities.Biological.BiologicalEntity>(out var c);
		UWGame.SimSide.Maps.MapEditor.BiologicalEntity.SetCaste(CasteKey, c);
		if (!string.IsNullOrEmpty(RaceKey))
		{
			c.SetRaceOnNewEntity(RaceKey);
		}
		float? age = null;
		if (AgeInYears != null)
		{
			age = (float)AgeInYears.GetRandomValue(The.Sim.GameplayRandomGenerator);
		}
		c.SetAgePreInit(age);
		if (entity.PersonEntity != null)
		{
			entity.PersonEntity.PortraitFlavour = 1 + The.Client.ClientRandomGenerator.Next(NoOfPortraitFlavours, "FillEntity");
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
		InitializeNames(ref firstNames, CommonFirstNames, UncommonFirstNames);
		if (CommonLastNames != null || UncommonLastNames != null)
		{
			InitializeNames(ref lastNames, CommonLastNames, UncommonLastNames);
		}
	}

	private static void InitializeNames(ref List<StringChance> list, string[] commonNames, string[] uncommonNames)
	{
		list = new List<StringChance>();
		float num = 0f;
		if (uncommonNames != null)
		{
			num += 0.1f;
			string[] array = uncommonNames;
			foreach (string text in array)
			{
				list.Add(new StringChance
				{
					String = text,
					Edge = num
				});
				num += 0.1f;
			}
		}
		if (commonNames != null)
		{
			num += 0.2f;
			string[] array = commonNames;
			foreach (string text2 in array)
			{
				list.Add(new StringChance
				{
					String = text2,
					Edge = num
				});
				num += 0.2f;
			}
		}
		float num2 = num;
		foreach (StringChance item in list)
		{
			item.Edge /= num2;
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
