#version 430 core
layout (location = 0) out float fragColor;
layout (location = 1) out vec4 debugColor;
//out vec4 fragColor; 

in vec2 texCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;
uniform sampler2D depth;

uniform int kernelSize = 64;
uniform float radius = 0.5;
uniform vec3 samples[64];
uniform mat4 projection;
uniform mat4 projMatrixInv;
uniform mat4 viewMatrixInv;
uniform mat4 view;

uniform float bias = 0.025;

uniform vec2 zValues;

float LinearizeDepth21(float depth) 
{
	float near = zValues.x;
	float far = zValues.y;
    return (2.0 * near * far) / (far + near - depth * (far - near));    
}

float linearize_depth(float d,float zNear,float zFar)
{
    return zNear * zFar / (zFar + d * (zNear - zFar));
}

float GetLinearDepth(float depth, float zNear, float zFar)
{
    float z_b = depth;
    float z_n = 2.0 * z_b - 1.0;
    float z_e = 2.0 * zNear * zFar / (zFar + zNear - z_n * (zFar - zNear));
    return z_e;
}

float makeDepthLinear(float d)
{
	//return linearize_depth(d,zValues.x,zValues.y);
	return GetLinearDepth(d,zValues.x,zValues.y);
}
//TRY
//https://gamedev.stackexchange.com/questions/108856/fast-position-reconstruction-from-depth
vec3 calculate_view_position(vec2 texture_coordinate, float depth_from_depth_buffer)
{
    vec3 clip_space_position = vec3(texture_coordinate, depth_from_depth_buffer) * 2.0 - vec3(1.0);

	vec2 part1 = vec2(projMatrixInv[0][0], projMatrixInv[1][1]) * clip_space_position.xy;
	float part2 = projMatrixInv[2][3] * clip_space_position.z + projMatrixInv[3][3];
    vec4 view_position = vec4(part1, vec2(part2));

    return(view_position.xyz / view_position.w);
}



// tile noise texture over screen, based on screen dimensions divided by noise size
uniform vec2 noiseScale = vec2(800.0/4.0, 600.0/4.0); // screen = 800x600

vec3 WorldPosFromDepth(float depth) {
    float z = depth * 2.0 - 1.0;

    vec4 clipSpacePosition = vec4(texCoords * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = projMatrixInv * clipSpacePosition;

    // Perspective division
    viewSpacePosition /= viewSpacePosition.w;

    vec4 worldSpacePosition = viewMatrixInv * viewSpacePosition;

    return worldSpacePosition.xyz;
}

vec3 reconstructPositionWithMat(vec2 texCoord)
{
    float depth = texture2D(depth, texCoord).x;
    depth = (depth * 2.0) - 1.0;
    vec2 ndc = (texCoord * 2.0) - 1.0;
    vec4 pos = vec4(ndc, depth, 1.0);
    pos = projMatrixInv * pos;
    return vec3(pos.xyz / pos.w);
}


vec3 ViewPosFromDepth(float depth) {
    float z = depth * 2.0 - 1.0;

    vec4 clipSpacePosition = vec4(texCoords * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = projMatrixInv * clipSpacePosition;

    // Perspective division
    viewSpacePosition /= viewSpacePosition.w;
	
	return viewSpacePosition.xyz;
}

vec3 VSPositionFromDepth(vec2 vTexCoord)
{
    // Get the depth value for this pixel
    float z = texture(depth, vTexCoord).r;
	z = z * 2.0 - 1.0;

    // Get x/w and y/w from the viewport position
    //float x = vTexCoord.x * 2 - 1;
    //float y = (1 - vTexCoord.y) * 2 - 1;

    vec4 vProjectedPos = vec4(texCoords*2.0-1.0, z, 1.0f);
    // Transform by the inverse projection matrix
    //vec4 vPositionVS = viewMatrixInv*vProjectedPos;  
	vec4 vPositionVS = inverse(projection)*vProjectedPos;  
	

    // Divide by w to get the view-space position
    return vPositionVS.xyz / vPositionVS.w;  
}

// this is supposed to get the world position from the depth buffer
vec3 WorldPosFromDepth200(float depth) {
    float z = depth * 2.0 - 1.0;

    vec4 clipSpacePosition = vec4(texCoords * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = projMatrixInv * clipSpacePosition;

    // Perspective division
    viewSpacePosition /= viewSpacePosition.w;

	return viewSpacePosition.xyz;
}

void main3()
{
	float depthVal = texture(depth,texCoords).r;
	vec3 fragPos = ViewPosFromDepth(depthVal);
	vec3 origin = fragPos;
	vec3 normal    = normalize(texture(gNormal, texCoords)).rgb;
	vec3 randomVec = texture(texNoise, texCoords * noiseScale).xyz;  

	vec3 rvec = texture(texNoise, texCoords * noiseScale).xyz;

    vec3 tangent = normalize(rvec - normal * dot(rvec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 tbn = mat3(tangent, bitangent, normal);

		float dh = texture(depth, texCoords).r;

	   float dd = makeDepthLinear(dh);
  

	float occlusion = 0.0;
	for (int i = 0; i < kernelSize; ++i) {
	// get sample position:
	   vec3 sampleV = tbn * samples[i];
	   sampleV = sampleV * radius + origin;
  
	// project sample position:
	   vec4 offset = vec4(sampleV, 1.0);
	   offset = projection * offset;
	   offset.xy /= offset.w;
	   offset.xy = offset.xy * 0.5 + 0.5;
  
	// get sample depth:
		float depth = texture(depth, offset.xy).r;

	   float sampleDepth = makeDepthLinear(depth);
  
	// range check & accumulate:
	   float rangeCheck= abs(origin.z - sampleDepth) < radius ? 1.0 : 0.0;
	   occlusion += (sampleDepth <= sampleV.z ? 1.0 : 0.0) * rangeCheck;

	   fragColor = dh;
	   debugColor = vec4(vec3(offset.xy, 0),1);
	   return;
	}
	//occlusion = 1.0 - (occlusion / kernelSize);
	fragColor = occlusion;  
	debugColor = vec4(vec3(dd), 1);
}
void main()
{
	vec3 fragPos   = texture(gPosition, texCoords).xyz;
	fragPos.z = LinearizeDepth21(fragPos.z);
	//fragPos =  abs((reconstructPositionWithMat(texCoords)));
	float depthVal = texture(depth, texCoords).r;
	fragPos = WorldPosFromDepth200(depthVal);
	vec3 normal    = normalize(texture(gNormal, texCoords)).rgb;

	normal = (view*vec4(normal,0.0)).rgb;//W set to zero

	vec3 randomVec = texture(texNoise, texCoords * noiseScale).xyz;  

	vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 TBN       = mat3(tangent, bitangent, normal);  

	float occlusion = 0.0;
	for(int i = 0; i < kernelSize; ++i)
	{
		// get sample position
		vec3 samplePos = TBN * samples[i]; // from tangent to view-space
		samplePos = fragPos + samplePos * radius; 

		vec4 offset = vec4(samplePos, 1.0);
		offset      = projection * offset;    // from view to clip-space
		offset.xyz /= offset.w;               // perspective divide
		offset.xyz  = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0  

		float sampleDepth = texture(gPosition, offset.xy).z; 
		sampleDepth = LinearizeDepth21(sampleDepth);

		depthVal = texture(depth, texCoords).r;
		sampleDepth = WorldPosFromDepth200(depthVal).z;

		//sampleDepth = -(reconstructPositionWithMat(offset.xy)).z;
		occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0); 

		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
		occlusion       += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck; 
	}  
	occlusion = 1.0 - (occlusion / kernelSize);
	fragColor = max(0,occlusion);
	fragColor = occlusion;

	debugColor = vec4(normal,1);
}
void main111()
{
	float depthVal = texture(depth,texCoords).r;
	vec3 fragPos = ViewPosFromDepth(depthVal);
	vec3 normal    = texture(gNormal, texCoords).rgb;
	vec3 randomVec = texture(texNoise, texCoords * noiseScale).xyz;  

	vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 TBN       = mat3(tangent, bitangent, normal);  

	float occlusion = 0.0;
	for(int i = 0; i < kernelSize; ++i)
	{
		// get sample position
		vec3 samplePos = TBN * samples[i]; // from tangent to view-space
		samplePos = fragPos + samplePos * radius; 
    
		vec4 offset = vec4(samplePos, 1.0);
		offset      = projection * offset;    // from view to clip-space
		offset.xyz /= offset.w;               // perspective divide
		offset.xyz  = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0  

		float depthValS = texture(depth,offset.xy).r;
		vec3 fragPosS = ViewPosFromDepth(depthValS);

		float sampleDepth = fragPosS.z; 

		//occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0);  
		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
		occlusion       += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck; 
	}  
	occlusion = 1.0 - (occlusion / kernelSize);
	fragColor = occlusion;  
}
void main5()
{
/*
	vec3 fragPos = (uViewMatrix*vec4(depthToWorld(sDepth,vTexcoord,uInverseViewProjectionBiased),1.0f)).xyz;
	fragPos.z = -fragPos.z;//multiplying with viewmatrix results in z coordinate being negative. multiply with -1 to make it positive
*/
	//vec3 fragPos   = texture(gPosition, texCoords).xyz;
	float depthVal = texture(depth,texCoords).r;
	vec3 fragPos = ViewPosFromDepth(depthVal);
	vec3 normal    = texture(gNormal, texCoords).rgb;
	vec3 randomVec = texture(texNoise, texCoords * noiseScale).xyz;  
	//TODO turn into viewspace from worldspace

	//fragPos = VSPositionFromDepth(texCoords);
	//fragPos = calculate_view_position(texCoords,depthVal);
	mat4 viewNoT = mat4(mat3(view));
	viewNoT = mat4(transpose(view));
	/*
	https://gamedev.stackexchange.com/questions/141214/convert-view-space-normal-to-world-normal
	In this case we're only using the upper-left 3x3 block of the view matrix, which should generally be a pure rotation matrix. A nice property of rotation matrices is that transposing them produces their inverse cheaply. This undoes the camera's orientation, rotating the normal back into worldspace.

	It does not correctly invert the translation component, but since we're zeroing-out and discarding the w, this won't impact the normal directions.
	float4x4 viewTranspose = transpose(UNITY_MATRIX_V);
	float3 worldNormal = mul(viewTranspose, float4(viewNormal.xyz, 0)).xyz;

	*/

	//fragPos = (viewNoT*vec4(fragPos,1)).rgb;

	//normal = ((view*vec4(normalize(normal),0.0)).xyz);//W set to zero


	//normal = ((viewMatrixInv*vec4(normal,0.0)).xyz);//W set to zero

	/*

	// fragment normal
vec3 fragment_normal = texture(normal, fs_in.texture_coords).xyz;

// discard fragment if normal is empty
if (fragment_normal == vec3(0.0f)) discard;

// fragment position in view space
vec3 fragment_position = vec3(camera.view * vec4(texture(position, fs_in.texture_coords).xyz, 1.0f));

// fragment normal in view space
fragment_normal = normalize(mat3(camera.view) * fragment_normal);



	*/

	if (normal == vec3(0.0f)) discard;


	//normal = normalize(mat3(view) * normal);

	vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 TBN       = mat3(tangent, bitangent, normal);  


	float occlusion = 0.0;
	for(int i = 0; i < kernelSize; ++i)
	{
		// get sample position
		vec3 samplePos = TBN * samples[i]; // from tangent to view-space
		samplePos = fragPos + samplePos * radius; 
    
		vec4 offset = vec4(samplePos, 1.0);
		offset      = projection * offset;    // from view to clip-space
		offset.xyz /= offset.w;               // perspective divide
		offset.xyz  = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0  

		float sampleDepth = texture(gPosition, offset.xy).z; 
		//float sampleDepth = makeDepthLinear(texture(depth, offset.xy).z);
		
		//sampleDepth = texture(depth,offset.xy).r;
		//sampleDepth =  makeDepthLinear(depthVal);
		//sampleDepth = ViewPosFromDepth(depthVal).z;

		//sampleDepth = makeDepthLinear(sampleDepth);


		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
		//occlusion       += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;   
		occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0);  
	}  

	occlusion = 1.0 - (occlusion / kernelSize);
	fragColor = occlusion;  

	debugColor = vec4(normal, 1);
	//fragColor = vec4(vec3(occlusion),0);
}