using Silk.NET.Assimp;
using Texture = Engine.Rendering.Texture;


// Referenced from: http://assimp.sourceforge.net/lib_html/materials.html
namespace Engine.AssetLoading.Assimp
{
    public struct TextureLayer
    {
        public string path;
        public Texture _texture;
        public float strength;
        public TextureOp TextureOp;
        public bool Additive;
    }
}