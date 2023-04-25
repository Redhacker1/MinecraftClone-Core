using System;
using System.Diagnostics;
using LoudPizza.Core;
using SDL2;
using static SharpInterop.SDL2.SDL;

namespace LoudPizza.TestApp
{
    public unsafe class Sdl2AudioBackend
    {
        private Stopwatch wat = new();
        private int loops;

        public SDL.SDL_AudioSpec gActiveAudioSpec;
        public uint gAudioDeviceID;

        private SDL.SDL_AudioCallback audioCallback;

        public SoLoud SoLoud { get; }

        public Sdl2AudioBackend(SoLoud soloud)
        {
            SoLoud = soloud ?? throw new ArgumentNullException(nameof(soloud));
        }

        public SoLoudStatus Initialize(uint sampleRate = 48000, uint bufferSize = 512, uint channels = 0)
        {
            //if (!SDL_WasInit(SDL_INIT_AUDIO))
            //{
            //    if (SDL_InitSubSystem(SDL_INIT_AUDIO) < 0)
            //    {
            //        return SOLOUD_ERRORS.UNKNOWN_ERROR;
            //    }
            //}

            audioCallback = soloud_sdl2static_audiomixer;

            SDL.SDL_AudioSpec spec;
            spec.silence = default;
            spec.userdata = default;
            spec.size = default;
            spec.callback = audioCallback;

            spec.freq = (int)sampleRate;
            spec.format = SDL.AUDIO_F32;
            spec.channels = (byte)channels;
            spec.samples = (ushort)bufferSize;

            int flags = (int)(SDL.SDL_AUDIO_ALLOW_ANY_CHANGE & (~SDL.SDL_AUDIO_ALLOW_FORMAT_CHANGE));

            gAudioDeviceID = SDL.SDL_OpenAudioDevice(IntPtr.Zero, 0, ref spec, out SDL.SDL_AudioSpec activeSpec, flags);
            if (gAudioDeviceID == 0)
            {
                spec.format = SDL.AUDIO_S16;

                gAudioDeviceID = SDL.SDL_OpenAudioDevice(IntPtr.Zero, 0, ref spec, out activeSpec, flags);
            }

            if (gAudioDeviceID == 0)
            {
                return SoLoudStatus.UnknownError;
            }

            SoLoud.postinit_internal((uint)activeSpec.freq, activeSpec.samples, activeSpec.channels);
            gActiveAudioSpec = activeSpec;

            SoLoud.mBackendCleanupFunc = soloud_sdl2_deinit;
            SoLoud.mBackendString = "SDL2";

            SDL.SDL_PauseAudioDevice(gAudioDeviceID, 0); // start playback

            return SoLoudStatus.Ok;
        }

        private void soloud_sdl2static_audiomixer(IntPtr userdata, IntPtr stream, int length)
        {
            wat.Start();
            if (gActiveAudioSpec.format == SDL.AUDIO_F32)
            {
                int samples = length / (gActiveAudioSpec.channels * sizeof(float));
                SoLoud.mix((float*)stream, (uint)samples);
            }
            else
            {
                int samples = length / (gActiveAudioSpec.channels * sizeof(short));
                SoLoud.mixSigned16((short*)stream, (uint)samples);
            }
            wat.Stop();

            loops++;
            if (loops >= 48000 / 512)
            {
                Console.WriteLine("Mixing time: " + wat.Elapsed.TotalMilliseconds + "ms");
                wat.Reset();
                loops = 0;
            }
        }

        private void soloud_sdl2_deinit(SoLoud aSoloud)
        {
            SDL.SDL_CloseAudioDevice(gAudioDeviceID);
        }
    }
}