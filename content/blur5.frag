#version 430 core


out vec4 fragColor;
in vec2 texCoords;


uniform sampler2D image;
  
uniform bool horizontal = false;
uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main()
{             
    vec2 tex_offset = 1.0 / textureSize(image, 0); // gets size of single texel
    vec3 result = texture(image, texCoords).rgb * weight[0]; // current fragment's contribution
    vec2 offset = vec2(0);
    if(horizontal)
    {
        offset = vec2(tex_offset.x, 0.0);
    }else{
        offset = vec2(0.0, tex_offset.y);
    }

    for(int i = 1; i < 5; ++i)
    {
        result += texture(image, texCoords + offset*i).rgb * weight[i];
        result += texture(image, texCoords - offset*i).rgb * weight[i];
    }

    fragColor = vec4(abs(result), 1.0);
}