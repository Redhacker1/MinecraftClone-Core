using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Engine.Renderable
{
    
    public class ImGUIPanel 
    {
        
        public string PanelName = string.Empty;
        public Vector2 Position = new Vector2();
        internal bool Draggable = true;

        internal static readonly List<ImGUIPanel> panels = new List<ImGUIPanel>();
        public bool Show = true;
        internal ImGuiWindowFlags Flags = ImGuiWindowFlags.None;


        public ImGUIPanel()
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
            

        }

        public void RemoveFlag(ImGuiWindowFlags FlagToDisable)
        {
            if (Flags <= 0) return;
            if (FlagToDisable == ImGuiWindowFlags.NoMove)
            {
                Draggable = true;
            }
            Flags = (ImGuiWindowFlags)((int)Flags & ~(int)FlagToDisable);
        }

        public virtual void CreateUI()
        {
            ImGui.ShowDemoWindow();
        }
    }
}