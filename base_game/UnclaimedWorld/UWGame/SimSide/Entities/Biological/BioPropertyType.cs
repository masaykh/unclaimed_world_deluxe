namespace UWGame.SimSide.Entities.Biological;

public class BioPropertyType
{
	public enum Interpolate
	{
		DefaultPriority,
		Average,
		Max,
		Min
	}

	public string KeyName;

	public Interpolate InterpolateSetting;
}
