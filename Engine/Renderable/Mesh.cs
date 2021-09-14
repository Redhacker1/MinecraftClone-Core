using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Engine.Objects;
using Engine.Rendering;
using Engine.Windowing;
using Silk.NET.OpenGL;

namespace Engine.Renderable
{
    internal enum MeshState
    {
        Normal,
        Dirty,
        Delete
    }
    public class Mesh
    {
        List<Vector3> Verticies;
        List<Vector2> Uvs;
        List<uint> Indicies = new();

        internal VertexArrayObject<float, uint> MeshReference;
        public static List<Mesh> Meshes = new();

        internal MeshState ActiveState = MeshState.Dirty; 
        bool _hasIndices = false;
        

        public Engine.MathLib.DoublePrecision_Numerics.Vector3 Position => objectReference.Pos;

        readonly MinimalObject objectReference;

        public readonly float Scale = 1;

        public readonly Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)objectReference.Rotation.X, (float)objectReference.Rotation.Y, (float)objectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(objectReference.Pos);

        public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector2> uvs, MinimalObject bindingobject)
        {
            objectReference = bindingobject;
            Verticies = (List<Vector3>)vertices;
            Uvs = (List<Vector2>)uvs;
            
            if (vertices.Count == uvs.Count)
            {
                Meshes.Add(this);
            }
            else
            {
                throw new ArgumentException(message: "Uvs and Vertex List sizes do not match!");
            }
        }

        float[] CreateVertexArray()
        {
            List<float> values = new List<float>((Verticies.Count * 3) + (Uvs.Count * 2));
            for (int i = 0; i < Verticies?.Count; i++)
            {
                values.Add(Verticies[i].X);
                values.Add(Verticies[i].Y);
                values.Add(Verticies[i].Z);
                values.Add(Uvs[i].X);
                values.Add(Uvs[i].Y);
            }

            return values.ToArray();
        }

        public void QueueVaoRegen()
        {
            ActiveState = MeshState.Dirty;
        }


        internal VertexArrayObject<float, uint> RegenerateVao()
        {
            uint[] indicies = Indicies.ToArray();
            float[] verticies = CreateVertexArray();

            BufferObject<uint> Ebo = new(WindowClass.GlHandle, new Span<uint>(indicies), BufferTargetARB.ElementArrayBuffer);
            BufferObject<float> Vbo = new(WindowClass.GlHandle, new Span<float>(verticies), BufferTargetARB.ArrayBuffer);
            VertexArrayObject<float, uint> Vao = new(Vbo, Ebo);

            Vao?.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao?.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            
            return Vao;
        }
        

        internal void Dispose()
        {
            MeshReference?.Dispose();
        }

        public void QueueDeletion()
        {
            ActiveState = MeshState.Delete;
        }
    }
}