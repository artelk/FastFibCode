[![NuGet](https://img.shields.io/nuget/v/FastFibCode)](https://www.nuget.org/packages/FastFibCode) 

# FastFibCode
Ultra-fast [Fibonacci encoding/decoding](https://en.wikipedia.org/wiki/Fibonacci_coding) methods.
The implementation is based on the [Fast Fibonacci Encoding Algorithm](https://ceur-ws.org/Vol-567/paper14.pdf) and the [The Fast Fibonacci Decompression Algorithm](https://arxiv.org/pdf/0712.0811), with additional optimizations to achieve even better performance. It is **10–30** times faster than the conventional algorithm (depending on the value distribution).

# Basic usage

> [!NOTE]
> **All values are internally incremented during encoding and decremented during decoding to allow the representation of zero. The maximum supported value is $2^{64}-2$.**

```cs
using static FastFibCode.Fibonacci;

UInt128 codeForULong = EncodeULong(0x0102030405060708);
ulong codeForUInt = EncodeUInt(0x01020304);
uint codeForShort = EncodeUShort(0x0102);
ushort codeForByte = EncodeByte(0x01);

MyFavoriteBitWriter bw = ...;

// assume lower bits are written first
bw.WriteBits(codeForULong, (int)UInt128.Log2(codeForULong) + 1);
bw.WriteBit(1); // "delimiter"

bw.WriteBits(codeForUInt, BitOperations.Log2(codeForUInt) + 1);
bw.WriteBit(1);

bw.WriteBits(codeForShort, BitOperations.Log2(codeForShort) + 1);
bw.WriteBit(1);

bw.WriteBits(codeForByte, BitOperations.Log2(codeForByte) + 1);
bw.WriteBit(1);

// =============================================================

MyFavoriteBitReader br = ...;

UInt128 code = br.ReadUpToDoubleOnes(); // with final '11'
code ^= UInt128.One << (int)UInt128.Log2(codeForULong); // clear the delimiter '1' bit
ulong value = DecodeAsULong(code);

// ...
```

# Benchmark

A comparison with conventional encoding/decoding algorithms was performed for various value distributions: Folded Normal (absolute value of a normal distribution), Exponential distribution, and Uniform distribution.
Each test processed an array of $10^6$ random values (or their Fibonacci codes) with the given distribution and standard deviation (shown as suffixes in the Distribution column). For the Uniform distribution, values were generated in the range $0 .. 5 \times 10^6$.


<details>
  <summary>The conventional algorithms used for comparison</summary>

```cs
ulong EncodeUInt(uint value, bool increment)
{
    ulong v = value;
    if (increment) v++;
    ulong result = 0;
    var log2 = BitOperations.Log2(v);
    var maxIndex = log2 + (log2 >> 1) + 1;
    maxIndex = Math.Min(maxIndex, 45);
    for (int i = maxIndex; i >= 0; i--)
    {
        if (v == 0) return result;
        var f = (uint)F[i];
        if (v >= f)
        {
            result |= 1UL << i;
            v -= f;
            i--;
        }
    }

    return result;
}

uint DecodeAsUInt(ulong code, bool decrement)
{
    uint result = 0;
    var maxIndex = BitOperations.Log2(code) + 1;
    maxIndex = Math.Min(maxIndex, 45);
    for (int i = 0; i <= maxIndex; i++)
    {
        var f = (uint)F[i];
        if ((code & (1UL << i)) != 0)
        {
            result += f;
            i++;
        }
    }
    if (decrement) result--;
    return result;
}
```
</details>


<details>
  <summary>Encoding benchmark results</summary>

```
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.302
  [Host]     : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2
  Job-ZJFXOJ : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2
```
| Method             | Distribution      | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD |
|------------------- |------------------ |-----------:|----------:|----------:|-----------:|------:|--------:|
| EncodeConventional | FoldedNormal_100  |  39.102 ms | 0.7629 ms | 0.8163 ms |  38.901 ms |  1.00 |    0.03 |
| EncodeFast         | FoldedNormal_100  |   1.542 ms | 0.0557 ms | 0.1607 ms |   1.489 ms |  0.04 |    0.00 |
| EncodeConventional | FoldedNormal_1K   |  54.080 ms | 1.0371 ms | 1.2346 ms |  53.907 ms |  1.00 |    0.03 |
| EncodeFast         | FoldedNormal_1K   |   2.906 ms | 0.0506 ms | 0.0846 ms |   2.902 ms |  0.05 |    0.00 |
| EncodeConventional | FoldedNormal_10K  |  69.528 ms | 1.2526 ms | 1.1716 ms |  69.309 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_10K  |   4.413 ms | 0.0856 ms | 0.1083 ms |   4.424 ms |  0.06 |    0.00 |
| EncodeConventional | FoldedNormal_100K |  84.035 ms | 1.5328 ms | 1.5055 ms |  84.026 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_100K |   3.906 ms | 0.0746 ms | 0.0944 ms |   3.898 ms |  0.05 |    0.00 |
| EncodeConventional | FoldedNormal_1M   |  98.920 ms | 1.8442 ms | 1.7251 ms |  98.421 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_1M   |   3.941 ms | 0.0776 ms | 0.0924 ms |   3.958 ms |  0.04 |    0.00 |
| EncodeConventional | FoldedNormal_10M  | 114.072 ms | 1.8719 ms | 1.7510 ms | 114.155 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_10M  |   8.203 ms | 0.1287 ms | 0.1264 ms |   8.207 ms |  0.07 |    0.00 |
| EncodeConventional | Exponential_100   |  35.178 ms | 0.6949 ms | 1.1611 ms |  34.747 ms |  1.00 |    0.05 |
| EncodeFast         | Exponential_100   |   1.537 ms | 0.0385 ms | 0.1092 ms |   1.509 ms |  0.04 |    0.00 |
| EncodeConventional | Exponential_1K    |  51.469 ms | 0.9935 ms | 1.0203 ms |  51.420 ms |  1.00 |    0.03 |
| EncodeFast         | Exponential_1K    |   2.369 ms | 0.0456 ms | 0.0856 ms |   2.371 ms |  0.05 |    0.00 |
| EncodeConventional | Exponential_10K   |  65.755 ms | 1.1349 ms | 1.0616 ms |  65.705 ms |  1.00 |    0.02 |
| EncodeFast         | Exponential_10K   |   5.003 ms | 0.0986 ms | 0.1211 ms |   4.995 ms |  0.08 |    0.00 |
| EncodeConventional | Exponential_100K  |  80.600 ms | 1.2722 ms | 1.1900 ms |  80.906 ms |  1.00 |    0.02 |
| EncodeFast         | Exponential_100K  |   4.007 ms | 0.0782 ms | 0.0931 ms |   4.004 ms |  0.05 |    0.00 |
| EncodeConventional | Exponential_1M    |  95.661 ms | 1.5023 ms | 1.3317 ms |  95.957 ms |  1.00 |    0.02 |
| EncodeFast         | Exponential_1M    |   3.930 ms | 0.0757 ms | 0.1086 ms |   3.924 ms |  0.04 |    0.00 |
| EncodeConventional | Exponential_10M   | 110.287 ms | 1.8801 ms | 1.7587 ms | 110.172 ms |  1.00 |    0.02 |
| EncodeFast         | Exponential_10M   |   8.518 ms | 0.1262 ms | 0.1596 ms |   8.490 ms |  0.08 |    0.00 |
| EncodeConventional | Uniform_5M        | 103.441 ms | 1.3436 ms | 1.2568 ms | 103.713 ms |  1.00 |    0.02 |
| EncodeFast         | Uniform_5M        |   3.917 ms | 0.0766 ms | 0.1321 ms |   3.908 ms |  0.04 |    0.00 |
</details>

<details>
  <summary>Decoding benchmark results</summary>

```
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.302
  [Host]     : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2
  Job-ZJFXOJ : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2
```
| Method             | Distribution      | Mean       | Error     | StdDev    | Ratio | RatioSD |
|------------------- |------------------ |-----------:|----------:|----------:|------:|--------:|
| DecodeConventional | FoldedNormal_100  | 35.279 ms | 0.3204 ms | 0.2676 ms |  1.00 |    0.01 |
| DecodeFast         | FoldedNormal_100  |  1.813 ms | 0.0450 ms | 0.1319 ms |  0.05 |    0.00 |
| DecodeConventional | FoldedNormal_1K   | 48.935 ms | 0.9005 ms | 0.7520 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_1K   |  3.008 ms | 0.0599 ms | 0.0531 ms |  0.06 |    0.00 |
| DecodeConventional | FoldedNormal_10K  | 61.887 ms | 1.1508 ms | 1.1302 ms |  1.00 |    0.03 |
| DecodeFast         | FoldedNormal_10K  |  3.595 ms | 0.0717 ms | 0.1095 ms |  0.06 |    0.00 |
| DecodeConventional | FoldedNormal_100K | 74.222 ms | 1.4658 ms | 1.4396 ms |  1.00 |    0.03 |
| DecodeFast         | FoldedNormal_100K |  2.686 ms | 0.0528 ms | 0.1191 ms |  0.04 |    0.00 |
| DecodeConventional | FoldedNormal_1M   | 86.271 ms | 1.3631 ms | 1.2750 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_1M   |  2.574 ms | 0.0510 ms | 0.0893 ms |  0.03 |    0.00 |
| DecodeConventional | FoldedNormal_10M  | 99.897 ms | 1.5131 ms | 1.3413 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_10M  |  5.395 ms | 0.1069 ms | 0.1664 ms |  0.05 |    0.00 |
| DecodeConventional | Exponential_100   | 31.101 ms | 0.6039 ms | 0.7637 ms |  1.00 |    0.03 |
| DecodeFast         | Exponential_100   |  1.773 ms | 0.0384 ms | 0.1127 ms |  0.06 |    0.00 |
| DecodeConventional | Exponential_1K    | 46.007 ms | 0.9017 ms | 0.9260 ms |  1.00 |    0.03 |
| DecodeFast         | Exponential_1K    |  2.551 ms | 0.0508 ms | 0.1135 ms |  0.06 |    0.00 |
| DecodeConventional | Exponential_10K   | 59.651 ms | 0.9695 ms | 0.8595 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_10K   |  4.371 ms | 0.0815 ms | 0.0800 ms |  0.07 |    0.00 |
| DecodeConventional | Exponential_100K  | 72.305 ms | 0.9786 ms | 0.7640 ms |  1.00 |    0.01 |
| DecodeFast         | Exponential_100K  |  2.782 ms | 0.0553 ms | 0.0925 ms |  0.04 |    0.00 |
| DecodeConventional | Exponential_1M    | 85.477 ms | 1.5396 ms | 1.4402 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_1M    |  2.621 ms | 0.0514 ms | 0.0815 ms |  0.03 |    0.00 |
| DecodeConventional | Exponential_10M   | 96.975 ms | 1.9232 ms | 1.7990 ms |  1.00 |    0.03 |
| DecodeFast         | Exponential_10M   |  6.630 ms | 0.1161 ms | 0.1192 ms |  0.07 |    0.00 |
| DecodeConventional | Uniform_5M        | 91.944 ms | 1.7965 ms | 2.2063 ms |  1.00 |    0.03 |
| DecodeFast         | Uniform_5M        |  2.635 ms | 0.0519 ms | 0.0881 ms |  0.03 |    0.00 |
</details>
