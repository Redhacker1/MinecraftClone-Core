using System;
using Engine.Rendering.VeldridBackend;
using Veldrid;

namespace Engine.Renderable;

public abstract class GenericBaseModel : BaseRenderable
{
    protected IndexBuffer<uint> ebo;
    protected BaseBufferUntyped vbo;
    
    public bool UseIndexedDrawing {get; protected set; }

    protected internal override void BindResources(CommandList list)
    {

        if (vbo.BufferType != BufferUsage.VertexBuffer || ebo?.BufferType == 0 && Disposed == false)
        {
            //Buffer has not been initialized, not an error, just has not been initialized,
            //should not trip, that being said, if run on another thread it might, and just covering bases, if it does we just need to skip ahead and move along
            if (vbo.BufferType == 0 || ebo.BufferType == 0)
            {
                return;
            }
                
            throw new InvalidOperationException("Cannot bind non vertex or non index buffer!");
        }

        if (UseIndexedDrawing)
        {
            ebo?.Bind(list);   
        }
        vbo.Bind(list, 1);
            
    }

    protected override void ReleaseEngineResources()
    {
        ebo?.Dispose();
        vbo?.Dispose();   
    }

    protected internal override void Draw(CommandList list, uint count, uint start)
    {
        if (UseIndexedDrawing)
        {
            list.DrawIndexed(VertexElements, count, 0, 0, start);
        }
        else
        {
            list.Draw(VertexElements, count, 0, start);      
        }
    }
}