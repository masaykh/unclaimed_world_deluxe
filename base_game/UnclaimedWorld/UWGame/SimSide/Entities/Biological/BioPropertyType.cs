namespace UWGame.SimSide.Entities.Biological;

/// <summary>
/// Defines how biological property values should be combined into one result.
/// </summary>
public class BioPropertyType
{
	/// <summary>
	/// DefaultPriority means age overrides race, which overrides caste.
	/// </summary>
	public enum Interpolate
	{
		DefaultPriority,
		Average,
		Max,
		Min
	}

	public string KeyName;

	/// <summary>
	/// DefaultPriority means age overrides race, which overrides caste.
	/// </summary>
	public Interpolate InterpolateSetting;
}
