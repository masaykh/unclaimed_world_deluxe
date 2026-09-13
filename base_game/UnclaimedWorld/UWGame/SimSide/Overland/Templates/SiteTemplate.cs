using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Overland.Templates;

public class SiteTemplate : IGameData
{
	public StringChanceSet[] Allegiances;

	public string[] Names;

	public string Description;

	public float? SizeFactor;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void FillSite(Site site, string allegianceKeyName = null, string expeditionKeyName = null)
	{
		if (Names != null)
		{
			List<string> list = null;
			string[] names = Names;
			foreach (string item in names)
			{
				if (!The.Sim.World.AllSites.Any((KeyValuePair<string, Site> s) => s.Value.Name == item))
				{
					Common.AddToList(ref list, item);
				}
			}
			if (list != null)
			{
				site.Name = Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator);
			}
			else
			{
				site.Name = KeyName;
			}
		}
		if (Description != null)
		{
			site.Description = Description;
		}
		if (Allegiances != null)
		{
			StringChanceSet[] allegiances = Allegiances;
			for (int i = 0; i < allegiances.Length; i++)
			{
				int stairstep;
				StringChance stairStepIndex = Common.GetStairStepIndex(allegiances[i].Chances, out stairstep, The.Sim.GameplayRandomGenerator);
				Allegiance.CreateFromAllegianceData(GameData.Instance.AllAllegianceData[stairStepIndex.String], site, SizeFactor, allegianceKeyName, expeditionKeyName);
			}
		}
	}

	private bool HasSkill(string[] group, SkillType skill)
	{
		return group?.Any((string s) => s == skill.KeyName) ?? false;
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
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
