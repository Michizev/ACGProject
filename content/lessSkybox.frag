#version 330 core
in vec3 dir;
//When using the gbuffer
//layout (location = 2) out vec4 color;
//Normally used
out vec4 color;
uniform samplerCube uTexEnv;

void main()
{
        color=texture(uTexEnv, dir);
}