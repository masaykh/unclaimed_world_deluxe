namespace UWGame.SimSide.AI.Constants.Rating;

public class Ratings
{
	public float PrinciplesAdaptationSpeedPerSecond = 5E-05f;

	public float PrinciplesTargetDelta = 0.05f;

	public Security Security;

	public Comfort Comfort;

	public Food Food;

	public float HappinessLimitForGroupMeeting = -0.05f;

	public float UnhappyExpeditionMembersPercentageForGroupMeeting = 0.25f;

	public Ratings()
	{
		Security = new Security();
		Comfort = new Comfort();
		Food = new Food();
	}
}
