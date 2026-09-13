using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class SkillCategory : IGameData, ICategoryType
{
	public string Name { get; set; }

	[XmlIgnore]
	public string IconSpriteName { get; set; }

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
