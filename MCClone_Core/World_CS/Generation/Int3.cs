using System;

namespace MCClone_Core.World_CS.Generation
{
    public record struct Int3(int X, int Y, int Z)
    {

        public static Int3 Zero 
        {
            get
            {
                return new Int3(0, 0, 0);
            }
        }
        
        public bool Equals(Int3 comparison)
        {
            return (X == comparison.X && Y == comparison.Y && Z == comparison.Z);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static Int3 operator +(Int3 other, Int3 thing)
        {
            return new Int3(thing.X + other.X, thing.Y + other.Y, thing.Z + other.Z);
        }
        public static Int3 operator -(Int3 other, Int3 thing)
        {
            return new Int3(thing.X - other.X, thing.Y - other.Y, thing.Z - other.Z);
        }
        
    }
}