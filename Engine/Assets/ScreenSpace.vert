#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 TexCoords;

layout(location = 0) out vec2 TCoords;

void main() 
{
    gl_Position = vec4(Position.xy, 1, 1);
    TCoords = TexCoords.xy;
}
