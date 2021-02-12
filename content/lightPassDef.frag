#version 430 core

out vec4 color;

in Data
{
	vec2 texCoords;
} i;

uniform sampler2D positionMap;
uniform sampler2D albedoMap;
uniform sampler2D metalRoughness;
uniform sampler2D normalMap;


uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);


uniform samplerCube envMap;

uniform vec3 cameraPosition;

void main()
{    
	
	vec3 v = normalize(texture(positionMap,i.texCoords).rgb - cameraPosition);

	vec4 fragColor = texture(albedoMap,i.texCoords);
	vec3 normal = normalize(texture(normalMap,i.texCoords).rgb);

	vec3 r = reflect(v, normal);
	
	 vec4 reflectColor = textureLod(envMap, r,0);
	 vec4 albedo = texture(albedoMap,i.texCoords);
	 float roughness = texture(metalRoughness,i.texCoords).g;

	//color=mix(albedo,reflectColor,roughness*0.1);
	color = reflectColor;
	//color=vec4(roughness,roughness,roughness,1);
	//color = vec4(1,1,0,1);

	//color = vec4(texture(metalRoughness,i.texCoords).rgb,1);
	
}  