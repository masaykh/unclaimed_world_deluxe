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
			CommunicationMethod.Direct => UWGame.Locale.Text("Direct"), 
			CommunicationMethod.Visual => UWGame.Locale.Text("Visual"), 
			CommunicationMethod.Radio => UWGame.Locale.Text("Radio"), 
			CommunicationMethod.Satellite => UWGame.Locale.Text("Satellite"), 
			_ => "", 
		};
	}
}
