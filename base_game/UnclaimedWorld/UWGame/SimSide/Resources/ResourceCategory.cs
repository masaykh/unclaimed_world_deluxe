using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Resources;

[DebuggerDisplay("{KeyName}")]
public class ResourceCategory : ICategoryType, IGameData
{
	public Color? Color;

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public string KeyName { get; set; }

	public bool ShouldSerializeColor()
	{
		return Color.HasValue;
	}

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
