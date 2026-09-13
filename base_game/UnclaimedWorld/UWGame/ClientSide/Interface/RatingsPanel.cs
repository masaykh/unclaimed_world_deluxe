using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances.Statistics;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class RatingsPanel : Panel
{
	private Label lblSecurity;

	private Label lblFoodSupply;

	private Label lblComfort;

	private Image imSecurityProgress;

	private Image imFoodProgress;

	private Image imComfortProgress;

	public RatingsPanel()
		: base(The.InGameUI, null, new Point(325, 0), new Vector2(150f, 69f), Level.Middle, PanelType.Ratings)
	{
		Window.Show();
		int y = 3;
		int num = 4;
		ImageButton imageButton = new ImageButton(Interface.gui);
		imageButton.Init(ImageButtonType.FoodRating);
		Window.Add(imageButton);
		imageButton.X = 16;
		imageButton.Y = y;
		imageButton.Click += btNutrition_Click;
		imageButton.ToolTip = "Click to see the nutrition graph";
		ImageButton imageButton2 = new ImageButton(Interface.gui);
		imageButton2.Init(ImageButtonType.SecurityRating);
		Window.Add(imageButton2);
		imageButton2.X = imageButton.Right + num;
		imageButton2.Y = y;
		imageButton2.Click += btSecurity_Click;
		imageButton2.ToolTip = "Click to see the security graph";
		ImageButton imageButton3 = new ImageButton(Interface.gui);
		imageButton3.Init(ImageButtonType.ComfortRating);
		Window.Add(imageButton3);
		imageButton3.X = imageButton2.Right + num;
		imageButton3.Y = y;
		imageButton3.Click += btComfort_Click;
		imageButton3.ToolTip = "Click to see the comfort graph";
		int y2 = 31;
		Label.LabelType type = Label.LabelType.PlainPanelNormal;
		int tooltipWidth = 280;
		lblFoodSupply = new Label(Interface.gui);
		Window.Add(lblFoodSupply);
		lblFoodSupply.Init(type);
		lblFoodSupply.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Food).ToString();
		lblFoodSupply.FitToText();
		lblFoodSupply.Y = y2;
		lblFoodSupply.ToolTip = " - ";
		lblFoodSupply.TooltipRequested += lblFoodSupply_TooltipRequested;
		lblFoodSupply.TooltipExpires = false;
		lblFoodSupply.TooltipWidth = tooltipWidth;
		lblSecurity = new Label(Interface.gui);
		Window.Add(lblSecurity);
		lblSecurity.Init(type);
		lblSecurity.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Security).ToString();
		lblSecurity.FitToText();
		lblSecurity.Y = y2;
		lblSecurity.ToolTip = " - ";
		lblSecurity.TooltipRequested += lblSecurity_TooltipRequested;
		lblSecurity.TooltipExpires = false;
		lblSecurity.TooltipWidth = tooltipWidth;
		lblComfort = new Label(Interface.gui);
		Window.Add(lblComfort);
		lblComfort.Init(type);
		lblComfort.Text = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(RatingTypes.Comfort).ToString();
		lblComfort.FitToText();
		lblComfort.Y = y2;
		lblComfort.ToolTip = " - ";
		lblComfort.TooltipRequested += lblComfort_TooltipRequested;
		lblComfort.TooltipExpires = false;
		lblComfort.TooltipWidth = tooltipWidth;
		int y3 = 46;
		imSecurityProgress = new Image(Interface.gui);
		imSecurityProgress.ScaleImageToSizeOfControl = false;
		imSecurityProgress.Y = y3;
		Window.Add(imSecurityProgress);
		imFoodProgress = new Image(Interface.gui);
		imFoodProgress.ScaleImageToSizeOfControl = false;
		imFoodProgress.Y = y3;
		Window.Add(imFoodProgress);
		imComfortProgress = new Image(Interface.gui);
		imComfortProgress.ScaleImageToSizeOfControl = false;
		imComfortProgress.Y = y3;
		Window.Add(imComfortProgress);
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(0, 0));
		RequestRatingsBreakdown(RatingTypes.Food, value: true);
		RequestRatingsBreakdown(RatingTypes.Comfort, value: true);
		RequestRatingsBreakdown(RatingTypes.Security, value: true);
		RefreshContent();
	}

	private void OpenGraphRosterPanel(Graphs graphType)
	{
		The.InGameUI.GraphPanel.SelectGraphType(graphType);
		The.InGameUI.ChangeRosterPanel(The.InGameUI.GraphPanel);
	}

	private void btComfort_Click(UIComponent sender, EventArgs e)
	{
		OpenGraphRosterPanel(Graphs.ComfortRating);
	}

	private void btSecurity_Click(UIComponent sender, EventArgs e)
	{
		OpenGraphRosterPanel(Graphs.SecurityRating);
	}

	private void btNutrition_Click(UIComponent sender, EventArgs e)
	{
		OpenGraphRosterPanel(Graphs.FoodRating);
	}

	private void lblComfort_TooltipDisplayed(UIComponent sender, bool value)
	{
		RequestRatingsBreakdown(RatingTypes.Comfort, value);
	}

	private void lblSecurity_TooltipDisplayed(UIComponent sender, bool value)
	{
		RequestRatingsBreakdown(RatingTypes.Security, value);
	}

	private void lblFoodSupply_TooltipDisplayed(UIComponent sender, bool value)
	{
		RequestRatingsBreakdown(RatingTypes.Food, value);
	}

	private void lblComfort_TooltipRequested(UIComponent sender)
	{
		RefreshRatingTooltip(RatingTypes.Comfort, lblComfort);
	}

	private void lblSecurity_TooltipRequested(UIComponent sender)
	{
		RefreshRatingTooltip(RatingTypes.Security, lblSecurity);
	}

	private void lblFoodSupply_TooltipRequested(UIComponent sender)
	{
		RefreshRatingTooltip(RatingTypes.Food, lblFoodSupply);
	}

	public override void Refresh()
	{
		RefreshContent();
	}

	private void RefreshContent()
	{
		int num = 39;
		int num2 = 35;
		RefreshRating(RatingTypes.Food, lblFoodSupply, imFoodProgress, num2);
		num2 += num;
		RefreshRating(RatingTypes.Security, lblSecurity, imSecurityProgress, num2);
		num2 += num;
		RefreshRating(RatingTypes.Comfort, lblComfort, imComfortProgress, num2);
	}

	private void RequestRatingsBreakdown(RatingTypes statType, bool value)
	{
		The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType).ToggleComposeBreakdown(value);
	}

	private void RefreshRatingTooltip(RatingTypes statType, Label label)
	{
		Rating statisticByKey = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType);
		label.ToolTip = statisticByKey.GetRatingsBreakdown();
	}

	private void RefreshRating(RatingTypes statType, Label label, Image image, int centerOnXPos)
	{
		Rating statisticByKey = The.InGameUI.UIAllegiance.Statistics.GetStatisticByKey(statType);
		double rating = statisticByKey.GetLatestValue();
		label.Text = Common.PercentageToString(rating);
		label.FitToText();
		Window.CenterHorizontally(centerOnXPos, label);
		double num = statisticByKey.GetChange();
		string spriteName = (Common.IsZero(num) ? "ratings_arrow_horizontal" : ((!Common.IsGreaterThan(num, 0.0)) ? "ratings_arrow_down" : "ratings_arrow_up"));
		image.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle(spriteName));
		image.ResizeControlToFitImage();
		Window.CenterHorizontally(centerOnXPos, image);
	}
}
