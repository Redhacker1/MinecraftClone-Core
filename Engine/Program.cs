using System;
using System.Numerics;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using ImGuiNET;
using Silk.NET.Input;
using Veldrid;

namespace Engine
{
    public class Program
    {
        static void Main()
        {
            Init.InitEngine(0,0, 1024,768, "BaseEngine", new GameTest());
        }
    }

    internal class GameTest : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();
            var Entity = new UITestEntity();
        }
    }

    class UITestEntity : Entity
    {
        DemoPanel panelTest;
        public override void _Process(double delta)
        {
            bool movable = InputHandler.KeyboardKeyDown(0, Key.E);
            if (movable)
            {
                Console.WriteLine("Panel should now be movable");
            }
            panelTest.Movable = movable;
        }

        public override void _Ready()
        {
            panelTest = new DemoPanel();
        }
    }

    class DemoPanel : ImGUIPanel
    {
        bool PressedBefore;
        internal bool Movable = false;

        public DemoPanel()
        {
            Draggable = true;
            if (Position != Vector2.Zero)
            {
                Position = Vector2.Zero * 6;
            }
            
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Hello Everyone";
        }

        public override void CreateUI()
        {
            if (Movable)
            {
                RemoveFlag(ImGuiWindowFlags.NoMove);
            }
            else
            {
                AddFlag(ImGuiWindowFlags.NoMove);
            }
            //ImGui.ShowDemoWindow();
        }
        
        
    }
}