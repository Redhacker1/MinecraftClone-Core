using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Renderable;
using Engine.Rendering.VeldridBackend;
using MCClone_Core.Temp;
using Veldrid;

namespace MCClone_Core.World_CS.Generation;

public struct VertexElements
{ 
    // ReSharper disable NotAccessedField.Global
    public int Pos;
    public Vector2 Uv;
    // ReSharper enable NotAccessedField.Global
}


public class ChunkMesh : BaseRenderable
{
    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;
    VertexBuffer<VertexElements> vbo;


    public ChunkMesh()
    {
        vbo = new VertexBuffer<VertexElements>(Engine.Engine.Renderer.Device, stackalloc VertexElements[1]);
    }

    void CreateVertexArray(ReadOnlySpan<int> vertices, ReadOnlySpan<Vector2> uvs, Span<VertexElements> vertexArray)
    {
        Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector3 vertexFloatingPoint = new Vector3();
        for (int i = 0; i < vertices.Length; i++)
        {
            int y = vertices[i] & 511;
            int x = vertices[i] >> 14;
            int z = (vertices[i] >> 9) & 31;
            
            
            vertexFloatingPoint.X = x;
            vertexFloatingPoint.Z = z;
            vertexFloatingPoint.Y = y;

            
            vertexArray[i].Pos = vertices[i];

            vertexArray[i].Uv = uvs[i];

            tempmin = Vector3.Min(vertexFloatingPoint, tempmin);
            tempmax = Vector3.Max(vertexFloatingPoint, tempmax);
        }
        Maxpoint = tempmax;
        Minpoint = tempmin;
    }
    
    
    /// <summary>
    /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
    /// </summary>
    public unsafe void GenerateMesh(ReadOnlySpan<int> vertices, ReadOnlySpan<Vector2> uvs, uint faceCount)
    {
        void* vertsPtr = NativeMemory.Alloc((nuint) (Unsafe.SizeOf<VertexElements>() * vertices.Length));
        Span<VertexElements> vertsTest = new Span<VertexElements>(vertsPtr, vertices.Length);
        CreateVertexArray(vertices, uvs, vertsTest);  
        
        VertexElements = faceCount;
        vbo.ModifyBuffer<VertexElements>(vertsTest, Engine.Engine.Renderer.Device);
        
        NativeMemory.Free(vertsPtr);
    }
    
    public override void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint)
    {
        minPoint = Minpoint;
        maxPoint = Maxpoint;
    }

    protected override void BindResources(CommandList list)
    {
        if (vbo.BufferType != BufferUsage.VertexBuffer && Disposed == false)
        {
            return;
        }
        list.SetVertexBuffer(1, vbo.BufferObject);
    }

    protected override void Draw(CommandList list, uint count, uint start)
    {
        list.SetIndexBuffer(ProcWorld.MasterIndexBuffer.BufferObject, IndexFormat.UInt32);
        list.DrawIndexed(VertexElements, count, 0, 0, start);
    }

    protected override void ReleaseEngineResources()
    {
        vbo?.Dispose();   
    }
}