using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using Pfim;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using IS = SixLabors.ImageSharp;

namespace Engine.Rendering
{
    
    //TODO: Expose options in constructor for materials (eg, flipmode, texture filtering, clamp mode, etc)
    public class Texture : IDisposable, IGraphicsResource
    {
        public Veldrid.Texture _texture;
        

        public unsafe Texture(GraphicsDevice device, string path, bool flipX = false, bool flipY = false)
        {
            IS.Image<Rgba32> img;

            if (Path.GetExtension(path) != ".dds")
            {
                img = (IS.Image<Rgba32>) IS.Image.Load(path);
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
                
                img = IS.Image.LoadPixelData<Rgba32>(newData, width, height);
            }

            if (flipX)
            {
                img.Mutate(x => x.Flip(FlipMode.Horizontal));
            }
            if (flipY)
            {
                img.Mutate(x => x.Flip(FlipMode.Vertical));
            }
            if (img.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan))
            {
                Load(device, pixelSpan.ToArray(), (uint) img.Width, (uint) img.Height);
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



        public Texture(GraphicsDevice device, byte[] data)
        {
            var img = IS.Image.Load(data);
            var tex = new Texture();
            if (img.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan))
            {
                tex.Load(device, pixelSpan.ToArray(), (uint) img.Width, (uint) img.Height);
            }
            else
            {
                throw new Exception("Engine failed to read texture");
            }
            img.Dispose();
        }


        void Load(GraphicsDevice graphicsDevice, Rgba32[] data, uint width, uint height)
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
