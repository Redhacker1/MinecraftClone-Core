using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Engine.Rendering
{
    public struct Pipeline : IEquatable<Pipeline>
    {
        ushort PipelineMask;
        readonly bool WriteDepthBuffer;
        readonly bool DepthTest;
        readonly ComparisonKind ComparisonKind;
        readonly BlendStateDescription blendState;
        readonly FaceCullMode _cullMode;
        readonly PolygonFillMode FillMode;
        readonly FrontFace faceDir;
        readonly PrimitiveTopology _topology;
        readonly IReadOnlyDictionary<ShaderStages ,Shader> _shaders;


        ushort HashValue;
        internal Veldrid.Pipeline _pipeline;

        public Pipeline(bool depthTest, bool writeDepthBuffer, ComparisonKind compare, FaceCullMode cullmode, FrontFace dir, PrimitiveTopology topology, PolygonFillMode filltype, IReadOnlyDictionary<ShaderStages ,Shader> shaders, GraphicsDevice device, VertexLayoutDescription vertexLayout,  params ResourceLayout[] layouts)
        {
            WriteDepthBuffer = writeDepthBuffer;
            DepthTest = depthTest;
            ComparisonKind = compare;
            blendState = BlendStateDescription.SingleOverrideBlend;
            _cullMode = cullmode;
            faceDir = dir;
            _topology = topology;
            FillMode = filltype;
            _shaders = shaders;
            
            List<Veldrid.Shader> Shader = new List<Veldrid.Shader>();
            ShaderSetDescription shaderSet = new ShaderSetDescription();

            foreach ((_, Shader value) in shaders)
            {
                Shader.Add(value.shader);
            }

            shaderSet.Shaders = Shader.ToArray();
            shaderSet.VertexLayouts = new[] {vertexLayout};
            
            ushort pipeline = (ushort)(BoolToNum(writeDepthBuffer) << 15); // boolean, 1 bit
            pipeline |= (ushort)(BoolToNum(depthTest) << 14); // boolean, 1 bit
            pipeline |= (ushort)((ushort)compare << 13); // enum range 0-7, three bits 
            pipeline |= (ushort)((ushort)cullmode << 10); // enum range 0-2, two bits 
            pipeline |= (ushort)((ushort)dir << 8); // enum range 0-1, one bit
            pipeline |= (ushort)((ushort)topology << 5); // enum range 0-4, three bits
            pipeline |= (ushort)((ushort)filltype << 4); // enum range 0-1, one bit
            HashValue = pipeline;
            
            
            PipelineMask = pipeline;
            GraphicsPipelineDescription description =
                new GraphicsPipelineDescription
                (
                    blendState,
                    new DepthStencilStateDescription(depthTest, writeDepthBuffer, compare),
                    new RasterizerStateDescription(cullmode, filltype, faceDir, true, false),
                    topology, 
                    shaderSet, 
                    layouts, 
                    device.MainSwapchain.Framebuffer.OutputDescription
                );

            _pipeline = device.ResourceFactory.CreateGraphicsPipeline(description);
            Console.WriteLine("Pipeline completed!");
        }

        public bool Equals(Pipeline other)
        {
            // TODO: Ensure that the shaders and (eventually) resources are the same before saying it is true
            if (PipelineMask == other.PipelineMask && _shaders?.Count == other._shaders?.Count)
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is Pipeline other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashValue;
        }

        public static bool operator ==(Pipeline left, Pipeline right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pipeline left, Pipeline right)
        {
            return !(left == right);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)] 
        static ushort BoolToNum(bool booleanValue)
        {
            return (booleanValue ? (ushort) 1u : (ushort) 0u);
        }
    }

}