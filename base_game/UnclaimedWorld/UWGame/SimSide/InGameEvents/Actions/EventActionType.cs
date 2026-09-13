using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

[XmlInclude(typeof(SpawnEntityAction))]
[XmlInclude(typeof(DestroyEntityAction))]
[XmlInclude(typeof(SetPropertyAction))]
[XmlInclude(typeof(CreateExpeditionAction))]
[XmlInclude(typeof(ExploreAction))]
[XmlInclude(typeof(ChangeResourcesAction))]
[XmlInclude(typeof(SpawnWorldAction))]
[XmlInclude(typeof(SpawnSiteAction))]
[XmlInclude(typeof(SpawnRouteAction))]
[XmlInclude(typeof(SpawnStockpileAction))]
[XmlInclude(typeof(SpawnAllegianceAction))]
[XmlInclude(typeof(SpawnAllegianceRelationAction))]
[XmlInclude(typeof(SpawnTriggerAction))]
[XmlInclude(typeof(ChangeCreditsAction))]
[XmlInclude(typeof(CreateJobAction))]
[XmlInclude(typeof(CancelJobAction))]
[XmlInclude(typeof(ClaimEntityAction))]
[XmlInclude(typeof(AttackEntityAction))]
[XmlInclude(typeof(ProcessAction))]
[XmlInclude(typeof(DetectAction))]
[XmlInclude(typeof(EventActionDialog))]
[XmlInclude(typeof(TalkAction))]
[XmlInclude(typeof(WinGameAction))]
[XmlInclude(typeof(LoseGameAction))]
[XmlInclude(typeof(SoundEffectAction))]
[XmlInclude(typeof(MusicAction))]
[XmlInclude(typeof(LogAction))]
[XmlInclude(typeof(SetViewAction))]
[XmlInclude(typeof(ParticleEffectAction))]
[XmlInclude(typeof(ShowTutorialAction))]
public abstract class EventActionType : IGameData
{
	public string Comments;

	public double DelayInSeconds;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public virtual bool UsesTriggeringEntity => false;

	public EventActionType(string keyName)
	{
		KeyName = keyName;
	}

	public EventActionType()
	{
	}

	public abstract bool Execute(EventAction eventAction, ref string failReason);

	public virtual void ExtractNestedActionTypes(ref List<string> duplicateKeyErrors)
	{
		if (!GameData.Instance.AllEventActionTypes.ContainsKey(KeyName))
		{
			GameData.Instance.AllEventActionTypes.Add(KeyName, this);
		}
		else
		{
			Common.AddToList(ref duplicateKeyErrors, "Duplicate key: " + KeyName);
		}
	}

	public virtual void LoadContent(ContentManager content)
	{
	}

	public virtual void PreInitValidate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));
	}

	public virtual void PostLoadContentValidate(ref List<string> listOfErrors)
	{
	}

	public virtual void Initialize()
	{
	}

	public virtual void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public virtual void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public static bool GetEntity(string entityName, TargetObject targetObject, EventAction action, out Entity entity, ref string failReason)
	{
		return GetEntity(entityName, targetObject, action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget, out entity, ref failReason);
	}

	public static bool GetEntity(string entityName, TargetObject targetObject, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, out Entity entity, ref string failReason)
	{
		entity = null;
		if (entityName != null)
		{
			entity = TalkAction.GetEntityByName(entityName);
			if (entity == null)
			{
				failReason = "No entity with name '" + entityName + "' exists.";
				return false;
			}
		}
		else
		{
			List<IHasExposedProperties> result = targetObject.GetResult(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (result.Count == 0)
			{
				failReason = "Entity lookup did not give any results.";
				return false;
			}
			entity = (Entity)result[0];
		}
		return true;
	}
}
