#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;


layout(set = 1, binding = 1) uniform sampler2D SurfaceTexture;
layout(set = 1, binding = 0) uniform sampler Samp;
void main()
{
    fsout_color = texture(sampler2D(SurfaceTexture, Samp), fsin_TexCoord);
}
