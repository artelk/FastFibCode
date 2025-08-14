using BenchmarkDotNet.Running;

namespace FastFibCode.Benchmark;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<EncodeBenchmarks>();
        BenchmarkRunner.Run<EncodeUIntBenchmarks>();
        BenchmarkRunner.Run<EncodeUShortBenchmarks>();
        BenchmarkRunner.Run<EncodeByteBenchmarks>();

        BenchmarkRunner.Run<DecodeBenchmarks>();
        BenchmarkRunner.Run<DecodeUIntBenchmarks>();
        BenchmarkRunner.Run<DecodeUShortBenchmarks>();
        BenchmarkRunner.Run<DecodeByteBenchmarks>();

        BenchmarkRunner.Run<CompareWithEliasDelta>();
    }
}
