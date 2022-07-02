using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Engine.Debug;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Plane = Engine.Rendering.Culling.Plane;

namespace MCClone_Core.World_CS.Generation;


public class ChunkMesher
{

    Thread _thread;
    Plane[] _planes = new Plane[6];
    Queue<ChunkCs> PreferredMeshes = new Queue<ChunkCs>();
    Queue<ChunkCs> BackupMeshes = new Queue<ChunkCs>();


    ConcurrentQueue<ChunkCs> Meshes = new ConcurrentQueue<ChunkCs>();

    void MeshOrderer()
    {
        Frustrum? frustum = Camera.MainCamera?.GetViewFrustum(_planes);

        if (frustum == null)
        {
            return;
        }
        
        

        for (int chunkIndex = 0; chunkIndex < PreferredMeshes.Count; chunkIndex++)
        {
            bool success = PreferredMeshes.TryDequeue(out ChunkCs chunk);
            if (success)
            {
                Meshes.Enqueue(chunk);
            }
        }
        for (int chunkIndex = 0; chunkIndex < BackupMeshes.Count; chunkIndex++)
        {
            bool success = BackupMeshes.TryDequeue(out ChunkCs chunk);
            if (success)
            {
                Meshes.Enqueue(chunk);
            }
        }
        

        for (int chunkIndex = 0; chunkIndex < Meshes.Count; chunkIndex++)
        {
            bool success = Meshes.TryDequeue(out ChunkCs mesh);
            if (success)
            {
                AABB boundingbox = new AABB(mesh.Pos, mesh.Pos + new Vector3(ChunkCs.MaxX, ChunkCs.MaxY, ChunkCs.MaxZ));
                if (IntersectionHandler.aabb_to_frustum(ref boundingbox, frustum.Value))
                {
                    PreferredMeshes.Enqueue(mesh);
                }
                else
                {
                    BackupMeshes.Enqueue(mesh);
                }   
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
        stop = true;
    }

    public void AddMesh(ChunkCs mesh)
    {
        Meshes.Enqueue(mesh);
    }





    bool stop = false;
    
    void _thread_Mesh()
    {
        ConsoleLibrary.DebugPrint("ThreadGen Thread Running");
        while (!stop)
        {
            MeshingProcess();
        }
    }

    void MeshingProcess()
    {
        MeshOrderer();
        ChunkCs chunk = null;

        
        
        
        for (int iteration = 0; iteration < PreferredMeshes.Count; iteration++)
        {
            PreferredMeshes.TryDequeue(out chunk);
            chunk?.Update();
        }
        if (BackupMeshes.Count > 0)
        {
            PreferredMeshes.TryDequeue(out chunk);
            chunk?.Update();
        }
    }
}