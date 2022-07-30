using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Veldrid;

namespace Engine.Renderable
{
    public class Mesh : BaseRenderable<float>, IDisposable
    {
        
        internal bool UpdatingMesh = true;

        public static List<Mesh> Meshes = new List<Mesh>();
        
        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;


        public Mesh()
        {
            Meshes.Add(this);
        }


        void CreateVertexArray(Span<Vector3> _vertices, Span<Vector3> _uvs, ref Span<float> vertexArray)
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i <_vertices.Length; i++)
            {
                vertexArray[i * 6] = _vertices[i].X;
                vertexArray[i * 6 + 1] = _vertices[i].Y;
                vertexArray[i * 6 + 2] =_vertices[i].Z;
                
                vertexArray[i * 6 + 3] =_uvs[i].X;
                vertexArray[i * 6 + 4] =_uvs[i].Y;
                vertexArray[i * 6 + 5] =_uvs[i].Z;

                tempmin = Vector3.Min(_vertices[i], tempmin);
                tempmax = Vector3.Max(_vertices[i], tempmax);
            }
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void GetMinMax(out Vector3 minPoint, out Vector3 maxpoint)
        {
            minPoint = Minpoint;
            maxpoint = Maxpoint;
        }

        /// <summary>
        /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
        /// </summary>
        public void GenerateMesh(Span<Vector3> _vertices, Span<Vector3> _uvs, Span<uint> _indices)
        {
            Span<float> vertsTest = new float[_vertices.Length * 6];
            CreateVertexArray(_vertices, _uvs, ref vertsTest);

            UpdatingMesh = true;
            VertexElements = (uint) _vertices.Length;
            if (_indices.Length > 0)
            {
                ebo = new IndexBuffer<uint>(WindowClass.Renderer.Device, _indices);
                VertexElements = (uint)_indices.Length;
            }
            
            vbo = new VertexBuffer<float>(WindowClass.Renderer.Device, vertsTest);
            UpdatingMesh = false;
        }

        public void GenerateMesh(MeshData meshData)
        {
            GenerateMesh(meshData._vertices, meshData._uvs, meshData._indices);
        }
        
        public void Dispose()
        {
            ebo?.Dispose();
            vbo?.Dispose();
            Meshes.Remove(this);
        }

        public uint GetMeshSize()
        {
            return UpdatingMesh ? 0 : VertexElements;
        }
        
        internal override void BindResources(CommandList list)
        {
            vbo.Bind(list);
            ebo?.Bind(list);
        }
    }
}