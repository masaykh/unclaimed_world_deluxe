using System.Text;

namespace UWGame.SimSide.SimEffects;

public class FlagEffectType : EffectType
{
	public AffectsFlags Affects;

	public bool Value;

	public override void AppendAsString(StringBuilder text, Background background)
	{
		string fieldAsString = GetFieldAsString();
		Common.Append(text, fieldAsString);
		Common.Append(text, ": ");
		string t = Common.BoolToString(Value, useColor: true);
		Common.Append(text, t);
	}

	private string GetFieldAsString()
	{
		return Affects switch
		{
			AffectsFlags.CanEmigrate => "Can emigrate", 
			AffectsFlags.CanComplain => "Can complain", 
			_ => null, 
		};
	}
}
