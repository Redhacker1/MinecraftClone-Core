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

        internal Veldrid.Pipeline _pipeline;

        public Pipeline(bool depthTest, bool writeDepthBuffer, ComparisonKind compare, FaceCullMode cullmode, FrontFace dir, PrimitiveTopology topology, IReadOnlyDictionary<ShaderStages ,Shader> shaders, GraphicsDevice device, VertexLayoutDescription vertexLayout,  params ResourceLayout[] layouts)
        {
            WriteDepthBuffer = writeDepthBuffer;
            DepthTest = depthTest;
            ComparisonKind = compare;
            blendState = BlendStateDescription.SingleOverrideBlend;
            _cullMode = cullmode;
            faceDir = dir;
            _topology = topology;
            FillMode = PolygonFillMode.Solid;
            _shaders = shaders;
            
            List<Veldrid.Shader> Shader = new List<Veldrid.Shader>();
            ShaderSetDescription shaderSet = new ShaderSetDescription();

            foreach ((_, Shader value) in shaders)
            {
                Shader.Add(value.shader);
            }

            shaderSet.Shaders = Shader.ToArray();
            shaderSet.VertexLayouts = new[] {vertexLayout};
            
            
            ushort pipeline = (ushort)(BoolToNum(WriteDepthBuffer) << 15); // boolean, 1 bit
            pipeline |= (ushort)(BoolToNum(DepthTest) << 14); // boolean, 1 bit
            pipeline |= (ushort)((ushort)compare << 13); // enum range 0-7, three bits 
            pipeline |= (ushort)((ushort)cullmode << 10); // enum range 0-2, two bits 
            pipeline |= (ushort)((ushort)dir << 9); // enum range 0-1, one bit
            
            PipelineMask = pipeline;

            GraphicsPipelineDescription description =
                new GraphicsPipelineDescription
                (
                    blendState,
                    new DepthStencilStateDescription(depthTest, writeDepthBuffer, compare),
                    new RasterizerStateDescription(cullmode, PolygonFillMode.Solid, faceDir, true, false),
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
            if (PipelineMask == other.PipelineMask && _shaders.Count == other._shaders.Count)
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
            int pipeline = BoolToNum(WriteDepthBuffer) << 31; // boolean, 1 bit
            pipeline |= BoolToNum(DepthTest) << 30; // boolean, 1 bit
            pipeline |= (int)ComparisonKind << 29; // enum range 0-7, three bits 
            pipeline |= (int)_cullMode << 25; // enum range 0-2, two bits 
            pipeline |= (int)faceDir << 24; // enum range 0-1, one bit
            return pipeline;
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
            return (ushort)(booleanValue ? 1 : 0);
        }
    }

}