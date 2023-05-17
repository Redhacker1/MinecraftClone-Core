#version 450

// Per Vert data
layout(location = 0) in vec3 Vertex;

// Model Matrix
// REQUIRED: Instance buffer containing world matrix. It is a 3D pass engine requirment to be stored this way (You can thank vulkan!)
layout(location = 0) in vec4 Matrix1xx;
layout(location = 1) in vec4 Matrix2xx;
layout(location = 2) in vec4 Matrix3xx;
layout(location = 3) in vec4 Matrix4xx;

// Per line data
layout(location = 1) in vec4 uColor;
layout(location = 2) in float uBlendFactor;
layout(location = 3) in float uLineWidth; //1.5..2.5

layout(location = 0) out vec2 vLineCenter;

layout(location = 1) out vec4 fragColor;
layout(location = 2) out float BlendFactor;
layout(location = 3) out float LineWidth;



layout(set = 0, binding = 0) uniform ViewInfo
{
    mat4 Projection;
    mat4 View;
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
    fragColor = uColor;
    BlendFactor = uBlendFactor;
    LineWidth = uLineWidth;
    
    
}