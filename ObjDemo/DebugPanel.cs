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

        object thing = new object();
        public override void CreateUI()
        {
            var frustum = Camera.MainCamera.GetViewFrustum(sides);
            ulong VertexCount = 0;
            Mesh[] currentsnapshot = Mesh.Meshes.ToArray();

            List<Mesh> meshes = new List<Mesh>();
            
            Parallel.ForEach(currentsnapshot, mesh =>
            {
                if (IntersectionHandler.MeshInFrustrum(mesh, frustum ))
                {
                    lock (thing)
                    {
                        meshes.Add(mesh);
                    }
                }
            });  
            foreach (Mesh mesh in meshes)
            {
                if (mesh != null )
                {
                    VertexCount += mesh.GetMeshSize();   
                }
            }

            ImGui.Text($"Memory is at: {CurrentProcess.WorkingSet64.ToString()} bytes!");
            ImGui.Text($"FPS Estimate: {WindowClass._renderer.FPS}");
            ImGui.Text($"Potentially visible mesh count: {meshes.Count}");
            ImGui.Text($"Rendered Vertex count is: {VertexCount}");

            Vector3 camerapos = Camera.MainCamera.Pos;
            ImGui.InputFloat3("Player Location: ", ref camerapos, null, ImGuiInputTextFlags.ReadOnly);
            


        }
        
        
    }
}