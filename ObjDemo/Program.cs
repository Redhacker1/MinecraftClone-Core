using System;
using System.Collections.Generic;
using System.Numerics;
using CullingTests;
using Engine;
using Engine.AssetLoading;
using Engine.Initialization;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using ObjParser;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace ObjDemo
{
    class ObjDemo : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            Camera fpCam = new Camera(new System.Numerics.Vector3(0, 0, 0), -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY,16f/9f, true );
            MeshSpawner thing = new MeshSpawner();
        }
    }

    class Entrypoint
    {
        static void Main()
        {
            Init.InitEngine( 10,10, 1600, 900, "Hello World", new ObjDemo());
        }
    }

    class MeshSpawner : GameObject
    {

        protected override void _Ready()
        {
            base._Ready();

            new Player();
        
            Obj obj = new Obj();
            var meshes = AssimpLoader.LoadMesh("./Assets/Wheel.fbx", this);

            foreach (var mesh in meshes)
            {
                mesh.QueueVaoRegen();
            }
            //Pos = Vector3.One;

            //BenchmarkEntity entTest = new BenchmarkEntity();
            //var gameDemoMesh = new Mesh(obj.VertexList, obj.TextureList, this);
            //gameDemoMesh.QueueVaoRegen();

            //FrustrumTest frustrumTest = new FrustrumTest(Camera.MainCamera);
            //frustrumTest.Pos = new Vector3(0, 0, 0);
        }
    }

    class BenchmarkEntity : Entity
    {
        public override void _Process(double delta)
        {
            //Console.WriteLine($"{Entity.Objects.Count} Objects, {1/delta} MS");
            //new Entity(Vector3.One, Vector2.Zero);
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
                Offset = System.Numerics.Vector3.Dot(Normal, p0);
            }
        }

        public FrustrumTest(Camera baseCamera)
        {
            System.Numerics.Vector3 nearCenter = op_Subtraction(Pos, baseCamera.Front) * baseCamera.NearPlane;
            System.Numerics.Vector3 farCenter = op_Subtraction(Pos, baseCamera.Front) * 100;

            float nearHeight = (float)(Math.Tan(baseCamera.GetFOV() / 2) * baseCamera.NearPlane);
            float farHeight = (float)(Math.Tan(baseCamera.GetFOV() / 2) * baseCamera.FarPlane);
            float nearWidth = nearHeight * baseCamera.AspectRatio;
            float farWidth = farHeight * baseCamera.AspectRatio;

            Vector3 farTopLeft = farCenter + baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farTopRight = farCenter + baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomLeft = farCenter - baseCamera.Up * (farHeight * 0.5f) - baseCamera.Right * (farWidth * 0.5f);
            Vector3 farBottomRight = farCenter - baseCamera.Up * (farHeight * 0.5f) + baseCamera.Right * (farWidth * 0.5f);

            Vector3 nearTopLeft = new Vector3(-nearWidth, nearHeight, 1); //nearCenter + baseCamera.Up * (nearHeight * 0.5f) - baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearTopRight = new Vector3(nearWidth, nearHeight, 1);//nearCenter + baseCamera.Up * (nearHeight * 0.5f) + baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearBottomLeft = new Vector3(-nearWidth, -nearHeight, 1);//nearCenter - baseCamera.Up * (nearHeight * 0.5f) - baseCamera.Right * (nearWidth * 0.5f);
            Vector3 nearBottomRight = new Vector3(nearWidth, nearHeight, 1);

            
            var UVs = new List<Vector2> { Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1) };
            
            var frontverts =new List<System.Numerics.Vector3>()
                { nearTopLeft, nearTopRight,  nearBottomLeft, nearBottomRight };
            Mesh Front = new Mesh(frontverts,UVs, this);
            
            var BackVerts = new List<System.Numerics.Vector3>() 
                {farTopLeft, farTopRight, farBottomLeft, farBottomRight};
            Mesh Back = new Mesh(BackVerts, UVs, this);

            var LeftVerts = new List<System.Numerics.Vector3>()
                { nearTopLeft, farTopLeft, nearBottomLeft, farBottomLeft };
            Mesh Left = new Mesh(LeftVerts, UVs, this);

            var RightVerts = new List<System.Numerics.Vector3>()
                { nearTopRight, farTopRight, nearBottomRight, farTopLeft };
            Mesh Right = new Mesh(RightVerts,UVs, this);

            var TopVerts = new List<System.Numerics.Vector3>() { nearTopLeft, farTopLeft, farTopRight, nearTopRight };
            Mesh Top = new Mesh(TopVerts,UVs, this);

            var BottomVerts = new List<System.Numerics.Vector3>() { nearBottomLeft, farBottomLeft, farBottomRight, nearBottomRight};
            Mesh Bottom = new Mesh(BottomVerts, UVs, this);

            Front.SetRenderMode(RenderMode.Line);
            Front.QueueVaoRegen();
            
            Back.SetRenderMode(RenderMode.Line);
            Back.QueueVaoRegen();
            
            Left.SetRenderMode(RenderMode.Line);
            Left.QueueVaoRegen();
            
            Right.SetRenderMode(RenderMode.Line);
            Right.QueueVaoRegen();
            
            Top.SetRenderMode(RenderMode.Line);
            Top.QueueVaoRegen();
            
            Bottom.SetRenderMode(RenderMode.Line);
            Bottom.QueueVaoRegen();
        }

        static Vector3 op_Subtraction(Vector3 first, Vector3 Second)
        {
            return Vector3.Subtract(first, Second);
        }
    }
}