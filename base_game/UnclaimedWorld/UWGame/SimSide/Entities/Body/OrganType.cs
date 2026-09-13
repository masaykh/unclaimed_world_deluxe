namespace UWGame.SimSide.Entities.Body;

public class OrganType
{
	public enum OrganDepth
	{
		Internal,
		External
	}

	public enum OrganFunctions
	{
		NerveSystem,
		Digestive,
		Respiratory,
		Vision,
		Hearing,
		Appearance
	}

	public string Name;

	public OrganDepth Depth;

	public bool IsVital;

	public OrganFunctions[] Functions;

	public override string ToString()
	{
		return Name;
	}
}
