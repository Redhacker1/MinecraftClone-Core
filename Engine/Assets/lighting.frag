#version 450


layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 2, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 2, binding = 1) uniform sampler SurfaceSampler;
layout(set = 2, binding = 2) uniform AmbientLight
{
    vec4 lightColor;
};



void main()
{

    vec4 objectColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
    fsout_color = lightColor;
}
