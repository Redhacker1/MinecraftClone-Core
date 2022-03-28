using Engine;
using Engine.Initialization;

namespace Physics;

class BulletDemo
{
    static void Main()
    {
        Init.InitEngine(0,0, 1920, 1080, "BulletDemo", new PhysicsTest());
    }
}

class PhysicsTest : Game
{
}