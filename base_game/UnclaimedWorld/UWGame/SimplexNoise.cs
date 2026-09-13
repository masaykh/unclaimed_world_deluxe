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

	public float Generate3D(float x, float y, float z)
	{
		float num = (x + y + z) * (1f / 3f);
		float x2 = x + num;
		float x3 = y + num;
		float x4 = z + num;
		int num2 = FastFloor(x2);
		int num3 = FastFloor(x3);
		int num4 = FastFloor(x4);
		float num5 = (float)(num2 + num3 + num4) * (1f / 6f);
		float num6 = (float)num2 - num5;
		float num7 = (float)num3 - num5;
		float num8 = (float)num4 - num5;
		float num9 = x - num6;
		float num10 = y - num7;
		float num11 = z - num8;
		int num12;
		int num13;
		int num14;
		int num15;
		int num16;
		int num17;
		if (num9 >= num10)
		{
			if (num10 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			else if (num9 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
		}
		else if (num10 < num11)
		{
			num12 = 0;
			num13 = 0;
			num14 = 1;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else if (num9 < num11)
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 1;
			num16 = 1;
			num17 = 0;
		}
		float num18 = num9 - (float)num12 + 1f / 6f;
		float num19 = num10 - (float)num13 + 1f / 6f;
		float num20 = num11 - (float)num14 + 1f / 6f;
		float num21 = num9 - (float)num15 + 1f / 3f;
		float num22 = num10 - (float)num16 + 1f / 3f;
		float num23 = num11 - (float)num17 + 1f / 3f;
		float num24 = num9 - 1f + 0.5f;
		float num25 = num10 - 1f + 0.5f;
		float num26 = num11 - 1f + 0.5f;
		int num27 = Mod(num2, 256);
		int num28 = Mod(num3, 256);
		int num29 = Mod(num4, 256);
		float num30 = 0.6f - num9 * num9 - num10 * num10 - num11 * num11;
		float num31;
		if (num30 < 0f)
		{
			num31 = 0f;
		}
		else
		{
			num30 *= num30;
			num31 = num30 * num30 * grad(SeedNumbers[num27 + SeedNumbers[num28 + SeedNumbers[num29]]], num9, num10, num11);
		}
		float num32 = 0.6f - num18 * num18 - num19 * num19 - num20 * num20;
		float num33;
		if (num32 < 0f)
		{
			num33 = 0f;
		}
		else
		{
			num32 *= num32;
			num33 = num32 * num32 * grad(SeedNumbers[num27 + num12 + SeedNumbers[num28 + num13 + SeedNumbers[num29 + num14]]], num18, num19, num20);
		}
		float num34 = 0.6f - num21 * num21 - num22 * num22 - num23 * num23;
		float num35;
		if (num34 < 0f)
		{
			num35 = 0f;
		}
		else
		{
			num34 *= num34;
			num35 = num34 * num34 * grad(SeedNumbers[num27 + num15 + SeedNumbers[num28 + num16 + SeedNumbers[num29 + num17]]], num21, num22, num23);
		}
		float num36 = 0.6f - num24 * num24 - num25 * num25 - num26 * num26;
		float num37;
		if (num36 < 0f)
		{
			num37 = 0f;
		}
		else
		{
			num36 *= num36;
			num37 = num36 * num36 * grad(SeedNumbers[num27 + 1 + SeedNumbers[num28 + 1 + SeedNumbers[num29 + 1]]], num24, num25, num26);
		}
		return 32f * (num31 + num33 + num35 + num37);
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

	private int Mod(int x, int m)
	{
		int num = x % m;
		if (num >= 0)
		{
			return num;
		}
		return num + m;
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

	private float grad(int hash, float x, float y, float z)
	{
		int num = hash & 0xF;
		float num2 = ((num < 8) ? x : y);
		float num3 = ((num < 4) ? y : ((num == 12 || num == 14) ? x : z));
		return (((num & 1) != 0) ? (0f - num2) : num2) + (((num & 2) != 0) ? (0f - num3) : num3);
	}

	private float grad(int hash, float x, float y, float z, float t)
	{
		int num = hash & 0x1F;
		float num2 = ((num < 24) ? x : y);
		float num3 = ((num < 16) ? y : z);
		float num4 = ((num < 8) ? z : t);
		return (((num & 1) != 0) ? (0f - num2) : num2) + (((num & 2) != 0) ? (0f - num3) : num3) + (((num & 4) != 0) ? (0f - num4) : num4);
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
