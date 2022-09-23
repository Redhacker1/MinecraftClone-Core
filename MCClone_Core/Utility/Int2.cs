using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MCClone_Core.Utility
{
    public struct Int2
    {
        
        Vector64<int> _backingVec = new Vector64<int>();
        
        public int X
        {
            get => _backingVec.GetElement(0);
            set => _backingVec = _backingVec.WithElement(0, value);
        }
        public int Y         {
            get => _backingVec.GetElement(1);
            set => _backingVec = _backingVec.WithElement(1, value);
        }
        public int Z         {
            get => _backingVec.GetElement(2);
            set => _backingVec = _backingVec.WithElement(2, value);
        }
        
        public Int2(int x, int y)
        {
            _backingVec = Vector64.Create(x, y);
        }
        
        Int2(Vector64<int> backingVec)
        {
            _backingVec = backingVec;
        }
        
        
        public static Int2 Zero => new Int2(0, 0);
        
        public bool Equals(ref Int2 comparison)
        {

            return Sse.MoveMask(Sse2.Xor(_backingVec.ToVector128(), comparison._backingVec.ToVector128()).AsSingle()) != 0;   
        }

        public readonly override int GetHashCode()
        {
            return _backingVec.GetHashCode();
        }

        public static Int2 operator +(Int2 other, Int2 thing)
        {
            return new Int2(thing.X + other.X, thing.Y + other.Y);
        }
        public static Int2 operator -(Int2 other, Int2 thing)
        {
            return new Int2(thing.X - other.X, thing.Y - other.Y);
        }
        
        public override string ToString()
        {
            return $"X:{X}, Y:{Y}";
        }
        
    }
}