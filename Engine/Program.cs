using System.Numerics;
using Engine.Initialization;
using Engine.Renderable;
using ImGuiNET;

namespace Engine
{
    public class Program
    {
        static void Main()
        {
            Init.InitEngine(0,0, 1024,768, "BaseEngine", new GameTest());
        }
    }

    class GameTest : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            ImGUIPanel panelTest = new DemoPanel();
        }
    }

    class DemoPanel : ImGUIPanel
    {
        bool PressedBefore = false;
        bool AutoResize = true;
        
        public DemoPanel()
        {
            AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            PanelName = "Hello Everyone";
        }

        public override void CreateUI()
        {
            ImGui.Text("Hello, my name is donovan");

            if (!PressedBefore)
            {
                PressedBefore = ImGui.Button("Are you gonna press me?");   
            }
            else
            {
                ImGui.TextColored(Vector4.One, "You Pressed me!");
            }
        }
        
        
    }
}