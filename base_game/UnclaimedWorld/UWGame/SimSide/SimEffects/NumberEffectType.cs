using System.Text;

namespace UWGame.SimSide.SimEffects;

public class NumberEffectType : EffectType
{
	public AffectsNumbers Affects;

	public NumberEffectOperator Operator;

	public float Intensity;

	public bool FormatAsPercentage;

	public override float? GetIntensity()
	{
		return Intensity;
	}

	public override void AppendAsString(StringBuilder text, Background background)
	{
		string fieldAsString = GetFieldAsString();
		Common.Append(text, fieldAsString);
		Common.Append(text, ": ");
		string operatorAsString = GetOperatorAsString();
		Common.Append(text, operatorAsString);
		bool useColoring = false;
		if (background == Background.White)
		{
			useColoring = true;
		}
		if (FormatAsPercentage)
		{
			Common.AppendPercentage(text, Intensity, useColoring, null);
			return;
		}
		string t = Common.ValueToDecimalString(Intensity, useColoring, null);
		Common.Append(text, t);
	}

	private string GetOperatorAsString()
	{
		switch (Operator)
		{
		case NumberEffectOperator.Add:
		case NumberEffectOperator.PermanentAdd:
			if (Intensity < 0f)
			{
				return "-";
			}
			return "";
		case NumberEffectOperator.Multiply:
			return "*";
		default:
			return "";
		}
	}

	private string GetFieldAsString()
	{
		return Affects switch
		{
			AffectsNumbers.AgentComfort => "Comfort", 
			AffectsNumbers.OfferedComfort => "Offered comfort", 
			AffectsNumbers.Stealth => "Stealth", 
			AffectsNumbers.Need => "Need", 
			AffectsNumbers.Morale => "Morale", 
			AffectsNumbers.NightSensorRange => "Night sensor range", 
			AffectsNumbers.Detection => "Detection", 
			_ => null, 
		};
	}
}
