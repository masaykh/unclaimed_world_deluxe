using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class TabButtonPanel
{
	public delegate void TabButtonHandler(object buttonArgument);

	public enum Layout
	{
		Horizontal,
		Vertical
	}

	public bool Visible = true;

	public Window tabPanel;

	private Window tabPanelEdge;

	private RadioGroup buttonGroup;

	private GUIManager gui;

	public const int verticalWidth = 94;

	public const int verticalOverlap = 16;

	private const int buttonYSpacing = 59;

	private Point pos;

	private const int buttonXSpacing = 69;

	private const int metalPosX = 12;

	private const int metalPosY = 12;

	private const int markingsTop = 10;

	private const int markingsLeft = 6;

	private Image imBottomDirt;

	private Box metal;

	private Dictionary<TabButtonContainer, object> tabButtons = new Dictionary<TabButtonContainer, object>();

	private Dictionary<ImageButton, TabButtonContainer> tabButtonContainers = new Dictionary<ImageButton, TabButtonContainer>();

	private List<TabButtonContainer> listOfTabButtons = new List<TabButtonContainer>();

	private Layout layout;

	public int X
	{
		get
		{
			return tabPanel.X;
		}
		set
		{
			tabPanel.X = value;
			PlacePanelEdge();
		}
	}

	public int Y
	{
		get
		{
			return tabPanel.Y;
		}
		set
		{
			tabPanel.Y = value;
			PlacePanelEdge();
		}
	}

	public event TabButtonHandler TabButtonEvent;

	public TabButtonPanel(GUIManager gui, Point pos, Layout layout, Window formToLevelWith)
	{
		this.gui = gui;
		this.layout = layout;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("tabsbase");
		tabPanel = new Window(gui);
		tabPanel.Skin = sourceRectangle;
		tabPanel.CornerSize = 15;
		Panel.AddSteelTexture(gui, tabPanel);
		Panel.AddDust(gui, tabPanel);
		tabPanel.Margin = 0;
		tabPanel.Position = pos;
		tabPanel.Show();
		tabPanel.Resizable = false;
		tabPanel.IsMovable = false;
		tabPanel.HasCloseButton = false;
		tabPanel.Level = formToLevelWith.Level;
		tabPanel.ZOrder = 0f;
		tabPanel.HasCRTOrLCDComponents = false;
		tabPanel.HasOverlayComponents = false;
		if (layout == Layout.Horizontal)
		{
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("tabribs_right");
			tabPanelEdge = new Window(gui);
			tabPanelEdge.Skin = sourceRectangle;
			tabPanelEdge.CornerSize = 2;
			tabPanelEdge.Margin = 0;
			tabPanelEdge.Position = new Point(tabPanel.X + tabPanel.Width, tabPanel.Y);
			tabPanelEdge.WindowSize = new Vector2(sourceRectangle.Width, sourceRectangle.Height);
			tabPanelEdge.Show();
			tabPanelEdge.Resizable = false;
			tabPanelEdge.IsMovable = false;
			tabPanelEdge.HasCloseButton = false;
			tabPanelEdge.Level = tabPanel.Level;
			tabPanelEdge.ZOrder = 0f;
			tabPanelEdge.HasCRTOrLCDComponents = false;
			tabPanelEdge.HasOverlayComponents = false;
			ExpandedPanel.AddDirtOnLeftEdge(gui, tabPanelEdge);
			ExpandedPanel.AddDirtOnBottomEdge(gui, tabPanelEdge);
		}
		if (layout == Layout.Horizontal)
		{
			metal = Panel.AddMetalPlate(gui, tabPanel, new Point(12, 12), new Point(20, 20));
		}
		else
		{
			metal = Panel.AddMetalPlate(gui, tabPanel, new Point(12, 12), new Point(20, 20));
		}
		ExpandedPanel.AddDirtOnLeftEdge(gui, tabPanel);
		imBottomDirt = ExpandedPanel.AddDirtOnBottomEdge(gui, tabPanel);
		SetSize();
		buttonGroup = new RadioGroup(gui);
		tabPanel.Add(buttonGroup);
	}

	public void SetSize()
	{
		if (layout == Layout.Horizontal)
		{
			int num = 36 + listOfTabButtons.Count * 69;
			int num2 = 65;
			int num3 = num2 + 24 - 2;
			tabPanel.WindowSize = new Vector2(num, num3);
			metal.Width = num - 24;
			metal.Height = num2;
			PlacePanelEdge();
		}
		else
		{
			int num4 = 44 + tabButtons.Count * 59 - 12;
			tabPanel.WindowSize = new Vector2(110f, num4);
			metal.Width = 81;
			metal.Height = num4 - 24;
			if (imBottomDirt != null)
			{
				imBottomDirt.Y = 260;
			}
		}
	}

	private void PlacePanelEdge()
	{
		if (tabPanelEdge != null)
		{
			tabPanelEdge.Height = tabPanel.Height;
			tabPanelEdge.X = tabPanel.X + tabPanel.Width;
			tabPanelEdge.Y = tabPanel.Y;
		}
	}

	public List<TabButtonContainer> GetListOfButtons()
	{
		return listOfTabButtons;
	}

	public TabButtonContainer GetButton(int index)
	{
		return listOfTabButtons[index];
	}

	public object GetButtonArgument(TabButtonContainer button)
	{
		return tabButtons[button];
	}

	public TabButtonContainer GetButtonContainer(object buttonArgument)
	{
		foreach (KeyValuePair<TabButtonContainer, object> tabButton in tabButtons)
		{
			if (tabButton.Value == buttonArgument)
			{
				return tabButton.Key;
			}
		}
		return null;
	}

	public TabButtonContainer AddButton(string text, string toolTip, object buttonArgument, int markingsFlavour)
	{
		TabButtonContainer tabButtonContainer;
		if (layout == Layout.Horizontal)
		{
			tabButtonContainer = new TabButtonContainer();
			tabButtonContainer.AddHorizontalMetalPanelButton(gui, tabPanel, 18 + tabButtons.Count * 69, 22, tabButtons.Count % 4 + 1, markingsFlavour, text);
		}
		else
		{
			tabButtonContainer = new TabButtonContainer();
			tabButtonContainer.AddHorizontalMetalPanelButton(gui, tabPanel, 18, 22 + tabButtons.Count * 59, tabButtons.Count % 4 + 1, markingsFlavour, text);
		}
		tabButtonContainer.button.Click += bt_Click;
		buttonGroup.Add(tabButtonContainer.button);
		tabButtons.Add(tabButtonContainer, buttonArgument);
		listOfTabButtons.Add(tabButtonContainer);
		tabButtonContainers.Add(tabButtonContainer.button, tabButtonContainer);
		SetSize();
		return tabButtonContainer;
	}

	public void Clear()
	{
		foreach (TabButtonContainer listOfTabButton in listOfTabButtons)
		{
			tabPanel.Remove(listOfTabButton.button);
			tabPanel.Remove(listOfTabButton.label);
			tabPanel.Remove(listOfTabButton.markings);
		}
		tabButtons.Clear();
		buttonGroup.Clear();
		listOfTabButtons.Clear();
		tabButtonContainers.Clear();
	}

	private void bt_Click(UIComponent sender, EventArgs e)
	{
		if (this.TabButtonEvent != null)
		{
			object buttonArgument = tabButtons[tabButtonContainers[(ImageButton)sender]];
			this.TabButtonEvent(buttonArgument);
		}
	}

	public void Hide()
	{
		tabPanel.Hide();
		if (tabPanelEdge != null)
		{
			tabPanelEdge.Hide();
		}
	}

	public void Show()
	{
		if (Visible)
		{
			tabPanel.Show();
			if (tabPanelEdge != null)
			{
				tabPanelEdge.Show();
			}
		}
	}
}
