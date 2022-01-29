using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;

namespace Engine.Rendering
{
    
    //TODO: Expose options in constructor for materials (eg, flipmode, texture filtering, clamp mode, etc)
    public class Texture : IDisposable, IGraphicsResource
    {
        public Veldrid.Texture _texture;

        public unsafe Texture(GraphicsDevice device, string path)
        {
            Image<Rgba32> img = (Image<Rgba32>) Image.Load(path);
            //img.Mutate(x => x.Flip(FlipMode.Horizontal));
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


        void Load(GraphicsDevice graphicsDevice, IEnumerable<Rgba32> data, uint width, uint height)
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
             _texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);
            
            graphicsDevice.UpdateTexture(_texture, data.ToArray(),0, 0,0, width, height, 1, 0, 0);
        }

        public void Dispose()
        {
            _texture.Dispose();
        }
    }
}
