// Maps each proxied type to its generated XML serialization proxy.
//
// GENERATED FILE - do not edit. Regenerate with build/20-generate-xml-proxies.sh.
//
// CustomXmlSerializer.GenerateProxyAssembly looks the proxy up here instead of
// compiling one (PORT DEVIATION 12). A compile-time reference rather than a
// reflection-by-name lookup, so a missing or renamed proxy is a build error and not
// a first-time-you-load-a-mod error.

using System;
using System.Collections.Generic;

namespace UWGame.Generated.XmlProxies;

internal static class XmlProxyRegistry
{
	internal static readonly Dictionary<Type, Type> ProxyTypes = new()
	{
		{ typeof(global::UWGame.BitMask64), typeof(global::UWGame.Generated.XmlProxies.BitMask64) },
		{ typeof(global::UWGame.ClientSide.GameEvents.SoundEffectAction), typeof(global::UWGame.Generated.XmlProxies.SoundEffectAction) },
		{ typeof(global::UWGame.ClientSide.Interface.GUIConstants), typeof(global::UWGame.Generated.XmlProxies.GUIConstants) },
		{ typeof(global::UWGame.ClientSide.Renderables.RenderableType), typeof(global::UWGame.Generated.XmlProxies.RenderableType) },
		{ typeof(global::UWGame.SimSide.Buildings.StructureType), typeof(global::UWGame.Generated.XmlProxies.StructureType) },
		{ typeof(global::UWGame.SimSide.Combat.AttackType), typeof(global::UWGame.Generated.XmlProxies.AttackType) },
		{ typeof(global::UWGame.SimSide.Entities.Biological.AgeGroupType), typeof(global::UWGame.Generated.XmlProxies.AgeGroupType) },
		{ typeof(global::UWGame.SimSide.Entities.Biological.BiologicalType), typeof(global::UWGame.Generated.XmlProxies.BiologicalType) },
		{ typeof(global::UWGame.SimSide.Entities.Biological.CasteType), typeof(global::UWGame.Generated.XmlProxies.CasteType) },
		{ typeof(global::UWGame.SimSide.Entities.Biological.ColorProbability), typeof(global::UWGame.Generated.XmlProxies.ColorProbability) },
		{ typeof(global::UWGame.SimSide.Entities.Biological.RaceType), typeof(global::UWGame.Generated.XmlProxies.RaceType) },
		{ typeof(global::UWGame.SimSide.Entities.Body.MachineBodyPartType), typeof(global::UWGame.Generated.XmlProxies.MachineBodyPartType) },
		{ typeof(global::UWGame.SimSide.Entities.DetectionFactor), typeof(global::UWGame.Generated.XmlProxies.DetectionFactor) },
		{ typeof(global::UWGame.SimSide.Entities.EntityType), typeof(global::UWGame.Generated.XmlProxies.EntityType) },
		{ typeof(global::UWGame.SimSide.Entities.IntelligenceType), typeof(global::UWGame.Generated.XmlProxies.IntelligenceType) },
		{ typeof(global::UWGame.SimSide.Entities.PersonType), typeof(global::UWGame.Generated.XmlProxies.PersonType) },
		{ typeof(global::UWGame.SimSide.Entities.SensorType), typeof(global::UWGame.Generated.XmlProxies.SensorType) },
		{ typeof(global::UWGame.SimSide.Entities.SkillType), typeof(global::UWGame.Generated.XmlProxies.SkillType) },
		{ typeof(global::UWGame.SimSide.InGameEvents.PolledEventType), typeof(global::UWGame.Generated.XmlProxies.PolledEventType) },
		{ typeof(global::UWGame.SimSide.Items.FoodNutrientAmount), typeof(global::UWGame.Generated.XmlProxies.FoodNutrientAmount) },
		{ typeof(global::UWGame.SimSide.Items.FoodNutrientProfile), typeof(global::UWGame.Generated.XmlProxies.FoodNutrientProfile) },
		{ typeof(global::UWGame.SimSide.Items.ItemType), typeof(global::UWGame.Generated.XmlProxies.ItemType) },
		{ typeof(global::UWGame.SimSide.Items.WeaponType), typeof(global::UWGame.Generated.XmlProxies.WeaponType) },
		{ typeof(global::UWGame.SimSide.Jobs.ProductionToolType), typeof(global::UWGame.Generated.XmlProxies.ProductionToolType) },
		{ typeof(global::UWGame.SimSide.Processes.ProcessType), typeof(global::UWGame.Generated.XmlProxies.ProcessType) },
		{ typeof(global::UWGame.SimSide.Resources.ResourceType), typeof(global::UWGame.Generated.XmlProxies.ResourceType) },
		{ typeof(global::UWGame.SimSide.Systems.Triggers.TriggerType), typeof(global::UWGame.Generated.XmlProxies.TriggerType) },
	};
}
