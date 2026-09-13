namespace UWGame.SimSide.Parser;

internal class ValueNode : EvalNode
{
	private decimal value;

	public ValueNode(decimal v)
	{
		value = v;
	}

	public override decimal Evaluate()
	{
		return value;
	}

	public override string ToString()
	{
		return value.ToString();
	}
}
