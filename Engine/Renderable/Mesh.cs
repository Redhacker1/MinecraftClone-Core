using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Windowing;
using Silk.NET.OpenGL;
using Engine.Rendering;
using Silk.NET.GLFW;

namespace Engine
{
    public class Mesh
    {

        List<Vector3> Verticies = new();
        List<Vector2> Uvs = new();
        List<uint> Indicies = new();

        List<float> MeshData = new List<float>();
        public AABB CullingAABB;

        public VertexArrayObject<float, uint> MeshReference;
        public static List<Mesh> Meshes = new();
        public static List<Mesh> OutofDateMeshes = new List<Mesh>();
        public static List<VertexArrayObject<float, uint>> QueuedForRemoval = new();
        bool HasIndicies = false;
        
        
        //A transform abstraction.
        //For a transform we need to have a position, a scale, and a rotation,
        //depending on what application you are creating, the type for these may vary.

        //Here we have chosen a vec3 for position, float for scale and quaternion for rotation,
        //as that is the most normal to go with.
        //Another example could have been vec3, vec3, vec4, so the rotation is an axis angle instead of a quaternion

        public Engine.MathLib.DoublePrecision_Numerics.Vector3 Position => objectReference.Pos;

        readonly MinimalObject objectReference;

        public readonly float Scale = 1;

        public readonly Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)objectReference.Rotation.X, (float)objectReference.Rotation.Y, (float)objectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(objectReference.Pos);


        uint IndicesCount; 

        


        public Mesh(List<Vector3>vertices, List<Vector2> Uvs,  List<uint> indicies, AABB cullingAabb)
        {
            HasIndicies = true;
            Verticies = vertices;
            this.Uvs = Uvs;
            Indicies = indicies;
            CullingAABB = cullingAabb;
            Meshes.Add(this);
        }
        
        public Mesh(List<Vector3>vertices, List<Vector2> Uvs, AABB cullingAabb)
        {
            HasIndicies = true;
            Verticies = vertices;
            this.Uvs = Uvs;
            CullingAABB = cullingAabb;
            Meshes.Add(this);
        }

        public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector2> uvs, MinimalObject bindingobject)
        {
            objectReference = bindingobject;
            this.Verticies = (List<Vector3>)vertices;
            this.Uvs = (List<Vector2>)uvs;
            
            if (vertices.Count == uvs.Count)
            {
                Vector3 min = new Vector3();
                Vector3 max = new Vector3();
                
                IndicesCount = (uint) vertices.Count;

                CullingAABB = new AABB();
                CullingAABB.max = max;
                CullingAABB.min = min;
                
                Meshes.Add(this);

            }
            else
            {
                throw new ArgumentException(message: "Uvs and Vertices size do not match!");
            }
        }

        ~Mesh()
        {
            Meshes?.Remove(this);
            OutofDateMeshes.Remove(this);
            QueuedForRemoval.Add(MeshReference);

        }



        AABB CalculateBoundingBox(List<Vector3> vertices)
        {
            Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = Vector3.Zero;
            
            for (int i =0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];

                    
                if (max.X < vertex.X)
                {
                    max.X = vertex.X;
                }
                else if (min.X > vertex.X)
                {
                    min.X = vertex.X;
                }
                    
                if (max.Y < vertex.Y)
                {
                    max.Y = vertex.Y;
                }
                else if (min.Y > vertex.Y)
                {
                    min.Y = vertex.Y;
                }
                    
                if (max.Z < vertex.Z)
                {
                    max.Y = vertex.Y;
                }
                else if (min.Z > vertex.Z)
                {
                    min.Z = vertex.Z;
                }
                    
                    
                //Verticies.Add(vertex);
            }
            
            var aabb = new AABB();
            aabb.max = max;
            aabb.min = min;
            return aabb;
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

        public void QueueVAORegen()
        {
            OutofDateMeshes.Add(this);
        }


        public VertexArrayObject<float, uint> RegenerateVao()
        {
            QueuedForRemoval.Add(MeshReference);
            var indicies = Indicies.ToArray();
            var verticies = CreateVertexArray();

            BufferObject<uint> Ebo = new(WindowClass.GlHandle, new Span<uint>(indicies), BufferTargetARB.ElementArrayBuffer);
            BufferObject<float> Vbo = new(WindowClass.GlHandle, new Span<float>(verticies), BufferTargetARB.ArrayBuffer);
            VertexArrayObject<float, uint> Vao = new(WindowClass.GlHandle, Vbo, Ebo, CullingAABB, this);

            Vao?.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao?.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            
            MeshReference = Vao;
            
            return Vao;
        }

        public void RemoveVBO()
        {
            Meshes?.Remove(this);
            //MeshReference.Dispose();
            QueuedForRemoval.Add(MeshReference);
        }



        public void ClearMesh(bool ClearVAO = false)
        {
            HasIndicies = false;
            Indicies.Clear();
            Verticies.Clear();
            IndicesCount = 0;

            if (ClearVAO)
            {
                QueuedForRemoval.Add(MeshReference);
            }
        }

        struct Vertex
        {
            public Vector3 Position;
            public Vector2 UV;

            public bool Equals(Vertex obj)
            {
                return Position == obj.Position && UV == obj.UV;
            }
        }
    }
}