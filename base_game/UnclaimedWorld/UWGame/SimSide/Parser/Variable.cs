namespace UWGame.SimSide.Parser;

public struct Variable
{
	public string Name;

	public decimal Value;

	public Variable(string n, decimal v)
	{
		Name = n;
		Value = v;
	}
}
