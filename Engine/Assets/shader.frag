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
    
    FragColor = texture(uTexture0, fUv);
  

    
}