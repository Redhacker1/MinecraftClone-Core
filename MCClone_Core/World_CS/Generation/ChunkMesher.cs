using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using Engine.Collision.Simple;
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
        uint count = 0;
        bool Inview = false;
        for (int chunkIndex = 0; chunkIndex < Meshes.Count; chunkIndex++)
        {
            bool success = Meshes.TryDequeue(out WeakReference<ChunkCs> meshRef);
            if (success && meshRef.TryGetTarget(out ChunkCs mesh) && mesh.Freed == false)
            {
                AABB boundingBox = new AABB
                {
                    Origin = (mesh.Instance3D.Position - Camera.MainCamera.Position)
                };
                boundingBox.SetExtents(new Vector3(ChunkCs.MaxX/2f, ChunkCs.MaxY, ChunkCs.MaxZ/2f));
                if (IntersectionHandler.aabb_to_frustum(ref boundingBox, frustum))
                {
                    Inview = true;
                    mesh.Update();
                    mesh.Instance3D.GetInstanceAabb(out boundingBox);
                }
                else
                {
                    count++;
                    Meshes.Enqueue(meshRef);
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