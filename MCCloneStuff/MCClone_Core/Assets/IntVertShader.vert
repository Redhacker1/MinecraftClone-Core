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

layout(location = 0) in int PositionXZ;
layout(location = 1) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    int Y =  PositionXZ & 511;
    int X =  PositionXZ >> 14;
    int Z = (PositionXZ >> 9) & 31;
    
    vec4 worldPosition = World * vec4(X, Y, Z, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = vec2(TexCoords);
}