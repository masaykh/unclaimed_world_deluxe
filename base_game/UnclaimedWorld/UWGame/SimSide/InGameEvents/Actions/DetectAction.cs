using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.InGameEvents.Actions;

public class DetectAction : EventActionType
{
	public TargetObject TargetObject;

	public string EntityName;

	public string DetectorAllegianceKey;

	public EvalNode DynamicDetectorAllegianceKey;

	public DetectAction(string keyName)
		: base(keyName)
	{
	}

	public DetectAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		if (!EventActionType.GetEntity(EntityName, TargetObject, action, out entity, ref failReason))
		{
			return false;
		}
		if (!AllegianceAndExpedition.ResolveAllegiance(DynamicDetectorAllegianceKey, DetectorAllegianceKey, action, out var allegiance, ref failReason))
		{
			return false;
		}
		allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity, testForUsesMemory: true, suppressClientFeedback: true);
		Point playSiteMapPosition = entity.PlaySiteMapPosition;
		if (!The.Map.GetTile(playSiteMapPosition).AllegiancesThatSeeThisTile.Contains(allegiance))
		{
			allegiance.SharedKnowledge.UnSeeEntity(entity);
		}
		return true;
	}

	public override string ToString()
	{
		return ("Detect : " + EntityName) ?? "";
	}
}
