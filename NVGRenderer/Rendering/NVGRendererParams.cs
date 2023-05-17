using Veldrid;

namespace NVGRenderer.Rendering
{
    public struct NvgRendererParams
    {
        public GraphicsDevice Device;

        public CommandList InitialCommandBuffer;
        
        public bool AdvanceFrameIndexAutomatically;

    }
}