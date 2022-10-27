﻿#version 330 core
in Data
{
	vec4 position;
} i;

uniform vec3 lightPos;
uniform float farPlane;

out float fragColor;

void main()
{
    // get distance between fragment and light source
    float lightDistance = length(i.position.xyz - lightPos);
    
    // map to [0;1] range by dividing by far_plane
    lightDistance = lightDistance / farPlane;
    
    // write this as modified depth
    //gl_FragDepth = lightDistance;
    fragColor = gl_FragCoord.z;
} 