#version 430 core
//!#define SOLUTION

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);

in vec3 position;
in vec3 normal;

out Data
{
    vec3 uv;
} o;

void main()
{
    o.uv = position.xyz;
    vec4 pos = world*camera * vec4(position.xyz, 1);
    gl_Position = pos.xyww;
    //vec4 position_world = world * vec4(position,1.0f);
	//gl_Position = camera * vec4(position,1.0f);
} 
