using System;
using System.Collections.Generic;
using System.Linq;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class SortingButtons<T> : RadioGroup where T : struct, IComparable, IFormattable, IConvertible
{
	private Dictionary<T, ICanBeChecked> buttons = new Dictionary<T, ICanBeChecked>();

	public SortingSettings<T> Settings;

	public event Action SortClicked;

	public SortingButtons(GUIManager gui)
		: base(gui)
	{
		Height = 50;
	}

	public void Fill(SortingSettings<T> settings)
	{
		Settings = settings;
		ICanBeChecked canBeChecked = buttons[settings.SortedBy];
		canBeChecked.IsChecked = true;
		SetSortOrderIcons(canBeChecked, settings.SortOrder);
	}

	public void EnableButton(T sortIndex, bool enable)
	{
		((UIComponent)buttons[sortIndex]).Enabled = enable;
	}

	public void AddTextButton(int width, string text, T sortIndex, string tooltip = null)
	{
		int x = 0;
		if (buttons.Count > 0)
		{
			x = ((UIComponent)buttons.Last().Value).Right;
		}
		CreateTextButton(x, width, text, sortIndex, tooltip);
	}

	public void CreateTextButton(int x, int width, string text, T sortIndex, string tooltip = null)
	{
		TextButton textButton = new TextButton(guiManager);
		textButton.Width = width;
		textButton.Init(TextButton.TextButtonType.LCDSortingArrows);
		textButton.CheckedMode = CheckedModes.CanBeChecked;
		textButton.Y = 0;
		textButton.Text = text;
		textButton.X = x;
		textButton.Width = width;
		textButton.Height = 30;
		textButton.OrderByTag1 = null;
		textButton.Click += tbSort_Click;
		InitButton(textButton, text, tooltip);
		textButton.Tag1 = sortIndex;
		buttons.Add(sortIndex, textButton);
	}

	private void InitButton(ICanBeChecked button, string text, string tooltip)
	{
		string text2 = ((tooltip == null) ? text : tooltip);
		((UIComponent)button).ToolTip = "Sort by:" + text2;
		base.Add(button);
	}

	public void CreateImageButton(int x, int width, string text, T sortIndex)
	{
		ImageButton imageButton = new ImageButton(guiManager);
		imageButton.Width = width;
		imageButton.Init(ImageButtonType.LCDSortingArrows);
		imageButton.CheckedMode = CheckedModes.CanBeChecked;
		imageButton.Click += tbSort_Click;
		imageButton.Y = 0;
		imageButton.X = x;
		imageButton.Width = width;
		imageButton.OrderByTag1 = null;
		InitButton(imageButton, text, null);
		imageButton.Tag1 = sortIndex;
		buttons.Add(sortIndex, imageButton);
	}

	private void tbSort_Click(UIComponent sender, EventArgs e)
	{
		ICanBeChecked clickedTextButton = sender as ICanBeChecked;
		T val = (T)sender.Tag1;
		if (EqualityComparer<T>.Default.Equals(val, Settings.SortedBy))
		{
			if (Settings.SortOrder == Grid.Sorting.Descending)
			{
				Settings.SortOrder = Grid.Sorting.Ascending;
			}
			else if (Settings.SortOrder == Grid.Sorting.Ascending)
			{
				Settings.SortOrder = Grid.Sorting.Descending;
			}
		}
		else
		{
			Settings.SetDefaultSortOrder();
			Settings.SortedBy = val;
		}
		SetSortOrderIcons(clickedTextButton, Settings.SortOrder);
		if (this.SortClicked != null)
		{
			this.SortClicked();
		}
	}

	public void SetSortOrderIcons(ICanBeChecked clickedTextButton, Grid.Sorting order)
	{
		UpdateSortDirectionIcon((UIComponent)clickedTextButton, order, showDirection: true);
		foreach (UIComponent control in base.Controls)
		{
			if (control != clickedTextButton)
			{
				UpdateSortDirectionIcon(control, null, showDirection: false);
			}
		}
	}

	private void UpdateSortDirectionIcon(UIComponent button, Grid.Sorting? order, bool showDirection)
	{
		Icon icon = ((!(button is TextButton textButton)) ? ((button as ImageButton).Controls[0] as Icon) : (textButton.Controls[2] as Icon));
		if (showDirection)
		{
			icon.CurrentSkin = ((order.Value == Grid.Sorting.Ascending) ? 1 : 0);
			icon.Visible = true;
		}
		else
		{
			icon.Visible = false;
		}
	}
}
