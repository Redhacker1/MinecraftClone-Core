#version 450

layout(location = 0) in vec3 Vertex;
layout(location = 0) out vec2 vLineCenter;


layout(set = 0, binding = 0) uniform ViewInfo
{
    mat4 Projection;
    mat4 View;
};


layout(set = 0, binding = 1) uniform ViewPortInfo
{
    mat4 Model;
};

layout(set = 0, binding = 2) uniform ViewPortInfo
{
    //Width and Height of the viewport
    vec2 ScreenResolution;
};



void main()
{
    vec4 pp = Projection * View * Model * Vertex;
    gl_Position = pp;
    vec2 vp = ScreenResolution;
    vLineCenter = 0.5*(pp.xy + vec2(1, 1))*vp;
}