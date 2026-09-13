using System;

namespace UWGame.SimSide.Parser;

internal class FunctionNode : EvalNode
{
	private EvalNode lhs = new ValueNode(0m);

	private EvalNode rhs = new ValueNode(0m);

	private string op = "+";

	public string Op
	{
		get
		{
			return op;
		}
		set
		{
			op = value;
		}
	}

	internal EvalNode Rhs
	{
		get
		{
			return rhs;
		}
		set
		{
			rhs = value;
		}
	}

	internal EvalNode Lhs
	{
		get
		{
			return lhs;
		}
		set
		{
			lhs = value;
		}
	}

	public override decimal Evaluate()
	{
		decimal result = default(decimal);
		switch (op)
		{
		case "+":
			return lhs.Evaluate() + rhs.Evaluate();
		case "-":
			return lhs.Evaluate() - rhs.Evaluate();
		case "*":
			return lhs.Evaluate() * rhs.Evaluate();
		case "/":
			return lhs.Evaluate() / rhs.Evaluate();
		case "%":
			return lhs.Evaluate() % rhs.Evaluate();
		case "^":
		{
			double x = Convert.ToDouble(lhs.Evaluate());
			double y = Convert.ToDouble(rhs.Evaluate());
			return Convert.ToDecimal(Math.Pow(x, y));
		}
		case "!":
			return Factorial(lhs.Evaluate());
		default:
			return result;
		}
	}

	private decimal Factorial(decimal factor)
	{
		if (factor < 1m)
		{
			return 1m;
		}
		return factor * Factorial(factor - 1m);
	}

	public override string ToString()
	{
		return "(" + lhs.ToString() + " " + op + " " + rhs.ToString() + ")";
	}
}
