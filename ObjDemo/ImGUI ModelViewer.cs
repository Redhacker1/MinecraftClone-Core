using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.Renderable;
using Engine.Windowing;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace ObjDemo
{
    public class ImGUI_ModelViewer : ImGUIPanel
    {
        public ImGUI_ModelViewer()
        {
            AddFlag(ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize);
            PanelName = "Hello world";
        }

        string PreviousBoxText = string.Empty;
        public bool EnterKeyPressed = false;
        
        public override void CreateUI()
        {
            var thing = new byte[255];
            
            ImGui.InputText("Model Name here!", thing, (uint)thing.Length);

            string currentstring = Encoding.ASCII.GetString(thing).Trim((char)0x00);

            if (currentstring == string.Empty && PreviousBoxText != string.Empty)
            {
                Console.WriteLine(PreviousBoxText);
            }

            PreviousBoxText = currentstring;
        }
    }
}