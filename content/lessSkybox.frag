#version 330 core
in vec3 dir;
//When using the gbuffer
//layout (location = 2) out vec4 color;
//Normally used
layout (location = 0) out vec4 color;
layout (location = 1) out vec4 brightColor;

uniform samplerCube uTexEnv;

void main()
{
        //color=texture(uTexEnv, dir);
        color=textureLod(uTexEnv, dir,1.2);
        brightColor=vec4(0);
}