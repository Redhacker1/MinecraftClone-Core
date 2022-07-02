using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering.Veldrid;
using Engine.Windowing;

namespace MCClone_Core.World_CS.Generation;

struct VertexElements
{ 
    public int pos;
    public Vector2 UV;
}


class ChunkMesh : BaseRenderable<VertexElements>
{
    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;

    bool UpdatingMesh = true;
    public ChunkMesh(MinimalObject chunkCs)
    {
        ebo = new IndexBuffer<uint>(WindowClass.Renderer.Device, 1);
        vbo = new VertexBuffer<VertexElements>(WindowClass.Renderer.Device, new VertexElements[1]);
    }


    void CreateVertexArray(Span<int> _vertices, Span<Vector2> _uvs, Span<VertexElements> vertexArray)
    {
        Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector3 VertexFloatingPoint = new Vector3();
        for (int i = 0; i <_vertices.Length; i++)
        {
            int Y = _vertices[i] & 511;
            int X = _vertices[i] >> 14;
            int Z = (_vertices[i] >> 9) & 31;
            
            
            VertexFloatingPoint.X = X;
            VertexFloatingPoint.Z = Z;
            VertexFloatingPoint.Y = Y;

            
            vertexArray[i].pos = _vertices[i];

            vertexArray[i].UV = _uvs[i];

            tempmin = Vector3.Min(VertexFloatingPoint, tempmin);
            tempmax = Vector3.Max(VertexFloatingPoint, tempmax);
        }
        Maxpoint = tempmax;
        Minpoint = tempmin;
    }
    

    /// <summary>
    /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
    /// </summary>
    public void GenerateMesh(Span<int> _vertices, Span<Vector2> _uvs, Span<uint> _indices)
    {
        Span<VertexElements> vertsTest = _vertices.Length <= 8192 ? stackalloc VertexElements[_vertices.Length] : new VertexElements[_vertices.Length];
        CreateVertexArray(_vertices, _uvs, vertsTest);

        UpdatingMesh = true;
        VertexElements = (uint) _vertices.Length;
        if (_indices.Length > 0)
        {
            ebo.ModifyBuffer(_indices, WindowClass.Renderer.Device);
            VertexElements = (uint)_indices.Length;
        }

        vbo.ModifyBuffer(vertsTest, WindowClass.Renderer.Device);
        UpdatingMesh = false;
    }

    public void Dispose()
    {
        ebo?.Dispose();
        vbo?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void GetMinMax(out Vector3 minPoint, out Vector3 maxpoint)
    {
        maxpoint = Maxpoint;
        minPoint = Minpoint;
    }
}