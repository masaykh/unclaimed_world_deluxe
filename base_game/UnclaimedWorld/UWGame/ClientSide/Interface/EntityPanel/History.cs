using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class History : EntityPanelTabPage
{
	public History(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
		: base(gui, entityPanel, hasCRT: false, fullLCD, halfLCD)
	{
		Title = "HISTORY";
	}
}
