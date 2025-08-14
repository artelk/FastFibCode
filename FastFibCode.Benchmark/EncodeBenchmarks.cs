using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

[InvocationCount(10)]
public class EncodeBenchmarks
{
    [ParamsAllValues]
    public Distribution Distribution { get; set; }

    private uint[] data;

    [GlobalSetup]
    public void GlobalSetup()
    {
        DistributionData.Init();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        data = DistributionData.GetValues(Distribution);
    }

    [Benchmark(Baseline = true)]
    public ulong EncodeConventional()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= FibonacciSlow.EncodeUInt(v, true);
        return r;
    }

    [Benchmark]
    public ulong EncodeFast()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUInt(v);
        return r;
    }
}
