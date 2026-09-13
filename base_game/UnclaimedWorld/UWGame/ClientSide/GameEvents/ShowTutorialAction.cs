using UWGame.ClientSide.HelpTopics;
using UWGame.SimSide;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents;

public class ShowTutorialAction : EventActionType
{
	public string TutorialPageKey;

	public ShowTutorialAction(string keyName)
		: base(keyName)
	{
	}

	public ShowTutorialAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		HelpTopic key = GameData.Instance.AllTutorialTopics[TutorialPageKey];
		The.InGameUI.HelpTopicDialogs[key].ShowInScreenSpace(0, 200);
		return true;
	}
}
