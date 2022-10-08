using System;
using Veldrid;

namespace NVGRenderer.Rendering.Pipelines
{
    public struct PipelineSettings : IEquatable<PipelineSettings>
    {

        public FaceCullMode CullMode;
        public FrontFace FrontFace;

        public bool DepthTestEnabled;
        public bool DepthWrite;
        public ComparisonKind DepthWriteMode;


        public ColorWriteMask ColourMask;

        public uint StencilWriteMask;

        public StencilOperation FrontStencilFailOp;
        public StencilOperation FrontStencilDepthFailOp;
        public StencilOperation FrontStencilPassOp;
        public StencilOperation BackStencilFailOp;
        public StencilOperation BackStencilDepthFailOp;
        public StencilOperation BackStencilPassOp;

        public StencilOperation StencilFailOp
        {
            set => FrontStencilFailOp = BackStencilFailOp = value;
        }

        public StencilOperation StencilDepthFailOp
        {
            set => FrontStencilDepthFailOp = BackStencilDepthFailOp = value;
        }

        public StencilOperation StencilPassOp
        {
            set => FrontStencilPassOp = BackStencilPassOp = value;
        }

        public ComparisonKind StencilFunc;
        public uint StencilRef;
        public uint StencilMask;

        public bool StencilTestEnable;

        public PrimitiveTopology Topology;

        public SilkyNvg.Blending.CompositeOperationState CompositeOperation;

        public BlendFactor SrcRgb => ConvertBlend(CompositeOperation.SrcRgb);

        public BlendFactor SrcAlpha => ConvertBlend(CompositeOperation.SrcAlpha);

        public BlendFactor DstRgb => ConvertBlend(CompositeOperation.DstRgb);

        public BlendFactor DstAlpha => ConvertBlend(CompositeOperation.DstAlpha);

        private static BlendFactor ConvertBlend(SilkyNvg.Blending.BlendFactor factor)
        {
            return factor switch
            {
                SilkyNvg.Blending.BlendFactor.Zero => BlendFactor.Zero,
                SilkyNvg.Blending.BlendFactor.One => BlendFactor.One,
                SilkyNvg.Blending.BlendFactor.SrcColour => BlendFactor.SourceColor,
                SilkyNvg.Blending.BlendFactor.OneMinusSrcColour => BlendFactor.InverseSourceColor,
                SilkyNvg.Blending.BlendFactor.DstColour => BlendFactor.DestinationColor,
                SilkyNvg.Blending.BlendFactor.OneMinusDstColour => BlendFactor.InverseSourceColor,
                SilkyNvg.Blending.BlendFactor.SrcAlpha => BlendFactor.SourceAlpha,
                SilkyNvg.Blending.BlendFactor.OneMinusSrcAlpha => BlendFactor.InverseSourceAlpha,
                SilkyNvg.Blending.BlendFactor.DstAlpha => BlendFactor.DestinationAlpha,
                SilkyNvg.Blending.BlendFactor.OneMinusDstAlpha => BlendFactor.InverseDestinationAlpha,
                SilkyNvg.Blending.BlendFactor.SrcAlphaSaturate => BlendFactor.InverseSourceAlpha,
                _ => BlendFactor.BlendFactor
            };
        }

        private static PipelineSettings Default()
        {
            
            return new PipelineSettings
            {
                CullMode = FaceCullMode.Back,
                FrontFace = FrontFace.CounterClockwise,
                DepthTestEnabled = false,
                ColourMask = ColorWriteMask.All,
                StencilMask = 0xffffffff,
                StencilFailOp = StencilOperation.Keep,
                StencilDepthFailOp = StencilOperation.Keep,
                StencilPassOp = StencilOperation.Keep,
                StencilFunc = ComparisonKind.Always,
                StencilRef = 0,
                StencilWriteMask = 0xffffffff,
            };
        }

        public static PipelineSettings ConvexFill(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();
            settings.CompositeOperation = compositeOperation;
            settings.StencilTestEnable = false;
            settings.Topology = PrimitiveTopology.TriangleStrip;
            return settings;
        }

        public static PipelineSettings ConvexFillEdgeAa(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = ConvexFill(compositeOperation);
            settings.Topology = PrimitiveTopology.TriangleStrip;
            return settings;
        }

        public static PipelineSettings FillStencil(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.StencilTestEnable = true;

            settings.StencilWriteMask = 0xff;

            settings.StencilFunc = ComparisonKind.Always;
            settings.StencilRef = 0;
            settings.StencilMask = 0xff;

            settings.ColourMask = 0;

            settings.FrontStencilFailOp = StencilOperation.Keep;
            settings.FrontStencilDepthFailOp = StencilOperation.Keep;
            settings.FrontStencilPassOp = StencilOperation.IncrementAndWrap;

            settings.BackStencilFailOp = StencilOperation.Keep;
            settings.BackStencilDepthFailOp = StencilOperation.Keep;
            settings.BackStencilPassOp = StencilOperation.DecrementAndWrap;

            settings.CullMode = FaceCullMode.Back;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings FillEdgeAa(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = FillStencil(compositeOperation);

            settings.CullMode = FaceCullMode.Back;
            settings.ColourMask = ColorWriteMask.All;

            settings.StencilFunc = ComparisonKind.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOperation.Keep;
            settings.StencilDepthFailOp = StencilOperation.Keep;
            settings.StencilPassOp = StencilOperation.Keep;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Fill(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = FillEdgeAa(compositeOperation);

            settings.StencilFunc = ComparisonKind.NotEqual;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOperation.Zero;
            settings.StencilDepthFailOp = StencilOperation.Zero;
            settings.StencilPassOp = StencilOperation.Zero;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Stroke(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStrokeStencil(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.StencilTestEnable = true;

            settings.StencilMask = 0xff;

            settings.StencilFunc = ComparisonKind.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOperation.Keep;
            settings.StencilDepthFailOp = StencilOperation.Keep;
            settings.StencilPassOp = StencilOperation.IncrementAndClamp;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStrokeEdgeAA(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = StencilStrokeStencil(compositeOperation);

            settings.StencilFunc = ComparisonKind.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOperation.Keep;
            settings.StencilDepthFailOp = StencilOperation.Keep;
            settings.StencilPassOp = StencilOperation.Keep;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStroke(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = StencilStrokeEdgeAA(compositeOperation);
            settings.ColourMask = ColorWriteMask.All;

            settings.StencilFunc = ComparisonKind.Always;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOperation.Zero;
            settings.StencilDepthFailOp = StencilOperation.Zero;
            settings.StencilPassOp = StencilOperation.Zero;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Triangles(SilkyNvg.Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public bool Equals(PipelineSettings other)
        {
            // TODO: need to write a bitmask containing this information in maybe a couple of longs.
            return CullMode == other.CullMode
                   && FrontFace == other.FrontFace
                   && DepthTestEnabled == other.DepthTestEnabled
                   && DepthWrite == other.DepthWrite
                   && DepthWriteMode == other.DepthWriteMode
                   && ColourMask == other.ColourMask
                   && StencilWriteMask == other.StencilWriteMask
                   && FrontStencilFailOp == other.FrontStencilFailOp
                   && FrontStencilDepthFailOp == other.FrontStencilDepthFailOp
                   && FrontStencilPassOp == other.FrontStencilPassOp
                   && BackStencilFailOp == other.BackStencilFailOp
                   && BackStencilDepthFailOp == other.BackStencilDepthFailOp
                   && BackStencilPassOp == other.BackStencilPassOp
                   && StencilFunc == other.StencilFunc
                   && StencilRef == other.StencilRef
                   && StencilMask == other.StencilMask
                   && StencilTestEnable == other.StencilTestEnable
                   && Topology == other.Topology
                   && CompositeOperation.Equals(other.CompositeOperation);
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add((int) CullMode);
            hashCode.Add((int) FrontFace);
            hashCode.Add(DepthTestEnabled);
            hashCode.Add(DepthWrite);
            hashCode.Add((int) DepthWriteMode);
            hashCode.Add((int) ColourMask);
            hashCode.Add(StencilWriteMask);
            hashCode.Add((int) FrontStencilFailOp);
            hashCode.Add((int) FrontStencilDepthFailOp);
            hashCode.Add((int) FrontStencilPassOp);
            hashCode.Add((int) BackStencilFailOp);
            hashCode.Add((int) BackStencilDepthFailOp);
            hashCode.Add((int) BackStencilPassOp);
            hashCode.Add((int) StencilFunc);
            hashCode.Add(StencilRef);
            hashCode.Add(StencilMask);
            hashCode.Add(StencilTestEnable);
            hashCode.Add((int) Topology);
            hashCode.Add(CompositeOperation);
            return hashCode.ToHashCode();
        }
    }
}