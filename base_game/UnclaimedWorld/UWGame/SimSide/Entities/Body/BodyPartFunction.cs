namespace UWGame.SimSide.Entities.Body;

public class BodyPartFunction
{
	public enum FunctionType
	{
		Locomotion,
		Vision,
		Appearance,
		Manipulation,
		Agility,
		Strength,
		UserComfort,
		Structure
	}

	public FunctionType Function;

	/// <summary>
	/// A value from 0 to 1.
	/// </summary>
	public float Weight;
}
