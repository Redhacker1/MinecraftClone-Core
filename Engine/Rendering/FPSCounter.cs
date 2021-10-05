using System.Diagnostics;
using System.Timers;

namespace Engine.Rendering
{
    struct  Statistics
    {
        ulong TotalFrameCount;
        ulong FrameCount;
        double TimeElapsed;
        internal double FPS;
        double FPMS;

        Statistics(bool thing = false)
        {
            TotalFrameCount = FrameCount = 0;
            TimeElapsed = FPS = FPMS = 0.0;
        }

        public void Update(Stopwatch frametimer)
        {
            ++TotalFrameCount;
            ++FrameCount;
            if (frametimer.Elapsed.Seconds - TimeElapsed >= 1.0)
            {
                FPS = (FrameCount);
                FPMS = 1000.0 / FPS;
                FrameCount = 0;
                TimeElapsed += 1.0;
            }
        }
    } 
}
