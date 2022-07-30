using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Engine.Collision;
using Engine.Debugging;
using Engine.Rendering.Abstract;

namespace MCClone_Core.World_CS.Generation;


public class ChunkMesher
{

    Thread _thread;
    //Queue<ChunkCs> PreferredMeshes = new Queue<ChunkCs>();


    ConcurrentQueue<WeakReference<ChunkCs>> Meshes = new ConcurrentQueue<WeakReference<ChunkCs>>();

    void MeshOrderer(ref Frustum frustum)
    {

        bool Inview = false;
        for (int chunkIndex = 0; chunkIndex < Meshes.Count; chunkIndex++)
        {
            bool success = Meshes.TryDequeue(out WeakReference<ChunkCs> meshref);
            if (success && meshref.TryGetTarget(out ChunkCs mesh) && mesh.Freed == false)
            {
                AABB boundingBox = new AABB
                {
                    Origin = mesh.Position
                };
                boundingBox.SetExtents(new Vector3(ChunkCs.MaxX, ChunkCs.MaxY, ChunkCs.MaxZ)/2);
                if (IntersectionHandler.aabb_to_frustum(ref boundingBox, frustum))
                {
                    Inview = true;
                    mesh.Update();
                }
                else if (mesh.Freed == false)
                {
                    Meshes.Enqueue(meshref);
                }
            }
        }

        if (Inview == false)
        {
            bool success = Meshes.TryDequeue(out WeakReference<ChunkCs> mesh);
            if (success)
            {
                mesh.TryGetTarget(out ChunkCs chunk);
                chunk?.Update();
            }
        }
        
        
        
    }

    public void Start()
    {
        _thread = new Thread(_thread_Mesh);
        _thread.Start();
    }
    
    public void Stop()
    {
        Console.WriteLine("Stop Meshing!");
        _stop = true;
    }

    public void AddMesh(ChunkCs mesh)
    {
        Meshes.Enqueue(new WeakReference<ChunkCs>(mesh));
    }





    bool _stop;
    const int MeshesPerFrame = 4;
    
    void _thread_Mesh()
    {
        ConsoleLibrary.DebugPrint("ThreadGen Thread Running");
        while (!_stop)
        {
            Frustum? frustum = Camera.MainCamera?.GetViewFrustum(out _);

            if (frustum.HasValue)
            {
                Frustum frustumVal = frustum.Value;
                for (int i = 0; i < MeshesPerFrame; i++)
                {
                    MeshOrderer(ref frustumVal);   
                }   
            }
        }
    }
    
}