using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Policies;

namespace UWGame.SimSide.Allegiances;

public class AllegianceData : IGameData
{
	public string Site;

	public string EntityType;

	public AllegianceType AllegianceType;

	public int? ForageAndHuntingRadius;

	public StatsData StatsData;

	public AllegiancePolicyData PolicyData;

	public bool PermitsImmigration;

	public StringChance[] AllegianceTemplates;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

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
