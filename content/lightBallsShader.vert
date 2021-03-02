#version 430 core
//!#define SOLUTION

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);




in vec3 position;
in vec3 normal;
in vec3 texCoords;

//in vec3 normal;
in vec3 instancePosition;
out Data
{
	//vec3 normal;
	vec3 position;
} o;

void main()
{
	vec3 pos = instancePosition + position;
	//o.normal = mat3(world) * normal;
	vec4 position_world = world * vec4(pos,1);
	o.position = position_world.xyz;
	gl_Position = camera * position_world;

}
