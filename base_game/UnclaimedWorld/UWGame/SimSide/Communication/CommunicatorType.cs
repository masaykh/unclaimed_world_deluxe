namespace UWGame.SimSide.Communication;

public class CommunicatorType
{
	public CommunicationMethod Method;

	public double? Range;

	public bool IsInRange(double distance)
	{
		if (!Range.HasValue || Range >= distance)
		{
			return true;
		}
		return false;
	}

	public static string GetName(CommunicationMethod method)
	{
		return method switch
		{
			CommunicationMethod.Direct => "Direct", 
			CommunicationMethod.Visual => "Visual", 
			CommunicationMethod.Radio => "Radio", 
			CommunicationMethod.Satellite => "Satellite", 
			_ => "", 
		};
	}
}
