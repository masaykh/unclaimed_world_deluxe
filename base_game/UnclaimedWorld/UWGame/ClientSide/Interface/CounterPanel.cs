using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class CounterPanel : Panel
{
	private Label lblCredits;

	private Label lblMembers;

	public CounterPanel(int xPos)
		: base(The.InGameUI, null, new Point(xPos, 0), new Vector2(104f, 59f), Level.Middle, PanelType.Counters)
	{
		Window.Show();
		int y = 6;
		int num = 4;
		ImageButton imageButton = new ImageButton(Interface.gui);
		imageButton.InitWithIcon(ImageButtonType.Counter, "counter_button_creditsIcon", hasCheckedState: true);
		Window.Add(imageButton);
		imageButton.X = 7;
		imageButton.Y = y;
		imageButton.Enabled = false;
		ImageButton imageButton2 = new ImageButton(Interface.gui);
		imageButton2.InitWithIcon(ImageButtonType.Counter, "counter_button_popIcon", hasCheckedState: true);
		Window.Add(imageButton2);
		imageButton2.X = imageButton.Right + num;
		imageButton2.Y = y;
		imageButton2.Enabled = false;
		int y2 = 28;
		Label.LabelType type = Label.LabelType.PlainPanelNormal;
		lblCredits = new Label(Interface.gui);
		Window.Add(lblCredits);
		lblCredits.Init(type);
		lblCredits.FitToText();
		lblCredits.Y = y2;
		lblCredits.ToolTip = "Available trade credits";
		lblMembers = new Label(Interface.gui);
		Window.Add(lblMembers);
		lblMembers.Init(type);
		lblMembers.Y = y2;
		lblMembers.ToolTip = "Current members of the colony";
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(50, -35));
		RefreshLabels();
	}

	private void RefreshLabels()
	{
		decimal amount = The.InGameUI.UIAllegiance.TradeCredits ?? 0m;
		lblCredits.Text = Common.MoneyAsString(amount, abbreviate: true);
		lblCredits.FitToText();
		Window.CenterHorizontally(27, lblCredits);
		int noOfPersons = The.InGameUI.UIAllegiance.GetNoOfPersons();
		lblMembers.Text = noOfPersons.ToString() + "/" + GameData.Instance.Constants.PopulationCap;
		lblMembers.FitToText();
		Window.CenterHorizontally(68, lblMembers);
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendHeaderOnLightBG(stringBuilder, "Colony members");
		Common.Append(stringBuilder, "Shows the current members and the maximum we can accept. (The current members do not want the community to grow too large, making their votes count less)");
		Common.AppendLine(stringBuilder);
		Common.AppendDivider(stringBuilder);
		Common.Append(stringBuilder, "Current: ");
		Common.Append(stringBuilder, noOfPersons.ToString(), tintAsValue: true);
		Common.AppendLine(stringBuilder);
		Common.Append(stringBuilder, "Maximum: ");
		Common.Append(stringBuilder, GameData.Instance.Constants.PopulationCap.ToString(), tintAsValue: true);
		lblMembers.ToolTip = stringBuilder.ToString();
		if (The.InGameUI.UIAllegiance.IsOverPopulationCap())
		{
			lblMembers.NormalColor = UIComponent.errorColor;
		}
		else
		{
			lblMembers.NormalColor = lblMembers.GetNormalColorForType();
		}
	}

	public override void Refresh()
	{
		RefreshLabels();
	}
}
