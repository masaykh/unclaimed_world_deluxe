namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class AlphaSetting : Setting
{
	public float Value { get; set; }

	public AlphaSetting(float defaultValue)
	{
		Value = defaultValue;
	}
}
