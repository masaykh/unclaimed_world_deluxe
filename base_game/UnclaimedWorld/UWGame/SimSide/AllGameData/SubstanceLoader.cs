using System.Collections.Generic;
using UWGame.SimSide.Entities.Substances;

namespace UWGame.SimSide.AllGameData;

public class SubstanceLoader
{
	public static List<SubstanceType> Init()
	{
		return new List<SubstanceType>
		{
			new SubstanceType
			{
				KeyName = "gold",
				Name = "Gold"
			},
			new SubstanceType
			{
				KeyName = "iron",
				Name = "Iron"
			},
			new SubstanceType
			{
				KeyName = "dirt",
				Name = "Dirt"
			},
			new SubstanceType
			{
				KeyName = "meat",
				Name = "Meat"
			},
			new SubstanceType
			{
				KeyName = "bones",
				Name = "Bones"
			},
			new SubstanceType
			{
				KeyName = "hide",
				Name = "Hide"
			},
			new SubstanceType
			{
				KeyName = "guts",
				Name = "Guts"
			},
			new SubstanceType
			{
				KeyName = "plating",
				Name = "Plating"
			},
			new SubstanceType
			{
				KeyName = "shell",
				Name = "Shell"
			}
		};
	}
}
