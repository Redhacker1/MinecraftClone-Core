#version 330

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
    mat4 View;
};

layout(set = 1, binding = 0) uniform mat4 WorldBuffer;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    gl_Position = Projection * View * World * vec4(vPos, 1);
    fsin_texCoords = TexCoords;
}