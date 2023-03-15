using Engine.Rendering.Abstract.View;
using Engine.Utilities.MathLib;
using Veldrid;

namespace Engine.Rendering.VeldridBackend;

class WindowRenderTarget : RenderTarget
{
    internal WindowRenderTarget(GraphicsDevice device) : base(device.SwapchainFramebuffer, device)
    {
    }

    public override void Resize(Int2 size)
    {
        Device.ResizeMainWindow((uint)size.X, (uint)size.Y);
        this._framebuffer = Device.SwapchainFramebuffer;
    }
}