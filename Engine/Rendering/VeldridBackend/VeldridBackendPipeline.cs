using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Engine.Rendering.Shared;
using Engine.Rendering.Shared.Pipeline;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
{
    sealed class VeldridBackendPipeline : GraphicsRenderState
    {
        internal Veldrid.Pipeline backingpipeline;

        internal VeldridBackendPipeline(RenderStateDescription description, GraphicsDevice graphicsDevice) : 
            base(description)
        {
            ComparisonKind comparetype = description.CompareType switch
            {
                ComparisonType.Greater => ComparisonKind.Greater,
                ComparisonType.GreaterOrEqual => ComparisonKind.GreaterEqual,
                ComparisonType.Less => ComparisonKind.Less,
                ComparisonType.LessOrEqual => ComparisonKind.LessEqual,
                ComparisonType.NotEqual => ComparisonKind.NotEqual,
                ComparisonType.Equal => ComparisonKind.Equal,
                ComparisonType.Always => ComparisonKind.Always,
                ComparisonType.Never => ComparisonKind.Never,
                _ => throw new ArgumentOutOfRangeException()
            };
            PrimitiveTopology topology = description._topology switch
            {
                Topology.TriangleStrip => PrimitiveTopology.TriangleList,
                Topology.TriangleList => PrimitiveTopology.TriangleList,
                _ => throw new ArgumentOutOfRangeException()
            };
            FaceCullMode cullMode = description.cullmode switch
            {
                FaceCullSetting.Front => FaceCullMode.Front,
                FaceCullSetting.Back => FaceCullMode.Back,
                FaceCullSetting.None => FaceCullMode.None,
                _ => throw new ArgumentOutOfRangeException()
            };
            FrontFace frontFace = description.frontface switch
            {
                FrontFaceDir.Clockwise => FrontFace.Clockwise,
                FrontFaceDir.CounterClockwise => FrontFace.CounterClockwise,
                _ => throw new ArgumentOutOfRangeException(nameof(description.frontface))
            };
            
            
            GraphicsPipelineDescription pipelinedata = new GraphicsPipelineDescription()
            {
                Outputs = graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                DepthStencilState = new DepthStencilStateDescription()
                {
                    DepthComparison = comparetype,
                    DepthTestEnabled = description.DepthTestEnabled,
                    DepthWriteEnabled = description.DepthWrite,
                },
                BlendState = BlendStateDescription.Empty,
                PrimitiveTopology = topology,
                RasterizerState = new RasterizerStateDescription()
                {
                    CullMode = cullMode,
                    FillMode = PolygonFillMode.Solid,
                    FrontFace = frontFace,
                    DepthClipEnabled = true,
                    ScissorTestEnabled = true
                },
                ShaderSet = ((VeldridShaderSet)description._shaders).backingShaderSet


            };
            backingpipeline = graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelinedata);
        }
    }
}