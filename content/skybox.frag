#version 430 core

in Data
{
	vec3 uv;
} i;

uniform samplerCube skybox;

out vec4 outputColor;

void main() 
{
	outputColor = texture(skybox,i.uv);
	
	//outputColor = vec4(i.uv,1);
	//outputColor = vec4(1);
}