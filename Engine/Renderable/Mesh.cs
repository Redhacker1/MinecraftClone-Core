using System;
using System.Collections.Generic;
using System.Linq;
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

    public enum MeshFlags
    {
        FrontFaceDirectionCw,
        FrontFaceDirectionCcw,
        CullFaceModeFront,
        CullFaceModeBack, 
        CullFaceModeNone
    }
    public enum RenderMode
    {
        Triangle,
        Line
    }
    
    public class Mesh : IRenderable
    {
        public Vector3[] _vertices;
        public Vector3[] _uvs;
        public uint[] _indices;

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
            _vertices = vertices.ToArray();
            _uvs = new Vector3[uvs.Count];

            if (vertices.Count == uvs.Count)
            {
                for (int i = 0; i < uvs.Count; i++)
                {
                    _uvs[i] = ( new Vector3(uvs[i].X, uvs[i].Y, 0));
                }
                Meshes.Add(this);
            }
            else
            {
                throw new ArgumentException(message: "Uvs and Vertex List sizes do not match!");
            }
        }
        
        public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3> uvs, MinimalObject bindingobject)
        {
            _objectReference = bindingobject;
            _vertices = vertices.ToArray();
            _uvs = uvs.ToArray();
            
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
            List<float> values = new List<float>((_vertices.Length * 3) + (_uvs.Length * 3));
            for (int i = 0; i < _vertices?.Length; i++)
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
                values.Add(_uvs[i].Z);
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
            uint[] indices = _indices?.ToArray();
            float[] vertices = CreateVertexArray();

            BufferObject<uint> ebo = new(WindowClass.GlHandle, new Span<uint>(indices), BufferTargetARB.ElementArrayBuffer);
            BufferObject<float> vbo = new(WindowClass.GlHandle, new Span<float>(vertices), BufferTargetARB.ArrayBuffer);
            VertexArrayObject<float, uint> vao = new(WindowClass.GlHandle,vbo, ebo);

            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
            vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);


            MeshReference = vao;
            
            return vao;
        }


        internal unsafe void Draw(GL glHandle)
        {
           // MeshReference._handle
            MeshReference.Bind();
            if (_indices?.Length > 0)
            {
                glHandle.DrawElements(GetRenderMode(), (uint)_indices.Length,GLEnum.UnsignedInt, null);
            }
            else
            {
                glHandle.DrawArrays(GetRenderMode(), 0, (uint)_vertices.Length);
            }
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

        void IRenderable.BindFlags()
        {
            
        }

        void IRenderable.BindResources()
        {
            MeshReference.Bind();
        }
    }
}