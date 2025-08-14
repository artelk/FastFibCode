using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;
public class EncodeUIntBenchmarks
{
    private static uint[] data = new uint[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (uint)Random.Shared.Next();
        }
    }

    [Benchmark(Baseline = true)]
    public UInt128 EncodeULong()
    {
        UInt128 r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeULong(v);
        return r;
    }

    [Benchmark]
    public ulong EncodeUInt()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUInt(v);
        return r;
    }
}
