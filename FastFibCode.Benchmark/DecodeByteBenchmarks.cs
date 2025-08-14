using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

public class DecodeByteBenchmarks
{
    private static ushort[] codes = new ushort[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < codes.Length; i++)
        {
            codes[i] = Fibonacci.EncodeByte((byte)Random.Shared.Next(256));
        }
    }

    [Benchmark(Baseline = true)]
    public ushort DecodeAsUShort()
    {
        ushort r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsUShort(v);
        return r;
    }

    [Benchmark]
    public byte DecodeAsByte()
    {
        byte r = 0;
        foreach (var v in codes)
            r ^= Fibonacci.DecodeAsByte(v);
        return r;
    }
}
