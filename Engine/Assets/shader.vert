#version 330 core
layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vUv;


layout(binding = 1) uniform MatrixData
{
    mat4 uModel;
    mat4 uView;
    mat4 uProjection;
    vec4 CameraPos;
};


out vec3 fUv;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos.xyz - CameraPos.xyz, 1);
    fUv = vUv.xyz;
}