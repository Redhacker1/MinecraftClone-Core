using Engine.Debugging;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Utilities.MathLib;

namespace Engine
{
    public class Program
    {
        

        static void Main(params string[] args)
        {
            Shader ps = ShaderExtensions.CreateShaderFromFile(ShaderType.Fragment, "Assets/HLSLTest.ps", "main", ShaderExtensions.ShadingLanguage.HLSL);


            // TODO: This is where the engine should always init. 

            WindowParams windowParams = new WindowParams()
            {
                Location = Int2.Zero,
                Size = new Int2(1920, 1080),
                Name = "Default window",
            };
            
            Init.InitEngine(windowParams, new GameTest());
        }
    }

    internal class GameTest : GameEntry
    {
        UiTestEntity _entity;
        protected internal override void GameStart()
        {
            base.GameStart();
            _entity = new UiTestEntity();
        }
    }

    class UiTestEntity : Entity
    {
        ImGUIPanel console;

        public override void _Ready()
        {
            console = new ConsoleText();
        }
    }
}