using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy;

public class PolicyPanel : RosterPanel
{
	private TiersPage tiersPage;

	private WeaponsPage weaponsPage;

	private TabControl tab;

	public PolicyPanel()
		: base("POLICY", 622, needBottomMarginForButtons: false)
	{
		GUIManager gui = Interface.gui;
		tab = new TabControl(gui, lcdSurface);
		tiersPage = new TiersPage(tab);
		weaponsPage = new WeaponsPage(tab);
		tab.NewPageSelected += tab_NewPageSelected;
	}

	private void tab_NewPageSelected(TabPage obj)
	{
		((TabPagePanel)obj).Refresh();
	}

	public new void Show()
	{
		((TabPagePanel)tab.DisplayedTabPage).Refresh();
	}

	public override void Refresh()
	{
		((TabPagePanel)tab.DisplayedTabPage).Refresh();
		base.Refresh();
	}
}
