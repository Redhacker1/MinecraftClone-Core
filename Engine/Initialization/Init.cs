using System.Collections.Generic;
using System.Text;
using Engine.Windowing;
using Veldrid.Sdl2;

namespace Engine.Initialization
{
    public class Init
    {
        public static void InitEngine(ref WindowParams windowParams, GameEntry gameClass, RenderBackend backend = RenderBackend.Auto)
        {
            var window = InitWindow(ref windowParams, gameClass, backend);
            WindowEvents windowEvents = new WindowEvents(window, gameClass);
            windowEvents.Run();

        }

        static Sdl2Window InitWindow(ref WindowParams windowParams, GameEntry gameClass, RenderBackend backend)
        {
            SDL_WindowFlags flags = SDL_WindowFlags.MouseFocus;
            if (backend == RenderBackend.OpenGL || backend == RenderBackend.Auto)
            {
                flags |= SDL_WindowFlags.OpenGL;
            }

            //SDL_Window window;

            return new Sdl2Window(windowParams.Name, windowParams.Location.X, windowParams.Location.Y,
                windowParams.Size.X, windowParams.Size.Y, flags, false);
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