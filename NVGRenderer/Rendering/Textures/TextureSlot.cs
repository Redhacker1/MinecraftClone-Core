using SilkyNvg.Images;
using Veldrid;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace NVGRenderer.Rendering.Textures;

public struct TextureSlot : IDisposable, IEquatable<TextureSlot>
{
    
    public ImageFlags Flags;
    public Sampler TextureSampler;
    public Texture _Texture;
    public int Id { get; internal set; }

    public void Dispose()
    {
        _Texture?.Dispose();
    }

    public bool HasFlag(ImageFlags flag)
    {
        return Flags.HasFlag(flag);
    }

    public bool Equals(TextureSlot other)
    {
        return Flags == other.Flags && _Texture.Equals(other._Texture) && Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is TextureSlot other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) Flags, _Texture, Id);
    }
}