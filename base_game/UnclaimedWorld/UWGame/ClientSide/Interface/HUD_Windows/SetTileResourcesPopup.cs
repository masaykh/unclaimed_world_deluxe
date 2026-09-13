using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class SetTileResourcesPopup : HUDPopup
{
	private Label lblHeader;

	private Spinner spMeanModifier;

	private TextBox tbAbsoluteMin;

	private TextBox tbAbsoluteMax;

	private ResourceType resourceType;

	public SetTileResourcesPopup()
		: base(200, 160)
	{
		DisplayWindow.Hide();
		int num = 30;
		int num2 = 64;
		int num3 = 10;
		int num4 = num3;
		lblHeader = new Label(gui);
		Add(lblHeader);
		lblHeader.Init(Label.LabelType.HUDWindow);
		lblHeader.Position = new Point(num4, 5);
		Label label = new Label(gui);
		Add(label);
		label.Text = "Modifier: ";
		label.Init(Label.LabelType.HUDWindow);
		label.Position = new Point(num4, num);
		num4 += num2;
		spMeanModifier = new Spinner(gui);
		Add(spMeanModifier);
		spMeanModifier.Init(Spinner.SpinnerType.HUD);
		spMeanModifier.Position = new Point(num4, num);
		spMeanModifier.NoOfDigits = 3;
		spMeanModifier.Width = 60;
		num4 += num2;
		num += 30;
		num4 = num3;
		label = new Label(gui);
		Add(label);
		label.Text = "Min/max:";
		label.Init(Label.LabelType.HUDWindow);
		label.Position = new Point(num4, num);
		num4 += num2;
		tbAbsoluteMin = new TextBox(gui);
		Add(tbAbsoluteMin);
		tbAbsoluteMin.Width = 55;
		tbAbsoluteMin.Position = new Point(num4, num);
		tbAbsoluteMin.IsEditable = true;
		tbAbsoluteMin.IsNumericBox = true;
		tbAbsoluteMin.Init(TextBox.TextBoxType.HUD);
		num4 += tbAbsoluteMin.Width + 8;
		tbAbsoluteMax = new TextBox(gui);
		Add(tbAbsoluteMax);
		tbAbsoluteMax.Width = 55;
		tbAbsoluteMax.Position = new Point(num4, num);
		tbAbsoluteMax.Height = 27;
		tbAbsoluteMax.IsEditable = true;
		tbAbsoluteMax.IsNumericBox = true;
		tbAbsoluteMax.Init(TextBox.TextBoxType.HUD);
		num += 30;
		num4 = num3;
		TextButton textButton = new TextButton(gui);
		Add(textButton);
		textButton.Text = "Save";
		textButton.Init(TextButton.TextButtonType.HUD);
		textButton.Position = new Point(num3, num);
		textButton.Click += btSave_Click;
		textButton.ScaleWidthToFitText();
		textButton.DebugTag = "HUDhover";
		num4 += textButton.Width + 8;
		TextButton textButton2 = new TextButton(gui);
		Add(textButton2);
		textButton2.Text = "Clear";
		textButton2.Init(TextButton.TextButtonType.HUD);
		textButton2.Position = new Point(num4, num);
		textButton2.Click += btClear_Click;
		textButton2.ScaleWidthToFitText();
		num4 += textButton2.Width + 8;
		TextButton textButton3 = new TextButton(gui);
		Add(textButton3);
		textButton3.Text = "Close";
		textButton3.Init(TextButton.TextButtonType.HUD);
		textButton3.Position = new Point(num4, num);
		textButton3.Click += btClose_Click;
		textButton3.ScaleWidthToFitText();
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		DisplayWindow.Hide();
	}

	private void btClear_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.SetTileResources.ClearResource(resourceType);
	}

	private void btSave_Click(UIComponent sender, EventArgs e)
	{
		int? numberAsInt = tbAbsoluteMin.GetNumberAsInt();
		int? numberAsInt2 = tbAbsoluteMax.GetNumberAsInt();
		if (!numberAsInt.HasValue || !numberAsInt2.HasValue || numberAsInt.Value <= numberAsInt2.Value)
		{
			The.InGameUI.SetTileResources.SaveResourceChanges(resourceType, numberAsInt, numberAsInt2, spMeanModifier.Count);
			DisplayWindow.Hide();
		}
	}

	public void Fill(ResourceType resourceType, Resource resourceData)
	{
		this.resourceType = resourceType;
		lblHeader.Text = resourceType.Name;
		spMeanModifier.Count = ((resourceData != null && resourceData.Modifier.HasValue) ? resourceData.Modifier.Value : 100);
		tbAbsoluteMin.Text = ((resourceData != null && resourceData.MinResourceItems.HasValue) ? resourceData.MinResourceItems.Value.ToString("G") : "");
		tbAbsoluteMax.Text = ((resourceData != null && resourceData.MaxResourceItems.HasValue) ? resourceData.MaxResourceItems.Value.ToString("G") : "");
	}
}
