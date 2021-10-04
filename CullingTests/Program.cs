using System;
using System.Collections.Generic;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;

namespace CullingTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Init.InitEngine(0,0, 1600, 900, "FrustrumTest", new GameTestCulling());
        }
    }

    class GameTestCulling : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();
            new Player();
            FrustrumTest frustrumTest = new FrustrumTest(Camera.MainCamera);

        }
    }

    class FrustrumTest : Entity
    {
        Camera camera;
        Mesh Front;
        Mesh Back;
        Mesh Left;
        Mesh Right;
        Mesh Top;
        Mesh Bottom;

        public FrustrumTest(Camera baseCamera)
        {
            GenFrustum(baseCamera);
            camera = baseCamera;
        }

        public override void _Process(double delta)
        {
            Front.QueueDeletion();
            Back.QueueDeletion();
            Left.QueueDeletion();
            Right.QueueDeletion();  
            Top.QueueDeletion();
            Bottom.QueueDeletion();
            
            GenFrustum(camera);
        }

        public void GenFrustum(Camera baseCamera)
        {
            Matrix4x4.Invert(baseCamera.GetViewMatrix(), out Matrix4x4 thingmat);

            
            Vector3 mat3 = new Vector3(thingmat.M41, thingmat.M42, thingmat.M43);
            Vector3 mat2 = new Vector3(thingmat.M31, thingmat.M32, thingmat.M33);
            Vector3 mat1 = new Vector3(thingmat.M21, thingmat.M22, thingmat.M23);
            Vector3 mat0 = new Vector3(thingmat.M11, thingmat.M12, thingmat.M13);
            
            Vector3 nearCenter = mat3 - mat2 * baseCamera.NearPlane;
            Vector3 farCenter = mat3 - mat2 * baseCamera.FarPlane;
            
            float nearHeight = MathF.Tan(baseCamera.GetFOV() * .5f) * baseCamera.NearPlane;
            float farHeight = MathF.Tan(baseCamera.GetFOV()  * .5f) * baseCamera.FarPlane;
            
            float nearWidth = nearHeight * baseCamera.AspectRatio;
            float farWidth = farHeight * baseCamera.AspectRatio;

            Vector3 farTopLeft = farCenter + mat1 * (farHeight * 0.5f) - mat0 * (farWidth * 0.5f);
            Vector3 farTopRight = farCenter + mat1 * (farHeight * 0.5f) + mat0 * (farWidth * 0.5f);
            Vector3 farBottomLeft = farCenter - mat1 * (farHeight * 0.5f) - mat0 * (farWidth * 0.5f);
            Vector3 farBottomRight = farCenter - mat1 * (farHeight * 0.5f) + mat0 * (farWidth * 0.5f);

            Vector3 nearTopLeft = nearCenter + mat1 * (nearHeight * 0.5f) - mat0 * (nearWidth * 0.5f);
            Vector3 nearTopRight = nearCenter + mat1 * (nearHeight * 0.5f) + mat0 * (nearWidth * 0.5f);
            Vector3 nearBottomLeft = nearCenter - mat1 * (nearHeight * 0.5f) - mat0 * (nearWidth * 0.5f);
            Vector3 nearBottomRight = nearCenter - mat1 * (nearHeight * 0.5f) + mat0 * (nearWidth * 0.5f);

            
            var UVs = new List<Vector2> { Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1) };
            
            var frontverts =new List<Vector3> { nearTopLeft, nearTopRight,  nearBottomLeft, nearBottomRight };
            Front = new Mesh(frontverts,UVs, this);
            Front._indices =  new uint[] {0,1,2,2,0,3 };
            
            var BackVerts = new List<Vector3> {farTopLeft, farTopRight, farBottomLeft, farBottomRight};
            Back = new Mesh(BackVerts, UVs, this);
            Back._indices =  new uint[] {0,1,2,2,0,3 };

            var LeftVerts = new List<Vector3> { nearTopLeft, farTopLeft, nearBottomLeft, farBottomLeft };
            Left = new Mesh(LeftVerts, UVs, this);
            Left._indices =  new uint[] {0,1,2,2,0,3 };

            var RightVerts = new List<Vector3> { nearTopRight, farTopRight, nearBottomRight, farTopLeft };
            Right = new Mesh(RightVerts,UVs, this);
            Right._indices =  new uint[] {0,1,2,2,0,3 };

            var TopVerts = new List<Vector3> { nearTopLeft, farTopLeft, farTopRight, nearTopRight };
            Top = new Mesh(TopVerts,UVs, this);
            Top._indices =  new uint[] {0,1,2,2,0,3 };

            var BottomVerts = new List<Vector3> { nearBottomLeft, farBottomLeft, farBottomRight, nearBottomRight};
            Bottom = new Mesh(BottomVerts, UVs, this);
            Bottom._indices =  new uint[] {0,1,2,2,0,3 };

            Front.SetRenderMode(RenderMode.Triangle);
            Front.QueueVaoRegen();
            
            Back.SetRenderMode(RenderMode.Triangle);
            Back.QueueVaoRegen();
            
            Left.SetRenderMode(RenderMode.Triangle);
            Left.QueueVaoRegen();
            
            Right.SetRenderMode(RenderMode.Triangle);
            Right.QueueVaoRegen();
            
            Top.SetRenderMode(RenderMode.Triangle);
            Top.QueueVaoRegen();
            
            Bottom.SetRenderMode(RenderMode.Triangle);
            Bottom.QueueVaoRegen();
        }
    }
}