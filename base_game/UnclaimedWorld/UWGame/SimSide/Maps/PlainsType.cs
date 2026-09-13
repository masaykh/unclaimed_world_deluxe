namespace UWGame.SimSide.Maps;

public class PlainsType : SurfaceType
{
	private static PlainsType instance;

	public static PlainsType Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			instance = new PlainsType();
			return instance;
		}
	}

	public override string Name => "Plains";

	private PlainsType()
	{
		costs = new byte[3, 6];
		costs[0, 0] = 3;
		costs[0, 1] = 3;
		costs[0, 2] = 3;
		costs[0, 3] = 3;
		costs[0, 4] = 3;
		costs[0, 5] = 5;
		costs[1, 0] = 5;
		costs[1, 1] = 5;
		costs[1, 2] = 4;
		costs[1, 3] = 2;
		costs[1, 4] = 1;
		costs[1, 5] = 8;
		costs[2, 0] = 4;
		costs[2, 1] = 4;
		costs[2, 2] = 3;
		costs[2, 3] = 2;
		costs[2, 4] = 2;
		costs[2, 5] = 6;
	}
}
