using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities;

public class PersonalityType : IGameData
{
	public string Comments;

	public SerializableDictionary<RatingTypes, NormalDistribution> Principles;

	public NormalDistribution Adventurousness;

	public NormalDistribution Adaptability;

	public SerializableDictionary<string, NormalDistribution> Attraction;

	public NormalDistribution Stability;

	public SerializableDictionary<string, string> SpokenLines;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public PersonalityType(string key)
	{
		KeyName = key;
	}

	public PersonalityType()
	{
	}

	public void FillEntity(Personality personality)
	{
		personality.Principles = new Dictionary<RatingTypes, float>();
		foreach (KeyValuePair<RatingTypes, NormalDistribution> principle in Principles)
		{
			personality.Principles.Add(principle.Key, (float)principle.Value.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true));
		}
		if (Attraction != null)
		{
			personality.Attraction = new Dictionary<AllegianceID, float>();
			foreach (KeyValuePair<string, NormalDistribution> item in Attraction)
			{
				Allegiance allegianceFromKey = The.Sim.World.GetAllegianceFromKey(item.Key);
				if (allegianceFromKey != null)
				{
					personality.Attraction.Add(allegianceFromKey.ID, (float)item.Value.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true));
				}
			}
		}
		personality.Adaptability = (float)Adaptability.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true);
		personality.Stability = (float)Stability.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true);
	}

	public string GetSpokenLine(string key, string defaultLine)
	{
		string value = null;
		if (SpokenLines.TryGetValue(key, out value))
		{
			return value;
		}
		return defaultLine;
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
