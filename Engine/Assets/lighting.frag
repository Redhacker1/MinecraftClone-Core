#version 450


layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 1, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler;
layout(set = 1, binding = 2) uniform AmbientLight
{
    vec4 LightData = vec4(1,1,1,0);
};


void main()
{
    fsout_color =  LightData; //texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
}
