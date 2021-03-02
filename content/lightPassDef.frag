#version 430 core

out vec4 fragColor;

in Data
{
	vec2 texCoords;
} i;

uniform sampler2D positionMap;

uniform sampler2D positionRawMap;
uniform sampler2D albedoMap;
uniform sampler2D metalRoughness;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform samplerCube irradianceMap;

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);


uniform samplerCube envMap;

uniform vec3 cameraPosition;

uniform vec3  ao = vec3(1);
uniform vec3[4] lightPositions;
uniform vec3[4] lightColors;

//Directional Light
uniform vec3 dirLightColor = vec3(1,0,0)*10;
uniform vec3 dirLight = normalize(vec3(1,1,0));

uniform mat4 dirLightMatrix = mat4(1);
uniform sampler2D dirLightShadowMap;

//SHADOWMAP
float readShadowMap(vec3 eyeDir)
{

    //Can all be done once in the vertex shader? //Bzw twice
    mat4 cameraViewToWorldMatrix = inverse(camera);
    //world to lightProjectionMatrix // lightViewToProjectionMatrix * worldToLightViewMatrix
    mat4 cameraViewToProjectedLightSpace = dirLightMatrix * cameraViewToWorldMatrix;
    
    //mat4 cameraViewToProjectedLightSpace = dirLightMatrix;
    vec4 projectedEyeDir = cameraViewToProjectedLightSpace * vec4(eyeDir,1);
    projectedEyeDir = projectedEyeDir/projectedEyeDir.w;

    vec2 textureCoordinates = projectedEyeDir.xy * vec2(0.5,0.5) + vec2(0.5,0.5);

    const float bias = 0.0001;
    float depthValue = texture2D(dirLightShadowMap, textureCoordinates ).x - bias;
    //return projectedEyeDir.z * 0.5 + 0.5 < depthValue;
    return depthValue;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

    
    //Check out of range
    if(projCoords.z > 1.0){
        return 0.5;
    }
    /*
    if(projCoords.z < 0){
        return 0.2;
    }
    */
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(dirLightShadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    /*
    if(currentDepth  < 0){
        return 0.7;
    }

    if(closestDepth  < 0){
        return 0.9;
    }
    */

    // check whether current frag pos is in shadow
    //float bias = 0.005;
    float bias = max(0.01 * (1.0 - dot(normal, lightDir)), 0.005);  
    //float bias = 0.005;
    //float bias = 0.01;
    //bias = 0.01;
    bias = 0.005;
    bias = 0;
    //float shadow = currentDepth-bias > closestDepth  ? 1.0 : 0.0;

    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    //return currentDepth-closestDepth;
    return shadow;
    //return closestDepth;
} 

//PBR THINGS
#define PI 3.1415926535897932384626433832795
float GeometrySchlickGGX(float NdotV, float k)
{
    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return nom / denom;
}
  
float GeometrySmith(vec3 N, vec3 V, vec3 L, float k)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx1 = GeometrySchlickGGX(NdotV, k);
    float ggx2 = GeometrySchlickGGX(NdotL, k);
    return ggx1 * ggx2;
}
float DistributionGGX(vec3 N, vec3 H, float a)
{
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float nom    = a2;
    float denom  = (NdotH2 * (a2 - 1.0) + 1.0);
    denom        = PI * denom * denom;
	
    return nom / denom;
}
//With cosTheta being the dot product result between the surface's normal n and the halfway h (or view v) direction. 
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}   

vec3 CalculateLight(vec3 albedo, vec3 radiance, vec3 N, vec3 V, vec3 L, vec3 F0, float roughness, float metallic)
{
        vec3 H = normalize(V + L);
        // cook-torrance brdf
        float NDF = DistributionGGX(N, H, roughness);        
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
        
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
        vec3 specular     = numerator / max(denominator, 0.001);  
            
        // add to outgoing radiance Lo
        float NdotL = max(dot(N, L), 0.0);                
        vec3 Lo = (kD * albedo / PI + specular) * radiance * NdotL; 
        return Lo;
}
void main()
{		
    float depth = texture(depthMap,i.texCoords).r;
    //if(depth>=0.99999999f)discard;
    //srgb to linear space
    //vec3 albedo     = pow(texture(albedoMap, TexCoords).rgb, 2.2);
    vec4 CWorldPos = texture(positionMap,i.texCoords);
    vec3 WorldPos = CWorldPos.rgb;
    vec3 V = normalize(WorldPos - cameraPosition);

	//vec4 fragColor = texture(albedoMap,i.texCoords);
	vec3 N = normalize(texture(normalMap,i.texCoords).rgb);

	vec3 albedo = texture(albedoMap,i.texCoords).rgb;
	vec3 metalRoughness = texture(metalRoughness,i.texCoords).rgb;

    float metallic = metalRoughness.r;
    float roughness = metalRoughness.g;

    metallic = 0.01;
    

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);
	           
    // reflectance equation
    vec3 Lo = vec3(0.0);
    //Directional light
    vec4 s = dirLightMatrix*vec4(WorldPos,1);
    //vec3 dirLightCalc = dirLight-WorldPos;
    float shadow = ShadowCalculation(s, N, dirLight);
    //shadow = 0;

        vec3 coord = s.xyz / s.w;
	float shadowDepth = textureLod(dirLightShadowMap, coord.xy * 0.5 + 0.5, 0).r;

    shadow = 1;
    if(shadowDepth>coord.z){

        shadow = 0;
    }



    Lo+=CalculateLight(albedo, dirLightColor, N, V, dirLight, F0, roughness, metallic)*(1.0 - shadow);
    
    //fragColor = vec4(vec3(1-shadow),1);



    //return;

    //Point Lights
    for(int i = 0; i < 4; ++i) 
    {
        
        vec3 L = normalize(lightPositions[i] - WorldPos);

        float distance    = length(lightPositions[i] - WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance     = lightColors[i] * attenuation;  


        Lo+=CalculateLight(albedo, radiance, N, V, L, F0, roughness, metallic);
        
        /*
        // calculate per-light radiance
        vec3 L = normalize(lightPositions[i] - WorldPos);
        vec3 H = normalize(V + L);
        float distance    = length(lightPositions[i] - WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance     = lightColors[i] * attenuation;        
        
        // cook-torrance brdf
        float NDF = DistributionGGX(N, H, roughness);        
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
        
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
        vec3 specular     = numerator / max(denominator, 0.001);  
            
        // add to outgoing radiance Lo
        float NdotL = max(dot(N, L), 0.0);                
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
        */
    }   
    
    vec3 kS = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness); 
    vec3 kD = 1.0 - kS;
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse    = irradiance * albedo;
    vec3 ambient    = (kD * diffuse) * ao; 
    
    //Alter Code
    //vec3 ambient = vec3(0.03) * albedo * ao;


    vec3 color = ambient + Lo;
	
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));  
   
    fragColor = vec4(color, 1.0);
    //fragColor = vec4(vec3(1),1.0);
} 



//PBR THINGS END
void main2()
{    
	
	vec3 v = normalize(texture(positionMap,i.texCoords).rgb - cameraPosition);

	vec4 color = texture(albedoMap,i.texCoords);
	vec3 normal = normalize(texture(normalMap,i.texCoords).rgb);

	vec3 r = reflect(v, normal);
	
	 vec4 reflectColor = textureLod(envMap, r,0);
	 vec4 albedo = texture(albedoMap,i.texCoords);
	 float roughness = texture(metalRoughness,i.texCoords).g;

	//color=mix(albedo,reflectColor,roughness*0.1);
	fragColor = reflectColor;
	//color=vec4(roughness,roughness,roughness,1);
	//color = vec4(1,1,0,1);

	//color = vec4(texture(metalRoughness,i.texCoords).rgb,1);
	
}  