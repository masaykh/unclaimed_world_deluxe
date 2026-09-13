using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Expeditions;

public class PopulationData
{
	public int StartMembers;

	public int MaxMembers;

	public float? GrowthInPercentagePerDay;

	public float? GrowthInMembersPerDay;

	public StringChance[] RandomMembers;

	public string[] StartMembersList;

	public float? SpawnRadius;

	public string[] SpawnSources;

	public string[] StartSpawnSources;

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (StartSpawnSources != null)
		{
			string[] startSpawnSources = StartSpawnSources;
			foreach (string key in startSpawnSources)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, key, GameData.Instance.AllEntityData, out var _);
			}
		}
	}
}
