#version 430 core
//!#define SOLUTION

uniform vec4 baseColor = vec4(1.0);
uniform vec3 cameraPos = vec3(1.0);


in Data
{
	//vec3 normal;
	vec3 position;
} i;

layout (location = 0) out vec4 outputColor;
layout (location = 1) out vec4 brightColor;

void main() 
{
	vec3 toLight = normalize(vec3(0, 1, 0));
	vec4 color = baseColor;
	//outputColor = metallic(toLight) + hemisphericalLight(toLight) * color;
	//outputColor = vec4(20);
	
	vec4 outputColor = color*20;

	float brightness = dot(outputColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0)
        brightColor = vec4(outputColor.rgb, 1.0);
    else
        brightColor = vec4(0.0, 0.0, 0.0, 1.0);
}