using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

public class EncodeUShortBenchmarks
{
    private static ushort[] data = new ushort[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (ushort)Random.Shared.Next(ushort.MaxValue);
        }
    }

    [Benchmark(Baseline = true)]
    public ulong EncodeUInt()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUInt(v);
        return r;
    }

    [Benchmark]
    public uint EncodeUShort()
    {
        uint r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUShort(v);
        return r;
    }
}
