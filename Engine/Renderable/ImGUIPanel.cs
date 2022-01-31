using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using SPIRVCross;

namespace Engine.Renderable
{
    
    public abstract class ImGUIPanel 
    {
        
        public string PanelName = string.Empty;
        public Vector2 Position = new Vector2();
        internal bool Draggable = true;
        bool comparevalues = false;
        
        internal static readonly List<ImGUIPanel> panels = new List<ImGUIPanel>();
        public bool Show = true;
        internal ImGuiWindowFlags Flags = ImGuiWindowFlags.None;


        protected ImGUIPanel()
        {
            panels.Add(this);
        }

        void RemovePanel()
        {
            panels.Remove(this);
        }

        public void AddFlag(ImGuiWindowFlags FlagToEnable)
        {
            
            if (FlagToEnable == ImGuiWindowFlags.NoMove)
            {
                Draggable = false;
            }

            if ((Flags & FlagToEnable) == ImGuiWindowFlags.None)
            {
                Flags |= FlagToEnable;                
            }

            comparevalues = true;

        }

        public void RemoveFlag(ImGuiWindowFlags FlagToDisable)
        {
            if (comparevalues)
            {
                Console.WriteLine(Flags);
            }
            if (Flags <= 0) return;
            if (FlagToDisable == ImGuiWindowFlags.NoMove)
            {
                Draggable = true;
            }
            Flags = (ImGuiWindowFlags)(((int)Flags & ~(int)FlagToDisable));
            if (comparevalues)
            {
                Console.WriteLine(Flags);
            }
        }

        public virtual void CreateUI()
        {
            ImGui.ShowDemoWindow();
        }
    }
}