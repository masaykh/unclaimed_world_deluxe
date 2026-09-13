using System.Xml.Serialization;
using UWGame.SimSide.Commands;

namespace UWGame.Control.Commands;

[XmlInclude(typeof(HuntArea))]
[XmlInclude(typeof(Build))]
[XmlInclude(typeof(SetProduction))]
[XmlInclude(typeof(Hunt))]
[XmlInclude(typeof(Salvage))]
[XmlInclude(typeof(PlaceExpedition))]
[XmlInclude(typeof(Discard))]
[XmlInclude(typeof(Claim))]
[XmlInclude(typeof(Scout))]
[XmlInclude(typeof(CancelJob))]
[XmlInclude(typeof(Examine))]
[XmlInclude(typeof(PatrolArea))]
[XmlInclude(typeof(SetTaskPriority))]
[XmlInclude(typeof(SetJobTypePriority))]
[XmlInclude(typeof(CreateMission))]
[XmlInclude(typeof(CreateMissionTemplate))]
[XmlInclude(typeof(DeleteZone))]
[XmlInclude(typeof(Gather))]
[XmlInclude(typeof(CreateStockpile))]
[XmlInclude(typeof(SpecialAction))]
[XmlInclude(typeof(Pause))]
[XmlInclude(typeof(Resume))]
[XmlInclude(typeof(SetGameSpeed))]
[XmlInclude(typeof(AdoptTierPolicy))]
[XmlInclude(typeof(SetUpgrade))]
[XmlInclude(typeof(SetStandingOrder))]
[XmlInclude(typeof(SetStandingOrderGatherInZone))]
[XmlInclude(typeof(SetStandingOrderHuntInZone))]
[XmlInclude(typeof(AttackArea))]
[XmlInclude(typeof(AttackAreaUpdateJob))]
[XmlInclude(typeof(PatrolAreaUpdateJob))]
[XmlInclude(typeof(AllowAmmoForVermin))]
public abstract class Command
{
	public int frameCalled;

	public abstract void Execute(bool giveClientFeedback = true);
}
