using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class EntityPanel : ExpandedPanel
{
	public TabButtonPanel tabButtonPanel;

	private SkillsAndAttributes SkillsAndAttributes;

	private History History;

	private CombatHistory CombatHistory;

	private int? tabPanelY;

	public EntityPanelTabPage CurrentTab;

	private UIComponent lcdSurfaceFull;

	private UIComponent lcdSurfaceHalf;

	private EntityCRTContent entityCRTContent;

	private Window HalfPanel;

	private Window FullPanel;

	private Entity selectedEntity;

	private Label lblFullPanelTitle;

	private TabButtonPanel.Layout tabLayout;

	public Entity SelectedEntity
	{
		get
		{
			return selectedEntity;
		}
		set
		{
			selectedEntity = value;
			if (MainPanelIsVisible())
			{
				RefreshTabPanel();
			}
		}
	}

	public EntityPanel(CommonInterface intf, FramedCRT framedCRT, Point pos, Vector2 dimensions, Level level, bool includeCables, TabButtonPanel.Layout tabLayout, int? tabXPos, int? tabYPos)
		: base("", intf, pos, dimensions, level, includeCables)
	{
		HasCRT = true;
		this.tabLayout = tabLayout;
		entityCRTContent = new EntityCRTContent(intf.gui, this, framedCRT);
		int num = 0;
		_ = framedCRT.DisplayWindow.Height;
		HalfPanel.Hide();
		FullPanel = Window;
		lblFullPanelTitle.Width = 300;
		Window.Position = pos;
		Window.Width = (int)dimensions.X;
		Window.Height = (int)dimensions.Y;
		InitTabPanel(tabXPos, tabYPos);
		InitMainPanels();
		CurrentTab = SkillsAndAttributes;
	}

	private bool MainPanelIsVisible()
	{
		if (!HalfPanel.Visible)
		{
			return FullPanel.Visible;
		}
		return true;
	}

	private void ChangeTab(EntityPanelTabPage newTab)
	{
		if (CurrentTab != null && CurrentTab.tabPanel != null)
		{
			CurrentTab.tabPanel.Hide();
		}
		bool flag = false;
		if (CurrentTab != null && CurrentTab.HasCRT)
		{
			flag = true;
		}
		CurrentTab = newTab;
		if (CurrentTab.tabPanel != null)
		{
			FullPanel.Hide();
			HalfPanel.Hide();
			CurrentTab.tabPanel.Show();
		}
		else if (CurrentTab.HasCRT)
		{
			FullPanel.Hide();
			HalfPanel.Show();
			lcdSurfaceHalf.Controls.Clear();
			lcdSurfaceHalf.Add(CurrentTab.lcdContent);
		}
		else
		{
			FullPanel.Show();
			HalfPanel.Hide();
			lblFullPanelTitle.Text = CurrentTab.Title;
			lcdSurfaceFull.Controls.Clear();
			lcdSurfaceFull.Add(CurrentTab.lcdContent);
		}
		if (CurrentTab.HasCRT)
		{
			entityCRTContent.Show(!flag);
		}
		else
		{
			entityCRTContent.Hide();
		}
		if (tabButtonPanel.tabPanel.Visible)
		{
			tabButtonPanel.tabPanel.BringToTop();
		}
	}

	private void RefreshTabPanel()
	{
		tabButtonPanel.Clear();
		if (SelectedEntity != null)
		{
			if (SelectedEntity.Intelligence != null && SelectedEntity.Locomotor.LeggedLocomotor != null)
			{
				tabButtonPanel.AddButton("STATS", "", SkillsAndAttributes, 3);
			}
			if (SelectedEntity.PersonEntity != null)
			{
				tabButtonPanel.AddButton("HOUSEHOLD", "", History, 2);
			}
			if (SelectedEntity.BiologicalEntity != null)
			{
				tabButtonPanel.AddButton("HEALTH", "", History, 1);
			}
			if (SelectedEntity.Intelligence != null && SelectedEntity.EntityType.IntelligenceType.AttackTypes != null && SelectedEntity.EntityType.IntelligenceType.AttackTypes.Count > 0)
			{
				tabButtonPanel.AddButton("COMBAT", "", CombatHistory, 4);
			}
			if (SelectedEntity.Intelligence != null)
			{
				tabButtonPanel.AddButton("HISTORY", "", History, 4);
			}
			TabButtonContainer buttonContainer = tabButtonPanel.GetButtonContainer(CurrentTab);
			if (tabButtonPanel.GetListOfButtons().Count == 0)
			{
				tabButtonPanel.Hide();
				return;
			}
			tabButtonPanel.SetSize();
			SetVerticalTabPanelPosition();
			tabButtonPanel.Show();
			if (buttonContainer != null)
			{
				buttonContainer.button.IsChecked = true;
				return;
			}
			buttonContainer = tabButtonPanel.GetButton(0);
			buttonContainer.button.IsChecked = true;
			EntityPanelTabPage entityPanelTabPage = (EntityPanelTabPage)tabButtonPanel.GetButtonArgument(buttonContainer);
			if (CurrentTab != entityPanelTabPage)
			{
				ChangeTab(entityPanelTabPage);
			}
		}
		else
		{
			tabButtonPanel.Hide();
		}
	}

	private void InitMainPanels()
	{
		LCDScreen lcdScreen = null;
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, FullPanel, 16, new Point(16, 16 + TitleHeight), out var display, out lcdSurfaceFull, ref lcdScreen);
		ExpandedPanel.AddDirtOnEdges(Interface.gui, FullPanel);
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, HalfPanel, 16, new Point(16, 16 + TitleHeight), out display, out lcdSurfaceHalf, ref lcdScreen);
		ExpandedPanel.AddDirtOnEdges(Interface.gui, HalfPanel);
		SkillsAndAttributes = new SkillsAndAttributes(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);
		History = new History(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);
		CombatHistory = new CombatHistory(Interface.gui, this, lcdSurfaceFull, lcdSurfaceHalf);
	}

	private void SetVerticalTabPanelPosition()
	{
		int num = ((!tabPanelY.HasValue) ? HalfPanel.Y : tabPanelY.Value);
		if (num + tabButtonPanel.tabPanel.Height > HalfPanel.Y + HalfPanel.Height)
		{
			tabButtonPanel.tabPanel.Y = HalfPanel.Y + HalfPanel.Height - tabButtonPanel.tabPanel.Height;
		}
		else
		{
			tabButtonPanel.tabPanel.Y = num;
		}
	}

	private void InitTabPanel(int? tabXPos, int? tabYPos)
	{
		int num = 0;
		tabPanelY = tabYPos;
		tabButtonPanel = new TabButtonPanel(pos: new Point((!tabXPos.HasValue) ? (Window.X - 94 + num) : tabXPos.Value, HalfPanel.Y), gui: Interface.gui, layout: TabButtonPanel.Layout.Vertical, formToLevelWith: Window);
		Panel.AddImage(Interface.gui, tabButtonPanel.tabPanel, "basic_dirt_bigsplotch", new Point(80, 0));
		tabButtonPanel.TabButtonEvent += topPanel_TabButtonEvent;
		tabButtonPanel.Hide();
	}

	private void topPanel_TabButtonEvent(object buttonArgument)
	{
		ChangeTab((EntityPanelTabPage)buttonArgument);
		if (SelectedEntity != null)
		{
			entityCRTContent.Refresh(SelectedEntity);
			CurrentTab.Refresh();
		}
		else
		{
			CurrentTab.Clear();
		}
	}

	public override void Show()
	{
		base.Show();
		ChangeTab(CurrentTab);
		tabButtonPanel.Show();
		RefreshTabPanel();
	}

	public override void Hide()
	{
		tabButtonPanel.Hide();
		HalfPanel.Hide();
		FullPanel.Hide();
		base.Hide();
	}

	public override void Refresh()
	{
		if (SelectedEntity != null)
		{
			entityCRTContent.Refresh(SelectedEntity);
			CurrentTab.Refresh();
		}
		else
		{
			CurrentTab.Clear();
		}
		base.Refresh();
	}

	public override void DrawContent(Window sender, SpriteBatch formSpriteBatch)
	{
		_ = SelectedEntity;
	}
}
