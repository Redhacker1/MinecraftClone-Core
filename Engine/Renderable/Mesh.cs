using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Culling;
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
        Material MeshMaterial;
        internal bool UpdatingMesh = true;
        public uint VertexElements => (uint) (_indices?.Length ?? _vertices.Length);
        public static List<Mesh> Meshes = new();

        Vector3[] _vertices;
        Vector3[] _uvs;
        uint[] _indices;

        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;

        public MathLib.DoublePrecision_Numerics.Vector3 Position => _objectReference.Pos;

        readonly MinimalObject _objectReference;

        public float Scale = 1;

        public Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)_objectReference.Rotation.X, (float)_objectReference.Rotation.Y, (float)_objectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(_objectReference.Pos -Camera.MainCamera.Pos);
        
        
        public Mesh(MinimalObject bindingobject, Material material)
        {
            MeshMaterial = material;
            _objectReference = bindingobject;
            
                Meshes.Add(this);   
            
        }


        float[] CreateVertexArray()
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            float[] values = new float[_vertices.Length * 6];
            for (int i = 0; i < _vertices.Length; i++)
            {
                
                
                values[i * 6] = (_vertices[i].X);
                values[i * 6 + 1] = _vertices[i].Y;
                values[i * 6 + 2] =(_vertices[i].Z);
                //values[i + 3] = 0;
                values[i * 6 + 3] =(_uvs[i].X);
                values[i * 6 + 4] =(_uvs[i].Y);
                values[i * 6 + 5] =(_uvs[i].Z);
                //values[i + 6] = 0;

                tempmin = Vector3.Min(_vertices[i] * Scale, tempmin);
                tempmax = Vector3.Max(_vertices[i] * Scale, tempmax);
            }

            Maxpoint = tempmax;
            Minpoint = tempmin;
            return values;
        }

        [Obsolete("Use Generate mesh instead")]
        public void QueueVaoRegen()
        {
            GenerateMesh();
        }



        public void SetMeshData(MeshData meshData)
        {
            _vertices =meshData._vertices;
            _uvs = meshData._uvs;
            _indices = meshData._indices;
            UseIndexedDrawing = (meshData._indices != null && meshData._indices.Length > 0);


        }

        /// <summary>
        /// Updates the Vertex Array, this allows for the mesh to be updated.
        /// </summary>
        public void GenerateMesh()
        {
            
            float[] vertices = CreateVertexArray();

            UpdatingMesh = true;

            if (_indices?.Length > 0)
            {
                ebo = new IndexBuffer<uint>(WindowClass._renderer.Device, _indices);   
            }
            vbo = new VertexBuffer<float>(WindowClass._renderer.Device, vertices);
            UpdatingMesh = false;
        }


        public void Dispose()
        {
            ebo?.Dispose();
            vbo?.Dispose();
            Meshes.Remove(this);
        }

        
        
        [Obsolete]
        public void QueueDeletion()
        {
            
        }

        public uint GetMeshSize()
        {
            if (UpdatingMesh == false)
            {
                return VertexElements;
            }
            else
            {
                return 0;
            }
        }
        
        

        internal bool BindResources(CommandList list)
        {
            bool success = MeshMaterial != null && MeshMaterial.BindMaterial(list);
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