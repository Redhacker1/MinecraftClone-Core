using Engine.Debugging;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Abstract;

namespace Engine
{
    public class Program
    {
        

        static void Main(params string[] args)
        {
            Shader ps = ShaderExtensions.CreateShaderHLSL(ShaderType.Fragment, "HLSLTest.ps", "main");


            // TODO: This is where the engine should always init. 
            Init.InitEngine(0,0, 1024,768, "BaseEngine", new GameTest());
        }
    }

    internal class GameTest : GameEntry
    {
        UITestEntity Entity;
        protected internal override void GameStart()
        {
            base.GameStart();
            Entity = new UITestEntity();
        }
    }

    class UITestEntity : Entity
    {
        ImGUIPanel console;

        protected internal override void _Ready()
        {
            console = new ConsoleText();
        }
    }
}