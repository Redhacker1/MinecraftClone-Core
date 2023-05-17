#version 450


layout(location = 0) in vec2 vLineCenter;
layout(location = 1) in vec4 fragColor;
layout(location = 2) in float BlendFactor;
layout(location = 3) in float LineWidth;

layout(location = 4) out vec4 FragColor;

void main()
{
    vec4 col = fragColor;
    float d = length(vLineCenter-gl_FragCoord.xy);
    float w = LineWidth;
    if (d>w)
    {
        col.w = 0;   
    }
    else
    {
        col.w *= pow(float((w-d)/w), BlendFactor);   
    }
    FragColor = col;
}