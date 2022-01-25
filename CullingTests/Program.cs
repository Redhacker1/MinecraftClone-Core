using System;
using System.Collections.Generic;
using Engine;
using Engine.Initialization;
using System.Numerics;
using Engine.Objects;
using Engine.Rendering;
using Engine.Rendering.Shared.Culling;

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
            new FrustrumTest(Camera.MainCamera);

        }
    }

    class FrustrumTest : Entity
    {
        public struct Plane
        {
            public Vector3 Normal;
            public float Offset;

            public Plane(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                Normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p1));
                Offset = Vector3.Dot(Normal, p0);
            }
        }

        public FrustrumTest(Camera baseCamera)
        {
            //Vector3 nearCenter = op_Subtraction(Vector3.Zero, baseCamera.Front) * baseCamera.NearPlane;
            ////Vector3 farCenter = op_Subtraction(Vector3.Zero ,baseCamera.Front) * baseCamera.FarPlane;

            float nearHeight = (float) (2 * Math.Tan(baseCamera.GetFov() / 2) * baseCamera.NearPlane);
            float farHeight = (float) (2 * Math.Tan(baseCamera.GetFov() / 2) * baseCamera.FarPlane);
            float nearWidth = nearHeight * baseCamera.AspectRatio;
            float farWidth = farHeight * baseCamera.AspectRatio;

            /*Vector3 farTopLeft = farCenter + baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farTopRight = farCenter + baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomLeft =
                farCenter - baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomRight =
                farCenter - baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);



            List<Vector2> UVs = new List<Vector2>
                {Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1)};*/

        /*
        List<Vector3> frontverts =new List<Vector3> { nearTopLeft, nearTopRight,  nearBottomLeft, nearBottomRight };
        Front = new Mesh(frontverts,UVs, this);
        Front._indices =  new uint[] {0,1,2,2,0,3 };
        
        List<Vector3> BackVerts = new List<Vector3> {farTopLeft, farTopRight, farBottomLeft, farBottomRight};
        Back = new Mesh(BackVerts, UVs, this);
        Back._indices =  new uint[] {0,1,2,2,0,3 };

        List<Vector3> LeftVerts = new List<Vector3> { nearTopLeft, farTopLeft, nearBottomLeft, farBottomLeft };
        Left = new Mesh(LeftVerts, UVs, this);
        Left._indices =  new uint[] {0,1,2,2,0,3 };

        List<Vector3> RightVerts = new List<Vector3> { nearTopRight, farTopRight, nearBottomRight, farTopLeft };
        Right = new Mesh(RightVerts,UVs, this, );
        Right._indices =  new uint[] {0,1,2,2,0,3 };

        List<Vector3> TopVerts = new List<Vector3> { nearTopLeft, farTopLeft, farTopRight, nearTopRight };
        Top = new Mesh(TopVerts,UVs, this);
        Top._indices =  new uint[] {0,1,2,2,0,3 };

        List<Vector3> BottomVerts = new List<Vector3> { nearBottomLeft, farBottomLeft, farBottomRight, nearBottomRight};
        Bottom = new Mesh(BottomVerts, UVs, this);
        Bottom._indices =  new uint[] {0,1,2,2,0,3 };
       */ 
        }
        public override void _Ready()
        {
            base._Ready();
        }
    }
}