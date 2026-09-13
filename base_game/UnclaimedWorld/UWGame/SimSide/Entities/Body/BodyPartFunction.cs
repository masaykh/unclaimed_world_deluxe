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

	public float Weight;
}
