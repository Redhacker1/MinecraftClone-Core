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
        IndexBuffer<uint> ebo;
        VertexBuffer<float> vbo;
        Material MeshMaterial;

        internal uint VertexElements;
        public static List<Mesh> Meshes = new();
        
        private Vector3[] _vertices;
        private Vector3[] _uvs;
        private uint[] _indices;

        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;

        public MathLib.DoublePrecision_Numerics.Vector3 Position => _objectReference.Pos;

        readonly MinimalObject _objectReference;

        public float Scale = 1;

        public readonly Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)_objectReference.Rotation.X, (float)_objectReference.Rotation.Y, (float)_objectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(_objectReference.Pos -Camera.MainCamera.Pos);
        
        
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
            List<float> values = new List<float>((_vertices.Length * 3) + (_uvs.Length * 3));
            for (int i = 0; i < _vertices?.Length; i++)
            {
                values.Add(_vertices[i].X);
                values.Add(_vertices[i].Y);
                values.Add(_vertices[i].Z);
                //values.Add(0);
                values.Add(_uvs[i].X);
                values.Add(_uvs[i].Y);
                values.Add(_uvs[i].Z);
                //values.Add(0);

                tempmin = Vector3.Min(_vertices[i], tempmin);
                tempmax = Vector3.Max(_vertices[i], tempmax);
            }

            Maxpoint = tempmax;
            Minpoint = tempmin;
            return values.ToArray();
        }

        [Obsolete]
        public void QueueVaoRegen()
        {
            GenerateMesh();
        }



        public void SetMeshData(MeshData meshData)
        {
            _vertices =meshData._vertices;
            _uvs = meshData._uvs;

            if (meshData._indices == null)
            {
                VertexElements = (uint)_vertices.Length;   
            }
            else
            {
                VertexElements = (uint) _indices.Length;
            }
            

        }

        public void GenerateMesh()
        {

            float[] vertices = CreateVertexArray();

            ebo = new IndexBuffer<uint>(WindowClass._renderer.Device, _indices);
            vbo = new VertexBuffer<float>(WindowClass._renderer.Device, vertices);
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