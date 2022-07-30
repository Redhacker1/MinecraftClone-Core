using System;
using System.Numerics;
using Engine.Collision;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Silk.NET.Assimp;

namespace MCClone_Core.World_CS.Generation;

public struct VertexElements
{ 
    public int pos;
    public Vector2 UV;
}


public class ChunkMesh : BaseRenderable<VertexElements>
{
    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;

    bool _updatingMesh = true;
    public ChunkMesh()
    {
        ebo = new IndexBuffer<uint>(WindowClass.Renderer.Device, 1);
        vbo = new VertexBuffer<VertexElements>(WindowClass.Renderer.Device, new VertexElements[1]);
    }

    void CreateVertexArray(Span<int> vertices, Span<Vector2> uvs, Span<VertexElements> vertexArray)
    {
        Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector3 vertexFloatingPoint = new Vector3();
        for (int i = 0; i <vertices.Length; i++)
        {
            int y = vertices[i] & 511;
            int x = vertices[i] >> 14;
            int z = (vertices[i] >> 9) & 31;
            
            
            vertexFloatingPoint.X = x;
            vertexFloatingPoint.Z = z;
            vertexFloatingPoint.Y = y;

            
            vertexArray[i].pos = vertices[i];

            vertexArray[i].UV = uvs[i];

            tempmin = Vector3.Min(vertexFloatingPoint, tempmin);
            tempmax = Vector3.Max(vertexFloatingPoint, tempmax);
        }
        Maxpoint = tempmax;
        Minpoint = tempmin;
    }
    

    /// <summary>
    /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
    /// </summary>
    public void GenerateMesh(Span<int> vertices, Span<Vector2> uvs, Span<uint> indices)
    {
        Span<VertexElements> vertsTest = vertices.Length <= 8192 ? stackalloc VertexElements[vertices.Length] : new VertexElements[vertices.Length];
        CreateVertexArray(vertices, uvs, vertsTest);

        _updatingMesh = true;
        VertexElements = (uint) vertices.Length;
        if (indices.Length > 0)
        {
            ebo.ModifyBuffer(indices, WindowClass.Renderer.Device);
            UseIndexedDrawing = true;
            VertexElements = (uint)indices.Length;
        }

        vbo.ModifyBuffer(vertsTest, WindowClass.Renderer.Device);
        _updatingMesh = false;
    }
    
    public override void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint)
    {
        minPoint = Minpoint;
        maxPoint = Maxpoint;
    }
}