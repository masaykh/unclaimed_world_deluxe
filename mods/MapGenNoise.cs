using System;

namespace UWGame.Mods;

/// <summary>
/// Seeded randomness and noise for <see cref="MapGenMod"/>.
///
/// Its own generator, never the game's: the simulation's GameplayRandomGenerator is part of what a
/// replay reproduces, and a map generated from it would shift every draw after it. SplitMix64 is a
/// few lines, fast, and gives the same numbers on every platform and every .NET - System.Random's
/// seeded sequence is only promised for the legacy algorithm, which is not a promise to lean on
/// when "seed 42" has to mean the same map next year.
/// </summary>
internal sealed class MapGenRandom
{
    private ulong state;

    public MapGenRandom(ulong seed)
    {
        state = seed;
    }

    public ulong NextULong()
    {
        ulong z = state += 0x9E3779B97F4A7C15ul;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
        return z ^ (z >> 31);
    }

    /// <summary>[0, 1).</summary>
    public double NextDouble() => (NextULong() >> 11) * (1.0 / (1ul << 53));

    /// <summary>[0, max).</summary>
    public int Next(int max) => max <= 0 ? 0 : (int)(NextULong() % (ulong)max);

    public double Range(double min, double max) => min + (max - min) * NextDouble();

    public bool Chance(double p) => NextDouble() < p;

    /// <summary>A generator for one purpose, so adding a step never changes the others' numbers.</summary>
    public MapGenRandom Fork(string purpose)
    {
        ulong h = 1469598103934665603ul;
        foreach (char ch in purpose)
        {
            h = (h ^ ch) * 1099511628211ul;
        }
        return new MapGenRandom(NextULong() ^ h);
    }
}

/// <summary>2D gradient (Perlin) noise with fractal octaves, seeded.</summary>
internal sealed class MapGenNoise
{
    private readonly int[] perm = new int[512];

    public MapGenNoise(MapGenRandom random)
    {
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;
        for (int i = 255; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }
        for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
    }

    /// <summary>About [-1, 1].</summary>
    public double At(double x, double y)
    {
        int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
        double xf = x - xi, yf = y - yi;
        int X = xi & 255, Y = yi & 255;
        double u = Fade(xf), v = Fade(yf);
        int aa = perm[perm[X] + Y], ab = perm[perm[X] + Y + 1];
        int ba = perm[perm[X + 1] + Y], bb = perm[perm[X + 1] + Y + 1];
        double x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        double x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
        return Lerp(x1, x2, v) * 1.41;
    }

    /// <summary>Fractal sum of <paramref name="octaves"/> layers, normalised to about [-1, 1].</summary>
    public double Fractal(double x, double y, int octaves, double persistence = 0.5, double lacunarity = 2.0)
    {
        double sum = 0, amplitude = 1, frequency = 1, norm = 0;
        for (int i = 0; i < octaves; i++)
        {
            sum += amplitude * At(x * frequency, y * frequency);
            norm += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return sum / norm;
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static double Grad(int hash, double x, double y)
    {
        switch (hash & 7)
        {
            case 0: return x + y;
            case 1: return -x + y;
            case 2: return x - y;
            case 3: return -x - y;
            case 4: return x;
            case 5: return -x;
            case 6: return y;
            default: return -y;
        }
    }
}
