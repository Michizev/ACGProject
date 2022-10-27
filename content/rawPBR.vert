#version 430 core


uniform mat4 view = mat4(1.0);
uniform mat4 projection = mat4(1.0);
uniform mat4 model = mat4(1.0);
uniform mat4 viewProjection = mat4(1.0);

in vec3 position;
in vec3 normal;
in vec2 texcoord_0;
in vec3 tangent;

uniform vec3 cameraPos;

out Data
{
	vec2 texCoords;
	vec3 normal;
	vec3 position;
	mat3 TBN;
	vec3 tangent;
	vec3 tangentPos;
    vec3 tangentCameraPos;
	vec3 positionRaw;
} o;

void main()
{
	mat4 modelView = view*model;
	vec3 worldNormal = normalize(mat3(modelView) * normal);
	
	vec4 position_world = model * vec4(position,1);
	o.positionRaw = position;
	o.position = position_world.xyz;
	o.texCoords = texcoord_0;

	mat4 modelViewProjection = viewProjection*model;
	gl_Position = viewProjection * position_world;

	vec3 T   = normalize(vec3(model * vec4(tangent,0)));

    //vec3 NT   = normalize(mat3(model) * normal);
	vec3 N   = normalize(vec3(model * vec4(normal,0)));
	// re-orthogonalize T with respect to N
	T = normalize(T - dot(T, N) * N);
	// then retrieve perpendicular vector B with the cross product of T and N
	vec3 B = cross(T, N);

	mat3 TBN = mat3(T, B, N);

	o.tangentCameraPos = TBN * cameraPos;
	o.tangentPos = TBN * position_world.xyz;

	o.normal = normal;
	o.tangent = tangent;
	o.TBN = TBN;
}

