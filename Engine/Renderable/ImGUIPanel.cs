using System.Collections.Generic;
using ImGuiNET;

namespace Engine.Renderable
{
    
    public abstract class ImGUIPanel 
    {
        
        public string PanelName = string.Empty;
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
            Flags |= FlagToEnable;
        }

        public void RemoveFlag(ImGuiWindowFlags FlagToDisable)
        {
            if (Flags <= 0) return;
            
            Flags = (Flags & (ImGuiWindowFlags)~(1 << (((int)FlagToDisable) - 1)));
        }

        public virtual void CreateUI()
        {
            
        }
    }
}