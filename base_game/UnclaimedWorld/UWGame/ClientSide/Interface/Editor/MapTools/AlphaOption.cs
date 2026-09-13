using System;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class AlphaOption : ToolOption
{
	private AlphaSetting setting;

	private FillableBar fbToolAlpha;

	public override int Order => 2;

	public AlphaOption(GUIManager gui)
		: base(gui)
	{
		Label label = new Label(gui);
		label.Init(Label.LabelType.LCDNormal);
		Add(label);
		label.Text = "ALPHA";
		label.FitToText();
		fbToolAlpha = new FillableBar(gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		Add(fbToolAlpha);
		fbToolAlpha.X = 60;
		fbToolAlpha.Width = 160;
		fbToolAlpha.MaxValue = 100;
		fbToolAlpha.ShowMaxValueLabelAtEnd = false;
		fbToolAlpha.ShowNotches = false;
		fbToolAlpha.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbToolAlpha.SliderMouseUp += fbTool_SliderMouseUp;
	}

	private void fbTool_SliderMouseUp(object sender, EventArgs e)
	{
		setting.Value = (float)fbToolAlpha.Value / 100f;
	}

	public void Set(AlphaSetting setting)
	{
		this.setting = setting;
		fbToolAlpha.Value = (int)(100f * setting.Value);
		fbToolAlpha.UpdateSliderPosition();
	}
}
