#version 430 core
//!#define SOLUTION

uniform vec4 baseColor = vec4(1.0);
uniform vec3 cameraPos = vec3(1.0);


in Data
{
	//vec3 normal;
	vec3 position;
} i;
/*
float specular(vec3 toLight, vec3 v, float shininess)
{
	if(0.0 > dot(i.normal, toLight)) return 0;
	vec3 r = reflect(-toLight, i.normal);
	float cosRV = dot(r, v);
	if(0 > cosRV) return 0;
	return pow(cosRV, shininess);
}

vec4 hemisphericalLight(vec3 toLight)
{
	const vec3 skyColor = vec3(0.229, 0.875, 1.0);
	const vec3 groundColor = vec3(0.72, 1.0, 0.44);
	float w = 0.5 + 0.5 * dot(i.normal, toLight);
	return vec4(mix(groundColor, skyColor, w), 1.0);
}

vec4 metallic(vec3 light)
{
	vec3 v = normalize(cameraPos - i.position);
	float spec = specular(light, v, 8.0);
	return vec4(spec * vec3(1), 1.0);
}
*/
layout (location = 0) out vec4 outputColor;
layout (location = 1) out vec4 brightColor;

void main() 
{
	vec3 toLight = normalize(vec3(0, 1, 0));
	vec4 color = baseColor;
	//outputColor = metallic(toLight) + hemisphericalLight(toLight) * color;
	outputColor = vec4(20);

	float brightness = dot(outputColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0)
        brightColor = vec4(outputColor.rgb, 1.0);
    else
        brightColor = vec4(0.0, 0.0, 0.0, 1.0);
}