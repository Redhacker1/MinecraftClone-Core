using System;
using System.Collections.Generic;
using System.Linq;
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

        public Texture(GraphicsDevice device, string path)
        {
            Image<Rgba32> img = (Image<Rgba32>) Image.Load(path);
            img.Mutate(x => x.Flip((FlipMode) 3));

            List<Rgba32> pictureData = new List<Rgba32>(img.Width * img.Height);

            for (int row = 0; row < img.Height; row++)
            {
                pictureData.AddRange(img.GetPixelRowSpan(row).ToArray());
            }

            Load(device,pictureData, (uint) img.Width, (uint) img.Height);

            img.Dispose();
        }
        

        private void Load(GraphicsDevice graphicsDevice, IEnumerable<Rgba32> data, uint width, uint height)
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UInt, TextureUsage.Sampled);
             _texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);
            
            graphicsDevice.UpdateTexture(_texture, data.ToArray(),0, 0,0, width, height, 1, 0, 0 );
        }

        public void Dispose()
        {
            _texture.Dispose();
        }
    }
}
