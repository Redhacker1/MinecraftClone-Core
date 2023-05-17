using System.Diagnostics;
using System.Numerics;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Windowing;
using ImGuiNET;

namespace ObjDemo
{
    class DebugPanel : ImGUIPanel
    {
        bool ThreadPool;
        int Distance;
        bool PressedBefore;
        internal bool Movable = false;
        Process CurrentProcess;
        public DebugPanel()
        {
            CurrentProcess = Process.GetCurrentProcess();
            
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Debugging";

        }

        object thing = new object();
        public override void CreateUI()
        {
            var frustum = Camera.MainCamera.GetViewFrustum(out _);
            ulong VertexCount = 0;


            ImGui.Text($"Memory is at: {CurrentProcess.WorkingSet64.ToString()} bytes!");
            ImGui.Text($"FPS Estimate: {Engine.Engine.Renderer.FPS}");
            ImGui.Text($"Rendered Vertex count is: {VertexCount}");

            Vector3 camerapos = Camera.MainCamera.Position;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null, ImGuiInputTextFlags.ReadOnly);
            


        }
        
        
    }
}