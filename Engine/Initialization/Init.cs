using Engine.Windowing;
using SharpInterop.SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static SharpInterop.SDL2.SDL;

namespace Engine.Initialization
{
    public static class Init
    {
        public static void InitEngine(WindowParams windowParams, GameEntry gameClass, RenderBackend backend = RenderBackend.Auto)
        {
            InitEnvironment();
            
            InitSDL();
            IntPtr window = InitWindow(ref windowParams, backend);
            Window windowEvents = new Window(window, gameClass);
            windowEvents.Run();

        }


        static void InitEnvironment()
        {
            NativeLibrary.SetDllImportResolver(typeof(SDL).Assembly, Resolver);
        }
        
        static DllImportResolver Resolver = EngineInputResolver;

        static IntPtr EngineInputResolver(string libraryname, Assembly assembly, DllImportSearchPath? searchpath)
        {
            string path = libraryname;
            string fileType;
            string Platform;
            string ProcessorType = Environment.Is64BitProcess ? "x64" :  "x32";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Platform = "Windows";
                fileType = "dll";
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Platform = "Linux";
                fileType = "so";
            }
            else
            {
                // Not a supported OS
                return IntPtr.Zero;
            }
            

            if (File.Exists(libraryname))
            {
                
            }
            else if(string.IsNullOrEmpty(libraryname) != true)
            {

                path = Path.Combine($"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\Dependencies", Platform, ProcessorType, $"{libraryname}.{fileType}");
            }
            else
            {
                return IntPtr.Zero;
            }
            
            return NativeLibrary.Load(path);
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
                flags |= SDL_WindowFlags.SDL_WINDOW_METAL| SDL_WindowFlags.SDL_WINDOW_VULKAN;
            }
            else switch (backend)
            {
                   case RenderBackend.Vulkan:
                   {
                       flags = SDL_WindowFlags.SDL_WINDOW_VULKAN;
                       break;
                   }
                   case RenderBackend.Metal:
                   {
                       flags = SDL_WindowFlags.SDL_WINDOW_METAL;
                       break;
                   }
                   case RenderBackend.OpenGL:
                   case RenderBackend.DirecX11:
                   default:
                       break;
            }

            IntPtr handle = SDL_CreateWindow(windowParams.Name, windowParams.Location.X, windowParams.Location.Y, windowParams.Size.X,
                windowParams.Size.Y, flags);

            SDL.SDL_SetRelativeMouseMode(SDL_bool.SDL_FALSE);

            string error = SDL_GetError();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }

            return handle;
        }

        static List<Parameter> ParseLaunchOptions(params string[] commands)
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
    }
}