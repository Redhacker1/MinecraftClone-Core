using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Engine.Rendering.Veldrid;
using Veldrid;
using Pipeline = Engine.Rendering.Veldrid.Pipeline;
using Texture = Engine.Rendering.Veldrid.Texture;

namespace Engine.Rendering.Abstract;

public class EngineSpriteRenderer : RenderPass
{
    #region Shaders
    public const string VertexShader = @"
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec4 aTint;
layout (location = 3) in float aRotation;
layout (location = 4) in vec2 aOrigin;
layout (location = 5) in vec2 aScale;
out vec2 frag_texCoords;
out vec4 frag_tint;
uniform mat4 uProjectionView;
void main()
{
    float cosRot = cos(aRotation);
    float sinRot = sin(aRotation);
    mat2 rot = mat2(vec2(cosRot, sinRot), vec2(-sinRot, cosRot));
    vec2 vertexPos = aPosition.xy - aOrigin;
    vertexPos *= aScale;
    vertexPos = rot * vertexPos;
    vertexPos += aOrigin;
    gl_Position = vec4(vertexPos, 0.0, 1.0) * uProjectionView;
    vec2 texCoords = aTexCoords;
    texCoords.y *= -1;
    frag_texCoords = texCoords;
    frag_tint = aTint;
}";

    public const string FragmentShader = @"
in vec2 frag_texCoords;
in vec4 frag_tint;
out vec4 out_color;
uniform sampler2D uTexture;
uniform bool uUseTexture;
void main()
{
    vec4 tex = texture(uTexture, frag_texCoords);
    // Invert it here to account for frag shaders that don't implement uUseTexture
    out_color = (!uUseTexture ? tex : vec4(1.0, 1.0, 1.0, tex.a)) * frag_tint;
}";
    #endregion
    

    Pipeline SpritePipeline;

    public EngineSpriteRenderer(Renderer renderer) : base(renderer)
    {
        /*SpritePipeline = new Pipeline(false, false,
            ComparisonKind.Less,
            FaceCullMode.Front,
            FrontFace.CounterClockwise,
            PrimitiveTopology.TriangleList, PolygonFillMode.Solid, new Dictionary<ShaderStages, Shader>()
            {
                {ShaderStages.Vertex, Shader.LoadShaderText(VertexShader, WindowClass._renderer.Device, ShaderStages.Vertex)},
                {ShaderStages.Fragment, Shader.LoadShaderText(FragmentShader, WindowClass._renderer.Device, ShaderStages.Fragment)}
            }, 
            WindowClass._renderer.Device,
            new VertexLayoutDescription(
                new VertexElementDescription(
                    
                    )
                ));
                */
    }

    protected override void Pass(CommandList list, List<Instance3D> instances, ref CameraInfo info)
    {
        throw new System.NotImplementedException();
    }
}

#region Sprites
public enum SpriteFlipMode
{
    /// <summary>
    /// The sprite will not be flipped.
    /// </summary>
    None,
    /// <summary>
    /// Flip the sprite in the X-axis.
    /// </summary>
    FlipX,
    /// <summary>
    /// Flip the sprite in the Y-axis.
    /// </summary>
    FlipY,
    /// <summary>
    /// Flip the sprite in both the X and Y axis.
    /// </summary>
    FlipXY
}

struct Sprite
{
    public Texture Texture;
    public Vector2 Position;
    public Rectangle? Source;
    public Color Tint;
    public float Rotation;
    public Vector2 Origin;
    public Vector2 Scale;
    public SpriteFlipMode Flip;
    public float Depth;
    public uint ID;
    public bool UseTexture;

    public Sprite(Texture texture, Vector2 position, Rectangle? source, Color tint, float rotation, Vector2 origin,
        Vector2 scale, SpriteFlipMode flip, float depth, uint id, bool useTexture)
    {
        Texture = texture;
        Position = position;
        Source = source;
        Tint = tint;
        Rotation = rotation;
        Origin = origin;
        Scale = scale;
        Flip = flip;
        Depth = depth;
        ID = id;
        UseTexture = useTexture;
    }
}

struct SpriteVertex
{
    public Vector2 Position;
    public Vector2 TexCoords;
    public Vector4 Tint;
    public float Rotation;
    public Vector2 Origin;
    public Vector2 Scale;

    public SpriteVertex(Vector2 position, Vector2 texCoords, Vector4 tint, float rotation, Vector2 origin, Vector2 scale)
    {
        Position = position;
        TexCoords = texCoords;
        Tint = tint;
        Rotation = rotation;
        Origin = origin;
        Scale = scale;
    }

    // Precalculated size in bytes. This is precalculated as it's used in the size constant above and you can't use
    // sizeof() structs in constant fields.
    public const uint SizeInBytes = 52;
}
#endregion
