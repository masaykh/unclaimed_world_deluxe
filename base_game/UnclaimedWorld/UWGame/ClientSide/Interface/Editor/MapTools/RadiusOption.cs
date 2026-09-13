using System;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class RadiusOption : ToolOption
{
	private RadiusSetting setting;

	private FillableBar fbToolSize;

	public override int Order => 1;

	public RadiusOption(GUIManager gui)
		: base(gui)
	{
		Label label = new Label(gui);
		label.Init(Label.LabelType.LCDNormal);
		Add(label);
		label.Text = "SIZE";
		label.FitToText();
		fbToolSize = new FillableBar(gui, FillableBar.FillableBarType.LCDSlider, canGrow: false);
		Add(fbToolSize);
		fbToolSize.X = 60;
		fbToolSize.Width = 160;
		fbToolSize.MaxValue = 300;
		fbToolSize.ShowMaxValueLabelAtEnd = true;
		fbToolSize.ShowNotches = false;
		fbToolSize.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
		fbToolSize.SliderMouseUp += fbToolSize_SliderMouseUp;
	}

	private void fbToolSize_SliderMouseUp(object sender, EventArgs e)
	{
		setting.Value = fbToolSize.Value;
	}

	public void Set(RadiusSetting setting)
	{
		this.setting = setting;
		fbToolSize.Value = (int)setting.Value;
		fbToolSize.UpdateSliderPosition();
	}
}
