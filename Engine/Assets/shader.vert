#version 450
// uniform buffer containing the projection and view matrices
layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
    mat4 View;
};



// REQUIRED: Instance buffer containing world matrix. It is a 3D pass engine requirment to be stored this way (You can thank vulkan!)
layout(location = 0) in vec4 Matrix1xx;
layout(location = 1) in vec4 Matrix2xx;
layout(location = 2) in vec4 Matrix3xx;
layout(location = 3) in vec4 Matrix4xx;

layout(location = 4) in vec3 Position;
layout(location = 5) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    mat4 World = mat4(Matrix1xx, Matrix2xx, Matrix3xx, Matrix4xx);
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = TexCoords;
}