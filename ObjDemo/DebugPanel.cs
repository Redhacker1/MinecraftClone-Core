using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using ImGuiNET;
using Plane = Engine.Rendering.Culling.Plane;

namespace ObjDemo
{
    class DebugPanel : ImGUIPanel
    {
        bool ThreadPool;
        int Distance;
        bool PressedBefore;
        internal bool Movable = false;
        Plane[] sides = new Plane[6];
        Process CurrentProcess;
        public DebugPanel()
        {
            CurrentProcess = Process.GetCurrentProcess();
            
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Debugging";

        }

        Frustrum frustum;
        List<Mesh> meshes = new List<Mesh>();
        ulong VertexCount;
        object thing = new object();
        public override void CreateUI()
        {
            meshes.Clear();
            frustum = Camera.MainCamera.GetViewFrustum(sides);



            ImGui.Text($"Memory is at: {CurrentProcess.WorkingSet64} bytes!");
            ImGui.Text($"FPS Estimate: {WindowClass._renderer.FPS}");
            ImGui.Text($"Potentially visible mesh count: {meshes.Count}");
            ImGui.Text($"Rendered Vertex count is: {VertexCount}");

            Vector3 camerapos = Camera.MainCamera.Pos;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null, ImGuiInputTextFlags.ReadOnly);
            


        }
        
        
    }
}