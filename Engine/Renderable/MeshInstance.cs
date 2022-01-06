using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Shared.Culling;
using Engine.Rendering.Shared.Pipeline;
using Engine.Rendering.Shared.Shaders;
using Engine.Rendering.Windowing;
using Shader = Engine.Rendering.Shared.Shaders.Shader;
using Texture = Engine.Rendering.Shared.Texture;

namespace Engine.Renderable
{
    public enum RenderState : byte
    {
        Render,
        DontRender
    }
    
    public class MeshInstance  : Renderable,  IDisposable
    {
        public Engine.MathLib.DoublePrecision_Numerics.Vector3 Position => ObjectReference.Pos;
        internal MinimalObject ObjectReference;

        public float Scale = 1;

        public readonly Quaternion Rotation  = Quaternion.Identity;

        //Note: The order here does matter.
        public Matrix4x4 ViewMatrix => Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll((float)ObjectReference.Rotation.X, (float)ObjectReference.Rotation.Y, (float)ObjectReference.Rotation.Z)) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(ObjectReference.Pos -Camera.MainCamera.Pos);
        internal GraphicsRenderState renderstate;
        
        public Shader Shader;
        public Texture Texture;

        public MeshData Mesh;

 
        internal static List<MeshInstance> MeshInstances = new List<MeshInstance>();
        public RenderState CurrentRenderstate = RenderState.Render;
        

        

        public MeshInstance(MeshData meshdata, MinimalObject objectReference, ShaderStageSet shaderdata)
        {
            renderstate = WindowClass.Backend.CreateRenderState(
                new RenderStateDescription(true, true, ComparisonType.Less, FaceCullSetting.Back, FrontFaceDir.Clockwise, Topology.TriangleList, shaderdata));
            Mesh = meshdata ?? throw new ArgumentNullException(nameof(meshdata));
            MeshInstances.Add(this);
            ObjectReference = objectReference;
        }

        public MeshInstance(MeshInstance meshdata, MinimalObject objectReference, ShaderStageSet shaderdata)
        {
            renderstate = WindowClass.Backend.CreateRenderState(
                new RenderStateDescription(true, true, ComparisonType.Less, FaceCullSetting.Back, FrontFaceDir.Clockwise, Topology.TriangleList, shaderdata));
            if (meshdata == null)
            {
                throw new ArgumentNullException(nameof(meshdata));
            }
            Mesh = meshdata.Mesh;
            ObjectReference = objectReference;
            MeshInstances.Add(this);

        }

        public MeshInstance(MinimalObject objectReference, MeshData meshdata, ShaderStageSet shaderdata)
        {
            renderstate = WindowClass.Backend.CreateRenderState(
                new RenderStateDescription(true, true, ComparisonType.Less, FaceCullSetting.Back, FrontFaceDir.Clockwise, Topology.TriangleList, shaderdata));
            ObjectReference = objectReference;
            Mesh = meshdata ?? throw new ArgumentNullException(nameof(meshdata));
            MeshInstances.Add(this);
        }

        public MeshData GetBackingMeshData()
        {
            return Mesh;
        }

        public void SetBackingMeshData(MeshData mesh)
        {
            Mesh = mesh;
        }

        public void SetMeshBackingObject(MinimalObject minimalObject)
        {
            ObjectReference = minimalObject;
        }

        public void Dispose()
        {
            MeshInstances.Remove(this);
        }

        internal override void BindFlags()
        {
            throw new NotImplementedException();
        }

        internal override void BindResources()
        {
            
        }

        internal void Draw()
        {
            if(Mesh?.MeshReference != null)
            {

            }
            else
            {
                Console.WriteLine("Null Instance!");
            }
        }


        internal Aabb GetAabb()
        {
            if (Mesh != null)
            {
                return new Aabb((MathLib.DoublePrecision_Numerics.Vector3)(Mesh.Minpoint * Scale) + (Position), (MathLib.DoublePrecision_Numerics.Vector3)(Mesh.Maxpoint * Scale) + (Position));   
            }
            else
            {
                return new Aabb();
            }
        }
        
        internal Aabb GetAabb(MathLib.DoublePrecision_Numerics.Vector3 subtractionOffset)
        {
            if (Mesh != null)
            {
                return new Aabb((MathLib.DoublePrecision_Numerics.Vector3)(Mesh.Minpoint * Scale) + (Position - subtractionOffset), (MathLib.DoublePrecision_Numerics.Vector3)(Mesh.Maxpoint * Scale) + (Position - subtractionOffset));   
            }
            else
            {
                return new Aabb();
            }
        }
    }
}