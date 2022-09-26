using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Rendering.Veldrid;
using Engine.Windowing;
using Veldrid;

namespace Engine.Renderable
{
    public struct GenericVertex
    {
        public Vector3 vertex;
        public Vector3 UV;
    }
    
    public class Mesh : BaseRenderable<GenericVertex>
    {
        
        
        internal bool UpdatingMesh = true;
        
        
        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;


        public Mesh()
        {
            ebo = new IndexBuffer<uint>(WindowClass.Renderer.Device, 1);
            vbo = new VertexBuffer<GenericVertex>(WindowClass.Renderer.Device, new GenericVertex[1]);
        }


        void CreateVertexArray(Span<Vector3> _vertices, Span<Vector3> _uvs, ref Span<GenericVertex> vertexArray)
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i <_vertices.Length; i++)
            {
                vertexArray[i].vertex = _vertices[i];
                vertexArray[i].UV = _uvs[i];

                tempmin = Vector3.Min(_vertices[i], tempmin);
                tempmax = Vector3.Max(_vertices[i], tempmax);
            }
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }
        
        
        
        public override void GetMinMax(out Vector3 minPoint, out Vector3 maxPoint)
        {
            minPoint = Minpoint;
            maxPoint = Maxpoint;
        }

        /// <summary>
        /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
        /// </summary>
        public void GenerateMesh(Span<Vector3> vertices, Span<Vector3> uvs, Span<uint> indices)
        {
            Span<GenericVertex> vertsTest = new GenericVertex[vertices.Length];
            CreateVertexArray(vertices, uvs, ref vertsTest);

            UpdatingMesh = true;
            VertexElements = (uint) vertices.Length;
            if (indices.Length > 0)
            {
                ebo.ModifyBuffer(indices, WindowClass.Renderer.Device);
                UseIndexedDrawing = true;
                VertexElements = (uint)indices.Length;
            }

            vbo.ModifyBuffer(vertsTest, WindowClass.Renderer.Device);
            UpdatingMesh = false;
        }

        public void GenerateMesh(MeshData meshData)
        {
            GenerateMesh(meshData._vertices, meshData._uvs, meshData._indices);
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