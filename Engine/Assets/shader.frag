#version 330 core
in vec2 fUv;

uniform sampler2D uTexture0;

out vec4 FragColor;

void main()
{
    if(texture(uTexture0, fUv).a < .5f)
    {
        discard;
    }
    
    //FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
    FragColor = texture(uTexture0, fUv);
    
}