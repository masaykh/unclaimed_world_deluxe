using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UWGame.SimSide.Parser;

public class Evaluator
{
	private string equation = "";

	private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

	private Dictionary<string, string> parenthesesText = new Dictionary<string, string>();

	public string Equation
	{
		get
		{
			return equation;
		}
		set
		{
			equation = value;
		}
	}

	public Variable[] Variables => new List<Variable>(variables.Values).ToArray();

	public void SetVariable(string name, decimal value)
	{
		if (variables.ContainsKey(name))
		{
			Variable value2 = variables[name];
			value2.Value = value;
			variables[name] = value2;
		}
	}

	public Evaluator(string equation)
	{
		this.equation = equation;
		SetVariables();
	}

	public decimal Evaluate()
	{
		return Evaluate(equation, new List<Variable>(variables.Values));
	}

	public decimal Evaluate(string text)
	{
		decimal num = default(decimal);
		equation = text;
		equation = equation.Replace(" ", "");
		return Parse(equation, "qx")?.Evaluate() ?? num;
	}

	public decimal Evaluate(string text, List<Variable> variables)
	{
		foreach (Variable variable in variables)
		{
			string text2 = text;
			string name = variable.Name;
			decimal value = variable.Value;
			text = text2.Replace(name, value.ToString());
		}
		return Evaluate(text);
	}

	private static bool EquationHasVariables(string equation)
	{
		return new Regex("[A-Za-z]").IsMatch(equation);
	}

	private void SetVariables()
	{
		foreach (Match item in new Regex("([A-Za-z]+)").Matches(equation, 0))
		{
			Variable value = new Variable(item.Groups[1].Value, 0m);
			if (!variables.ContainsKey(value.Name))
			{
				variables.Add(value.Name, value);
			}
		}
	}

	private EvalNode Parse(string eq, string replaceText)
	{
		int num = 0;
		eq = eq.Replace(" ", "");
		if (eq.Length == 0)
		{
			return new ValueNode(0m);
		}
		int leftParentIndex = -1;
		int rightParentIndex = -1;
		SetIndexes(eq, ref leftParentIndex, ref rightParentIndex);
		while (leftParentIndex == 0 && rightParentIndex == eq.Length - 1)
		{
			eq = eq.Substring(1, eq.Length - 2);
			SetIndexes(eq, ref leftParentIndex, ref rightParentIndex);
		}
		replaceText = GetNextReplaceText(replaceText, num);
		while (leftParentIndex != -1 && rightParentIndex != -1)
		{
			string text = eq.Substring(leftParentIndex, rightParentIndex - leftParentIndex + 1);
			eq = eq.Replace(text, replaceText);
			parenthesesText.Add(replaceText, text);
			leftParentIndex = 0;
			rightParentIndex = 0;
			replaceText = replaceText.Remove(replaceText.LastIndexOf(num.ToString()));
			num++;
			replaceText = GetNextReplaceText(replaceText, num);
			SetIndexes(eq, ref leftParentIndex, ref rightParentIndex);
		}
		char[] anyOf = new char[2] { '+', '-' };
		char[] anyOf2 = new char[3] { '*', '/', '%' };
		char[] anyOf3 = new char[1] { '^' };
		char[] anyOf4 = new char[1] { '!' };
		int num2 = eq.LastIndexOfAny(anyOf);
		if (num2 > -1)
		{
			return CreateFunctionNode(eq, num2, replaceText + "0");
		}
		int num3 = eq.LastIndexOfAny(anyOf2);
		if (num3 > -1)
		{
			return CreateFunctionNode(eq, num3, replaceText + "0");
		}
		int num4 = eq.IndexOfAny(anyOf3);
		if (num4 > -1)
		{
			return CreateFunctionNode(eq, num4, replaceText + "0");
		}
		int num5 = eq.LastIndexOfAny(anyOf4);
		if (num5 > -1)
		{
			return CreateFunctionNode(eq, num5, replaceText + "0");
		}
		eq = eq.Replace("(", "");
		eq = eq.Replace(")", "");
		if (char.IsLetter(eq[0]))
		{
			return Parse(parenthesesText[eq], replaceText + "0");
		}
		return new ValueNode(decimal.Parse(eq));
	}

	private string GetNextReplaceText(string replaceText, int randomKeyIndex)
	{
		while (parenthesesText.ContainsKey(replaceText))
		{
			replaceText += randomKeyIndex;
		}
		return replaceText;
	}

	private EvalNode CreateFunctionNode(string eq, int index, string randomKey)
	{
		return new FunctionNode
		{
			Op = eq[index].ToString(),
			Lhs = Parse(eq.Substring(0, index), randomKey),
			Rhs = Parse(eq.Substring(index + 1), randomKey)
		};
	}

	private static void SetIndexes(string eq, ref int leftParentIndex, ref int rightParentIndex)
	{
		leftParentIndex = eq.IndexOf('(');
		rightParentIndex = eq.IndexOf(')');
		int num = eq.IndexOf('(', leftParentIndex + 1);
		while (num != -1 && num < rightParentIndex)
		{
			rightParentIndex = eq.IndexOf(')', rightParentIndex + 1);
			num = eq.IndexOf('(', num + 1);
		}
	}
}
