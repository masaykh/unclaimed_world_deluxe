using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.SimEffects;

[DebuggerDisplay("{KeyName}")]
[XmlInclude(typeof(NumberEffectType))]
[XmlInclude(typeof(FlagEffectType))]
public class EffectType : IGameData
{
	public enum Background
	{
		White,
		EntityTypeTooltipGreen
	}

	public double? DurationInDays;

	public EvalNode DynamicDurationInDays;

	public AIDesirability AIDesirability;

	public Falloff Falloff;

	public string[] AffectsTypeKey;

	public string[] AffectsTypeTag;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public virtual float? GetIntensity()
	{
		return null;
	}

	public virtual void AppendAsString(StringBuilder text, Background background)
	{
	}

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
