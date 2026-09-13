using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class MessageBox : Dialog
{
	private static Rectangle defaultInfoSkin = new Rectangle(1, 91, 25, 25);

	private static Rectangle defaultErrorSkin = new Rectangle(27, 91, 25, 25);

	private static Rectangle defaultWarningSkin = new Rectangle(53, 91, 25, 25);

	private static Rectangle defaultQuestionSkin = new Rectangle(79, 91, 25, 25);

	private const int LargeSeperation = 10;

	private const int SmallSeperation = 5;

	private Rectangle infoSkin;

	private Rectangle errorSkin;

	private Rectangle warningSkin;

	private Rectangle questionSkin;

	private Icon icon;

	private Label message;

	private MessageBoxButtons buttons;

	private List<TextButton> buttonList;

	private MessageBoxType type;

	public static Rectangle DefaultInfoSkin
	{
		set
		{
			defaultInfoSkin = value;
		}
	}

	public static Rectangle DefaultErrorSkin
	{
		set
		{
			defaultErrorSkin = value;
		}
	}

	public static Rectangle DefaultWarningSkin
	{
		set
		{
			defaultWarningSkin = value;
		}
	}

	public static Rectangle DefaultQuestionSkin
	{
		set
		{
			defaultQuestionSkin = value;
		}
	}

	public Rectangle InfoSkin
	{
		set
		{
			defaultInfoSkin = value;
			ArrangeWindow(type);
		}
	}

	public Rectangle ErrorSkin
	{
		set
		{
			defaultErrorSkin = value;
			ArrangeWindow(type);
		}
	}

	public Rectangle WarningSkin
	{
		set
		{
			defaultWarningSkin = value;
			ArrangeWindow(type);
		}
	}

	public Rectangle QuestionSkin
	{
		set
		{
			questionSkin = value;
			ArrangeWindow(type);
		}
	}

	public MessageBox(GUIManager guiManager, string message, string title, MessageBoxButtons buttons, MessageBoxType type)
		: base(guiManager)
	{
		buttonList = new List<TextButton>();
		icon = new Icon(guiManager);
		this.message = new Label(guiManager);
		this.buttons = buttons;
		if (this.buttons == MessageBoxButtons.OK || this.buttons == MessageBoxButtons.Yes_No)
		{
			base.HasCloseButton = false;
		}
		infoSkin = defaultInfoSkin;
		errorSkin = defaultErrorSkin;
		warningSkin = defaultWarningSkin;
		questionSkin = defaultQuestionSkin;
		this.type = type;
		Add(this.message);
		base.TitleText = title;
		this.message.Text = message;
		int num = 0;
		if (this.buttons == MessageBoxButtons.OK)
		{
			num = 1;
		}
		else if (this.buttons == MessageBoxButtons.OK_Cancel || this.buttons == MessageBoxButtons.Yes_No)
		{
			num = 2;
		}
		else if (this.buttons == MessageBoxButtons.Yes_No_Cancel)
		{
			num = 3;
		}
		for (int i = 0; i < num; i++)
		{
			TextButton textButton = new TextButton(guiManager);
			buttonList.Add(textButton);
			Add(textButton);
			switch (i)
			{
			case 0:
				if (this.buttons == MessageBoxButtons.OK || this.buttons == MessageBoxButtons.OK_Cancel)
				{
					textButton.Text = "OK";
				}
				else
				{
					textButton.Text = "Yes";
				}
				break;
			case 1:
				if (this.buttons == MessageBoxButtons.OK_Cancel)
				{
					textButton.Text = "Cancel";
				}
				else
				{
					textButton.Text = "No";
				}
				break;
			default:
				textButton.Text = "Cancel";
				break;
			}
			textButton.Click += OnClick;
		}
		ArrangeWindow(type);
	}

	private void ArrangeWindow(MessageBoxType type)
	{
		bool flag = true;
		switch (type)
		{
		case MessageBoxType.Info:
			icon.SetSkinLocation(SkinState.Normal, infoSkin);
			break;
		case MessageBoxType.Error:
			icon.SetSkinLocation(SkinState.Normal, errorSkin);
			break;
		case MessageBoxType.Warning:
			icon.SetSkinLocation(SkinState.Normal, warningSkin);
			break;
		case MessageBoxType.Question:
			icon.SetSkinLocation(SkinState.Normal, questionSkin);
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			Add(icon);
			icon.X = 10;
			icon.Y = 10;
			icon.ResizeControlToFitImage();
			icon.CanHaveFocus = false;
			message.X = icon.X + icon.Width + 10;
		}
		else
		{
			message.X = 10;
		}
		message.Y = 10;
		message.Width = message.TextWidth;
		message.Height = message.TextHeight;
		base.Resizable = false;
		base.ClientWidth = message.X + message.Width + 10;
		int num = 0;
		int num2 = 0;
		foreach (TextButton button in buttonList)
		{
			button.Y = message.Y + message.Height + 20;
			num += button.Width;
			if (num2 != buttonList.Count - 1)
			{
				num += 5;
			}
			num2++;
		}
		if (base.ClientWidth < num + 20)
		{
			base.ClientWidth = num + 20;
		}
		int num3 = (base.ClientWidth - num) / 2;
		foreach (TextButton button2 in buttonList)
		{
			button2.X = num3;
			num3 += button2.Width + 5;
		}
		base.ClientHeight = buttonList[0].Y + buttonList[0].Height + 10;
		CenterWindow();
	}

	public override void CleanUp()
	{
		icon.CleanUp();
		base.CleanUp();
	}

	private void OnClick(UIComponent sender, EventArgs e)
	{
		for (int i = 0; i < buttonList.Count; i++)
		{
			if (sender != buttonList[i])
			{
				continue;
			}
			switch (i)
			{
			case 0:
				if (buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OK_Cancel)
				{
					SetDialogResult(DialogResult.OK);
				}
				else
				{
					SetDialogResult(DialogResult.Yes);
				}
				break;
			case 1:
				if (buttons != MessageBoxButtons.OK_Cancel)
				{
					SetDialogResult(DialogResult.No);
				}
				break;
			}
			break;
		}
		Hide();
	}
}
