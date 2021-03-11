#version 430 core

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec4 brightColor;

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
uniform vec3 dirLightColor = vec3(1,0,0);
uniform vec3 dirLight = normalize(vec3(1,1,0));

uniform mat4 dirLightMatrix = mat4(1);
uniform sampler2D dirLightShadowMap;


void GetBrightColor()
{
    float brightness = dot(fragColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0)
        brightColor = vec4(fragColor.rgb, 1.0);
    else
        brightColor = vec4(0.0, 0.0, 0.0, 1.0);
}
//SHADOWMAP
float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    //Check out of range
    if(projCoords.z > 1.0){
        return 0.5;
    }
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(dirLightShadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;

    // check whether current frag pos is in shadow
    float bias = max(0.01 * (1.0 - dot(normal, lightDir)), 0.005);  

    bias = 0.005;
    bias = 0;

    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;
    return shadow;
} 



//PBR THINGS
#define PI 3.1415926535897932384626433832795
float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}   

vec3 CalcLight(vec3 N, vec3 L, vec3 V, float roughness, float metallic, vec3 F0, vec3 albedo, vec3 radiance)
{
        vec3 H = normalize(V + L);
        // cook-torrance brdf
        float NDF = DistributionGGX(N, H, roughness);        
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(max(dot(N, V), 0.0), F0);       
        
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
        
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(H, V), 0.0) * max(dot(N, L), 0.0);
        vec3 specular     = numerator / max(denominator, 0.001);  
            
        // add to outgoing radiance Lo
        float NdotL = max(dot(N, L), 0.0);                
        //return F;
        return (kD * albedo / PI + specular) * radiance * NdotL; 

}
void main()
{		
    vec4 CWorldPos = texture(positionMap,i.texCoords);
    vec3 WorldPos = CWorldPos.rgb;
    vec3 V = normalize(cameraPosition-WorldPos);

	//vec4 fragColor = texture(albedoMap,i.texCoords);
	vec3 N = normalize(texture(normalMap,i.texCoords).rgb);

	vec3 albedo = texture(albedoMap,i.texCoords).rgb;
	vec3 metalRoughness = texture(metalRoughness,i.texCoords).rgb;

    float metallic = metalRoughness.b;
    float roughness = metalRoughness.g;
    //metallic = min(metallic,0.99);
    roughness = max(roughness, 0.05);


    //roughness = 0.05;
    //metallic = 0.90;
   

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);
	
    

    // reflectance equation
    vec3 Lo = vec3(0.0);
    

    //irradianceMap
    
    vec3 kS = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness); 
    vec3 kD = 1.0 - kS;
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse    = irradiance * albedo;
    vec3 ambient    = (kD * diffuse) * ao; 



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
    
    //vec3 CalcLight(vec3 N, vec3 L, vec3 V, float roughness, float metallic, vec3 F0, vec3 albedo, vec3 radiance)

    Lo+=CalcLight(N,dirLight,V,roughness,metallic,F0,albedo,dirLightColor)*(1.0 - shadow);
    

    for(int i = 0; i < 4; ++i) 
    {
        // calculate per-light radiance
        vec3 L = normalize(lightPositions[i] - WorldPos);
        vec3 H = normalize(V + L);
        float distance    = length(lightPositions[i] - WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance     = lightColors[i] * attenuation ;        
        
        // cook-torrance brdf
        float NDF = DistributionGGX(N, H, roughness);        
        float G   = GeometrySmith(N, V, L, roughness);      
        //vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);    
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);  
        //vec3 F    = fresnelSchlick(max(dot(N, V)*-1,0), F0);       

        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
        
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
        vec3 specular     = numerator / max(denominator, 0.001);  
            
        // add to outgoing radiance Lo
        float NdotL = max(dot(N, L), 0.0);                
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
        //fragColor = vec4(F,1.0);
    }   
  
    vec3 color = ambient + Lo;
   

    fragColor = vec4(color, 1.0);
    

    float brightness = dot(fragColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0)
        brightColor = vec4(fragColor.rgb, 1.0);
    else
        brightColor = vec4(0.0, 0.0, 0.0, 1.0);
}  

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}  


float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}