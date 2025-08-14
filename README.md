[![NuGet](https://img.shields.io/nuget/v/FastFibCode)](https://www.nuget.org/packages/FastFibCode) 

# FastFibCode
Ultra-fast [Fibonacci encoding/decoding](https://en.wikipedia.org/wiki/Fibonacci_coding) methods.
The implementation is based on the [Fast Fibonacci Encoding Algorithm](https://ceur-ws.org/Vol-567/paper14.pdf) and the [The Fast Fibonacci Decompression Algorithm](https://arxiv.org/pdf/0712.0811), with additional optimizations to achieve even better performance. Encoding is **5–20** times faster than the conventional algorithm, and decoding is **10–30** times faster (depending on the value distribution).

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
        if (code == 0) break;
        var f = (uint)F[i];
        if ((code & (1UL << i)) != 0)
        {
            result += f;
            code -= 1UL << i;
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
| EncodeConventional | FoldedNormal_100  |  38.502 ms | 0.7226 ms | 0.6759 ms |  38.627 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_100  |   1.578 ms | 0.0545 ms | 0.1590 ms |   1.505 ms |  0.04 |    0.00 |
| EncodeConventional | FoldedNormal_1K   |  53.121 ms | 1.0194 ms | 1.2893 ms |  53.399 ms |  1.00 |    0.03 |
| EncodeFast         | FoldedNormal_1K   |   3.425 ms | 0.0665 ms | 0.0792 ms |   3.445 ms |  0.06 |    0.00 |
| EncodeConventional | FoldedNormal_10K  |  69.517 ms | 0.6760 ms | 0.6323 ms |  69.586 ms |  1.00 |    0.01 |
| EncodeFast         | FoldedNormal_10K  |   9.399 ms | 0.0760 ms | 0.0593 ms |   9.408 ms |  0.14 |    0.00 |
| EncodeConventional | FoldedNormal_100K |  83.362 ms | 1.6621 ms | 1.9141 ms |  83.686 ms |  1.00 |    0.03 |
| EncodeFast         | FoldedNormal_100K |   9.326 ms | 0.1662 ms | 0.1555 ms |   9.318 ms |  0.11 |    0.00 |
| EncodeConventional | FoldedNormal_1M   |  96.797 ms | 1.9266 ms | 1.9785 ms |  96.614 ms |  1.00 |    0.03 |
| EncodeFast         | FoldedNormal_1M   |   9.282 ms | 0.1066 ms | 0.0998 ms |   9.324 ms |  0.10 |    0.00 |
| EncodeConventional | FoldedNormal_10M  | 111.931 ms | 2.0003 ms | 1.9646 ms | 111.899 ms |  1.00 |    0.02 |
| EncodeFast         | FoldedNormal_10M  |  17.811 ms | 0.3399 ms | 0.2838 ms |  17.809 ms |  0.16 |    0.00 |
| EncodeConventional | Exponential_100   |  33.897 ms | 0.5218 ms | 0.6408 ms |  33.766 ms |  1.00 |    0.03 |
| EncodeFast         | Exponential_100   |   1.501 ms | 0.0299 ms | 0.0625 ms |   1.491 ms |  0.04 |    0.00 |
| EncodeConventional | Exponential_1K    |  51.299 ms | 0.5524 ms | 0.4897 ms |  51.185 ms |  1.00 |    0.01 |
| EncodeFast         | Exponential_1K    |   2.756 ms | 0.0545 ms | 0.1125 ms |   2.758 ms |  0.05 |    0.00 |
| EncodeConventional | Exponential_10K   |  65.541 ms | 1.2765 ms | 1.7041 ms |  65.485 ms |  1.00 |    0.04 |
| EncodeFast         | Exponential_10K   |   9.293 ms | 0.1809 ms | 0.1777 ms |   9.280 ms |  0.14 |    0.00 |
| EncodeConventional | Exponential_100K  |  78.653 ms | 1.4803 ms | 1.7047 ms |  78.794 ms |  1.00 |    0.03 |
| EncodeFast         | Exponential_100K  |   9.292 ms | 0.1714 ms | 0.1603 ms |   9.240 ms |  0.12 |    0.00 |
| EncodeConventional | Exponential_1M    |  94.293 ms | 1.8825 ms | 2.0143 ms |  94.042 ms |  1.00 |    0.03 |
| EncodeFast         | Exponential_1M    |   9.439 ms | 0.1774 ms | 0.3461 ms |   9.342 ms |  0.10 |    0.00 |
| EncodeConventional | Exponential_10M   | 110.468 ms | 2.1560 ms | 2.1175 ms | 110.368 ms |  1.00 |    0.03 |
| EncodeFast         | Exponential_10M   |  17.423 ms | 0.1855 ms | 0.1822 ms |  17.434 ms |  0.16 |    0.00 |
| EncodeConventional | Uniform_5M        | 102.580 ms | 1.9180 ms | 2.0522 ms | 101.648 ms |  1.00 |    0.03 |
| EncodeFast         | Uniform_5M        |   9.038 ms | 0.1758 ms | 0.1727 ms |   8.992 ms |  0.09 |    0.00 |

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
| DecodeConventional | FoldedNormal_100  |  37.105 ms | 0.6784 ms | 0.6014 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_100  |   1.792 ms | 0.0395 ms | 0.1164 ms |  0.05 |    0.00 |
| DecodeConventional | FoldedNormal_1K   |  51.609 ms | 0.9845 ms | 1.0110 ms |  1.00 |    0.03 |
| DecodeFast         | FoldedNormal_1K   |   3.030 ms | 0.0603 ms | 0.1190 ms |  0.06 |    0.00 |
| DecodeConventional | FoldedNormal_10K  |  64.725 ms | 1.2735 ms | 1.2507 ms |  1.00 |    0.03 |
| DecodeFast         | FoldedNormal_10K  |   3.504 ms | 0.0662 ms | 0.1522 ms |  0.05 |    0.00 |
| DecodeConventional | FoldedNormal_100K |  79.719 ms | 1.4444 ms | 1.2805 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_100K |   2.570 ms | 0.0506 ms | 0.0950 ms |  0.03 |    0.00 |
| DecodeConventional | FoldedNormal_1M   |  92.603 ms | 1.7892 ms | 1.5860 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_1M   |   2.638 ms | 0.0527 ms | 0.1028 ms |  0.03 |    0.00 |
| DecodeConventional | FoldedNormal_10M  | 107.962 ms | 1.3682 ms | 1.2798 ms |  1.00 |    0.02 |
| DecodeFast         | FoldedNormal_10M  |   5.363 ms | 0.1061 ms | 0.1180 ms |  0.05 |    0.00 |
| DecodeConventional | Exponential_100   |  35.046 ms | 0.6799 ms | 0.9531 ms |  1.00 |    0.04 |
| DecodeFast         | Exponential_100   |   1.790 ms | 0.0502 ms | 0.1431 ms |  0.05 |    0.00 |
| DecodeConventional | Exponential_1K    |  50.208 ms | 0.7323 ms | 0.6115 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_1K    |   2.622 ms | 0.0512 ms | 0.1237 ms |  0.05 |    0.00 |
| DecodeConventional | Exponential_10K   |  63.904 ms | 1.2136 ms | 1.1352 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_10K   |   4.355 ms | 0.0871 ms | 0.1069 ms |  0.07 |    0.00 |
| DecodeConventional | Exponential_100K  |  77.443 ms | 1.3332 ms | 1.1133 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_100K  |   2.735 ms | 0.0544 ms | 0.1324 ms |  0.04 |    0.00 |
| DecodeConventional | Exponential_1M    |  91.375 ms | 1.8214 ms | 2.6123 ms |  1.00 |    0.04 |
| DecodeFast         | Exponential_1M    |   2.643 ms | 0.0527 ms | 0.1076 ms |  0.03 |    0.00 |
| DecodeConventional | Exponential_10M   | 104.419 ms | 1.6462 ms | 1.5398 ms |  1.00 |    0.02 |
| DecodeFast         | Exponential_10M   |   6.494 ms | 0.1272 ms | 0.1824 ms |  0.06 |    0.00 |
| DecodeConventional | Uniform_5M        |  96.945 ms | 1.7427 ms | 1.6301 ms |  1.00 |    0.02 |
| DecodeFast         | Uniform_5M        |   2.589 ms | 0.0517 ms | 0.1371 ms |  0.03 |    0.00 |
</details>
