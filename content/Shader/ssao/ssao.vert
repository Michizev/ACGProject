#version 430 core
#pragma optimize (off)
//!#define SOLUTION

in vec3 position;


out vec2 texCoords;


void main() 
{
					const vec2 vertices[4] = vec2[4](vec2(-1.0, -1.0),
						vec2( 1.0, -1.0),
						vec2( -1.0,  1.0),
						vec2( 1.0,  1.0));

					texCoords = vertices[gl_VertexID]*0.5+0.5;

					gl_Position = vec4(vertices[gl_VertexID], -0.5, 1.0);
}