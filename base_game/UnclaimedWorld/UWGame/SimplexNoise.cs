using UWGame.Control.Replays;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame;

public class SimplexNoise : ISnapshot
{
	public byte[] SeedNumbers;

	private float offsetX;

	private float offsetY;

	private int randomSeed;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public SimplexNoise()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			offsetX = 100000f * (float)The.Sim.GameplayRandomGenerator.NextDouble("SimplexNoise");
			offsetY = 100000f * (float)The.Sim.GameplayRandomGenerator.NextDouble("SimplexNoise");
			randomSeed = The.Sim.GameplayRandomGenerator.Next("SimplexNoise");
			CreateSeedNumbers();
		}
	}

	private void CreateSeedNumbers()
	{
		RandomGenerator generator = new RandomGenerator(randomSeed, RandomGenerator.GeneratorType.Sim);
		SeedNumbers = CreateSeedNumbers(generator);
	}

	public float Generate1D(float x, NoiseParams parms)
	{
		return Generate1D(x, parms.NoiseFrequency ?? 1f, parms.NoiseAmplitude ?? 1f, parms.NoiseAddend ?? 0f);
	}

	public float Generate1D(float x, float? noiseFrequency = null, float amplitude = 1f, float noiseAddend = 0f)
	{
		x += offsetX;
		if (noiseFrequency.HasValue)
		{
			x *= noiseFrequency.Value;
		}
		int num = FastFloor(x);
		int num2 = num + 1;
		float num3 = x - (float)num;
		float num4 = num3 - 1f;
		float num5 = 1f - num3 * num3;
		float num6 = num5 * num5;
		float num7 = num6 * num6 * grad(SeedNumbers[num & 0xFF], num3);
		float num8 = 1f - num4 * num4;
		float num9 = num8 * num8;
		float num10 = num9 * num9 * grad(SeedNumbers[num2 & 0xFF], num4);
		return 0.395f * (num7 + num10) * amplitude + noiseAddend;
	}

	public float Generate2D(float x, float y, float? noiseFrequency = null)
	{
		x += offsetX;
		y += offsetY;
		if (noiseFrequency.HasValue)
		{
			x *= noiseFrequency.Value;
			y *= noiseFrequency.Value;
		}
		float num = (x + y) * 0.3660254f;
		float x2 = x + num;
		float x3 = y + num;
		int num2 = FastFloor(x2);
		int num3 = FastFloor(x3);
		float num4 = (float)(num2 + num3) * 0.21132487f;
		float num5 = (float)num2 - num4;
		float num6 = (float)num3 - num4;
		float num7 = x - num5;
		float num8 = y - num6;
		int num9;
		int num10;
		if (num7 > num8)
		{
			num9 = 1;
			num10 = 0;
		}
		else
		{
			num9 = 0;
			num10 = 1;
		}
		float num11 = num7 - (float)num9 + 0.21132487f;
		float num12 = num8 - (float)num10 + 0.21132487f;
		float num13 = num7 - 1f + 0.42264974f;
		float num14 = num8 - 1f + 0.42264974f;
		int num15 = num2 % 256;
		int num16 = num3 % 256;
		float num17 = 0.5f - num7 * num7 - num8 * num8;
		float num18;
		if (num17 < 0f)
		{
			num18 = 0f;
		}
		else
		{
			num17 *= num17;
			num18 = num17 * num17 * grad(SeedNumbers[num15 + SeedNumbers[num16]], num7, num8);
		}
		float num19 = 0.5f - num11 * num11 - num12 * num12;
		float num20;
		if (num19 < 0f)
		{
			num20 = 0f;
		}
		else
		{
			num19 *= num19;
			num20 = num19 * num19 * grad(SeedNumbers[num15 + num9 + SeedNumbers[num16 + num10]], num11, num12);
		}
		float num21 = 0.5f - num13 * num13 - num14 * num14;
		float num22;
		if (num21 < 0f)
		{
			num22 = 0f;
		}
		else
		{
			num21 *= num21;
			num22 = num21 * num21 * grad(SeedNumbers[num15 + 1 + SeedNumbers[num16 + 1]], num13, num14);
		}
		return 40f * (num18 + num20 + num22);
	}

	public byte[] CreateSeedNumbers(RandomGenerator generator)
	{
		byte[] array = new byte[512];
		generator.NextBytes(array, "");
		return array;
	}

	private int FastFloor(float x)
	{
		if (!(x > 0f))
		{
			return (int)x - 1;
		}
		return (int)x;
	}

	private float grad(int hash, float x)
	{
		int num = hash & 0xF;
		float num2 = 1f + (float)(num & 7);
		if ((num & 8) != 0)
		{
			num2 = 0f - num2;
		}
		return num2 * x;
	}

	private float grad(int hash, float x, float y)
	{
		int num = hash & 7;
		float num2 = ((num < 4) ? x : y);
		float num3 = ((num < 4) ? y : x);
		return (((num & 1) != 0) ? (0f - num2) : num2) + (((num & 2) != 0) ? (-2f * num3) : (2f * num3));
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		randomSeed = sn.DoInt32(randomSeed);
		offsetX = sn.DoFloat(offsetX);
		offsetY = sn.DoFloat(offsetY);
		sn.Ignore(SeedNumbers);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateSeedNumbers();
	}
}
