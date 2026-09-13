using System.Collections.Generic;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers;

[DebuggerDisplay("{KeyName}")]
public class UpgradeCategory : IGameData
{
	public string Description;

	public int SortOrder;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
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
