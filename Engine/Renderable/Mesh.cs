using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Objects;
using Engine.Rendering;
using Engine.Windowing;
using Veldrid;

namespace Engine.Renderable
{
    public class Mesh : IDisposable
    {
        //UniformBuffer<float> UVs;
        IndexBuffer<uint> ebo;
        VertexBuffer<float> vbo;
        
        internal static object scenelock = new object();
        internal bool UseIndexedDrawing;
        internal bool UpdatingMesh = true;
        internal Material MeshMaterial;
        public uint VertexElements;
        public static List<Mesh> Meshes = new();



        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;

        public Vector3 Position => _objectReference.Pos;

        readonly MinimalObject _objectReference;

        public float Scale = 1;

        public Quaternion Rotation =>
            Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(_objectReference.Rotation.X),
                MathHelper.DegreesToRadians(_objectReference.Rotation.Y),
                MathHelper.DegreesToRadians(_objectReference.Rotation.Z));

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(
            Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(_objectReference.Rotation.X), 
                MathHelper.DegreesToRadians(_objectReference.Rotation.Y), 
                MathHelper.DegreesToRadians(_objectReference.Rotation.Z))) * Matrix4x4.CreateScale(Scale)
                                                                           * Matrix4x4.CreateTranslation(_objectReference.Pos -Camera.MainCamera.Pos);
        
        
        public Mesh(MinimalObject bindingobject, Material material)
        {
            MeshMaterial = material;
            _objectReference = bindingobject;
            
                Meshes.Add(this);   
            
        }


        void CreateVertexArray(Span<Vector3> _vertices, Span<Vector3> _uvs, ref Span<float> vertexArray)
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            //float[] values = new float[_vertices.Length * 6];
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

                tempmin = Vector3.Min(_vertices[i], tempmin);
                tempmax = Vector3.Max(_vertices[i], tempmax);
            }
            
            
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }

        internal void GetMinMaxRotated(Span<Vector3> outValues, Vector3 Offset)
        {
            Matrix4x4 rotation = 
                Matrix4x4.CreateFromQuaternion(Rotation) * 
                Matrix4x4.CreateScale(Scale) * 
                Matrix4x4.CreateTranslation(_objectReference.Pos - Offset);

            outValues[0] = Vector3.Transform(Minpoint, rotation);
            outValues[1] = Vector3.Transform(Maxpoint, rotation);

        }

        /// <summary>
        /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
        /// </summary>
        public void GenerateMesh(Span<Vector3> _vertices, Span<Vector3> _uvs, Span<uint> _indices)
        {
            Span<float> vertsTest = stackalloc float[_vertices.Length * 6];
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
        
        

        internal bool BindResources(CommandList list, Material DontSwitchIfme)
        {
            bool success = false;
            if (MeshMaterial != DontSwitchIfme)
            {
                success = MeshMaterial != null && MeshMaterial.Bind(list);    
            }
            else
            {
                success = true;
            }
            
            
            if (vbo != null && success)
            {
                vbo.Bind(list);
                ebo?.Bind(list);

                return true;
            }

            return false;

        }
    }
}