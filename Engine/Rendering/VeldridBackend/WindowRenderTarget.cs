using Engine.Rendering.Abstract.View;
using Engine.Utilities.MathLib;
using Veldrid;

namespace Engine.Rendering.VeldridBackend;

class WindowRenderTarget : RenderTarget
{
    internal WindowRenderTarget(GraphicsDevice device) : base(device.SwapchainFramebuffer, device)
    {
        Targets.Remove(this);
    }

    public override void Resize(Int2 size)
    {
        this.size = size;
    }

}