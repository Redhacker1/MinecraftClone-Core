using System;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Utilities.Concurrency;
using Engine.Utilities.MathLib;
using Veldrid;

namespace Engine.Rendering.Abstract.View;


// TODO: Replace Veldrid specific types with generic types where possible.
public class RenderTarget : IDisposable
{
    protected readonly GraphicsDevice Device;
    internal static readonly ThreadSafeList<RenderTarget> Targets = new ThreadSafeList<RenderTarget>();
    public bool ValidTarget = false;

    public Viewport Viewport { private get; set; }
    
    public Int2 Size
    {
        get => new Int2((int)_framebuffer.Width, (int)_framebuffer.Height);
        set => Resize(value);
    }
    

    Framebuffer _framebuffer;

    readonly RenderStage.RenderStage[] _stages = new RenderStage.RenderStage[16];

    readonly CommandList _framebufferCommandList;


    internal RenderTarget(Framebuffer  framebuffer, GraphicsDevice device)
    {
        _framebuffer = framebuffer;
        _framebufferCommandList = device.ResourceFactory.CreateCommandList();
        Device = device;
        Targets.Add(this);
        ValidTarget = true;
    }

    internal void Flush(RenderState state)
    {
        _framebufferCommandList.Begin();
        _framebufferCommandList.SetFramebuffer(_framebuffer);
        _framebufferCommandList.SetViewport(0, new global::Veldrid.Viewport(Viewport.Offset.X, Viewport.Offset.Y, Viewport.Size.X, Viewport.Size.Y, Viewport.MinDepth, Viewport.MaxDepth));
        lock (_stages)
        {
            foreach (RenderStage.RenderStage stage in _stages)
            {
                stage?.RunStage(state, this);
            }   
        }
        _framebufferCommandList.End();
        state.Device.SubmitCommands(_framebufferCommandList);
    }

    public virtual void Resize(Int2 size)
    {
        int colorTargetLength = _framebuffer.ColorTargets.Count;

        Texture[] colourTextures = new Texture[colorTargetLength];
        Texture depthTarget = null;

        for (int i = 0; i < colorTargetLength; i++)
        {
            Texture currentTexture = _framebuffer.ColorTargets[i].Target;
            colourTextures[i] = Device.ResourceFactory.CreateTexture(
                new TextureDescription(
                    (uint) size.X,
                    (uint) size.Y,
                    currentTexture.Depth,
                    currentTexture.MipLevels,
                    currentTexture.ArrayLayers,
                    currentTexture.Format,
                    TextureUsage.RenderTarget,
                    TextureType.Texture2D,
                    currentTexture.SampleCount
                )
            );
            Device.DisposeWhenIdle(currentTexture);
        }
        
        if (_framebuffer.DepthTarget.HasValue)
        {
            Texture depthTexture = _framebuffer.DepthTarget.Value.Target;
            depthTarget = Device.ResourceFactory.CreateTexture(
                new TextureDescription(
                    (uint) size.X,
                    (uint) size.Y,
                    depthTexture.Depth,
                    depthTexture.MipLevels,
                    depthTexture.ArrayLayers,
                    depthTexture.Format,
                    TextureUsage.RenderTarget,
                    TextureType.Texture2D,
                    depthTexture.SampleCount
                )
            );
            Device.DisposeWhenIdle(depthTexture);
        }

        Device.DisposeWhenIdle(_framebuffer);
        _framebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthTarget, colourTextures));
    }




    public bool AddPass(int index, RenderStage.RenderStage pass)
    {
        lock (_stages)
        {
            if (index > 0 && _stages.Length > index)
            {
                _stages[index] = pass;
                return true;
            }   
        }
        return false;
    }

    public bool RemovePass(int index)
    {
        lock (_stages)
        {
            if (index < _stages.Length)
            {
  
                _stages[index] = null;
            }
        }
        return false;
    }


    public void Dispose()
    {
        ValidTarget = false;
        Targets.Remove(this);
        GC.SuppressFinalize(this);
        Device?.Dispose();
        _framebuffer?.Dispose();
        _framebufferCommandList?.Dispose();
        
    }
}