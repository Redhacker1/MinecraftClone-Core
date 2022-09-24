using System.Diagnostics;
using System.Numerics;
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
        public DebugPanel(ProcWorld procWorld)
        {
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Debugging";
            
            ThreadPooled = procWorld.UseThreadPool;
            
        }
        
        public override void CreateUI()
        {
            ulong VertexCount = 0;

            CurrentProcess.Refresh();
            
            float MemUsage = CurrentProcess.WorkingSet64 / 1048576f;

            //ImGui.Text($"Memory: {CurrentProcess.WorkingSet64 / 1048576f}MB");
            ImGui.Text($"FPS Estimate: {WindowClass.Renderer.FPS}");
            //ImGui.Text($"Potentially visible mesh count: {0}, Mesh count: {currentsnapshot.Count}");
            ImGui.Text($"Rendered Vertex count is: {VertexCount}");
            ImGui.Text($"Chunk Count: {ProcWorld.Instance.LoadedChunks.Count}");

            Vector3 camerapos = Camera.MainCamera.Position;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null);
            //ImGui.Text($"Heapool: {ChunkSingletons.ChunkPool.MaxCapacity} bytes maxed, {ChunkSingletons.ChunkPool.AvailableBytes} free!");
            
            
            Distance = ProcWorld.Instance._loadRadius;
            bool updated = ImGui.SliderInt("Render distance", ref Distance, 4, 90);

            if (updated)
            {
                ProcWorld.Instance.UpdateRenderDistance(Distance);
            }

            string buttonText = ProcWorld.Instance.UseThreadPool ? "Disable ThreadPool" : "Enable ThreadPool";
            if (ImGui.Button(buttonText))
            {
                ProcWorld.Instance.UseThreadPool = !ProcWorld.Instance.UseThreadPool;
            }
        }
        
        
    }
}