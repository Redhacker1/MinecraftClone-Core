using System.Collections.Generic;
using ImGuiNET;
using Silk.NET.Input;

namespace Engine.Renderable
{
    
    public class ImGUIPanel 
    {
        
        public string PanelName = string.Empty;
        internal static readonly List<ImGUIPanel> panels = new List<ImGUIPanel>();
        

        protected ImGUIPanel()
        {
            panels.Add(this);
        }

        void RemovePanel()
        {
            panels.Remove(this);
        }

        public virtual void CreateUI()
        {
            ImGui.ShowDemoWindow();
        }
    }
}