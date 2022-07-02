#version 450

layout(set = 0, binding = 0) uniform ViewProjBuffer
{
    mat4 Projection;
    mat4 View;
};
layout(set = 0, binding = 1) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 TexCoords;
layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    gl_Position = vec4(Position, 1);
    fsin_texCoords = TexCoords.xy;
}
