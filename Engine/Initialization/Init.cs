using System;
using System.Collections.Generic;
using System.Text;
using Engine.Windowing;
using static SharpInterop.SDL2.SDL;

namespace Engine.Initialization
{
    public class Init
    {
        public static void InitEngine(ref WindowParams windowParams, GameEntry gameClass, RenderBackend backend = RenderBackend.Auto)
        {
            InitSDL();
            var window = InitWindow(ref windowParams, backend);
            WindowEvents windowEvents = new WindowEvents(window, gameClass);
            windowEvents.Run();

        }

        static void InitSDL()
        {
            SDL_Init(SDL_INIT_EVERYTHING);
        }

        static IntPtr InitWindow(ref WindowParams windowParams, RenderBackend backend)
        {
            
            SDL_WindowFlags flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

            if (backend == RenderBackend.Auto)
            {
                flags = SDL_WindowFlags.SDL_WINDOW_METAL | SDL_WindowFlags.SDL_WINDOW_OPENGL |
                        SDL_WindowFlags.SDL_WINDOW_VULKAN;
            }

            IntPtr handle = SDL_CreateWindow(windowParams.Name, windowParams.Location.X, windowParams.Location.Y, windowParams.Size.X,
                windowParams.Size.Y, flags);

            return handle;
        }

        List<Parameter> ParseLaunchOptions(params string[] commands)
        {
            List<Parameter> launchOptions = new List<Parameter>();
            bool expectsCommand = true;
            Parameter currentParam = new Parameter();
            StringBuilder workingStringBuilder = new StringBuilder();
            bool stringCMD = false;
            char characterize = (char) 0;
            
            foreach (var str in commands)
            {
                string workingString = str;

                bool isCommand = str.StartsWith('-');
                if (!stringCMD)
                {
                    workingString = workingString.Trim();
                }

                if (!isCommand)
                {
                    if (!stringCMD)
                    {
                        workingString = workingString.Trim();
                        launchOptions.Add(currentParam);
 
                    }
                    if (expectsCommand)
                    {
                        continue;
                    }

                    characterize = workingString[0] switch
                    {
                        '"' => '"',
                        ',' => ',',
                        _ => characterize
                    };

                    if (characterize != (char)0 && workingString[^1] != characterize)
                    {
                        workingStringBuilder.Append(workingString);
                        stringCMD = true;
                    }
                    else if(stringCMD)
                    {
                        workingStringBuilder.Append(workingString);
                        if (workingString[^1] == characterize)
                        {
                            characterize = (char)0;
                            workingString = workingStringBuilder.ToString();
                            currentParam.variable = workingString;
                            expectsCommand = true;
                            launchOptions.Add(currentParam);

                        }
                    }
                    else
                    {
                        currentParam.variable = str;
                        expectsCommand = true;
                        launchOptions.Add(currentParam);
                    }

                }
                else
                {
                    currentParam = new Parameter
                    {
                        prefix = workingString
                    };
                    expectsCommand = false;
                }
            }

            return launchOptions;
        }


        public void LoadGame()
        {
            
        }
    }
}