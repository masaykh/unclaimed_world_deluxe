using System.Collections.Generic;
using UWGame.SimSide.Entities.Templates;

namespace UWGame.SimSide.AllGameData;

public class TraitTemplateLoader
{
	public static List<TraitTemplate> Init()
	{
		List<TraitTemplate> list = new List<TraitTemplate>();
		list.Add(new TraitTemplate
		{
			KeyName = "electronicsSpecialist",
			ExpertSkills = new string[4] { "electronics", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[0],
			MediumSkills = new string[4] { "bushcraft", "shooting", "construction", "mechanics" },
			LowSkills = new string[14]
			{
				"menial", "medicine", "butchering", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking", "hunting",
				"fishing", "foraging", "weaving", "carpentry"
			},
			ZeroSkills = new string[4] { "biology", "psychology", "smithing", "chemistry" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "mechanicsSpecialist",
			ExpertSkills = new string[4] { "mechanics", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[0],
			MediumSkills = new string[5] { "bushcraft", "shooting", "construction", "electronics", "carpentry" },
			LowSkills = new string[14]
			{
				"smithing", "menial", "medicine", "butchering", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking",
				"hunting", "fishing", "foraging", "weaving"
			},
			ZeroSkills = new string[3] { "biology", "psychology", "chemistry" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "smithingSpecialist",
			ExpertSkills = new string[4] { "smithing", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[1] { "bushcraft" },
			MediumSkills = new string[5] { "menial", "armedMelee", "unarmedFighting", "construction", "carpentry" },
			LowSkills = new string[12]
			{
				"butchering", "farming", "shooting", "cooking", "archery", "sneaking", "hunting", "fishing", "foraging", "chemistry",
				"mechanics", "weaving"
			},
			ZeroSkills = new string[4] { "medicine", "biology", "psychology", "electronics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "farmingSpecialist",
			ExpertSkills = new string[4] { "farming", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[1] { "menial" },
			MediumSkills = new string[6] { "bushcraft", "construction", "unarmedFighting", "butchering", "foraging", "carpentry" },
			LowSkills = new string[10] { "smithing", "shooting", "cooking", "archery", "armedMelee", "sneaking", "hunting", "fishing", "biology", "weaving" },
			ZeroSkills = new string[5] { "medicine", "psychology", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "constructionSpecialist",
			ExpertSkills = new string[4] { "construction", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[1] { "bushcraft" },
			MediumSkills = new string[3] { "menial", "armedMelee", "carpentry" },
			LowSkills = new string[11]
			{
				"farming", "shooting", "cooking", "archery", "sneaking", "unarmedFighting", "hunting", "fishing", "foraging", "butchering",
				"weaving"
			},
			ZeroSkills = new string[7] { "medicine", "psychology", "smithing", "biology", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "huntingSpecialist",
			ExpertSkills = new string[6] { "hunting", "archery", "shooting", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[5] { "bushcraft", "armedMelee", "sneaking", "foraging", "butchering" },
			MediumSkills = new string[3] { "menial", "unarmedFighting", "fishing" },
			LowSkills = new string[6] { "farming", "cooking", "biology", "construction", "carpentry", "weaving" },
			ZeroSkills = new string[6] { "psychology", "smithing", "medicine", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "menialSpecialist",
			ExpertSkills = new string[4] { "menial", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[0],
			MediumSkills = new string[5] { "unarmedFighting", "armedMelee", "sneaking", "butchering", "shooting" },
			LowSkills = new string[10] { "farming", "cooking", "hunting", "construction", "fishing", "bushcraft", "foraging", "archery", "carpentry", "weaving" },
			ZeroSkills = new string[7] { "psychology", "smithing", "biology", "medicine", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "cookingSpecialist",
			ExpertSkills = new string[5] { "cooking", "butchering", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[0],
			MediumSkills = new string[6] { "unarmedFighting", "armedMelee", "foraging", "shooting", "menial", "psychology" },
			LowSkills = new string[10] { "farming", "hunting", "construction", "archery", "sneaking", "fishing", "bushcraft", "medicine", "weaving", "carpentry" },
			ZeroSkills = new string[5] { "smithing", "biology", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "bushcraftSpecialist",
			ExpertSkills = new string[4] { "bushcraft", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[4] { "foraging", "fishing", "hunting", "archery" },
			MediumSkills = new string[9] { "unarmedFighting", "armedMelee", "shooting", "menial", "construction", "sneaking", "cooking", "butchering", "carpentry" },
			LowSkills = new string[3] { "farming", "medicine", "weaving" },
			ZeroSkills = new string[6] { "psychology", "smithing", "biology", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "securitySpecialist",
			ExpertSkills = new string[5] { "shooting", "armedMelee", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[3] { "archery", "unarmedFighting", "sneaking" },
			MediumSkills = new string[7] { "menial", "medicine", "psychology", "foraging", "fishing", "hunting", "bushcraft" },
			LowSkills = new string[6] { "farming", "construction", "cooking", "butchering", "weaving", "carpentry" },
			ZeroSkills = new string[5] { "smithing", "biology", "electronics", "chemistry", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "medicineSpecialist",
			ExpertSkills = new string[4] { "medicine", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[2] { "biology", "psychology" },
			MediumSkills = new string[3] { "bushcraft", "shooting", "butchering" },
			LowSkills = new string[14]
			{
				"menial", "construction", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking", "hunting", "fishing",
				"foraging", "chemistry", "weaving", "carpentry"
			},
			ZeroSkills = new string[3] { "smithing", "electronics", "mechanics" }
		});
		list.Add(new TraitTemplate
		{
			KeyName = "chemistrySpecialist",
			ExpertSkills = new string[4] { "chemistry", "grasping", "fruitPicking", "weeding" },
			HighSkills = new string[1] { "biology" },
			MediumSkills = new string[6] { "bushcraft", "shooting", "butchering", "medicine", "cooking", "farming" },
			LowSkills = new string[12]
			{
				"psychology", "menial", "construction", "armedMelee", "unarmedFighting", "archery", "sneaking", "hunting", "fishing", "foraging",
				"weaving", "carpentry"
			},
			ZeroSkills = new string[3] { "smithing", "electronics", "mechanics" }
		});
		return list;
	}
}
