namespace UWGame.SimSide.AllGameData;

public class UserDataLoader : DataLoader
{
	public UserDataLoader()
		: base(Config.DataType.UserScenarios, 0.1f)
	{
	}
}
