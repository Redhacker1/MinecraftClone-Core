"%VULKAN_SDK%/Bin/glslc.exe" "unlit_texture_shader.frag" -o "unlit_frag.spv"
"%VULKAN_SDK%/Bin/glslc.exe" "shader.vert" -o "vert.spv"
"%VULKAN_SDK%/Bin/glslc.exe" "lighting.frag" -o "light_frag.spv"
"%VULKAN_SDK%/Bin/glslc.exe" "Screenspaceshader.vert" -o "vert2D.spv"