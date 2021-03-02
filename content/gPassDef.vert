#version 430 core
#pragma optimize (off)
//!#define SOLUTION

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);
uniform mat4 model = mat4(1.0);

in vec3 position;
in vec3 normal;
in vec2 texcoord_0;
in vec3 tangent;

uniform vec3 cameraPos;

out Data
{
	vec2 texCoords;
	vec3 normal;
	vec4 position;
	mat3 TBN;
	vec3 tangent;
	vec3 tangentPos;
    vec3 tangentCameraPos;
	vec3 positionRaw;
} o;

void main()
{
	vec3 worldNormal = normalize(mat3(world) * normal);
	o.normal = worldNormal;
	vec4 position_world = world * vec4(position,1);
	o.positionRaw = position;
	o.position = position_world;
	o.texCoords = texcoord_0;
	mat4 modelViewProjection = camera*world;
	gl_Position = camera * position_world;

	//Case forward renderer:
	
	
	//NormalCalc
	/*
	vec3 T = normalize(vec3(modelViewProjection * vec4(tangent,   0.0)));
	vec3 B = normalize(vec3(modelViewProjection * vec4(cross(normal,tangent), 0.0)));
	vec3 N = normalize(vec3(modelViewProjection * vec4(normal,    0.0)));
	*/
	//mat4 normalMatrix = transpose(inverse(world));#
	mat4 normalMatrix = transpose(inverse(world));

    vec3 worldTangent = normalize(mat3(world)*tangent);
	
	vec3 T = normalize(vec3(normalMatrix * vec4(worldTangent,   0.0)));
	vec3 B = normalize(vec3(normalMatrix * vec4(cross(worldNormal,worldTangent), 0.0)));
	vec3 N = normalize(vec3(normalMatrix * vec4(worldNormal,    0.0)));
	
	/*
	vec3 T = normalize(Tangent - dot(Tangent, Normal) * Normal);
    vec3 B = cross(Tangent, Normal);
    mat3 TBN = mat3(Tangent, Bitangent, Normal);
	*/

	mat3 TBN = mat3(T, B, N);

	o.TBN = TBN;
	o.tangent = worldTangent;
	/*
	vec3 TT = normalize(vec3(model * vec4(tangent,   0.0)));
	vec3 BT = normalize(vec3(model * vec4(cross(normal,tangent), 0.0)));
	vec3 NT = normalize(vec3(model * vec4(normal,    0.0)));
	*/
	vec3 TT   = normalize(mat3(world) * tangent);
    vec3 BT   = normalize(mat3(world) * cross(normal,tangent));
    vec3 NT   = normalize(mat3(world) * normal);
	mat3 TTBN = mat3(TT, BT, NT);

	o.tangentCameraPos = TTBN * cameraPos;
	o.tangentPos = TTBN * position_world.xyz;

}
