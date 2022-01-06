using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Rendering.VeldridBackend;
using Engine.Rendering.Windowing;
using Silk.NET.OpenGL;

namespace Engine.Renderable
{
    public enum MeshState
    {
        Dirty,
        Delete,
        None
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

    struct PositionUvVertex
    {
        internal Vector3 Position;
        internal Vector3 UV;

        public PositionUvVertex(Vector3 position, Vector3 uv)
        {
            Position = position;
            UV = uv;
        }
    }
    
    public class MeshData : Renderable
    {
        
        readonly List<Vector3> _vertices;
        readonly List<Vector3> _uvs;
        private readonly List<uint> _indices = new List<uint>();
        
        public static List<MeshData> Meshes = new List<MeshData>();


        internal Vector3 Minpoint;
        internal Vector3 Maxpoint;
        
        internal MeshState ActiveState = MeshState.Dirty;
        internal RenderMode ActiveRenderMode = RenderMode.Triangle;

        internal VeldridBuffer<PositionUvVertex> MeshVertexData;

        public MeshData(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector2> uvs)
        {
            
            
            ActiveState = MeshState.Dirty;
            _vertices = (List<Vector3>)vertices;
            CalcMeshBounds();
            
            _uvs = new List<Vector3>(uvs.Count);

            if (vertices.Count == uvs.Count)
            {
                for (int i = 0; i < uvs.Count; i++)
                {
                    _uvs.Add( new Vector3(uvs[i].X, uvs[i].Y, 0));
                }
                Meshes.Add(this);
            }
            else
            {
                throw new ArgumentException(message: "Uvs and Vertex List sizes do not match!");
            }
        }
        
        public MeshData(IReadOnlyCollection<Vector3> vertices, IReadOnlyCollection<Vector3> uvs)
        {
            _vertices = vertices.ToList();
            CalcMeshBounds();
            
            _uvs = uvs.ToList();
            
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
            List<float> values = new List<float>((_vertices.Count * 3) + (_uvs.Count * 3));
            for (int i = 0; i < _vertices?.Count; i++)
            {
                values.Add(_vertices[i].X);
                values.Add(_vertices[i].Y);
                values.Add(_vertices[i].Z);
                values.Add(_uvs[i].X);
                values.Add(_uvs[i].Y);
                values.Add(_uvs[i].Z);
            }
            return values.ToArray();
        }
        
        public void CalcMeshBounds()
        {
            Vector3 tempmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 tempmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < _vertices?.Count; i++)
            {
                tempmin.X = Math.Min(tempmin.X, _vertices[i].X);
                tempmin.Y = Math.Min(tempmin.Y, _vertices[i].Y);
                tempmin.Z = Math.Min(tempmin.Z, _vertices[i].Z);
                
                tempmax.X = Math.Max(tempmax.X, _vertices[i].X);
                tempmax.Y = Math.Max(tempmax.Y, _vertices[i].Y);
                tempmax.Z = Math.Max(tempmax.Z, _vertices[i].Z);
            }
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }

        public void QueueVaoRegen()
        {
            ActiveState = MeshState.Dirty;
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
        
        
        internal override void BindFlags()
        {
            
        }
        
    }
}