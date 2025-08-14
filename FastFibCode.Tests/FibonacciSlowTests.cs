namespace FastFibCode.Tests;

public class FibonacciSlowTests
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

    private static void Check(ulong v)
    {
        Assert.That(FibonacciSlow.DecodeAsULong(FibonacciSlow.EncodeULong(v, true), true), Is.EqualTo(v), $"[v = {v}]");
        if (v <= uint.MaxValue)
            Assert.That(FibonacciSlow.DecodeAsUInt(FibonacciSlow.EncodeUInt((uint)v, true), true), Is.EqualTo(v), $"[v = {v}]");
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
