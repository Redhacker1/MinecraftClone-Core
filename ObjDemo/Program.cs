#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            FPSEntity entity = new FPSEntity();
            Camera fpCam = new Camera(new System.Numerics.Vector3(0, 0, 0), -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY,16f/9f, true );
            MeshSpawner thing = new MeshSpawner();
            
            BenchmarkEntity entTest = new BenchmarkEntity();
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
        Mesh?[]? meshes;

        protected override void _Ready()
        {
            base._Ready();

            new Player();
        
            Obj obj = new Obj();
            meshes = AssimpLoader.LoadMesh(@".\Assets\Mickey Mouse.obj", this);

            if (meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    mesh?.QueueVaoRegen();
                }   
            }
        }
    }
    class FPSEntity : Entity
    {
        Stopwatch fpstimer = Stopwatch.StartNew();
        int frames = 0;
        public override void _Process(double delta)
        {
            frames+=1;
            if (fpstimer.Elapsed.Seconds >= 1)
            {
                Console.WriteLine($"Elapsed: {(fpstimer.Elapsed.Seconds * 1000)}");
                Console.WriteLine($"Frames: {frames}");
                frames = 0;
                fpstimer.Restart();
            }
        }
    }

    class BenchmarkEntity : Entity
    {
        public override void _Process(double delta)
        {
            //Console.WriteLine($"{Entity.Objects.Count} Objects, {1/delta} MS");
            new FPSEntity();
        }
    }


}