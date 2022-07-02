using System;

namespace MCClone_Core.Random
{
    public class Random
    {
        long _seed;
        double _nextNextGaussian;
        bool _haveNextNextGaussian;

        public Random()
        {
            _seed = DateTime.Now.Ticks;
        }

        public Random(long seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(long seed)
        {
            _seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
            _haveNextNextGaussian = false;
        }

        protected int Next(int bits)
        {
            _seed = (_seed * uint.MaxValue + 11L) & ((1L << 48) - 1);

            return (int)((ulong)_seed >> (48 - bits));
        }

        public void NextBytes(byte[] bytes)
        {
            for (int I = 0; I < bytes.Length;)
            {
                for (int rnd = NextInt(), n = Math.Min(bytes.Length - I, 4); n-- > 0; rnd >>= 8)
                {
                    bytes[I++] = (byte)rnd;
                }
            }
        }

        public int NextInt()
        {
            return Next(32);
        }

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentException("n must be a positive non-zero integer");

            if ((n & -n) == n) return (int)((n * (long)Next(31)) >> 31); // Bound is a power of two

            int bits, val;

            do
            {
                bits = Next(31);
                val = bits % n;
            } while (bits - val + (n - 1) < 0);

            return val;
        }

        public long NextLong()
        {
            return ((long)Next(32) << 32) + Next(32);
        }

        public bool NextBoolean()
        {
            return Next(1) != 0;
        }

        public float NextFloat()
        {
            return Next(24) / (float)(1 << 24);
        }

        public double NextDouble()
        {
            return (((long)Next(26) << 27) + Next(27)) / (double)(1L << 53);
        }
        
        public int NextInt(int min, int max)
        {
            return NextInt(max - min + 1) + min;
        }
        

        public double NextGaussian()
        {
            if (_haveNextNextGaussian)
            {
                _haveNextNextGaussian = false;
                return _nextNextGaussian;
            }

            double v1, v2, s; 
            do
            {
                v1 = 2 * NextDouble() - 1;
                v2 = 2 * NextDouble() - 1;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1 || s == 0);
            double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
            _nextNextGaussian = v2 * multiplier;
            _haveNextNextGaussian = true;
            return v1 * multiplier;
        }
    }
}