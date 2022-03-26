#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
    mat4 View;
};

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};
layout(set = 2, binding = 0 ) uniform AtlasInfo
{
    int length;
    int width;
    ivec2 TextureSize;
};

layout(location = 0) in int PositionXZ;
layout(location = 1) in int PositionY;
layout(location = 2) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    int X = PositionXZ >> 5;
    int Z = PositionXZ & 31;


    int textureLength = length / TextureSize.x;
    int textureWidth = width / TextureSize.y;
    
    vec4 worldPosition = World * vec4(X, PositionY, Z, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = vec2(TexCoords);
}