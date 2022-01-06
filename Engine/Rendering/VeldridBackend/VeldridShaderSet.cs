using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Engine.Rendering.Shared.Buffers;
using Engine.Rendering.Shared.Shaders;
using Veldrid;
using Shader = Engine.Rendering.Shared.Shaders.Shader;

namespace Engine.Rendering.VeldridBackend
{
    internal record VeldridShaderSet : ShaderStageSet
    {
        internal Veldrid.ShaderSetDescription backingShaderSet;
        internal VeldridShaderSet(IReadOnlyDictionary<ShaderType, Shader> shaders, IReadOnlyList<VertexLayout> layouts) :
            base(shaders, layouts)
        {
            List<Veldrid.Shader> shaderObjects = new List<Veldrid.Shader>(8);
            shaderObjects.AddRange(from variable in shaders where variable.Value is VeldridShader select ((VeldridShader)variable.Value).BackingShader);

            VertexElementDescription[] vertexLayout = new VertexElementDescription[layouts.Count];
            for (int elementI = 0; elementI < layouts.Count; elementI++)
            {
                VertexLayout element = layouts[elementI];
                if (element != null)
                {
                    VertexElementFormat elementFormat = element.Type switch
                    {
                        AttributeType.Float => VertexElementFormat.Float1,
                        AttributeType.Int => VertexElementFormat.Int1,
                        AttributeType.Half => VertexElementFormat.Half1,
                        AttributeType.UnsignedInteger => VertexElementFormat.UInt1,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    //TODO: Implement DX11 compliant VertexElementSemantic option.
                    vertexLayout[elementI] = new VertexElementDescription(element.Name, VertexElementSemantic.TextureCoordinate, elementFormat, element.Offset);
                }
            }

            VertexLayoutDescription layout = new VertexLayoutDescription(vertexLayout);
            backingShaderSet = new ShaderSetDescription()
            {
                Shaders = shaderObjects.ToArray(),
                VertexLayouts = new[]{layout}

            };
        }
    }
}