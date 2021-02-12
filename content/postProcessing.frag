#version 430 core
//!#define SOLUTION

uniform sampler2D texImage;
uniform sampler2D lastTexImage;
uniform sampler2D depthTex;
uniform float time;
uniform int effect;

uniform samplerCube cube;

uniform mat4 camera = mat4(1.0);
uniform mat4 otherCamera = mat4(1.0);

in vec2 uv;


float grayScale(vec3 color)
{
	return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 color2;

void main() 
{
	vec3 inputColor = texture(texImage, uv).rgb;
	if(effect==1)
	{
		vec3 lastColor = texture(lastTexImage, uv).rgb;
		color = vec4(inputColor*0.8+lastColor*0.2,1);

		 // Get the depth buffer value at this pixel. 
		 //float depth = texture(depthTex,uv).r; 
		 // H is the viewport position at this pixel in the range -1 to 1. 
		 //vec4 H = vec4(uv.x * 2 - 1, (1 - uv.y) * 2 - 1, zOverW, 1); 
		 // Transform by the view-projection inverse.    
		 //vec4 D = mul(H, g_ViewProjectionInverseMatrix); 
		 // Divide by w to get the world position.    
		 //vec4 worldPos = D / D.w;

		 //color = vec4(vec3(pow(depth.r, 32.0)), 1.0);

		 /*
		 vec4 pos = inverse(camera)*inverse(uv);

		 vec4 rev = vec4(pos*otherCamera);

		 vec3 col = texture(lastTexImage, vec2(rev.r,rev.g)).rgb;
		 color2 = vec4(col,1);
		 
		 color2=vec4(vec2(rev.r,rev.g),1,1);
		 */
		 color2 = texture(cube,vec3(uv,1));
	}
	else{
		color = vec4(inputColor,1);
	}

	
	//color=vec4(inputColor,1);
}