#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 0) uniform sampler SurfaceSampler;
layout(set = 0, binding = 1) uniform texture2D SurfaceTexture;

void main()
{
    if(texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords).a < .1f)
    {
        discard;
    }
    fsout_color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
}
