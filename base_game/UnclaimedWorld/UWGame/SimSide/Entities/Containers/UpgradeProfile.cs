using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers;

[DebuggerDisplay("{KeyName}")]
/// <summary>
/// Defines the upgrade categories supported by a container type such as a home or workshop container.
/// </summary>
public class UpgradeProfile : IGameData
{
	/// <summary>
	/// Upgrade category keys available to this profile.
	/// </summary>
	public string[] UpgradeCategories;

	[XmlIgnore]
	public List<UpgradeCategory> UpgradeCategoriesFinal;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PostDataCompleteInitialize()
	{
		UpgradeCategoriesFinal = new List<UpgradeCategory>();
		string[] upgradeCategories = UpgradeCategories;
		foreach (string key in upgradeCategories)
		{
			UpgradeCategoriesFinal.Add(GameData.Instance.AllUpgradeCategories[key]);
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
