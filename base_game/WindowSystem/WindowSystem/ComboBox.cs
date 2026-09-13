using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class ComboBox : UIComponent
{
	private static int defaultWidth = 200;

	private static int defaultHeight = 20;

	private static Rectangle defaultButtonSkin = new Rectangle(138, 5, 20, 20);

	private static Rectangle defaultButtonHoverSkin = new Rectangle(159, 5, 20, 20);

	private static Rectangle defaultButtonPressedSkin = new Rectangle(180, 5, 20, 20);

	private TextBox textBox;

	private TextButton headerbox;

	private ImageButton button;

	private ListBox listBox;

	private bool isListBoxOpen;

	private Dictionary<object, Label> entriesByKey;

	private object selectedKey;

	private ComboBoxTypes type;

	public static int DefaultWidth
	{
		set
		{
			defaultWidth = value;
		}
	}

	public static int DefaultHeight
	{
		set
		{
			defaultHeight = value;
		}
	}

	public static Rectangle DefaultButtonSkin
	{
		set
		{
			defaultButtonSkin = value;
		}
	}

	public static Rectangle DefaultButtonHoverSkin
	{
		set
		{
			defaultButtonHoverSkin = value;
		}
	}

	public static Rectangle DefaultButtonPressedSkin
	{
		set
		{
			defaultButtonPressedSkin = value;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return listBox.SelectedIndex;
		}
		set
		{
			listBox.SelectedIndex = value;
			if (listBox.SelectedIndex == -1)
			{
				if (textBox != null)
				{
					textBox.Text = "";
				}
				else if (headerbox != null)
				{
					headerbox.Text = "";
				}
				selectedKey = null;
			}
			else
			{
				UpdateSelectedKey();
			}
		}
	}

	public object SelectedKey
	{
		get
		{
			return selectedKey;
		}
		set
		{
			selectedKey = value;
			Label selectedText = entriesByKey[value];
			listBox.SelectedText = selectedText;
		}
	}

	public Dictionary<object, Label> EntriesByKey => entriesByKey;

	public Rectangle ButtonSkin
	{
		set
		{
			button.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle ButtonHoverSkin
	{
		set
		{
			button.SetSkinLocation(1, value);
		}
	}

	public Rectangle ButtonPressedSkin
	{
		set
		{
			button.SetSkinLocation(2, value);
		}
	}

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (base.Enabled != value)
			{
				base.Enabled = value;
				button.Enabled = value;
				GetTextBoxOrButton().Enabled = value;
				if (!value)
				{
					GetTextBoxOrButton().DebugTag = "disabledcombo";
				}
				else
				{
					GetTextBoxOrButton().DebugTag = "";
				}
			}
		}
	}

	public int Count => listBox.Count;

	public override string ToolTip
	{
		get
		{
			if (textBox != null)
			{
				return textBox.ToolTip;
			}
			if (headerbox != null)
			{
				return headerbox.ToolTip;
			}
			return null;
		}
		set
		{
			if (textBox != null)
			{
				textBox.ToolTip = value;
			}
			else if (headerbox != null)
			{
				headerbox.ToolTip = value;
			}
		}
	}

	public event SelectionChangedHandler SelectionChanged;

	private void UpdateSelectedKey()
	{
		Label selectedText = listBox.SelectedText;
		foreach (KeyValuePair<object, Label> item in entriesByKey)
		{
			if (item.Value == selectedText)
			{
				selectedKey = item.Key;
				break;
			}
		}
	}

	public ComboBox(GUIManager guiManager, ListBoxType type, bool isEditable)
		: base(guiManager)
	{
		isListBoxOpen = false;
		entriesByKey = new Dictionary<object, Label>();
		if (isEditable)
		{
			textBox = new TextBox(guiManager);
		}
		else
		{
			headerbox = new TextButton(guiManager);
			headerbox.DebugTag = "headerBox";
		}
		button = new ImageButton(guiManager);
		listBox = new ListBox(guiManager, type);
		listBox.DebugTag = "combolist";
		listBox.SetDebugTagOnScrollbar("comboListScrollbar");
		GetTextBoxOrButton().Move += HeaderButton_Move;
		Add(GetTextBoxOrButton());
		Add(button);
		listBox.CanGrowInHeight = true;
		listBox.ScrollBarEnabled = false;
		Width = defaultWidth;
		Height = defaultHeight;
		ButtonSkin = defaultButtonSkin;
		ButtonHoverSkin = defaultButtonHoverSkin;
		ButtonPressedSkin = defaultButtonPressedSkin;
		button.Click += headerbox_Click;
		button.LoseFocus += OnButtonLoseFocus;
		listBox.SelectedChanged += OnSelectionChanged;
		listBox.SelectedSame += listBox_SelectedSame;
		listBox.LoseFocus += OnListBoxLoseFocus;
		listBox.MouseSelected += listBox_MouseSelected;
	}

	private void HeaderButton_Move(UIComponent sender)
	{
		if (isListBoxOpen)
		{
			SetListboxDimensions();
		}
	}

	private void listBox_SelectedSame(UIComponent sender)
	{
	}

	private void listBox_MouseSelected(UIComponent obj)
	{
		CloseListBox(resetHeaderBox: true);
	}

	private UIComponent GetTextBoxOrButton()
	{
		if (textBox != null)
		{
			return textBox;
		}
		if (headerbox != null)
		{
			return headerbox;
		}
		return null;
	}

	public void Init(ComboBoxTypes type)
	{
		this.type = type;
		switch (type)
		{
		case ComboBoxTypes.LCD:
			if (textBox != null)
			{
				textBox.Init(TextBox.TextBoxType.LCDCombo);
				button.Init(ImageButtonType.LCDArrowDownNew);
				Height = textBox.Height;
			}
			else
			{
				headerbox.Init(TextButton.TextButtonType.LCDCombo);
				headerbox.Click += headerbox_Click;
				headerbox.LoseFocus += OnButtonLoseFocus;
				Remove(button);
				Height = headerbox.Height;
			}
			RenderType = RenderType.CRTAndLCD;
			listBox.Init(Label.LabelType.LCDComboBoxItem);
			break;
		case ComboBoxTypes.Default:
			Width = defaultWidth;
			Height = defaultHeight;
			ButtonSkin = defaultButtonSkin;
			ButtonHoverSkin = defaultButtonHoverSkin;
			ButtonPressedSkin = defaultButtonPressedSkin;
			break;
		}
	}

	public override void CleanUp()
	{
		listBox.CleanUp();
		base.CleanUp();
	}

	public void BeginAddingEntries()
	{
		listBox.BeginAddingEntries();
	}

	public void EndAddingEntries()
	{
		listBox.EndAddingEntries();
	}

	public void AddEntry(string text)
	{
		Label value = listBox.AddEntry(text);
		entriesByKey.Add(text, value);
	}

	public void AddEntry(object key, string text)
	{
		Label value = listBox.AddEntry(text);
		entriesByKey.Add(key, value);
	}

	public void Clear()
	{
		listBox.Clear();
		entriesByKey.Clear();
		selectedKey = null;
		SetText("");
	}

	protected void CloseListBox(bool resetHeaderBox)
	{
		if (isListBoxOpen)
		{
			base.GUIManager.Remove(listBox);
			isListBoxOpen = false;
			if (resetHeaderBox && headerbox != null)
			{
				headerbox.IsChecked = false;
			}
		}
	}

	protected void OnSelectionChanged(UIComponent sender)
	{
		string selectedText = listBox.GetSelectedText();
		SetText(selectedText);
		UpdateSelectedKey();
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(this);
		}
	}

	private void SetText(string text)
	{
		if (text != null)
		{
			if (textBox != null)
			{
				textBox.Text = text;
			}
			if (headerbox != null)
			{
				headerbox.Text = text;
			}
		}
	}

	protected void OnButtonMouseOver(MouseEventArgs args)
	{
		if (!isListBoxOpen)
		{
			button.CurrentSkinState = SkinState.Hover;
		}
	}

	protected void OnButtonMouseOut(MouseEventArgs args)
	{
		if (!isListBoxOpen)
		{
			button.CurrentSkinState = SkinState.Normal;
		}
	}

	private void headerbox_Click(UIComponent sender, EventArgs e)
	{
		if (!isListBoxOpen && listBox.Count > 0)
		{
			SetListboxDimensions();
			listBox.Level = base.Level + 1;
			base.GUIManager.Add(listBox);
			guiManager.SetFocus(listBox);
			isListBoxOpen = true;
		}
		else
		{
			CloseListBox(resetHeaderBox: false);
		}
	}

	private void SetListboxDimensions()
	{
		if (type == ComboBoxTypes.LCD)
		{
			listBox.X = base.AbsolutePosition.X + 7;
			listBox.Y = base.AbsolutePosition.Y + Height - 7;
			listBox.Width = Width - 14;
		}
		else
		{
			listBox.X = base.AbsolutePosition.X;
			listBox.Y = base.AbsolutePosition.Y + Height - 1;
			listBox.Width = Width;
		}
	}

	protected void OnListBoxLoseFocus()
	{
		if (base.GUIManager.GetFocus() != headerbox)
		{
			CloseListBox(resetHeaderBox: true);
		}
	}

	protected void OnButtonLoseFocus()
	{
		if (base.GUIManager.GetFocus() != listBox)
		{
			CloseListBox(resetHeaderBox: true);
		}
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (type != ComboBoxTypes.LCD)
		{
			button.Width = Height;
			button.Height = Height;
			button.X = Width - button.Width;
			textBox.Width = Width - button.Width;
			textBox.Height = Height;
		}
		else
		{
			GetTextBoxOrButton().Width = Width;
			GetTextBoxOrButton().Height = Height;
		}
	}
}
