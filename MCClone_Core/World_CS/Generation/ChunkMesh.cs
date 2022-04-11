using System;
using System.Numerics;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;

namespace MCClone_Core.World_CS.Generation;

struct VertexElements
{ 
    public int pos;
    public Vector2 UV;
}


public class ChunkMesh : Renderable
{
    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;

    bool UpdatingMesh = true;
    public ChunkMesh(MinimalObject chunkCs, Material instanceMaterial)
    {
        ebo = new IndexBuffer<uint>(WindowClass._renderer.Device, 1);
        vbo = new VertexBuffer<VertexElements>(WindowClass._renderer.Device, new VertexElements[1]);
        _objectReference = chunkCs;
    }

    public override bool ShouldRender(Frustrum frustum)
    {
        return !UpdatingMesh && MeshInFrustrum(this, frustum) && vbo != null;
    }
    
    
    void CreateVertexArray(Span<int> _vertices, Span<Vector2> _uvs, VertexElements[] vertexArray)
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
        VertexElements[] vertsTest = new VertexElements[_vertices.Length];
        CreateVertexArray(_vertices, _uvs, vertsTest);

        UpdatingMesh = true;
        VertexElements = (uint) _vertices.Length;
        if (_indices.Length > 0)
        {
            ebo.ModifyBuffer(_indices, WindowClass._renderer.Device);
            UseIndexedDrawing = true;
            VertexElements = (uint)_indices.Length;
        }

        vbo.ModifyBuffer<VertexElements>(vertsTest, WindowClass._renderer.Device);
        UpdatingMesh = false;
    }

    public void Dispose()
    {
        ebo?.Dispose();
        vbo?.Dispose();
    }

    public void GetMinMaxScaled(Span<Vector3> outValues, Vector3 Offset)
    {
        if (_objectReference != null)
        {
            var cullingmatrix = ViewMatrix;
            cullingmatrix.Translation = Position - Offset;
                
            var TempMin = Vector3.Transform(Minpoint, cullingmatrix);
            var TempMax = Vector3.Transform(Maxpoint, cullingmatrix);
                
            outValues[0] = Vector3.Min(TempMax, TempMin);
            outValues[1] = Vector3.Max(TempMax, TempMin);
        }

    }
    
    public static bool MeshInFrustrum(ChunkMesh mesh, Frustrum frustum)
    {
        if (mesh != null)
        {

            Span<Vector3> outValues = stackalloc Vector3[2];
            mesh?.GetMinMaxScaled(outValues, frustum.camerapos);
            AABB aabb = new(outValues[0], outValues[1]);
            return IntersectionHandler.aabb_to_frustum(ref aabb, frustum);   
        }

        return true;
    }
}