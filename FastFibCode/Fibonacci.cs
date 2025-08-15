using System;
using System.Runtime.CompilerServices;

namespace FastFibCode;

public static class Fibonacci
{
    private const int L = 2583;
    private static UShort2583 codes;
    private static UInt2584 values16;
    private static ULong2584 values32;
    private static ULong2584 values48;
    private static ULong2584 values64;
    private static ULong2584 values80;
    private static UShort65536 valueByCode;

    static Fibonacci()
    {
        // init tables
        for (uint i = 0; i < L; i++)
        {
            var v = i + 1;
            var code = (ushort)FibonacciSlow.EncodeUInt(v, false);
            codes[i] = code;
            values16[i] = (uint)FibonacciSlow.DecodeAsULong(((UInt128)code) << 16, false);
            values32[i] = FibonacciSlow.DecodeAsULong(((UInt128)code) << 32, false);
            values48[i] = FibonacciSlow.DecodeAsULong(((UInt128)code) << 48, false);
            values64[i] = FibonacciSlow.DecodeAsULong(((UInt128)code) << 64, false);
            values80[i] = FibonacciSlow.DecodeAsULong(((UInt128)code) << 80, false);
        }

        values16[L] = uint.MaxValue;
        values32[L] = ulong.MaxValue;
        values48[L] = ulong.MaxValue;
        values64[L] = ulong.MaxValue;
        values80[L] = ulong.MaxValue;

        //all codes including the codes with several ones in a row
        for (uint code = 0; code <= ushort.MaxValue; code++)
        {
            valueByCode[code] = (ushort)FibonacciSlow.DecodeAsUInt(code, false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 EncodeULong(ulong value)
    {
        value++;

        if (value < F49)
        {
            ulong result = 0;

            if (value >= F33)
            {
                var i = (uint)Math.BigMul(k32, value, out var low) - 1 + (uint)(low >> 63);
                i -= (values32[i] > value).ToByte();
                value -= values32[i];
                result |= (ulong)codes[i] << 32;
            }

            if (value >= F17)
            {
                var i = (uint)Math.BigMul(k16, value, out var low) - 1 + (uint)(low >> 63);
                i -= (values16[i] > value).ToByte();
                value -= values16[i];
                result |= (ulong)codes[i] << 16;
            }

            if (value != 0)
                result |= codes[(uint)value - 1];

            return result;
        }
        else
        {
            return EncodeULongSlowPath(value);
        }
    }

    private static UInt128 EncodeULongSlowPath(ulong value)
    {
        ulong upper = 0;
        ulong lower = 0;

        // see https://ceur-ws.org/Vol-567/paper14.pdf for details
        // 1/psi^shift numbers are precalculated for every segment and stored as long numbers after multiplication by 2^64
        if (value >= F81)
        {
            var i = (uint)Math.BigMul(k80, value, out var low) - 1 + (uint)(low >> 63);
            i -= (values80[i] > value).ToByte();
            value -= values80[i];
            upper |= (ulong)codes[i] << (80 - 64);
        }

        if (value >= F65)
        {
            var i = (uint)Math.BigMul(k64, value, out var low) - 1 + (uint)(low >> 63);
            i -= (values64[i] > value).ToByte();
            value -= values64[i];
            upper |= codes[i];
        }

        if (value >= F49)
        {
            var i = (uint)Math.BigMul(k48, value, out var low) - 1 + (uint)(low >> 63);
            i -= (values48[i] > value).ToByte();
            value -= values48[i];
            lower |= (ulong)codes[i] << 48;
        }

        if (value >= F33)
        {
            var i = (uint)Math.BigMul(k32, value, out var low) - 1 + (uint)(low >> 63);
            i -= (values32[i] > value).ToByte();
            value -= values32[i];
            lower |= (ulong)codes[i] << 32;
        }

        if (value >= F17)
        {
            var i = (uint)Math.BigMul(k16, value, out var low) - 1 + (uint)(low >> 63);
            i -= (values16[i] > value).ToByte();
            value -= values16[i];
            lower |= (ulong)codes[i] << 16;
        }

        if (value != 0)
            lower |= codes[(uint)value - 1];

        return new UInt128(upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong EncodeUInt(uint value)
    {
        ulong v = value;
        v++;
        ulong result = 0;

        if (v >= F33)
        {
            var i = (uint)Math.BigMul(k32, v, out var low) - 1 + (uint)(low >> 63);
            i -= (values32[i] > v).ToByte();
            v -= (uint)values32[i];
            result |= (ulong)codes[i] << 32;
        }

        if (v >= F17)
        {
            var i = (uint)Math.BigMul(k16, v, out var low) - 1 + (uint)(low >> 63);
            i -= (values16[i] > v).ToByte();
            v -= values16[i];
            result |= (ulong)codes[i] << 16;
        }

        if (v != 0)
            result |= codes[(uint)v - 1];

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint EncodeUShort(ushort value)
    {
        uint v = value;
        v++;

        uint result = 0;

        if (v >= F17)
        {
            var i = (uint)Math.BigMul(k16, v, out var low) - 1 + (uint)(low >> 63);
            i -= (values16[i] > v).ToByte();
            v -= (ushort)values16[i];
            result |= (uint)codes[i] << 16;
        }

        if (v != 0)
            result |= codes[v - 1];

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort EncodeByte(byte value)
    {
        return codes[value];
    }

    public static ulong DecodeAsULong(UInt128 code)
    {
        var (upper, lower) = code.Split();
        ulong result = valueByCode[(ushort)lower];
        result += DecodeSegment((ushort)(lower >> 16), F16, F15);
        result += DecodeSegment((ushort)(lower >> 32), F32, F31);
        result += DecodeSegment((ushort)(lower >> 48), F48, F47);
        if (upper != 0)
        {
            result += DecodeSegment((ushort)upper, F64, F63);
            result += DecodeSegment((ushort)(upper >> 16), F80, F79);
        }
        result--;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint DecodeAsUInt(ulong code)
    {
        ulong result = valueByCode[(ushort)code];
        result += DecodeSegment((ushort)(code >> 16), F16, F15);
        result += DecodeSegment((ushort)(code >> 32), F32, F31);
        result--;
        return (uint)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort DecodeAsUShort(uint code)
    {
        ulong result = valueByCode[(ushort)code];
        result += DecodeSegment((ushort)(code >> 16), F16, F15);
        result--;
        return (ushort)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte DecodeAsByte(ushort code)
    {
        uint result = valueByCode[code];
        result--;
        return (byte)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong DecodeSegment(ushort code, ulong Fk, ulong Fkm1)
    {
        if (code == 0) return 0;
        var codeUInt = (uint)code;
        // great formula from https://arxiv.org/pdf/0712.0811 :
        // V(F(n) <<F k) = F[k] * V(F(n)) + F[k-1] * V(F(n) >>F 1)
        ulong v1 = valueByCode[codeUInt];
        ulong v2 = valueByCode[codeUInt >> 1] + (codeUInt & 1);
        return Fk * v1 + Fkm1 * v2;
    }

    private unsafe struct UShort2583
    {
        public fixed ushort P[2583];
        public ref ushort this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref P[0], index);
        }
    }

    private unsafe struct UInt2584
    {
        public fixed uint P[2584];
        public ref uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref P[0], index);
        }
    }

    private unsafe struct ULong2584
    {
        public fixed ulong P[2584];
        public ref ulong this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref P[0], index);
        }
    }

    private unsafe struct UShort65536
    {
        public fixed ushort P[65536];
        public ref ushort this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref P[0], index);
        }
    }


    //private static readonly double phi = (1 + Math.Sqrt(5)) / 2;
    //private static readonly ulong k16 = (ulong)Math.Round((double)(UInt128.One << 64) / Math.Pow(phi, 16));
    //private static readonly ulong k32 = (ulong)Math.Round((double)(UInt128.One << 64) / Math.Pow(phi, 32));
    //private static readonly ulong k48 = (ulong)Math.Round((double)(UInt128.One << 64) / Math.Pow(phi, 48));
    //private static readonly ulong k64 = (ulong)Math.Round((double)(UInt128.One << 64) / Math.Pow(phi, 64));
    //private static readonly ulong k80 = (ulong)Math.Round((double)(UInt128.One << 64) / Math.Pow(phi, 80));

    private const ulong k16 = 8358290829580121;
    private const ulong k32 = 3787173785937;
    private const ulong k48 = 1715983037;
    private const ulong k64 = 777519;
    private const ulong k80 = 352;

    #region Fibonacci numbers
    public const ulong F1 = 1;
    public const ulong F2 = 2;
    public const ulong F3 = 3;
    public const ulong F4 = 5;
    public const ulong F5 = 8;
    public const ulong F6 = 13;
    public const ulong F7 = 21;
    public const ulong F8 = 34;
    public const ulong F9 = 55;
    public const ulong F10 = 89;
    public const ulong F11 = 144;
    public const ulong F12 = 233;
    public const ulong F13 = 377;
    public const ulong F14 = 610;
    public const ulong F15 = 987;
    public const ulong F16 = 1597;
    public const ulong F17 = 2584;
    public const ulong F18 = 4181;
    public const ulong F19 = 6765;
    public const ulong F20 = 10946;
    public const ulong F21 = 17711;
    public const ulong F22 = 28657;
    public const ulong F23 = 46368;
    public const ulong F24 = 75025;
    public const ulong F25 = 121393;
    public const ulong F26 = 196418;
    public const ulong F27 = 317811;
    public const ulong F28 = 514229;
    public const ulong F29 = 832040;
    public const ulong F30 = 1346269;
    public const ulong F31 = 2178309;
    public const ulong F32 = 3524578;
    public const ulong F33 = 5702887;
    public const ulong F34 = 9227465;
    public const ulong F35 = 14930352;
    public const ulong F36 = 24157817;
    public const ulong F37 = 39088169;
    public const ulong F38 = 63245986;
    public const ulong F39 = 102334155;
    public const ulong F40 = 165580141;
    public const ulong F41 = 267914296;
    public const ulong F42 = 433494437;
    public const ulong F43 = 701408733;
    public const ulong F44 = 1134903170;
    public const ulong F45 = 1836311903;
    public const ulong F46 = 2971215073;
    public const ulong F47 = 4807526976;
    public const ulong F48 = 7778742049;
    public const ulong F49 = 12586269025;
    public const ulong F50 = 20365011074;
    public const ulong F51 = 32951280099;
    public const ulong F52 = 53316291173;
    public const ulong F53 = 86267571272;
    public const ulong F54 = 139583862445;
    public const ulong F55 = 225851433717;
    public const ulong F56 = 365435296162;
    public const ulong F57 = 591286729879;
    public const ulong F58 = 956722026041;
    public const ulong F59 = 1548008755920;
    public const ulong F60 = 2504730781961;
    public const ulong F61 = 4052739537881;
    public const ulong F62 = 6557470319842;
    public const ulong F63 = 10610209857723;
    public const ulong F64 = 17167680177565;
    public const ulong F65 = 27777890035288;
    public const ulong F66 = 44945570212853;
    public const ulong F67 = 72723460248141;
    public const ulong F68 = 117669030460994;
    public const ulong F69 = 190392490709135;
    public const ulong F70 = 308061521170129;
    public const ulong F71 = 498454011879264;
    public const ulong F72 = 806515533049393;
    public const ulong F73 = 1304969544928657;
    public const ulong F74 = 2111485077978050;
    public const ulong F75 = 3416454622906707;
    public const ulong F76 = 5527939700884757;
    public const ulong F77 = 8944394323791464;
    public const ulong F78 = 14472334024676221;
    public const ulong F79 = 23416728348467685;
    public const ulong F80 = 37889062373143906;
    public const ulong F81 = 61305790721611591;
    public const ulong F82 = 99194853094755497;
    public const ulong F83 = 160500643816367088;
    public const ulong F84 = 259695496911122585;
    public const ulong F85 = 420196140727489673;
    public const ulong F86 = 679891637638612258;
    public const ulong F87 = 1100087778366101931;
    public const ulong F88 = 1779979416004714189;
    public const ulong F89 = 2880067194370816120;
    public const ulong F90 = 4660046610375530309;
    public const ulong F91 = 7540113804746346429;
    public const ulong F92 = 12200160415121876738;
    #endregion
}
