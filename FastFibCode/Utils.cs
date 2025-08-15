using System;
using System.Runtime.CompilerServices;

namespace FastFibCode;

internal static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (ulong upper, ulong lower) Split(this UInt128 value)
    {
        var pair = Unsafe.BitCast<UInt128, (ulong, ulong)>(value);
        return BitConverter.IsLittleEndian
            ? (pair.Item2, pair.Item1)
            : (pair.Item1, pair.Item2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte ToByte(this bool value) => Unsafe.BitCast<bool, byte>(value);
}
