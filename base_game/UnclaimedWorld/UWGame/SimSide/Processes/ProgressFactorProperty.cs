using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.Processes;

public class ProgressFactorProperty
{
	public string InputType;

	public bool UseActingOnEntity;

	public string PropertyKey;

	public string SubstanceKey;

	public string TooltipCaption;

	public float? ShiftByAmount;

	public float? ScaleByAmount;

	public EvalNode Factor;
}
