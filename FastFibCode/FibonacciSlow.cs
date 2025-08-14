using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FastFibCode;

internal static class FibonacciSlow
{
    internal static ULong92 F;

    static FibonacciSlow()
    {
        F[0] = 1; F[1] = 2;
        for (int i = 2; i < 92; i++)
            F[i] = F[i - 1] + F[i - 2];
    }

    internal static UInt128 EncodeULong(ulong value, bool increment)
    {
        if (increment) value++;
        UInt128 result = 0;
        var log2 = BitOperations.Log2(value);
        var maxIndex = log2 + (log2 >> 1) + 1;
        maxIndex = Math.Min(maxIndex, 91);
        for (int i = maxIndex; i >= 0; i--)
        {
            if (value == 0) return result;
            var f = F[i];
            if (value >= f)
            {
                result |= UInt128.One << i;
                value -= f;
                i--; // cannot have two ones in a row
            }
        }

        return result;
    }

    internal static ulong EncodeUInt(uint value, bool increment)
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
                i--; // cannot have two ones in a row
            }
        }

        return result;
    }

    internal static ulong DecodeAsULong(UInt128 code, bool decrement)
    {
        ulong result = 0;
        var maxIndex = (int)UInt128.Log2(code) + 1;
        maxIndex = Math.Min(maxIndex, 91);
        for (int i = 0; i <= maxIndex; i++)
        {
            if (code == 0) break;
            var f = F[i];
            if ((code & (UInt128.One << i)) != 0)
            {
                result += f;
                code -= UInt128.One << i;
                i++; // cannot have two ones in a row
            }
        }
        if (decrement) result--;
        return result;
    }

    internal static uint DecodeAsUInt(ulong code, bool decrement)
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
                i++; // cannot have two ones in a row
            }
        }
        if (decrement) result--;
        return result;
    }

    internal unsafe struct ULong92
    {
        public fixed ulong P[92];
        public ref ulong this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref P[0], index);
        }
    }
}
