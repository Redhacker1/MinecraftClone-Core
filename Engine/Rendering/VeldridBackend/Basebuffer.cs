using System;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Engine.Rendering.VeldridBackend;

// TODO: Add uniform buffers backed by this.
// TODO: When implementing other RHI/Backends, reuse this code and break Veldrid types out and move to engine replacements
public abstract class BaseBufferUntyped : GraphicsResource
{

    protected BaseBufferUntyped(GraphicsDevice device) : base(device)
    {
        
    }

    public override (ResourceKind, BindableResource) GetUnderlyingResource()
    {
        if (BufferType == BufferUsage.UniformBuffer)
        {
            return (ResourceKind.UniformBuffer, BufferObject);
        }
        else
        {
           throw new NotSupportedException("GetUnderlyingResources is not supported on non uniform buffers!");
        }
    }


    public DeviceBuffer BufferObject { get; protected set; }
    public BufferUsage BufferType { protected init; get; }
    

    public uint LengthInBytes => BufferObject.SizeInBytes;
    
    protected void SafeCreateBuffer<T>(GraphicsDevice device, uint count)
    {
        if (count == 0)
        {
            count = 1;
        }
        BufferObject = device.ResourceFactory.CreateBuffer(new BufferDescription((uint) ( Unsafe.SizeOf<T>() *  count), BufferType));
    }


    /// <summary>
    /// Used for most buffers in Veldrid, with the exception of Uniform buffers and SSBOs
    /// </summary>
    /// <param name="list">The command list to bind to</param>
    /// <param name="slot">Only used for vertex buffers, index buffers will not respect this</param>
    internal abstract void Bind(CommandList list, uint slot = 0);

    public void ModifyBuffer<T>(ReadOnlySpan<T> data, GraphicsDevice device) where T : unmanaged
    {
        if (data.IsEmpty)
        {
            return;
        }
        
        if (data.Length * Unsafe.SizeOf<T>() > LengthInBytes || BufferObject == null)
        {
            _device.DisposeWhenIdle(BufferObject);
            SafeCreateBuffer<T>(device, (uint)data.Length);

        }
        device.UpdateBuffer(BufferObject, 0, data);  
    }
    
    

    /// <summary>
    /// The GC should properly dispose of the object if it is otherwise leaked irresponsibly. NOTE: might cause problems with APIs that don't allow for explicit multithreading, and possibly more work for those that do!
    /// TODO: The developer or even possibly the rendering backend should never let this happen! Resources should be properly freed by the developer or the Renderer!
    /// </summary>
    ~BaseBufferUntyped()
    {
        Dispose();
    }
    
}
/// <summary>
/// Typed buffer class meant for keeping track of the internal buffer 
/// </summary>
/// <typeparam name="TBaseType"></typeparam>

public abstract unsafe class BaseBufferTyped<TBaseType> : BaseBufferUntyped where TBaseType: unmanaged
{
    public uint Length
    {
        get
        {
            if (BufferObject?.IsDisposed == false)
            {
                return (uint)(BufferObject.SizeInBytes / sizeof(TBaseType));
            }
            else
            {
                return 0;
            }
        }
    }

    public void ModifyBuffer(ReadOnlySpan<TBaseType> readOnlySpan, GraphicsDevice device)
    {
        ModifyBuffer<TBaseType>(readOnlySpan, device);
    }

    /// <summary>
    /// Modifies a portion of the buffer with an offset and count.
    /// </summary>
    /// <param name="readOnlySpan">The data you which to feed into your index buffer</param>
    /// <param name="device">The graphics device you wish to use to feed into your buffer</param>
    /// <param name="count">The number of elements you wish to insert into TData from the offset</param>
    /// <typeparam name="TBaseType">The type of variable you wish to fill in</typeparam>
    public void ModifyBuffer(ReadOnlySpan<TBaseType> readOnlySpan, GraphicsDevice device, int count)
    {
        ModifyBuffer(readOnlySpan[..count], device);
    }

    protected BaseBufferTyped(GraphicsDevice device) : base(device)
    {
    }
}