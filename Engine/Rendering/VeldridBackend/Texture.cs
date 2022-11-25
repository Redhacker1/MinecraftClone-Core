using System;
using System.IO;
using DdsKtxSharp;
using Engine.Utilities.MathLib;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using IS = SixLabors.ImageSharp;

namespace Engine.Rendering.VeldridBackend
{
    struct BC1
    {
        ushort[] rgb; // 565 colors
        uint bitmap; // 2bpp rgb bitmap

        public BC1()
        {
            rgb = new ushort[2];
            bitmap = 0;
        }
    };
    
    
    //TODO: Expose options in constructor for materials (eg, flip-mode, texture filtering, clamp mode, etc)
    public class Texture : GraphicsResource
    {

        public Veldrid.Texture _Texture;

        public Texture(GraphicsDevice device, string path, bool flipX = false, bool flipY = false) : base(device)
        {
            var cfg = IS.Configuration.Default;
            cfg.PreferContiguousImageBuffers = true;
            IS.Image<Rgba32> img;

            if (Path.GetExtension(path) != ".dds")
            {
                img = (IS.Image<Rgba32>) IS.Image.Load(cfg, path);
            }
            else
            {

                DdsKtxParser parser = DdsKtxParser.FromMemory(File.ReadAllBytes(path));
                byte[] imageData = parser.GetSubData(0, 0, 0, out DdsKtx.ddsktx_sub_data _);
                int width = parser.Info.width;
                int height = parser.Info.height;
                switch (parser.Info.format)
                {

                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGBA8:
                        img = IS.Image.LoadPixelData<Bgra32>(cfg, imageData, width, height).CloneAs<Rgba32>();
                        break;
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC1:
                        return;
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC2:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC3:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC4:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC5:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC6H:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BC7:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ETC1:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ETC2:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ETC2A:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ETC2A1:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC12:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC14:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC12A:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC14A:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC22:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_PTC24:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ATC:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ATCE:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ATCI:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC4x4:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC5x5:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC6x6:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC8x5:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC8x6:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_ASTC10x5:
                    case DdsKtx.ddsktx_format._DDSKTX_FORMAT_COMPRESSED:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_A8:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_R8:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGBA8S:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG16:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGB8:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_R16:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_R32F:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_R16F:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG16F:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG16S:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGBA16F:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGBA16:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_BGRA8:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RGB10A2:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG11B10F:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG8:
                    case DdsKtx.ddsktx_format.DDSKTX_FORMAT_RG8S:
                    case DdsKtx.ddsktx_format._DDSKTX_FORMAT_COUNT:
                    default:
                        Console.WriteLine(parser.Info.format);
                        //throw new ArgumentOutOfRangeException();
                        //break;
                        return;
                }



                // Since image sharp can't handle data with line padding in a stride
                // we create an stripped down array if any padding is detected
                int tightStride = width * height;



                if (imageData.Length != tightStride)
                {
                    var newData = new byte[tightStride];
                    for (int i = 0; i < height; i++)
                    {
                        Buffer.BlockCopy(newData, i * width, newData, i * tightStride, tightStride);
                    }

                    imageData = newData;
                }

            }

            if (flipX)
            {
                img.Mutate(x => x.Flip(FlipMode.Horizontal));
            }

            if (flipY)
            {
                img.Mutate(x => x.Flip(FlipMode.Vertical));
            }

            if (img.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            {

                Load(device, memory.Span, (uint) img.Width, (uint) img.Height);
            }
            else
            {
                throw new Exception("Engine failed to read texture");
            }

            img.Dispose();
        }

        Texture(GraphicsDevice device) : base(device)
        {

        }

        public Texture(uint width, uint height, uint mips, GraphicsDevice device, PixelFormat format = PixelFormat.R8_G8_B8_A8_UNorm) : base(device)
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mips, 1,
                format, TextureUsage.Sampled);
            _Texture = device.ResourceFactory.CreateTexture(textureDescription);
        }

        public static Texture CreateFromBytes(GraphicsDevice device, uint width, uint height, ReadOnlySpan<byte> data, PixelFormat format = PixelFormat.R8_G8_B8_A8_UNorm)
        {
            Texture tex = new Texture(device);
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                format, TextureUsage.Sampled);
            tex._Texture = device.ResourceFactory.CreateTexture(textureDescription);
            device.UpdateTexture(tex._Texture, data, 0, 0, 0, width, height, 1, 0, 0);
            return tex;
        }



        public Texture(GraphicsDevice device, Span<byte> data) : base(device)
        {

            IS.Configuration cfg = IS.Configuration.Default;
            cfg.PreferContiguousImageBuffers = true;
            IS.Image<Rgba32> img = IS.Image.Load<Rgba32>(cfg, data);
            Texture tex = new Texture(device);

            if (img.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            {
                tex.Load(device, memory.Span, (uint) img.Width, (uint) img.Height);
            }
            else
            {
                throw new Exception("Engine failed to read texture");
            }

            img.Dispose();
        }


        public void UpdateTextureBytes(GraphicsDevice device, ReadOnlySpan<byte> bytes, Int2 offset, Int2 bounds)
        {
            // if the addition totally fits inside the current texture, update the current texture and return.
            if (bounds.X + offset.X <= _Texture.Width && bounds.Y + offset.Y <= _Texture.Height)
            {
                device.UpdateTexture(_Texture, bytes, (uint)offset.X, (uint)offset.Y, 0, (uint)bounds.X, (uint)bounds.Y, 1, 0, 0);

                return;
            }

            // Store the current texture for proper disposal at a later time and for reference when setting the new texture up.
            var oldTexture = _Texture;

            // Create a new texture fit to the correct size.
            TextureDescription textureDescription = TextureDescription.Texture2D(
                (uint)offset.X + (uint)bounds.X, (uint)offset.Y + (uint)bounds.Y, mipLevels: 1, 1, oldTexture.Format, oldTexture.Usage);
            _Texture = device.ResourceFactory.CreateTexture(textureDescription);

            // Copy the old texture's contents into the new one. We need to use a command list for this unfortunately...
            using (CommandList commandBuffer = device.ResourceFactory.CreateCommandList())
            {
                commandBuffer.CopyTexture(oldTexture, 0, 0, 0, 0, 0, _Texture, 0, 0, 0, 0, 0, oldTexture.Width, oldTexture.Height, oldTexture.Depth, oldTexture.ArrayLayers);
                device.SubmitCommands(commandBuffer);
            }

            // Update the texture we just created with the new information
            device.UpdateTexture(_Texture, bytes, 0, 0, 0, (uint)offset.X, (uint)offset.Y, 1, 0, 0);

            //dispose of the old texture and command list used to copy the texture
            device.DisposeWhenIdle(oldTexture);
        }

        void Load(GraphicsDevice graphicsDevice, ReadOnlySpan<Rgba32> data, uint width, uint height)
        {
            TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
            _Texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);

            graphicsDevice.UpdateTexture(_Texture, data, 0, 0, 0, width, height, 1, 0, 0);
        }
        
        void Load(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> data, uint width, uint height, PixelFormat format)
        {
            if (graphicsDevice.GetPixelFormatSupport(format, TextureType.Texture2D, TextureUsage.Sampled))
            {
                TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels: 1, 1,
                    format, TextureUsage.Sampled);
                _Texture = graphicsDevice.ResourceFactory.CreateTexture(textureDescription);

                graphicsDevice.UpdateTexture(_Texture, data, 0, 0, 0, width, height, 1, 0, 0);
            }
            else
            {
                throw new NotImplementedException("This texture format is not implemented in your Graphics Device!");
            }

        }

        protected override void OnDispose()
        {
            _device.DisposeWhenIdle(_Texture);
        }

        public override (ResourceKind, BindableResource) GetUnderlyingResource()
        {
            return (ResourceKind.TextureReadOnly, _Texture);
        }
        
        ~Texture()
        {
            _device.DisposeWhenIdle(_Texture);
        }
    }
    
}
