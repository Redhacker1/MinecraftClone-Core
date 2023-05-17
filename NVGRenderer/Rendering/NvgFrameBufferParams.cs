using Engine.Rendering.Abstract.View;
using Silk.NET.Maths;
using Veldrid;

namespace NVGRenderer.Rendering
{
    public struct NvgFrameBufferParams
    {
        public GraphicsDevice GraphicsDevice;
        public CommandList List;
        public Vector2D<uint> Size;
        public RenderTarget Framebuffer;
        
    }
    
}