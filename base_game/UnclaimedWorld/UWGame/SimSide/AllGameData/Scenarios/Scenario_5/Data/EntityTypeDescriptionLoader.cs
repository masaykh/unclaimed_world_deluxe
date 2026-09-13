using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

public class EntityTypeDescriptionLoader
{
	public static List<EntityTypeDescription> Init()
	{
		return new List<EntityTypeDescription>
		{
			new EntityTypeDescription
			{
				KeyName = "personDescription",
				EntityType = "entity:human",
				EntityName = "Inhabitant of Headway",
				SummaryDescription = "This person lives in the town of Headway.",
				Description = ""
			},
			new EntityTypeDescription
			{
				KeyName = "inactivatedFoodCoolerUnitDescription",
				EntityType = "item:inactivatedFoodCoolerUnit",
				EntityName = "Refrigerator unit (inactivated)",
				SummaryDescription = "Used for cooling a food container",
				Description = "Can be placed in a food container to keep temperature at 5 degrees Celsius. Would have a battery life of 2-5 months depending on environment."
			},
			new EntityTypeDescription
			{
				KeyName = "activatedFoodCoolerUnitDescription",
				EntityType = "item:activatedFoodCoolerUnit",
				EntityName = "Refrigerator unit (activated)",
				SummaryDescription = "Used for cooling a food container",
				Description = "The device is activated and will keep the surrounding temperature at 5 degrees Celsius. Will run out of battery in 2-5 months depending on environment."
			},
			new EntityTypeDescription
			{
				KeyName = "cooledFoodCacheDescription",
				EntityType = "structure:cooledFoodCache",
				EntityName = "Cooled food cache",
				SummaryDescription = "Cooled with a refrigerator unit",
				Description = "It is possible to refrigerate food by storing it in a hole together with a refrigerator unit. The hole must be lined with large stones and covered with spoak leaves and rocks to keep animals out."
			},
			new EntityTypeDescription
			{
				KeyName = "scrapMetalDescription",
				EntityType = "item:scrapMetal",
				EntityName = "Scrap metal",
				SummaryDescription = "Pieces of various types of metal",
				Description = "Has a varying quality. Comes from different sources such as vehicles, machines and buildings."
			},
			new EntityTypeDescription
			{
				KeyName = "panelScrapsDescription",
				EntityType = "item:panelScraps",
				EntityName = "Panel scraps",
				SummaryDescription = "Thermoplastics/composite panels",
				Description = "Pieces of interior panels made of composite materials and thermoplastics. They can probably find some use for building improvised shelter."
			},
			new EntityTypeDescription
			{
				KeyName = "lean-toScrapsDescription",
				EntityType = "structure:lean-toScraps",
				EntityName = "Lean-to (Scraps)",
				SummaryDescription = "An improvised 2-person shelter made from panel scraps",
				Description = "Pieces of interior panels made of composite materials and thermoplastics. They can probably find some use for building improvised shelter."
			},
			new EntityTypeDescription
			{
				KeyName = "A-frameScrapsDescription",
				EntityType = "structure:A-frameScraps",
				EntityName = "A-frame (Scraps)",
				SummaryDescription = "A crude 1-person shelter made from panel scraps",
				Description = "Has room for one."
			},
			new EntityTypeDescription
			{
				KeyName = "superconductingWireDescription",
				EntityType = "item:superconductingWire",
				EntityName = "Superconducting wire",
				SummaryDescription = "Pieces of wire used in advanced electrical systems",
				Description = "Although designed for electric power transmission, this wire has enough ductility and tensile strength that it may find use in simple construction tasks."
			},
			new EntityTypeDescription
			{
				KeyName = "improvisedCookingPotDescription",
				EntityType = "item:improvisedCookingPot",
				EntityName = "Improvised cooking pot",
				SummaryDescription = "Metal pot made from scrap metal",
				Description = "This cooking pot is better than nothing."
			}
		};
	}
}
