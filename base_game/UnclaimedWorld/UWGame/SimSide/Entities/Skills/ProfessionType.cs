using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Skills;

public class ProfessionType : IGameData
{
	public string Icon;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
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
