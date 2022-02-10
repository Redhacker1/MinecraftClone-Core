using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Engine.MathLib;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using ImGuiNET;
using MCClone_Core.Temp;
using MCClone_Core.World_CS.Generation;
using Plane = Engine.Rendering.Culling.Plane;

namespace Engine
{
    class DebugPanel : ImGUIPanel
    {
        int Distance;
        bool PressedBefore;
        internal bool Movable = false;
        Plane[] sides = new Plane[6];
        bool ThreadPooled = false;
        public DebugPanel()
        {
            
            //AddFlag(ImGuiWindowFlags.NoMove);
            AddFlag(ImGuiWindowFlags.AlwaysAutoResize);
            AddFlag(ImGuiWindowFlags.NoCollapse);
            PanelName = "Debugging";
            ThreadPooled = ProcWorld.Instance.UseThreadPool;
        }

        object thing = new object();
        public override unsafe void CreateUI()
        {
            


            var frustum = Camera.MainCamera.GetViewFrustum(sides);
            ulong VertexCount = 0;
            var currentsnapshot = Mesh.Meshes.ToArray();
            

            List<Mesh> meshes = new List<Mesh>(currentsnapshot.Length);
            
            Parallel.ForEach(currentsnapshot, (mesh) =>
            {
                if (IntersectionHandler.MeshInFrustrum(mesh, frustum))
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
            
            
            ImGui.Text($"FPS Estimate: {WindowClass._renderer.FPS}");
            ImGui.Text($"Potentially visible mesh count: {meshes.Count}");
            ImGui.Text($"Rendered Vertex count is: {VertexCount}");

            Vector3 camerapos = Camera.MainCamera.Pos.CastToNumerics();
            ImGui.InputFloat3("Player Location: ", ref camerapos, null, ImGuiInputTextFlags.ReadOnly);
            
            Distance = ProcWorld.Instance._loadRadius;
            var updated = ImGui.SliderInt("Render distance", ref Distance, 4, 40);

            if (updated == true)
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