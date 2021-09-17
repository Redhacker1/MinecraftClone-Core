﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Engine.Objects;
using Engine.Rendering;
using Engine.Windowing;
using Silk.NET.OpenGL;

namespace Engine.Renderable
{
    public enum MeshState
    {
        Render,
        Dirty,
        Delete,
        DontRender
    }
    public enum RenderMode
    {
        Triangle,
        Line
    }
    
    public class Mesh
    {
        readonly List<Vector3> _vertices;
        readonly List<Vector2> _uvs;
        readonly List<uint> _indices = new();

        internal VertexArrayObject<float, uint> MeshReference;
        public static List<Mesh> Meshes = new();


        internal Vector3 minpoint;
        internal Vector3 maxpoint;
        
        internal MeshState ActiveState = MeshState.Dirty;
        internal RenderMode ActiveRenderMode = RenderMode.Triangle;


        public Engine.MathLib.DoublePrecision_Numerics.Vector3 Position => _objectReference.Pos;

        readonly MinimalObject _objectReference;

        public readonly float Scale = 1;

        public readonly Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)_objectReference.Rotation.X, (float)_objectReference.Rotation.Y, (float)_objectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(_objectReference.Pos -Camera.MainCamera.Pos);

        
        
        public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector2> uvs, MinimalObject bindingobject)
        {
            ActiveState = MeshState.DontRender;
            _objectReference = bindingobject;
            _vertices = (List<Vector3>)vertices;
            _uvs = (List<Vector2>)uvs;
            
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
            var tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            List<float> values = new List<float>((_vertices.Count * 3) + (_uvs.Count * 2));
            for (int i = 0; i < _vertices?.Count; i++)
            {
                tempmin.X = Math.Min(tempmin.X, _vertices[i].X);
                tempmin.Y = Math.Min(tempmin.Y, _vertices[i].Y);
                tempmin.Z = Math.Min(tempmin.Z, _vertices[i].Z);
                
                tempmax.X = Math.Max(tempmax.X, _vertices[i].X);
                tempmax.Y = Math.Max(tempmax.Y, _vertices[i].Y);
                tempmax.Z = Math.Max(tempmax.Z, _vertices[i].Z);


                values.Add(_vertices[i].X);
                values.Add(_vertices[i].Y);
                values.Add(_vertices[i].Z);
                values.Add(_uvs[i].X);
                values.Add(_uvs[i].Y);
            }

            maxpoint = tempmax;
            minpoint = tempmin;
            return values.ToArray();
        }

        public void QueueVaoRegen()
        {
            ActiveState = MeshState.Dirty;
        }


        internal VertexArrayObject<float, uint> RegenerateVao()
        {
            uint[] indices = _indices.ToArray();
            float[] vertices = CreateVertexArray();

            BufferObject<uint> ebo = new(WindowClass.GlHandle, new Span<uint>(indices), BufferTargetARB.ElementArrayBuffer);
            BufferObject<float> vbo = new(WindowClass.GlHandle, new Span<float>(vertices), BufferTargetARB.ArrayBuffer);
            VertexArrayObject<float, uint> vao = new(vbo, ebo);

            vao?.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            vao?.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            
            return vao;
        }
        

        internal void Dispose()
        {
            MeshReference?.Dispose();
        }

        public void QueueDeletion()
        {
            ActiveState = MeshState.Delete;
        }

        public void SetRenderMode(RenderMode mode)
        {
            ActiveRenderMode = mode;
        }

        public GLEnum GetRenderMode()
        {
            if (ActiveRenderMode == RenderMode.Triangle)
            {
                return GLEnum.Triangles;
            }
            else if (ActiveRenderMode == RenderMode.Line)
            {
                return GLEnum.Lines;
            }
            else
            {
                return GLEnum.LineLoop;
            }
        }
    }
}