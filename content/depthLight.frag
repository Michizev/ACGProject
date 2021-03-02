#version 330 core

out vec4 color;
float bias = 0.001;

in Data
{
	vec4 position;
} i;

void main()
{             
    // gl_FragDepth = gl_FragCoord.z;
    color = vec4(bias + i.position.z / i.position.w);
}  