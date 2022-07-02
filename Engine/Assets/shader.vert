#version 450
// uniform buffer containing the projection and view matrices
layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
    mat4 View;
};



// REQUIRED: Instance buffer containing world matrix. It is a 3D pass engine requirment to be stored this way (You can thank vulkan!)
layout(location = 0) in mat4 InstanceTransform;

layout(location = 4) in vec3 Position;
layout(location = 5) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    vec4 worldPosition = InstanceTransform * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = TexCoords;
}