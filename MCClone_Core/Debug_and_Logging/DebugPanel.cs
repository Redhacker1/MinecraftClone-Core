using System.Diagnostics;
using System.Numerics;
using Engine.Renderable;
using Engine.Rendering.Abstract;
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

            ImGui.Text($"FPS Estimate: {Engine.Engine.Renderer.FPS}");
            ImGui.Text($"Chunk Count: {ProcWorld.Instance.LoadedChunks.Count}");

            Vector3 camerapos = Camera.MainCamera.Position;
            Vector3 cameraFront = Camera.MainCamera.Front;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null);
            ImGui.InputFloat3("Rotation", ref cameraFront, null);


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