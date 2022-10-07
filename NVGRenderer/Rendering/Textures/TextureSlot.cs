using System.Runtime.CompilerServices;
using SilkyNvg.Images;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace NVGRenderer.Rendering.Textures;

public struct TextureSlot : IDisposable, IEquatable<TextureSlot>
{

    static Random _random = new Random();
    public ImageFlags _flags;
    public Texture _texture;
    public int Id { get; internal set; }

    public void Dispose()
    {
        _texture.Dispose();
    }

    public bool HasFlag(ImageFlags flag)
    {
        return _flags.HasFlag(flag);
    }

    public bool Equals(TextureSlot other)
    {
        return _flags == other._flags && _texture.Equals(other._texture) && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextureSlot other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) _flags, _texture, Id);
    }
}