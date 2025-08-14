using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

public class DecodeUIntBenchmarks
{
    private static ulong[] codes = new ulong[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < codes.Length; i++)
        {
            codes[i] = Fibonacci.EncodeUInt((uint)Random.Shared.Next());
        }
    }

    [Benchmark(Baseline = true)]
    public ulong DecodeAsULong()
    {
        ulong r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsULong(v);
        return r;
    }

    [Benchmark]
    public uint DecodeAsUInt()
    {
        uint r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsUInt(v);
        return r;
    }
}
