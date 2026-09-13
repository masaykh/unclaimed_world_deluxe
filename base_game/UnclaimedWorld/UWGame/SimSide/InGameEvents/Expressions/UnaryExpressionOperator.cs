namespace UWGame.SimSide.InGameEvents.Expressions;

public enum UnaryExpressionOperator
{
	Not,
	DateFromRelativeDays,
	DateFromAbsoluteDays,
	DateFromRelativeSeconds,
	DateFromAbsoluteSeconds,
	DateToAbsoluteString,
	DateToJournalString,
	DateToRelativeSeconds,
	DateToRelativeDays,
	ComfortRatingToString,
	FoodRatingToString,
	SecurityRatingToString,
	ClampToWithinZeroAndOne
}
