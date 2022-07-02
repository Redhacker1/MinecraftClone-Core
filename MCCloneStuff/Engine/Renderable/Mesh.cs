using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using Veldrid;

namespace Engine.Renderable
{
    public class Mesh : Renderable, IDisposable
    {

        internal static object scenelock = new object();
        internal bool UpdatingMesh = true;

        public static List<Mesh> Meshes = new();
        
        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;


        public Mesh(MinimalObject bindingobject, Material material)
        {
            _objectReference = bindingobject;
            Meshes.Add(this);

        }


        void CreateVertexArray(Span<Vector3> _vertices, Span<Vector3> _uvs, ref Span<float> vertexArray)
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i <_vertices.Length; i++)
            {
                vertexArray[i * 6] = (_vertices[i].X);
                vertexArray[i * 6 + 1] = _vertices[i].Y;
                vertexArray[i * 6 + 2] =(_vertices[i].Z);
                //values[i + 3] = 0;
                vertexArray[i * 6 + 3] =(_uvs[i].X);
                vertexArray[i * 6 + 4] =(_uvs[i].Y);
                vertexArray[i * 6 + 5] =(_uvs[i].Z);
                //values[i + 6] = 0;

                tempmin = Vector3.Min(_vertices[i] * Scale, tempmin);
                tempmax = Vector3.Max(_vertices[i] * Scale, tempmax);
            }
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }

        public void GetMinMaxScaled(Span<Vector3> outValues, Vector3 Offset)
        {
            if (_objectReference != null)
            {
                Matrix4x4 cullingmatrix = ViewMatrix;
                cullingmatrix.Translation = GetObjectReference().Pos - Offset;
                
                Vector3 TempMin = Vector3.Transform(Minpoint, cullingmatrix);
                Vector3 TempMax = Vector3.Transform(Maxpoint, cullingmatrix);
                
                outValues[0] = Vector3.Min(TempMax, TempMin);
                outValues[1] = Vector3.Max(TempMax, TempMin);
            }

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
                ebo = new IndexBuffer<uint>(WindowClass._renderer.Device, _indices);
                UseIndexedDrawing = true;
                VertexElements = (uint)_indices.Length;
            }
            
            vbo = new VertexBuffer<float>(WindowClass._renderer.Device, vertsTest);
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
            if (UpdatingMesh == false)
            {
                return VertexElements;
            }

            return 0;
        }
        
        

        internal override void BindResources(CommandList list)
        {
            vbo.Bind(list);
            ebo?.Bind(list);
        }
        public override bool ShouldRender(Frustrum frustum)
        {
            return !UpdatingMesh && IntersectionHandler.MeshInFrustrum(this, frustum) && vbo != null;
        }
    }
}