using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Engine.Utilities.MathLib
{
    public struct Int2 : IEquatable<Int2>
    {
        
        internal Vector64<int> _backingVec = new Vector64<int>();
        
        public int X
        {
            get => _backingVec.GetElement(0);
            set => _backingVec = _backingVec.WithElement(0, value);
        }
        public int Y         {
            get => _backingVec.GetElement(1);
            set => _backingVec = _backingVec.WithElement(1, value);
        }

        public static readonly Int2 Zero = new Int2(0, 0);

        public Int2(int x, int y)
        {
            _backingVec = Vector64.Create(x, y);
        }
        
        Int2(Vector64<int> backingVec)
        {
            _backingVec = backingVec;
        }
        
        
        public bool Equals(ref Int2 comparison)
        {

            if (Sse2.IsSupported)
            {
                return Sse.MoveMask(Sse2.Xor(_backingVec.ToVector128(), comparison._backingVec.ToVector128()).AsSingle()) != 0;    
            }
            else
            {
                return X == comparison.X && Y == comparison.Y;
            }
        }

        public readonly override int GetHashCode()
        {
            return _backingVec.GetHashCode();
        }

        public static Int2 operator +(Int2 other, Int2 thing)
        {
            if (Sse2.IsSupported)
            {
                return new Int2(Sse2.Add(other._backingVec.ToVector128(), thing._backingVec.ToVector128()).GetLower());
            }
            else
            {
                return new Int2(thing.X + other.X, thing.Y + other.Y);
            }
        }
        
        public static Int2 operator -(Int2 other, Int2 thing)
        {
            if (Sse2.IsSupported)
            {
                return new Int2(Sse2.Subtract(other._backingVec.ToVector128(), thing._backingVec.ToVector128()).GetLower());
            }
            else
            {
                return new Int2(thing.X - other.X, thing.Y - other.Y);
            }
        }
        
        public static Int2 operator +(Int2 other, int integer)
        {
            if (Sse2.IsSupported)
            {
                return new Int2(Sse2.Add(other._backingVec.ToVector128(), Vector128.Create(integer, integer, 0, 0)).GetLower());
            }
            else
            {
                return new Int2(integer + other.X, integer + other.Y);
            }
        }

        public int Length()
        {
            return (int)Math.Floor(Math.Sqrt(LengthSquared()));
        }
        public int LengthSquared()
        {
            return X + Y;
        }

        public static Int2 operator -(Int2 other, int integer)
        {
            if (Sse2.IsSupported)
            {
                return new Int2(Sse2.Subtract(other._backingVec.ToVector128(), Vector128.Create(integer, integer, 0, 0))
                    .GetLower());
            }
            else
            {
                return new Int2(integer - other.X, integer - other.Y);
            }
        }

        public override string ToString()
        {
            return $"X:{X}, Y:{Y}";
        }

        public bool Equals(Int2 other)
        {
            return other._backingVec.AsInt64() == _backingVec.AsInt64();
        }

        public override bool Equals(object obj)
        {
            Int2? other = obj as Int2?;
            return other.HasValue && Equals(other);
        }

        public static bool operator ==(Int2 lhs, Int2 rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Int2 lhs, Int2 rhs)
        {
            return !lhs.Equals(rhs);
        }

    }
}