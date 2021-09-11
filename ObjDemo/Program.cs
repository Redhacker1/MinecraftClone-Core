using Engine;
using Engine.Initialization;
using Engine.MathLib.DoublePrecision_Numerics;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using ObjParser;

namespace ObjDemo
{
    class ObjDemo : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            Camera fpCam = new Camera(new System.Numerics.Vector3(10, 16, 1), -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY,16f/9f, true );
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
        
            Obj obj = new Obj();
            obj.LoadObj(@".\Assets\Mickey Mouse.obj");
            Pos = Vector3.One;
            Rotation = new Vector3(1, 1, 1);
        

            Mesh gameDemoMesh = new Mesh(obj.VertexList, obj.TextureList, this);
            gameDemoMesh.QueueVAORegen();
        }
    }
}