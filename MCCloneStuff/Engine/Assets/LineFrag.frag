#version 450


layout(set = 1, location = 0) uniform float uLineWidth;
layout(set = 1, location = 1) uniform vec4 uColor;
layout(set = 1, location = 2) uniform float uBlendFactor; //1.5..2.5

layout(location = 0) in vec2 vLineCenter;
layout(location = 4) out vec4 FragColor;

void main()
{
    vec4 col = uColor;
    float d = length(vLineCenter-gl_FragCoord.xy);
    float w = uLineWidth;
    if (d>w)
    {
        col.w = 0;   
    }
    else
    {
        col.w *= pow(float((w-d)/w), uBlendFactor);   
    }
    FragColor = col;
}