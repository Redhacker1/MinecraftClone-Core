using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Engine.Audio
{
    public unsafe partial class SdlAudioUtil
    {
        [LibraryImport("SDL2_mixer", EntryPoint = "SDL_GetAudioDeviceSpec")]
        public static partial int SDL_GetAudioDeviceSpec(int index, int iscapture, void* spec);
        
        private string?[] _deviceNames;
        private SDL_AudioSpec[] _deviceSpecs;

        public SdlAudioUtil(bool isCapture)
        {
            int is_capture = isCapture ? 1 : 0;
            int count = SDL_GetNumAudioDevices(is_capture);

            string?[] deviceNames = new string[count];
            SDL_AudioSpec[] deviceSpecs = new SDL_AudioSpec[count];

            for (int i = 0; i < count; i++)
            {
                deviceNames[i] = SDL_GetAudioDeviceName(i, is_capture);
                
                int code = SDL_GetAudioDeviceSpec(i, is_capture, out SDL_AudioSpec spec);
                if (code == 0)
                {
                    deviceSpecs[i] = spec;
                }
            }

            _deviceNames = deviceNames;
            _deviceSpecs = deviceSpecs;
        }
    }
}
