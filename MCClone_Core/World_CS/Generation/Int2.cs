using System;

namespace MCClone_Core.World_CS.Generation
{
    public record struct Int2(int X, int Y)
    {

        public static Int2 Zero
        {
            get
            {
                return new Int2(0, 0);
            }
        }
        
        public bool Equals(Int2 comparison)
        {
            return (X == comparison.X && Y == comparison.Y);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Int2 operator +(Int2 other, Int2 thing)
        {
            return new Int2(thing.X + other.X, thing.Y + other.Y);
        }
        public static Int2 operator -(Int2 other, Int2 thing)
        {
            return new Int2(thing.X - other.X, thing.Y - other.Y);
        }
        
    }
}