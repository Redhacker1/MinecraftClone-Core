using System;
using System.Collections.Generic;
using System.IO;
using Pfim;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using IS = SixLabors.ImageSharp;

namespace Engine.Rendering
{
    
    //TODO: Expose options in constructor for materials (eg, flipmode, texture filtering, clamp mode, etc)
    public class Texture : IDisposable, IGraphicsResource
    {
        public Veldrid.Texture _texture;
        

        public Texture(GraphicsDevice device, string path, bool flipX = false, bool flipY = false)
        {
            var cfg = IS.Configuration.Default;
            cfg.PreferContiguousImageBuffers = true;
            IS.Image<Rgba32> img;

            if (Path.GetExtension(path) != ".dds")
            {
                img = (IS.Image<Rgba32>) IS.Image.Load(cfg,path);
            }
            else
            {
                byte[] newData;
                int width;
                int height;
                using (IImage image = Pfim.Pfim.FromFile(path))
                {
                    width = image.Width;
                    height = image.Height;
                    
                    // Since image sharp can't handle data with line padding in a stride
                    // we create an stripped down array if any padding is detected
                    var tightStride = image.Width * image.BitsPerPixel / 8;
                    if (image.Stride != tightStride)
                    {
                        newData = new byte[image.Height * tightStride];
                        for (int i = 0; i < image.Height; i++)
                        {
                            Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
                        }
                    }
                    else
                    {
                        newData = image.Data;
                    }
                    image.Dispose();
                }
                
                img = IS.Image.LoadPixelData<Rgba32>(cfg, newData, width, height);
            }

            if (flipX)
            {
                img.Mutate(x => x.Flip(FlipMode.Horizontal));
            }
            if (flipY)
            {
                img.Mutate(x => x.Flip(FlipMode.Vertical));
            }
            var Memory = new Memory<Rgba32>();
            if (img.DangerousTryGetSinglePixelMemory(out Memory))
            {
                
                Load(device, Memory.Span, (uint) img.Width, (uint) img.Height);
            }
            else
            {
                throw new Exception("Engine failed to read texture");
            }
            img.Dispose();
        }

        Texture()
        {
            
        }

        public static Texture CreateFromBytes(GraphicsDevice device, uint width, uint height, Span<byte> data)
        {
            Texture tex = new Texture();
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
            tex._texture = device.ResourceFactory.CreateTexture(textureDescription);
            device.UpdateTexture(tex._texture, data,0, 0,0, width, height, 1, 0, 0);
            return tex;
        }



        public Texture(GraphicsDevice device, Span<byte> data)
        {
            
            var cfg = IS.Configuration.Default;
            cfg.PreferContiguousImageBuffers = true;
            IS.Image<Rgba32> img = IS.Image.Load<Rgba32>(cfg, data);
            Texture tex = new Texture();
            var Memory = new Memory<Rgba32>();
            
            if (img.DangerousTryGetSinglePixelMemory(out Memory))
            {
                tex.Load(device, Memory.Span, (uint) img.Width, (uint) img.Height);
            }
            else
            {
                throw new Exception("Engine failed to read texture");
            }
            img.Dispose();
        }


        public void UpdateTextureBytes(GraphicsDevice device, Span<byte> bytes, uint width, uint height)
        {

            if (width == _texture.Width && height == _texture.Height)
            {
                device.UpdateTexture(_texture, bytes,0, 0,0, width, height, 1, 0, 0);
                return;
            }
            _texture.Dispose();
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
            _texture = device.ResourceFactory.CreateTexture(textureDescription);
            device.UpdateTexture(_texture,bytes,0,0,0, width, height, 1, 0, 0);
            
        }

        void Load(GraphicsDevice graphicsDevice, ReadOnlySpan<Rgba32> data, uint width, uint height)
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
             _texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);
            
            graphicsDevice.UpdateTexture(_texture, data,0, 0,0, width, height, 1, 0, 0);
        }

        public void Dispose()
        {
            _texture.Dispose();
        }

        (ResourceKind, BindableResource) IGraphicsResource.GetUnderlyingResources()
        {
            return (ResourceKind.TextureReadOnly, _texture);
        }
    }
}
