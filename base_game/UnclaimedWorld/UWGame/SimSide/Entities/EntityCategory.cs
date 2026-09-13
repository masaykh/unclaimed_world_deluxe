using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

[DebuggerDisplay("{KeyName}")]
public class EntityCategory : ICategoryType, IHasIcon, IGameData
{
	public enum CategoryColors
	{
		None,
		Blue,
		Green,
		Red
	}

	public CategoryColors CategoryColor;

	public bool DisplayStructureIcon;

	public bool IsWaste;

	public string SpriteName;

	public int SortOrder = 1000;

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	[XmlIgnore]
	public string IconSpriteName { get; set; }

	public string KeyName { get; set; }

	public void Initialize()
	{
		IconSpriteName = SpriteName + "_icon";
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
