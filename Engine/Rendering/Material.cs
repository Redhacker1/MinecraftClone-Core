using System;

namespace Engine.Rendering
{
    public struct Material
    {
        Shader currentShader;
        Texture Albedo;

        Material(Shader shader, Texture albedo)
        {
            currentShader = shader;
            Albedo = albedo;
        }

        internal void UseMaterial()
        {
            currentShader.Use();
            Albedo.Bind();
        }
    }
}