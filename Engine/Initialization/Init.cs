using System.Collections.Generic;
using System.Text;
using Engine.Windowing;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Initialization
{
    public class Init
    {
        public static void InitEngine(int x, int y, int width, int height, string windowName, GameEntry gameclass)
        {
            InitWindow(x, y, width, height, windowName, gameclass);
            WindowClass.Handle?.Run();
           // WindowClass.Handle?.Dispose();
        }

        static WindowClass InitWindow(int x, int y, int width, int height, string windowName, GameEntry gameclass)
        {

            WindowOptions options = new WindowOptions
            {
                Size = new Vector2D<int>(width, height),
                Position = new Vector2D<int>(x, y),
                Title = windowName,
                VSync = false,
                API = GraphicsAPI.Default with {Version = new APIVersion(4, 6)},
                PreferredBitDepth = new Vector4D<int>(32),
                PreferredDepthBufferBits = 32,
                PreferredStencilBufferBits = 8,
            };

            WindowClass window = new WindowClass(options, gameclass);

            return window;
        }

        List<Parameter> ParseLaunchOptions(params string[] commands)
        {
            List<Parameter> LaunchOptions = new List<Parameter>();
            bool expectsCommand = true;
            Parameter currentparam = new Parameter();
            StringBuilder workingstringBuilder = new StringBuilder();
            bool stringCMD = false;
            char characterize = (char) 0;
            
            foreach (var str in commands)
            {
                var workingstring = str;

                bool isCommand = str.StartsWith('-');
                if (!stringCMD)
                {
                    workingstring = workingstring.Trim();
                }

                if (!isCommand)
                {
                    if (!stringCMD)
                    {
                        workingstring = workingstring.Trim();
                        LaunchOptions.Add(currentparam);
 
                    }
                    if (expectsCommand)
                    {
                        continue;
                    }

                    characterize = workingstring[0] switch
                    {
                        '"' => '"',
                        ',' => ',',
                        _ => characterize
                    };

                    if (characterize != (char)0 && workingstring[^1] != characterize)
                    {
                        workingstringBuilder.Append(workingstring);
                        stringCMD = true;
                    }
                    else if(stringCMD)
                    {
                        workingstringBuilder.Append(workingstring);
                        if (workingstring[^1] == characterize)
                        {
                            characterize = (char)0;
                            workingstring = workingstringBuilder.ToString();
                            currentparam.variable = workingstring;
                            expectsCommand = true;
                            LaunchOptions.Add(currentparam);

                        }
                    }
                    else
                    {
                        currentparam.variable = str;
                        expectsCommand = true;
                        LaunchOptions.Add(currentparam);
                    }

                }
                else
                {
                    currentparam = new Parameter
                    {
                        prefix = workingstring
                    };
                    expectsCommand = false;
                }
            }

            return LaunchOptions;
        }


        public void LoadGame()
        {
            
        }
    }
}