#version 330 core
in vec3 position;

uniform mat4 lightSpaceMatrix;
uniform mat4 model;

out Data
{
	vec4 position;
} o;

void main()
{
	vec4 pos = lightSpaceMatrix * model * vec4(position, 1.0);
    gl_Position = pos;
	o.position = pos;
} 