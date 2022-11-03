using Engine.Rendering.Abstract;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;

namespace Engine.Rendering.VeldridBackend;

public class VeldridRenderInterface : BaseRenderInterface
{
    public GraphicsDevice Device;
    public VeldridRenderInterface(IView view) : base(view)
    {
        Device = view.CreateGraphicsDevice(new GraphicsDeviceOptions(true, PixelFormat.R32_Float, false, ResourceBindingModel.Default, true, true), GraphicsBackend.Direct3D11);
    }

    public override BaseBufferTyped<T> CreateBuffer<T>()
    {
        throw new System.NotImplementedException();
    }
}