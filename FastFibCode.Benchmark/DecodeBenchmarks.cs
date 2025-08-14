using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

[InvocationCount(10)]
public class DecodeBenchmarks
{
    [ParamsAllValues]
    public Distribution Distribution { get; set; }

    private ulong[] codes;

    [GlobalSetup]
    public void GlobalSetup()
    {
        DistributionData.Init();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        codes = DistributionData.GetCodes(Distribution);
    }

    [Benchmark(Baseline = true)]
    public uint DecodeConventional()
    {
        uint r = 0;
        foreach (var v in codes)
            r ^= FibonacciSlow.DecodeAsUInt(v, true);
        return r;
    }

    [Benchmark]
    public uint DecodeFast()
    {
        uint r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsUInt(v);
        return r;
    }
}
