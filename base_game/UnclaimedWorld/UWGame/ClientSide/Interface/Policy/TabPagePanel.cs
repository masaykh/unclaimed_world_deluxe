using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy;

public abstract class TabPagePanel : TabPage
{
	public TabPagePanel(TabControl parent)
		: base(parent.guiManager)
	{
	}

	public new virtual void Refresh()
	{
	}
}
