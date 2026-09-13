namespace UWGame.ClientSide.Interface;

public class SidePanelEmpty : RosterPanel
{
	public SidePanelEmpty()
		: base(The.InGameUI.sidePanelHeight, isInfoPanel: true)
	{
		HasStatusCRT = false;
	}
}
