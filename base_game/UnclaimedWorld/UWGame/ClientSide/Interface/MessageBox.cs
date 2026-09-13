using System;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MessageBox : Panel
{
	public enum ButtonOptions
	{
		OK,
		OKAndCancel,
		None
	}

	protected Box display;

	protected LCDScreen lcdScreen;

	protected UIComponent lcdSurface;

	private TextArea taContent;

	private TextButton btOK;

	private TextButton btCancel;

	public string Text
	{
		set
		{
			taContent.Text = value;
		}
	}

	public event EventHandler CancelClick;

	public event EventHandler OKClick;

	/// <summary>
	/// PORT: the size is a parameter now, defaulting to the studio's 330x250 so every existing
	/// call is unchanged. One message the port added - what modded content a save needs, against
	/// what this session has - is two lists and cannot be made to fit in that box: it came out
	/// clipped on both sides, which is worse than not showing it.
	/// </summary>
	public MessageBox(CommonInterface intf, string title = "MESSAGE", Vector2? size = null)
		: base(intf, title, new Point(420, 300), size ?? new Vector2(330f, 250f), Level.MessageBox, PanelType.RegularEdges)
	{
		int num = 6;
		btOK = new TextButton(Interface.gui);
		Window.Add(btOK);
		btOK.Init(TextButton.TextButtonType.White);
		btOK.Text = "OK";
		PlaceLeftButtonUnderLCD(btOK);
		btOK.Click += btOK_Click;
		btOK.ScaleWidthToFitText();
		btCancel = new TextButton(Interface.gui);
		Window.Add(btCancel);
		btCancel.Init(TextButton.TextButtonType.White);
		btCancel.Text = "CANCEL";
		btCancel.Click += btCancel_Click;
		btCancel.ScaleWidthToFitText();
		PlaceRightButtonUnderLCD(btCancel);
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, Window, 52, Panel.RosterMargin, out display, out lcdSurface, ref lcdScreen);
		taContent = new TextArea(Interface.gui, ListBoxType.LCD);
		taContent.RenderType = RenderType.CRTAndLCD;
		taContent.Init(Label.LabelType.LCDNormal);
		taContent.CanGrowInHeight = true;
		lcdSurface.Add(taContent);
		taContent.X = 6;
		taContent.Y = 0;
		taContent.Width = lcdSurface.Width - 12;
		AddDirtOnStraightEdges();
		Hide();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}

	private void btOK_Click(UIComponent sender, EventArgs e)
	{
		Hide();
		if (this.OKClick != null)
		{
			this.OKClick(this, null);
		}
	}

	public virtual void ShowMessage(string message, string title = null, bool modal = true, ButtonOptions buttonOptions = ButtonOptions.OK)
	{
		taContent.Text = message;
		switch (buttonOptions)
		{
		case ButtonOptions.OK:
			btOK.Visible = true;
			btCancel.Visible = false;
			break;
		case ButtonOptions.OKAndCancel:
			btOK.Visible = true;
			btCancel.Visible = true;
			break;
		case ButtonOptions.None:
			btOK.Visible = false;
			btCancel.Visible = false;
			break;
		}
		lblTitle.Text = title ?? "MESSAGE";
		base.ShowDialog(modal);
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
