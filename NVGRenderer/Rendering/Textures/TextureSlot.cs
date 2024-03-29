﻿using SilkyNvg.Images;
using Veldrid;
using Texture = Engine.Rendering.VeldridBackend.Texture;

namespace NVGRenderer.Rendering.Textures;

public struct TextureSlot : IDisposable, IEquatable<TextureSlot>
{
    
    public ImageFlags Flags;
    public Sampler TextureSampler;
    public Texture Texture;
    public int Id { get; internal set; }

    public void Dispose()
    {
        Texture?.Dispose();
    }

    public bool HasFlag(ImageFlags flag)
    {
        return FlagHelper.HasFlag((int) Flags, (int)flag);
    }

    public bool Equals(TextureSlot other)
    {
        return Flags == other.Flags && Texture.Equals(other.Texture) && Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is TextureSlot other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) Flags, Texture, Id);
    }
}