using System;

namespace Engine.Rendering.Abstract;

[Flags]
public enum BufferType : ushort
{
    Error = 1 << 0,
    Uniform = 1 << 1,
    Vertex = 1 << 2,
    Index = 1 << 3,
    Staging = 1 << 4,
    StructuredReadWrite = 1 << 5,
    StructuredReadOnly = 1 << 6,
    Mappable = 1 << 7,
    
}

public enum ShaderType
{
    Vertex,
    Fragment
}

public enum TextureDimensionality
{
    Texture1D,
    Texture2D,
    Texture3D
}

[Flags]
public enum TextureUsages
{
    Sampled = 1 << 0,
    ReadWrite = 1 << 1,
    RenderTarget = 1 << 2,
    DepthStencil = 1 << 3,
    MipMapped = 1 << 4,
    CubeMapped = 1 << 5,
    StagingTexture = 1 << 0
}

public enum TexelFormat
{
    R8G8B8A8_UNorm,
    R16_Float,
    R8_UNorm,
    R8G8_UNorm



}