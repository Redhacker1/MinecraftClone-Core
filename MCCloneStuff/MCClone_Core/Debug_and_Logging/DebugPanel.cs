using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using Engine.Collision;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Windowing;
using ImGuiNET;
using MCClone_Core.World_CS.Generation;
using Plane = System.Numerics.Plane;

namespace MCClone_Core.Debug_and_Logging
{
    class DebugPanel : ImGUIPanel
    {
        int Distance;
        bool PressedBefore;
        internal bool Movable = false;
        Plane[] sides = new Plane[6];
        bool ThreadPooled;
        Process CurrentProcess = Process.GetCurrentProcess();
        public DebugPanel()
        {
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Debugging";
            ThreadPooled = ProcWorld.Instance.UseThreadPool;
        }
        
        public override void CreateUI()
        {
            ulong VertexCount = 0;
            var currentsnapshot = Mesh.Meshes;
            
            CurrentProcess.Refresh();
            
            float MemUsage = CurrentProcess.WorkingSet64 / 1048576f;

            ImGui.Text($"Memory: {CurrentProcess.WorkingSet64 / 1048576f}MB");
            ImGui.Text($"FPS Estimate: {WindowClass.Renderer.FPS}");
            //ImGui.Text($"Potentially visible mesh count: {0}, Mesh count: {currentsnapshot.Count}");
            //ImGui.Text($"Rendered Vertex count is: {VertexCount}");
            ImGui.Text($"Chunk Count: {ProcWorld.Instance.LoadedChunks.Count}");

            Vector3 camerapos = Camera.MainCamera.Pos;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null);
            
            Distance = ProcWorld.Instance._loadRadius;
            var updated = ImGui.SliderInt("Render distance", ref Distance, 4, 40);

            if (updated)
            {
                ProcWorld.Instance.UpdateRenderDistance(Distance);
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            }

            string buttonText = ProcWorld.Instance.UseThreadPool ? "Disable ThreadPool" : "Enable ThreadPool";
            if (ImGui.Button(buttonText))
            {
                ProcWorld.Instance.UseThreadPool = !ProcWorld.Instance.UseThreadPool;
            }
        }
        
        
    }
}