#version 330 core
in vec3 fUv;

uniform sampler2D uTexture0;

out vec4 FragColor;

void main()
{
    
    FragColor = texture(uTexture0, vec2(fUv.x, fUv.y));
    //FragColor = vec4(1,1,1,1);
  

    
}