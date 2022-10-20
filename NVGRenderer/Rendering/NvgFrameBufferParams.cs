using Silk.NET.Maths;
using Veldrid;

namespace NVGRenderer.Rendering
{
    public struct NvgFrameBufferParams
    {
        public GraphicsDevice GraphicsDevice;
        public CommandList List;
        public Vector2D<uint> Size;
        public Framebuffer Framebuffer;
        
    }
    
}