using Silk.NET.Assimp;
using Texture = Engine.Rendering.Texture;


// Referenced from: http://assimp.sourceforge.net/lib_html/materials.html
namespace Engine.AssetLoading.AssimpIntegration
{
    public struct TextureLayer
    {
        public string path;

        public TextureOp TextureOp;
        public TextureMapping ProjectionMode;
        public TextureFlags TextureFlags;
        public BlendMode BlendingMode;

        public TextureMapMode MappingModeU;
        public uint UVWSource;

        
        public float BlendFactor;
    }
}