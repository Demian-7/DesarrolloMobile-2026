using System;
using UnityEngine;

namespace Game.Foundation
{
    [Serializable]
    public struct CurrencyValue : IComparable<CurrencyValue>, IEquatable<CurrencyValue>
    {
        [field: SerializeField] public double Mantissa { get; private set; }
        [field: SerializeField] public int Exponent { get; private set; }

        public static readonly CurrencyValue Zero = new(0);

        private const double Epsilon = 1e-10;

        private static readonly string[] Suffixes =
        {
            "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd",
            "Spd", "Ocd", "Nod", "Vg"
        };

        public CurrencyValue(double mantissa, int exponent = 0)
        {
            if (mantissa == 0)
            {
                Mantissa = 0;
                Exponent = 0;
                return;
            }

            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
        }

        private void Normalize()
        {
            if (Mantissa == 0)
            {
                Exponent = 0;
                return;
            }

            int expChange = (int)Math.Floor(Math.Log10(Math.Abs(Mantissa)));
            if (expChange != 0)
            {
                Mantissa /= Math.Pow(10, expChange);
                Exponent += expChange;
            }

            // Guard against 9.99999999999 caused by floating point error.
            if (Math.Abs(Mantissa) >= 10d - 1e-11)
            {
                Mantissa = Math.Sign(Mantissa);
                Exponent++;
            }
        }

        private CurrencyValue Normalized()
            => Mantissa == 0 ? Zero : new CurrencyValue(Mantissa, Exponent);

        public static CurrencyValue operator +(CurrencyValue a, CurrencyValue b)
        {
            a = a.Normalized();
            b = b.Normalized();

            if (a.Mantissa == 0)
                return b;

            if (b.Mantissa == 0)
                return a;

            int expDiff = a.Exponent - b.Exponent;
            if (Math.Abs(expDiff) > 15)
                return a.Exponent > b.Exponent ? a : b;

            if (expDiff >= 0)
            {
                return new CurrencyValue(a.Mantissa + b.Mantissa * Math.Pow(10, -expDiff), a.Exponent);
            }
            else
            {
                return new CurrencyValue(a.Mantissa * Math.Pow(10, expDiff) + b.Mantissa, b.Exponent);
            }
        }

        public static CurrencyValue operator -(CurrencyValue a, CurrencyValue b)
        {
            a = a.Normalized();
            b = b.Normalized();

            if (b.Mantissa == 0)
                return a;

            if (a.Mantissa == 0)
                return new CurrencyValue(-b.Mantissa, b.Exponent);

            int expDiff = a.Exponent - b.Exponent;
            if (expDiff > 15)
                return a; // B is negligible
            if (expDiff < -15)
                return new CurrencyValue(-b.Mantissa, b.Exponent); // A is negligible

            if (expDiff >= 0)
            {
                double diff = a.Mantissa - b.Mantissa * Math.Pow(10, -expDiff);
                if (Math.Abs(diff) < 1e-12 * Math.Abs(a.Mantissa)) return Zero;
                return new CurrencyValue(diff, a.Exponent);
            }
            else
            {
                double diff = a.Mantissa * Math.Pow(10, expDiff) - b.Mantissa;
                if (Math.Abs(diff) < 1e-12 * Math.Abs(b.Mantissa)) return Zero;
                return new CurrencyValue(diff, b.Exponent);
            }
        }

        public static CurrencyValue operator *(CurrencyValue a, double multiplier)
        {
            a = a.Normalized();

            if (multiplier == 0 || a.Mantissa == 0)
                return Zero;

            return new CurrencyValue(a.Mantissa * multiplier, a.Exponent);
        }

        public static CurrencyValue Max(CurrencyValue a, CurrencyValue b)
        {
            return a >= b ? a.Normalized() : b.Normalized();
        }

        public static bool operator >=(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) >= 0;
        public static bool operator <=(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) <= 0;
        public static bool operator >(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) > 0;
        public static bool operator <(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) < 0;

        public int CompareTo(CurrencyValue other)
        {
            CurrencyValue a = Normalized();
            CurrencyValue b = other.Normalized();

            int signA = Math.Sign(a.Mantissa);
            int signB = Math.Sign(b.Mantissa);

            if (signA != signB)
                return signA.CompareTo(signB);

            if (signA == 0)
                return 0;

            if (a.Exponent != b.Exponent)
            {
                int byExponent = a.Exponent.CompareTo(b.Exponent);
                return signA > 0 ? byExponent : -byExponent;
            }

            if (Math.Abs(a.Mantissa - b.Mantissa) < Epsilon)
                return 0;

            return a.Mantissa.CompareTo(b.Mantissa);
        }

        public bool Equals(CurrencyValue other) => CompareTo(other) == 0;

        public override bool Equals(object obj) => obj is CurrencyValue other && Equals(other);

        public override int GetHashCode()
        {
            CurrencyValue n = Normalized();
            return HashCode.Combine(Math.Round(n.Mantissa, 9), n.Exponent);
        }

        public override string ToString()
        {
            return ToString(NotationType.Standard);
        }

        public string ToString(NotationType notation)
        {
            CurrencyValue v = Normalized();

            if (v.Mantissa == 0)
                return "0";

            if (v.Exponent < 3 && notation != NotationType.OoM)
            {
                return (v.Mantissa * Math.Pow(10, v.Exponent)).ToString("F0");
            }

            switch (notation)
            {
                case NotationType.Standard:
                    int suffixIndex = v.Exponent / 3;
                    int shift = v.Exponent % 3;
                    double displayMantissa = v.Mantissa * Math.Pow(10, shift);

                    if (suffixIndex < Suffixes.Length)
                    {
                        return $"{displayMantissa:F0}{Suffixes[suffixIndex]}";
                    }

                    return $"{v.Mantissa:F0}e{v.Exponent}"; // Fallback if out of suffixes

                case NotationType.Engineering:
                    int engShift = v.Exponent % 3;
                    double engMantissa = v.Mantissa * Math.Pow(10, engShift);
                    int engExponent = v.Exponent - engShift;
                    return $"{engMantissa:F0}e{engExponent}";

                case NotationType.OoM:
                    double logValue = v.Exponent + Math.Log10(v.Mantissa);
                    return $"e{logValue:F3}";

                case NotationType.Scientific:
                default:
                    return $"{v.Mantissa:F0}e{v.Exponent}";
            }
        }
    }
}