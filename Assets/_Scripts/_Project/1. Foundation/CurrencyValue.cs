using System;

namespace Game.Foundation
{
    public struct CurrencyValue : IComparable<CurrencyValue>, IEquatable<CurrencyValue>
    {
        public double Mantissa { get; private set; }
        public int Exponent { get; private set; }

        public static readonly CurrencyValue Zero = new(0);

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
        }

        public static CurrencyValue operator +(CurrencyValue a, CurrencyValue b)
        {
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
            if (b.Mantissa == 0)
                return a;

            if (a.Mantissa == 0)
                return new CurrencyValue(-b.Mantissa, b.Exponent);

            int expDiff = a.Exponent - b.Exponent;
            if (expDiff > 15) 
                return a; //B is negligible
            if (expDiff < -15)
                return new CurrencyValue(-b.Mantissa, b.Exponent); //A is negligible
            
            if (expDiff >= 0)
            {
                return new CurrencyValue(a.Mantissa - b.Mantissa * Math.Pow(10, -expDiff), a.Exponent);
            }
            else
            {
                return new CurrencyValue(a.Mantissa * Math.Pow(10, expDiff) - b.Mantissa, b.Exponent);
            }
        }

        public static CurrencyValue operator *(CurrencyValue a, double multiplier)
        {
            if (multiplier == 0 || a.Mantissa == 0)
                return Zero;
            
            return new CurrencyValue(a.Mantissa * multiplier, a.Exponent);
        }

        public static CurrencyValue Max(CurrencyValue a, CurrencyValue b)
        {
            return a >= b ? a : b;
        }

        public static bool operator >=(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) >= 0;
        public static bool operator <=(CurrencyValue a, CurrencyValue b) => a.CompareTo(b) <= 0;

        public int CompareTo(CurrencyValue other)
        {
            if (Mantissa == 0 && other.Mantissa == 0)
                return 0;

            if (Exponent != other.Exponent)
                return Exponent.CompareTo(other.Exponent);

            return Mantissa.CompareTo(other.Mantissa);
        }
        
        public bool Equals(CurrencyValue other)
        {
            return Math.Abs(Mantissa - other.Mantissa) < 1e-10 && Exponent == other.Exponent;
        }

        public override bool Equals(object obj) => obj is CurrencyValue other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Mantissa, Exponent);

        public override string ToString()
        {
            if (Exponent < 3)
                return (Mantissa * Math.Pow(10, Exponent)).ToString("F0");

            return $"{Mantissa:F2}e{Exponent}";
        }
    }
}