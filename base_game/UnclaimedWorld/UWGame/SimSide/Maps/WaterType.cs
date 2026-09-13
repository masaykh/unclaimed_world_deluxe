namespace UWGame.SimSide.Maps;

public class WaterType : SurfaceType
{
	private static WaterType instance;

	public static WaterType Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			instance = new WaterType();
			return instance;
		}
	}

	public override string Name => "Water";

	private WaterType()
	{
		costs = new byte[3, 5];
		costs[0, 0] = 0;
		costs[0, 1] = 0;
		costs[0, 2] = 0;
		costs[0, 3] = 0;
		costs[0, 4] = 0;
		costs[1, 0] = 0;
		costs[1, 1] = 0;
		costs[1, 2] = 0;
		costs[1, 3] = 0;
		costs[1, 4] = 0;
		costs[2, 0] = 0;
		costs[2, 1] = 0;
		costs[2, 2] = 0;
		costs[2, 3] = 0;
		costs[2, 4] = 0;
	}
}
