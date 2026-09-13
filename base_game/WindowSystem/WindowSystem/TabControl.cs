using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class TabControl : UIComponent
{
	private RadioGroup rgAccessButtons;

	public List<TabPage> TabPages = new List<TabPage>();

	private Grid grdSurface;

	private const int accessHeight = 36;

	private Box headerBanner;

	private Dictionary<TabPage, ICanBeChecked> accessButtons = new Dictionary<TabPage, ICanBeChecked>();

	public TabPage DisplayedTabPage { get; private set; }

	public event Action<TabPage> NewPageSelected;

	public TabControl(GUIManager guiManager, UIComponent surface, int bottomMargin = 0)
		: base(guiManager)
	{
		DebugTag = "tabControl";
		headerBanner = new Box(guiManager);
		headerBanner.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("basic_header_big"));
		headerBanner.Height = 36;
		headerBanner.Width = surface.Width;
		headerBanner.CornerSize = 18;
		Add(headerBanner);
		rgAccessButtons = new RadioGroup(guiManager);
		headerBanner.Add(rgAccessButtons);
		rgAccessButtons.ButtonMargin = 6;
		rgAccessButtons.DebugTag = "rgAccessButtons";
		rgAccessButtons.NewMemberChecked += rgAccessButtons_NewMemberChecked;
		grdSurface = new Grid(guiManager, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grdSurface.FixedItemHeights = false;
		grdSurface.RenderType = RenderType.Normal;
		Add(grdSurface);
		grdSurface.Font = GUIManager.LCDandHUDBodyFontPath;
		grdSurface.Width = surface.Width;
		grdSurface.CanHaveFocus = false;
		Width = surface.Width;
		Height = surface.Height - bottomMargin;
		surface.Add(this);
		SetHeight();
	}

	private void rgAccessButtons_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
	{
		TabPage tabPage = (TabPage)((UIComponent)arg1).Tag1;
		SelectTab(tabPage);
		if (this.NewPageSelected != null)
		{
			this.NewPageSelected(tabPage);
		}
	}

	private void SetHeight()
	{
		grdSurface.Position = new Point(0, headerBanner.Bottom + 6);
		grdSurface.Height = Height - grdSurface.Y;
	}

	public void SelectTab(TabPage tabItem)
	{
		if (DisplayedTabPage != null)
		{
			grdSurface.RemoveEntry(DisplayedTabPage);
		}
		DisplayedTabPage = tabItem;
		grdSurface.AddEntry(tabItem, tabItem);
		rgAccessButtons.SelectMember(accessButtons[tabItem]);
	}

	public void AddTabPage(TabPage tabPage, string caption, string tooltip)
	{
		TabPages.Add(tabPage);
		tabPage.Width = grdSurface.Width;
		RadioButton radioButton = new RadioButton(guiManager);
		radioButton.Init(CheckBoxType.LCDRadio);
		radioButton.Text = caption;
		radioButton.ToolTip = tooltip;
		radioButton.FitToText();
		radioButton.DebugTag = "rbAccess";
		radioButton.Tag1 = tabPage;
		rgAccessButtons.Add(radioButton, addAsControl: true, setHorizPosition: true);
		rgAccessButtons.Height = radioButton.Height;
		rgAccessButtons.Width = radioButton.Right;
		headerBanner.CenterChildVertically(rgAccessButtons);
		accessButtons.Add(tabPage, radioButton);
		if (DisplayedTabPage == null)
		{
			SelectTab(tabPage);
		}
		SetHeight();
	}

	public TabPage CreateTabItem(string caption, string tooltip)
	{
		TabPage tabPage = new TabPage(guiManager);
		AddTabPage(tabPage, caption, tooltip);
		return tabPage;
	}

	public void AddTabItem(string caption, string tooltip, TabPage tabItem)
	{
		RadioButton radioButton = new RadioButton(guiManager);
		radioButton.Init(CheckBoxType.LCD);
		radioButton.Text = caption;
		radioButton.ToolTip = tooltip;
		rgAccessButtons.Add(radioButton, addAsControl: true, setHorizPosition: true);
		rgAccessButtons.Height = radioButton.Height;
		rgAccessButtons.Width = radioButton.Right;
	}
}
