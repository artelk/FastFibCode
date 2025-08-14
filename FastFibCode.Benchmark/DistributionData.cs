namespace FastFibCode.Benchmark;

using static Distribution;

public static class DistributionData
{
    private static readonly Dictionary<Distribution, uint[]> values = new Dictionary<Distribution, uint[]>();
    private static readonly Dictionary<Distribution, ulong[]> codes = new Dictionary<Distribution, ulong[]>();

    public static uint[] GetValues(Distribution distribution) => values[distribution];
    public static ulong[] GetCodes(Distribution distribution) => codes[distribution];

    public static void Init()
    {
        values.Clear();

        values[FoldedNormal_100] = Create(() => FoldedNormalDistributionRandom(100));
        values[FoldedNormal_1K] = Create(() => FoldedNormalDistributionRandom(1000));
        values[FoldedNormal_10K] = Create(() => FoldedNormalDistributionRandom(10_000));
        values[FoldedNormal_100K] = Create(() => FoldedNormalDistributionRandom(100_000));
        values[FoldedNormal_1M] = Create(() => FoldedNormalDistributionRandom(1000_000));
        values[FoldedNormal_10M] = Create(() => FoldedNormalDistributionRandom(10_000_000));

        values[Exponential_100] = Create(() => ExponentialDistributionRandom(100));
        values[Exponential_1K] = Create(() => ExponentialDistributionRandom(1000));
        values[Exponential_10K] = Create(() => ExponentialDistributionRandom(10_000));
        values[Exponential_100K] = Create(() => ExponentialDistributionRandom(100_000));
        values[Exponential_1M] = Create(() => ExponentialDistributionRandom(1_000_000));
        values[Exponential_10M] = Create(() => ExponentialDistributionRandom(10_000_000));

        values[Uniform_5M] = Create(() => (uint)Random.Shared.Next(0, 5_000_000));

        codes.Clear();
        foreach (var p in values)
        {
            var cs = new ulong[p.Value.Length];
            for (int i = 0; i < p.Value.Length; i++)
                cs[i] = Fibonacci.EncodeUInt(p.Value[i]);
            codes[p.Key] = cs;
        }
    }

    private static uint[] Create(Func<uint> rnd)
    {
        uint[] data = new uint[1000_000];
        for (int i = 0; i < data.Length; i++)
            data[i] = rnd();
        return data;
    }

    public static uint ExponentialDistributionRandom(uint sigma)
        => (uint)Math.Round(-Math.Log(1 - Random.Shared.NextDouble()) * sigma);

    public static uint FoldedNormalDistributionRandom(uint sigma)
    {
        // https://en.wikipedia.org/wiki/Folded_normal_distribution
        double s = sigma;
        var normalSigma = Math.Sqrt(s * s / (1 - 2 / Math.PI));
        double u1, u2;
        do
        {
            u1 = Random.Shared.NextDouble();
        }
        while (u1 == 0);
        u2 = Random.Shared.NextDouble();

        var mag = normalSigma * Math.Sqrt(-2.0 * Math.Log(u1));
        return (uint)Math.Abs(mag * Math.Cos(2 * Math.PI * u2));
    }
}
