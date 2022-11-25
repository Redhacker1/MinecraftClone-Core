using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Rendering.VeldridBackend;
using Engine.Windowing;

namespace Engine.Renderable;

[StructLayout(LayoutKind.Sequential)]
public struct VertexElements
{ 
    public Vector3 Vertex;
    public Vector3 UVCoordinates;

    public VertexElements(Vector3 vertex, Vector3 uvCoords)
    {
        Vertex = vertex;
        UVCoordinates = uvCoords;
    }
}


public class Mesh : BaseRenderable<VertexElements>
{
    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;

    bool _updatingMesh = true;
    public Mesh()
    {
        ebo = new IndexBuffer<uint>(Engine.Renderer.Device, 1);
        vbo = new VertexBuffer<VertexElements>(Engine.Renderer.Device, new VertexElements[1]);
    }

    void CreateVertexArray(Span<Vector3> vertices, Span<Vector3> uvs, Span<VertexElements> vertexArray)
    {
        Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        for (int i = 0; i < vertices.Length; i++)
        {

            
            
            vertexArray[i].Vertex = vertices[i];

            vertexArray[i].UVCoordinates = uvs[i];

            tempmin = Vector3.Min(vertices[i], tempmin);
            tempmax = Vector3.Max(vertices[i], tempmax);
        }
        Maxpoint = tempmax;
        Minpoint = tempmin;
    }
    

    /// <summary>
    /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
    /// </summary>
    public void GenerateMesh(Span<Vector3> vertices, Span<Vector3> uvs, Span<uint> indices)
    {
        Span<VertexElements> vertsTest = vertices.Length <= 8192 ? stackalloc VertexElements[vertices.Length] : new VertexElements[vertices.Length];
        CreateVertexArray(vertices, uvs, vertsTest);

        _updatingMesh = true;
        VertexElements = (uint) vertices.Length;
        if (indices.Length > 0)
        {
            ebo.ModifyBuffer(indices, Engine.Renderer.Device);
            UseIndexedDrawing = true;
            VertexElements = (uint)indices.Length;
        }

        vbo.ModifyBuffer(vertsTest, Engine.Renderer.Device);
        _updatingMesh = false;
    }
    
    public override void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint)
    {
        minPoint = Minpoint;
        maxPoint = Maxpoint;
    }

    public void GenerateMesh(MeshData meshStruct)
    {
        GenerateMesh(meshStruct._vertices, meshStruct._uvs, meshStruct._indices);
    }

    public void GenerateMesh(VertexElements[] meshData, uint[] indices)
    {

        _updatingMesh = true;
        VertexElements = (uint) meshData.Length;
        if (indices.Length > 0)
        {
            ebo.ModifyBuffer(indices, Engine.Renderer.Device);
            UseIndexedDrawing = true;
            VertexElements = (uint)indices.Length;
        }

        vbo.ModifyBuffer(meshData, Engine.Renderer.Device);
        _updatingMesh = false;
    }
}