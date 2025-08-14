using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FastFibCode.Benchmark;

public class CompareWithEliasDelta
{
    private static uint[] data = new uint[1_000_000];

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = DistributionData.ExponentialDistributionRandom(1000_000);
        }
    }

    [Benchmark(Baseline = true)]
    public ulong EliasDeltaEncode()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= EliasDeltaEncode(v);
        return r;
    }

    [Benchmark]
    public ulong FibonacciEncode()
    {
        ulong r = 0;
        foreach (var v in data)
            r ^= Fibonacci.EncodeUInt(v);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong EliasDeltaEncode(uint value)
    {
        value++;
        var log2 = BitOperations.Log2(value);
        var logLog2 = BitOperations.Log2((uint)log2 + 1);
        ulong result = ((ulong)log2 + 1) ^ (1UL << logLog2);
        result <<= (logLog2 + 1);
        result |= (1U << logLog2);
        // highest bit not cleared and bit ordering is different but it's ok for the test
        result |= value << (2 * logLog2 + 1);
        return result;
    }
}
