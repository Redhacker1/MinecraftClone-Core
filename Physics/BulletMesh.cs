using System.Numerics;
using BulletSharp.Math;
using Engine.MathLib;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Engine.Windowing;
using Veldrid;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Physics;

public class BulletMesh: Renderable, IDisposable
{

    internal static object scenelock = new object();
    internal bool UpdatingMesh = true;
    List<BulletMesh> Meshes = new List<BulletMesh>();


    internal Vector3 Minpoint;
    internal Vector3 Maxpoint;

    public Vector3 Position => GetObjectReference().Pos;
    
    public Matrix BulletMatrix = default;

    public Quaternion Rotation =>
            Quaternion.CreateFromYawPitchRoll(MathHelper.DegreesToRadians(GetObjectReference().Rotation.X),
                MathHelper.DegreesToRadians(GetObjectReference().Rotation.Y),
                MathHelper.DegreesToRadians(GetObjectReference().Rotation.Z));


    public BulletMesh(MinimalObject bindingobject, Material material)
        {
            _objectReference = new WeakReference<MinimalObject>(bindingobject);
            Meshes.Add(this);

        }

        MinimalObject GetObjectReference()
        {
            MinimalObject objectReference;
            _objectReference.TryGetTarget(out objectReference);
            return objectReference;
        }


        void CreateVertexArray(Span<Vector3> _vertices, Span<Vector3> _uvs, ref Span<float> vertexArray)
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
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

                tempmin = Vector3.Min(_vertices[i], tempmin);
                tempmax = Vector3.Max(_vertices[i], tempmax);
            }
            
            
            Maxpoint = tempmax;
            Minpoint = tempmin;
        }

        internal void GetMinMaxScaled(Span<Vector3> outValues, Vector3 Offset)
        {

            if (_objectReference != null)
            {
                outValues[0] = Minpoint * Scale + (Position - Offset);
                outValues[1] = Maxpoint * Scale + (Position - Offset);   
            }
            else
            {
                outValues[0] = Vector3.Zero;
                outValues[1] = Vector3.Zero;
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

        public override bool ShouldRender(Frustrum frustum)
        {
            return !UpdatingMesh && MeshInFrustrum(this, frustum) && vbo != null;
        }
        
        public static bool MeshInFrustrum(BulletMesh mesh, Frustrum frustum)
        {
            if (mesh != null)
            {
                AABB boundingBox = new AABB();
                
                Span<Vector3> outValues = stackalloc Vector3[2];
                mesh?.GetMinMaxScaled(outValues, frustum.camerapos);
                AABB aabb = new(outValues[0], outValues[1]);
                return IntersectionHandler.aabb_to_frustum(ref aabb, frustum);   
            }

            return true;
        }
    
}