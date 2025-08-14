using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

public class DecodeUShortBenchmarks
{
    private static uint[] codes = new uint[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < codes.Length; i++)
        {
            codes[i] = Fibonacci.EncodeUShort((ushort)Random.Shared.Next(ushort.MaxValue));
        }
    }

    [Benchmark(Baseline = true)]
    public uint DecodeAsUInt()
    {
        uint r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsUInt(v);
        return r;
    }

    [Benchmark]
    public ushort DecodeAsUShort()
    {
        ushort r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsUShort(v);
        return r;
    }
}
