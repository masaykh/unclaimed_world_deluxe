namespace WindowSystem;

internal class Legend
{
	public CheckBox CheckBox;

	public Legend(GUIManager guiManager, Graph graph)
	{
		CheckBox = new CheckBox(guiManager);
		CheckBox.Init(CheckBoxType.LCDTinting);
		CheckBox.Click += graph.LegendCheckBox_Click;
	}
}
