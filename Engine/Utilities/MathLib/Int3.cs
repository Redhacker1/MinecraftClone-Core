using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Engine.Utilities.MathLib
{
    public struct Int3
    {
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

        public Int3(int x, int y, int z)
        {
            _backingVec = Vector128.Create(x, y, z, 0);
        }
        
        Int3(Vector128<int> backingVec)
        {
            _backingVec = backingVec;
        }
        
        
        
        Vector128<int> _backingVec = new Vector128<int>();
        public static Int3 Zero => new Int3(0, 0, 0);
        public static Int3 One => new Int3(1, 1, 1);
        public static Int3 NegOne => new Int3(-1, -1, -1);

        public bool Equals(Int3 comparison)
        {
            return Sse2.MoveMask(Sse2.Xor(_backingVec, comparison._backingVec).AsByte()) == 0;   
        }

        public override int GetHashCode()
        {
            return _backingVec.GetHashCode();
        }

        public static Int3 operator +(Int3 left, Int3 right)
        {
            return new Int3(Sse2.Add(left._backingVec, right._backingVec));
        }
        public static Int3 operator -(Int3 left, Int3 right)
        {
            return new Int3(Sse2.Subtract(left._backingVec, right._backingVec));
        }
        
        public static Int3 operator *(Int3 left, Int3 right)
        {
            return new Int3(left.X *  right.X, left.Y *  right.Y, left.Z *  right.Z);
        }
        
        public static Int3 operator / (Int3 left, Int3 right)
        {
            return new Int3(left.X /  right.X, left.Y /  right.Y, left.Z / right.Z);
        }
        
        public override string ToString()
        {
            return $"X:{X}, Y:{Y}, Z:{Z}";
        }
        
    }
    
}