using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

[DebuggerDisplay("{KeyName}")]
public class ActionSets : IGameData
{
	public string Comments;

	public float? ChanceToFire;

	public EvalNode DynamicChanceToFire;

	public bool SupressWhenSpawning;

	public ActionSetsToFire FireMode;

	public ActionSetType[] SetsOfActions;

	public TargetObject ActionTargets;

	private bool isInitialized;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void Fire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, out bool isExpired, bool isSpawning = false)
	{
		isExpired = false;
		if (isSpawning && SupressWhenSpawning)
		{
			isExpired = false;
			return;
		}
		if (DynamicChanceToFire != null)
		{
			EntityID? triggeringEntity2 = null;
			if (triggeringEntity != null)
			{
				triggeringEntity2 = triggeringEntity.ID;
			}
			PropertyResult? propertyResult = DynamicChanceToFire.Evaluate(triggeringEntity2, targetEntity, polledEventSource, null);
			if (propertyResult.HasValue)
			{
				ChanceToFire = propertyResult.Value.NumberResult;
			}
		}
		if (ChanceToFire.HasValue && !Common.IsEqual(ChanceToFire.Value, 1f) && The.Sim.GameplayRandomGenerator.NextDouble("") > (double)ChanceToFire.Value)
		{
			isExpired = false;
			return;
		}
		if (ActionTargets != null)
		{
			List<IHasExposedProperties> result = ActionTargets.GetResult(triggeringEntity?.ID, targetEntity, polledEventSource, null);
			if (result != null)
			{
				foreach (IHasExposedProperties item in result)
				{
					FireSingle(triggeringEntity, targetEntity, polledEventSource, item, ref isExpired);
					if (isExpired)
					{
						break;
					}
				}
			}
		}
		else
		{
			FireSingle(triggeringEntity, targetEntity, polledEventSource, null, ref isExpired);
		}
		if (The.Sim != null)
		{
			isExpired = SetsOfActions.All((ActionSetType a) => The.Sim.PlaySite.EventManager.IsExpended(a));
		}
	}

	public void TestFire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		List<Exception> list = null;
		ActionSetType[] setsOfActions = SetsOfActions;
		foreach (ActionSetType actionSetType in setsOfActions)
		{
			try
			{
				new ActionSetData(actionSetType).Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			}
			catch (Exception innerException)
			{
				Common.AddToList(ref list, new Exception(Environment.NewLine + "Error in ActionSetType " + actionSetType.KeyName + ": " + Environment.NewLine, innerException));
			}
		}
		if (list != null)
		{
			throw new AggregateException(list.ToArray());
		}
	}

	private void FireSingle(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref bool isExpired)
	{
		List<ActionSetType> list = null;
		ActionSetType[] setsOfActions = SetsOfActions;
		foreach (ActionSetType actionSetType in setsOfActions)
		{
			if (The.Sim.PlaySite.EventManager.IsExpended(actionSetType) || !actionSetType.ConditionsAreFulfilled(triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
			{
				continue;
			}
			switch (FireMode)
			{
			case ActionSetsToFire.AllValid:
				new ActionSetData(actionSetType).Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				break;
			case ActionSetsToFire.FirstValid:
				new ActionSetData(actionSetType).Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (The.Sim != null)
				{
					isExpired = SetsOfActions.All((ActionSetType a) => The.Sim.PlaySite.EventManager.IsExpended(a));
				}
				else
				{
					isExpired = true;
				}
				return;
			case ActionSetsToFire.RandomValid:
				Common.AddToList(ref list, actionSetType);
				break;
			}
		}
		if (FireMode == ActionSetsToFire.RandomValid && list != null)
		{
			new ActionSetData(Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator)).Fire(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
	}

	public void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
	{
		ActionSetType[] setsOfActions = SetsOfActions;
		for (int i = 0; i < setsOfActions.Length; i++)
		{
			setsOfActions[i].ExtractNestedGameData(ref duplicateKeyErrors);
		}
	}

	public void LoadContent(ContentManager content)
	{
		if (SetsOfActions != null)
		{
			ActionSetType[] setsOfActions = SetsOfActions;
			for (int i = 0; i < setsOfActions.Length; i++)
			{
				setsOfActions[i].LoadContent(content);
			}
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
		ActionSetType[] setsOfActions = SetsOfActions;
		for (int i = 0; i < setsOfActions.Length; i++)
		{
			setsOfActions[i].PreInitValidate(ref errors);
		}
	}

	public void Initialize()
	{
		if (!isInitialized)
		{
			ActionSetType[] setsOfActions = SetsOfActions;
			for (int i = 0; i < setsOfActions.Length; i++)
			{
				setsOfActions[i].Initialize();
			}
			isInitialized = true;
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
		ActionSetType[] setsOfActions = SetsOfActions;
		for (int i = 0; i < setsOfActions.Length; i++)
		{
			setsOfActions[i].PostInitValidate(ref errors);
		}
	}

	public void PostLoadContentValidate(List<string> listOfErrors)
	{
		ActionSetType[] setsOfActions = SetsOfActions;
		for (int i = 0; i < setsOfActions.Length; i++)
		{
			setsOfActions[i].PostLoadContentValidate(ref listOfErrors);
		}
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (SetsOfActions != null)
		{
			ActionSetType[] setsOfActions = SetsOfActions;
			for (int i = 0; i < setsOfActions.Length; i++)
			{
				setsOfActions[i].PostDataCompleteValidate(ref listOfErrors);
			}
		}
	}
}
