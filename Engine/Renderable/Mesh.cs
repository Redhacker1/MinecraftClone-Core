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
        public uint VertexElements = 0;
        public static List<Mesh> Meshes = new();



        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;

        public MathLib.DoublePrecision_Numerics.Vector3 Position => _objectReference.Pos;

        readonly MinimalObject _objectReference;

        public float Scale = 1;

        public Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.CreateFromQuaternion(
            Quaternion.CreateFromYawPitchRoll((float)MathHelper.DegreesToRadians(_objectReference.Rotation.X), 
                (float)MathHelper.DegreesToRadians(_objectReference.Rotation.Y), 
                (float)MathHelper.DegreesToRadians(_objectReference.Rotation.Z))) * Matrix4x4.CreateScale(Scale)
              * Matrix4x4.CreateTranslation(_objectReference.Pos -Camera.MainCamera.Pos);
        
        
        public Mesh(MinimalObject bindingobject, Material material)
        {
            MeshMaterial = material;
            _objectReference = bindingobject;
            
                Meshes.Add(this);   
            
        }


        float[] CreateVertexArray(MeshData data)
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            float[] values = new float[data._vertices.Length * 6];
            for (int i = 0; i <data._vertices.Length; i++)
            {
                
                
                values[i * 6] = (data._vertices[i].X);
                values[i * 6 + 1] = data._vertices[i].Y;
                values[i * 6 + 2] =(data._vertices[i].Z);
                //values[i + 3] = 0;
                values[i * 6 + 3] =(data._uvs[i].X);
                values[i * 6 + 4] =(data._uvs[i].Y);
                values[i * 6 + 5] =(data._uvs[i].Z);
                //values[i + 6] = 0;

                tempmin = Vector3.Min(data._vertices[i] * Scale, tempmin);
                tempmax = Vector3.Max(data._vertices[i] * Scale, tempmax);
            }
            
            VertexElements = (uint) (data._indices?.Length ?? data._vertices.Length);
            Maxpoint = tempmax;
            Minpoint = tempmin;
            return values;
        }

        /// <summary>
        /// Updates the Vertex Array, this allows for the mesh to be updated with the supplied data from the MeshData.
        /// </summary>
        public void GenerateMesh(ref MeshData data)
        {
            
            float[] vertices = CreateVertexArray(data);

            UpdatingMesh = true;

            if (data._indices?.Length > 0)
            {
                ebo = new IndexBuffer<uint>(WindowClass._renderer.Device, data._indices);
                UseIndexedDrawing = true;
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
                success = MeshMaterial != null && MeshMaterial.BindMaterial(list);    
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