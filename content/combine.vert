#version 430 core
#pragma optimize (off)
//!#define SOLUTION

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);

out Data
{
	vec2 texCoords;
} o;


void main() 
{
					const vec2 vertices[4] = vec2[4](vec2(-1.0, -1.0),
						vec2( 1.0, -1.0),
						vec2( -1.0,  1.0),
						vec2( 1.0,  1.0));

					o.texCoords = vertices[gl_VertexID]*0.5+0.5;

					gl_Position = vec4(vertices[gl_VertexID], -0.5, 1.0);
}
