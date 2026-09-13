using System;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class ActionButtonPanel : UIComponent
{
	public enum ActionType
	{
		Cancel,
		Ok,
		Scrap
	}

	public delegate void ActionButtonHandler(ActionType action);

	private Box brown;

	private TextButton bt1;

	private TextButton bt2;

	private TextButton bt3;

	private Image scratch1;

	private Image scratch2;

	private ActionType action1;

	private ActionType action2;

	private ActionType action3;

	private const int buttonX = 2;

	protected const int defaultHeight = 40;

	public override int Width
	{
		get
		{
			return base.Width;
		}
		set
		{
			brown.Width = value;
			base.Width = value;
		}
	}

	public override int Height
	{
		get
		{
			return base.Height;
		}
		set
		{
			brown.Height = value;
			base.Height = value;
		}
	}

	public event ActionButtonHandler ActionButtonEvent;

	public ActionButtonPanel(GUIManager guiManager)
		: base(guiManager)
	{
		base.CanHaveFocus = false;
		brown = new Box(guiManager);
		bt1 = new TextButton(guiManager);
		bt2 = new TextButton(guiManager);
		bt3 = new TextButton(guiManager);
		scratch1 = new Image(guiManager);
		scratch2 = new Image(guiManager);
		Add(brown);
		Add(scratch1);
		Add(scratch2);
		Add(bt1);
		Add(bt2);
		Add(bt3);
	}

	public void Init()
	{
		Height = 40;
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_darkblue");
		brown.SetSkinLocation(SkinState.Normal, sourceRectangle);
		brown.CornerSize = 3;
		brown.Height = 40;
		sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scratch1");
		scratch1.SetSkinLocation(SkinState.Normal, sourceRectangle);
		scratch1.Position = new Point(brown.X - 4, brown.Y - 9);
		scratch1.ResizeControlToFitImage();
		scratch1.CanHaveFocus = false;
		sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scratch2");
		scratch1.SetSkinLocation(SkinState.Normal, sourceRectangle);
		scratch1.Position = new Point(brown.X - 10, brown.Y + 22);
		scratch1.ResizeControlToFitImage();
		scratch1.CanHaveFocus = false;
		int num = -2;
		bt1.Init(TextButton.TextButtonType.White);
		bt1.Position = new Point(2, brown.Y);
		bt1.Click += bt1_Click;
		bt2.Init(TextButton.TextButtonType.White);
		bt2.Position = new Point(bt1.X + bt1.Width + num, brown.Y);
		bt2.Click += bt2_Click;
		bt3.Init(TextButton.TextButtonType.White);
		bt3.Position = new Point(bt2.X + bt2.Width + num, brown.Y);
		bt3.Click += bt3_Click;
	}

	public void SpreadButtons()
	{
		if (!Contains(bt3))
		{
			bt2.X = Width - bt2.Width - 2;
		}
	}

	public void ClearCommands()
	{
		Remove(bt1);
		Remove(bt2);
		Remove(bt3);
	}

	public void SetCommands(string text1, string tooltip1)
	{
		Remove(bt2);
		Remove(bt3);
		SetCommand1(text1, tooltip1);
	}

	public void SetCommands(string text1, string tooltip1, string text2, string tooltip2)
	{
		Remove(bt3);
		SetCommand1(text1, tooltip1);
		SetCommand2(text2, tooltip2);
	}

	public void SetCommands(string text1, string tooltip1, string text2, string tooltip2, string text3, string tooltip3)
	{
		SetCommand1(text1, tooltip1);
		SetCommand2(text2, tooltip2);
		SetCommand3(text3, tooltip3);
	}

	private void SetCommand1(string text1, string tooltip1)
	{
		Add(bt1);
		bt1.Text = text1;
		bt1.ToolTip = tooltip1;
	}

	private void SetCommand2(string text1, string tooltip1)
	{
		Add(bt2);
		bt2.Text = text1;
		bt2.ToolTip = tooltip1;
	}

	private void SetCommand3(string text1, string tooltip1)
	{
		Add(bt3);
		bt3.Text = text1;
		bt3.ToolTip = tooltip1;
	}

	private void bt3_Click(UIComponent sender, EventArgs e)
	{
		if (this.ActionButtonEvent != null)
		{
			this.ActionButtonEvent(action3);
		}
	}

	private void bt2_Click(UIComponent sender, EventArgs e)
	{
		if (this.ActionButtonEvent != null)
		{
			this.ActionButtonEvent(action2);
		}
	}

	private void bt1_Click(UIComponent sender, EventArgs e)
	{
		if (this.ActionButtonEvent != null)
		{
			this.ActionButtonEvent(action1);
		}
	}
}
