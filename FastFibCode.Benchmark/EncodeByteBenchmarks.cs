using BenchmarkDotNet.Attributes;

namespace FastFibCode.Benchmark;

public class EncodeByteBenchmarks
{
    private static byte[] data = new byte[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)Random.Shared.Next(256);
        }
    }

    [Benchmark(Baseline = true)]
    public uint EncodeUShort()
    {
        uint r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUShort(v);
        return r;
    }

    [Benchmark]
    public ushort EncodeByte()
    {
        ushort r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeByte(v);
        return r;
    }
}
