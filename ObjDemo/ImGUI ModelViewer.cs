using System;
using System.Text;
using Engine.Renderable;
using ImGuiNET;

namespace ObjDemo
{
    public class ImGUI_ModelViewer 
    {

        string PreviousBoxText = string.Empty;
        public bool EnterKeyPressed = false;

        public void CreateUI()
        {
            byte[] thing = new byte[255];

            ImGui.InputText("Model Name here!", thing, (uint) thing.Length);

            string currentstring = Encoding.ASCII.GetString(thing).Trim((char) 0x00);
        }
    }
}