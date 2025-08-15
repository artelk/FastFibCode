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
