namespace UWGame.ClientSide.Interface.Editor.MapTools;

public class RadiusSetting : Setting
{
	public float Value { get; set; }

	public RadiusSetting(float defaultValue)
	{
		Value = defaultValue;
	}
}
