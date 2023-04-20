using System;
using System.Collections.Generic;
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
    
    public Camera viewCamera;

    protected Int2 size;
    
    public Int2 Size
    {
        get => size;
        set => Resize(value);
    }
    

    public Framebuffer Framebuffer;

    readonly RenderStage.RenderStage[] _stages = new RenderStage.RenderStage[16];

    readonly CommandList _framebufferCommandList;


    internal RenderTarget(Framebuffer  framebuffer, GraphicsDevice device)
    {
        Framebuffer = framebuffer;
        _framebufferCommandList = device.ResourceFactory.CreateCommandList();
        Device = device;
        Targets.Add(this);
        ValidTarget = true;
        
        size = new Int2((int)Framebuffer.Width, (int)Framebuffer.Height);
    }
    
    
    internal RenderTarget(GraphicsDevice device, Int2 size, IReadOnlyList<PixelFormat> pixelFormats, PixelFormat? depthStencil)
    {

        Texture depthAttachment = null;
        if (depthStencil.HasValue)
        {
            depthAttachment = device.ResourceFactory.CreateTexture(new TextureDescription((uint)size.X, (uint)size.Y, 1, 1, 0,
                depthStencil.Value, TextureUsage.DepthStencil, TextureType.Texture2D));
        }

        Texture[] textures = new Texture[pixelFormats.Count];
        for (int index = 0; index < pixelFormats.Count; index++)
        {
            textures[index] = device.ResourceFactory.CreateTexture(new TextureDescription((uint)size.X, (uint)size.Y, 1, 1, 0,
                pixelFormats[index], TextureUsage.RenderTarget, TextureType.Texture2D));
        }


        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthAttachment, textures));
        _framebufferCommandList = device.ResourceFactory.CreateCommandList();
        Device = device;
        Targets.Add(this);
        ValidTarget = true;
    }

    public virtual void Resize(Int2 size)
    {
        int colorTargetLength = Framebuffer.ColorTargets.Count;

        Texture[] colourTextures = new Texture[colorTargetLength];
        Texture depthTarget = null;

        for (int i = 0; i < colorTargetLength; i++)
        {
            Texture currentTexture = Framebuffer.ColorTargets[i].Target;
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
        
        if (Framebuffer.DepthTarget.HasValue)
        {
            Texture depthTexture = Framebuffer.DepthTarget.Value.Target;
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

        Device.DisposeWhenIdle(Framebuffer);
        Framebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthTarget, colourTextures));
    }


    public void Dispose()
    {
        ValidTarget = false;
        GC.SuppressFinalize(this);
        
        Targets.Remove(this);
        Framebuffer?.Dispose();
        _framebufferCommandList?.Dispose();
        
    }

    public void Bind(CommandList list)
    {
        list.SetFramebuffer(Framebuffer);
    }
}