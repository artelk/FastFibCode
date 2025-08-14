namespace FastFibCode.Tests;

public class FibonacciTests
{
    [Test]
    public void Test()
    {
        Check(ulong.MaxValue - 1);
        Check(uint.MaxValue);
        Check(ushort.MaxValue);
        Check(byte.MaxValue);

        for (ulong v = 1; v < 100_000; v++)
        {
            Check(v);
            Check(ulong.MaxValue - v);
        }

        for (int i = 1; i < 92; i++)
        {
            var f = FibonacciSlow.F[i];
            Check(f);
            Check(f - 1);
            Check(f + 1);
        }

        for (int stepShift = 1; stepShift <= 16; stepShift++)
            TestDecreasing(stepShift);

        for (int i = 0; i < 100_000; i++)
        {
            var rnd = (ulong)Random.Shared.NextInt64();
            var v1 = rnd + 1;
            var v2 = ~rnd;
            Check(v1);
            Check(v2);
        }
    }

    [Test]
    public void TestByte()
    {
        for (int i = 0; i <= byte.MaxValue; i++)
        {
            byte v = (byte)i;
            var encoded = Fibonacci.EncodeByte(v);
            Assert.That((UInt128)encoded, Is.EqualTo(Fibonacci.EncodeULong(v)), $"[v = {v}]");
            Assert.That(Fibonacci.DecodeAsByte(encoded), Is.EqualTo(v), $"[v = {v}]");
        }
    }

    [Test]
    public void TestUShort()
    {
        for (int i = 0; i <= ushort.MaxValue; i++)
        {
            ushort v = (ushort)i;
            var encoded = Fibonacci.EncodeUShort(v);
            Assert.That((UInt128)encoded, Is.EqualTo(Fibonacci.EncodeULong(v)), $"[v = {v}]");
            Assert.That(Fibonacci.DecodeAsUShort(encoded), Is.EqualTo(v), $"[v = {v}]");
        }
    }

    [Test]
    public void TestUInt()
    {
        for (long i = 0; i <= uint.MaxValue; i += 1000)
        {
            uint v = (uint)i;
            var encoded = Fibonacci.EncodeUInt(v);
            Assert.That((UInt128)encoded, Is.EqualTo(Fibonacci.EncodeULong(v)), $"[v = {v}]");
            Assert.That(Fibonacci.DecodeAsUInt(encoded), Is.EqualTo(v), $"[v = {v}]");
        }

        {
            uint v = uint.MaxValue;
            var encoded = Fibonacci.EncodeUInt(v);
            Assert.That((UInt128)encoded, Is.EqualTo(Fibonacci.EncodeULong(v)), $"[v = {v}]");
            Assert.That(Fibonacci.DecodeAsUInt(encoded), Is.EqualTo(v), $"[v = {v}]");
        }
    }

    private static void Check(ulong v)
    {
        {
            var encodedSlow = FibonacciSlow.EncodeULong(v, true);
            var encodedFast = Fibonacci.EncodeULong(v);
            Assert.That(encodedFast, Is.EqualTo(encodedSlow), $"[v = {v}]");

            var decodedSlow = FibonacciSlow.DecodeAsULong(encodedSlow, true);
            var decodedFast = Fibonacci.DecodeAsULong(encodedFast);
            Assert.That(decodedFast, Is.EqualTo(decodedSlow), $"[v = {v}]");

            Assert.That(decodedFast, Is.EqualTo(v), $"[v = {v}]");
        }

        if (v <= uint.MaxValue)
        {
            var x = (uint)v;
            var encodedSlow = FibonacciSlow.EncodeUInt(x, true);
            var encodedFast = Fibonacci.EncodeUInt(x);
            Assert.That(encodedFast, Is.EqualTo(encodedSlow), $"[x = {x}]");

            var decodedSlow = FibonacciSlow.DecodeAsUInt(encodedSlow, true);
            var decodedFast = Fibonacci.DecodeAsUInt(encodedFast);
            Assert.That(decodedFast, Is.EqualTo(decodedSlow), $"[x = {x}]");

            Assert.That(decodedFast, Is.EqualTo(x), $"[x = {x}]");
        }
    }

    private static void TestDecreasing(int stepShift)
    {
        var v = ulong.MaxValue;
        while (true)
        {
            var diff = v >> stepShift;
            if (diff == 0) break;
            v -= diff;
            Check(v);
        }
    }
}
