#version 430 core
//!#define SOLUTION

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);

in vec4 position;
in vec3 normal;

out Data
{
	vec3 normal;
	vec3 position;
} o;

void main()
{
	o.normal = mat3(world) * normal;
	vec4 position_world = world * position;
	o.position = position_world.xyz;
	gl_Position = camera * position_world;

}
