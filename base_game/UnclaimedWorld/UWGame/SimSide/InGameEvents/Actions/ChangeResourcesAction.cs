using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ChangeResourcesAction : EventActionType
{
	public enum Operation
	{
		Multiply,
		Set,
		Add,
		SetMinimum
	}

	public EvalNode DynamicResourceType;

	public string[] ResourceType;

	public string[] ResourceCategory;

	public bool? AllResources;

	public string[] ExcludeResourceTypes;

	public string[] ExcludeResourceCategories;

	public Area Area;

	public EvalNode Value;

	public NoiseParams NoiseParameters;

	public Operation OperationToUse;

	public ChangeResourcesAction(string keyName)
		: base(keyName)
	{
	}

	public ChangeResourcesAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		List<ResourceType> resourceTypes = GetResourceTypes(action);
		List<ResourceContainer> list = ((Area != null) ? GetLocalResourceContainers(resourceTypes, action) : GetGlobalResourceContainers(resourceTypes));
		if (list != null)
		{
			SetValues(list, action);
		}
		return true;
	}

	private void SetValues(List<ResourceContainer> resourceContainers, EventAction action)
	{
		float? num = null;
		foreach (ResourceContainer resourceContainer in resourceContainers)
		{
			float num2;
			if (Value != null)
			{
				if (!num.HasValue)
				{
					num = Value.Evaluate(action).Value.NumberResult.Value;
				}
				num2 = num.Value;
			}
			else
			{
				if (NoiseParameters == null)
				{
					break;
				}
				num2 = GetResourceNoiseValue(CreateNoiseSeed(resourceContainer.ResourceType), NoiseParameters, resourceContainer.MapPosition);
				if (!(num2 > 0f))
				{
				}
			}
			int noOfHarvestableItems = resourceContainer.NoOfHarvestableItems;
			int f;
			switch (OperationToUse)
			{
			case Operation.Multiply:
				f = (int)Math.Round(num2 * (float)noOfHarvestableItems);
				break;
			case Operation.Add:
				f = (int)Math.Round(num2 + (float)noOfHarvestableItems);
				break;
			case Operation.SetMinimum:
				f = (int)Math.Round(num2);
				f = Math.Max(noOfHarvestableItems, f);
				break;
			default:
				f = (int)Math.Round(num2);
				break;
			}
			f = Common.ClampBottom(f, 0);
			_ = 0;
			resourceContainer.SetResourceItems(f);
		}
	}

	public override string ToString()
	{
		if (ResourceType != null)
		{
			return string.Concat(ResourceType);
		}
		if (ResourceCategory != null)
		{
			return string.Concat(ResourceCategory);
		}
		return "";
	}

	public static float GetResourceNoiseValue(SimplexNoise simplexNoise, NoiseParams noiseParams, Point tilePos)
	{
		float num = simplexNoise.Generate2D(tilePos.X, tilePos.Y, noiseParams.NoiseFrequency);
		num += 1f;
		float num2 = ((!noiseParams.NoiseAmplitude.HasValue) ? num : (num * noiseParams.NoiseAmplitude.Value));
		if (noiseParams.NoiseAddend.HasValue)
		{
			num2 += noiseParams.NoiseAddend.Value;
		}
		return num2;
	}

	private SimplexNoise CreateNoiseSeed(ResourceType resourceType)
	{
		if (The.Sim.ResourceNoiseSeeds == null)
		{
			The.Sim.ResourceNoiseSeeds = new Dictionary<ResourceType, Tuple<NoiseParams, SimplexNoise>>();
		}
		SimplexNoise simplexNoise;
		if (!The.Sim.ResourceNoiseSeeds.TryGetValue(resourceType, out var value))
		{
			simplexNoise = new SimplexNoise();
			value = new Tuple<NoiseParams, SimplexNoise>(NoiseParameters, simplexNoise);
			The.Sim.ResourceNoiseSeeds.Add(resourceType, value);
		}
		else
		{
			simplexNoise = value.Item2;
		}
		return simplexNoise;
	}

	private List<ResourceType> GetResourceTypes(EventAction action)
	{
		List<ResourceType> list = new List<ResourceType>();
		if (AllResources == true)
		{
			foreach (KeyValuePair<string, ResourceType> allResourceType in GameData.Instance.AllResourceTypes)
			{
				list.Add(allResourceType.Value);
			}
		}
		if (DynamicResourceType != null)
		{
			PropertyResult? propertyResult = DynamicResourceType.Evaluate(action);
			if (propertyResult.HasValue)
			{
				AppendResourceType(list, propertyResult.Value.StringResult);
			}
		}
		if (ResourceType != null)
		{
			string[] resourceType = ResourceType;
			foreach (string resourceKey in resourceType)
			{
				AppendResourceType(list, resourceKey);
			}
		}
		if (ResourceCategory != null)
		{
			string[] resourceType = ResourceCategory;
			foreach (string categoryKey in resourceType)
			{
				AppendResourceCategory(list, categoryKey);
			}
		}
		if (ExcludeResourceTypes != null)
		{
			string[] resourceType = ExcludeResourceTypes;
			foreach (string resourceKey2 in resourceType)
			{
				RemoveResourceType(list, resourceKey2);
			}
		}
		if (ExcludeResourceCategories != null)
		{
			string[] resourceType = ExcludeResourceCategories;
			foreach (string categoryKey2 in resourceType)
			{
				RemoveResourceCategory(list, categoryKey2);
			}
		}
		return list.Distinct().ToList();
	}

	private List<ResourceContainer> GetGlobalResourceContainers(List<ResourceType> resourceTypes)
	{
		List<ResourceContainer> list = new List<ResourceContainer>();
		foreach (ResourceType resourceType in resourceTypes)
		{
			if (The.Sim.PlaySite.Resources.TryGetValue(resourceType, out var value))
			{
				list.AddRange(value.GetAsList());
			}
		}
		return list;
	}

	private List<ResourceContainer> GetLocalResourceContainers(List<ResourceType> resourceTypes, EventAction action)
	{
		List<ResourceContainer> list = new List<ResourceContainer>();
		foreach (TilePos item in Area.ComputeArea(action))
		{
			TerrainTile tile = The.Map.GetTile(item);
			foreach (ResourceType resourceType in resourceTypes)
			{
				if (!tile.TileResources.TryGetValue(resourceType, out var value))
				{
					value = tile.AddResource(resourceType, 0);
				}
				list.Add(value);
			}
		}
		return list;
	}

	private List<ResourceContainer> GetContainers(List<ResourceType> resourceTypes)
	{
		List<ResourceContainer> list = new List<ResourceContainer>();
		foreach (ResourceType resourceType in resourceTypes)
		{
			if (The.Sim.PlaySite.Resources.TryGetValue(resourceType, out var value))
			{
				list.AddRange(value.GetAsList());
			}
		}
		return list;
	}

	private void AppendResourceType(List<ResourceType> resourceTypes, string resourceKey)
	{
		resourceTypes.Add(GameData.Instance.AllResourceTypes[resourceKey]);
	}

	private void RemoveResourceType(List<ResourceType> resourceTypes, string resourceKey)
	{
		ResourceType type = GameData.Instance.AllResourceTypes[resourceKey];
		resourceTypes.RemoveAll((ResourceType r) => r == type);
	}

	private void AppendResourceCategory(List<ResourceType> resourceTypes, string categoryKey)
	{
		IEnumerable<ResourceType> resourceTypesOfCategory = GetResourceTypesOfCategory(categoryKey);
		resourceTypes.AddRange(resourceTypesOfCategory);
	}

	private IEnumerable<ResourceType> GetResourceTypesOfCategory(string categoryKey)
	{
		ResourceCategory category = GameData.Instance.AllResourceCategories[categoryKey];
		return from k in GameData.Instance.AllResourceTypes
			where k.Value.Category == category
			select k.Value;
	}

	private void RemoveResourceCategory(List<ResourceType> resourceTypes, string categoryKey)
	{
		IEnumerable<ResourceType> resourceTypesOfCategory = GetResourceTypesOfCategory(categoryKey);
		resourceTypes.RemoveAll((ResourceType r) => resourceTypesOfCategory.Contains(r));
	}
}
