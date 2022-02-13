namespace Engine.MathLib.Random
{
    public class MixedNoiseClass
    {
        readonly int _iterations;

        readonly NoiseUtil[] _noiseFilters;

        public MixedNoiseClass(int iterations, NoiseUtil noise)
        {
            _iterations = iterations;
            _noiseFilters = new NoiseUtil[iterations];
            long seed = noise.GetSeed();

            for (int i = 0; i < iterations; i++)
            {
                _noiseFilters[i] = new NoiseUtil(noise);
                _noiseFilters[i].SetSeed(seed * (i + 1));
            }
        }
        
        public MixedNoiseClass(NoiseUtil[] noiseFilters)
        {
            _iterations = noiseFilters.Length;
            _noiseFilters = noiseFilters;
        }

        public double GetMixedNoiseSimplex(double x, double y, double z)
        {
            double iterationResults = 0;
            for (int i = 0; i < _iterations; i++)
            {
                iterationResults += _noiseFilters[i].GetSimplexFractal(x,y,z);
            }
            return iterationResults / _iterations;
        }

        public double GetMixedNoiseSimplex(double x, double y)
        {
            double iterationResults = 0;

            for (int i = 0; i < _iterations; i++)
            {
                iterationResults += _noiseFilters[i].GetSimplexFractal(x,y);
            }
            return iterationResults / _iterations;
        }
    }
}