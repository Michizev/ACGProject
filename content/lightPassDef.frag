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
uniform sampler2D emissiveMap;
uniform sampler2D vNormalMap;



uniform samplerCube irradianceMap;

uniform mat4 camera = mat4(1.0);
uniform mat4 world = mat4(1.0);


uniform samplerCube envMap;

uniform vec3 cameraPosition;

uniform vec3[4] lightPositions;
uniform vec3[4] lightColors;

//Directional Light
uniform vec3 dirLightColor = vec3(1,1,1);
uniform vec3 dirLight = normalize(vec3(1,1,0));

uniform mat4 dirLightMatrix = mat4(1);
uniform sampler2D dirLightShadowMap;
uniform float farPlaneDirLight;

uniform samplerCube prefilterMap;
uniform sampler2D   brdfLUT;  

uniform bool useSsao = false;
uniform sampler2D ssaoMap;

uniform mat4 projMatrixInv;
uniform mat4 viewMatrixInv;

uniform samplerCube pointLightShadow;
uniform float pointLightFar;

void GetBrightColor()
{
    float brightness = dot(fragColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0)
        brightColor = vec4(fragColor.rgb, 1.0);
    else
        brightColor = vec4(0.0, 0.0, 0.0, 1.0);
}
//SHADOWMAP
vec4 WorldPosFromDepth(float depth,vec2 texCoord) {
    float z = depth * 2.0 - 1.0;

    vec4 clipSpacePosition = vec4(texCoord * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = projMatrixInv * clipSpacePosition;

    // Perspective division
    viewSpacePosition /= viewSpacePosition.w;

    vec4 worldSpacePosition = viewMatrixInv * viewSpacePosition;

    return worldSpacePosition;
}

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

float ShadowCubeCalculation(vec3 fragPos, vec3 lightPos, sampler3D depthMap, float far_plane)
{
    // get vector between fragment position and light position
    vec3 fragToLight = fragPos - lightPos;
    // use the light to fragment vector to sample from the depth map    
    float closestDepth = texture(depthMap, fragToLight).r;
    // it is currently in linear range between [0,1]. Re-transform back to original value
    closestDepth *= far_plane;
    // now get current linear depth as the length between the fragment and light position
    float currentDepth = length(fragToLight);
    // now test for shadows
    float bias = 0.05; 
    float shadow = currentDepth -  bias > closestDepth ? 1.0 : 0.0;

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

//current shadow
float CalculateShadow(vec3 WorldPos, vec3 normal, vec3 lightDir)
{
    //Directional light
    vec4 s = dirLightMatrix*vec4(WorldPos,1);
    //vec3 dirLightCalc = dirLight-WorldPos;
    //float shadow = ShadowCalculation(s, N, lightDir);
    //shadow = 0;

    vec3 coord = s.xyz / s.w;
	float shadowDepth = textureLod(dirLightShadowMap, coord.xy * 0.5 + 0.5, 0).r;


    float shadow = 1;
    float bias = max(0.05 * (1.0 - dot(normal,dirLight)), 0.005);  
    
    //#define NOPCF
    //NO PCF
    #ifdef NOPCF
    if(shadowDepth >coord.z- bias ){

        shadow = 0;
    }
    return shadow;
    #else
    //SIMPLE PCF
    shadow = 0;
    vec2 texelSize = 1.0 / textureSize(dirLightShadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            vec2 offset = vec2(x, y) * texelSize;
            float pcf = textureLod(dirLightShadowMap, (coord.xy * 0.5 + 0.5) + offset,0).r; 
            if(pcf >coord.z- bias ){
                    shadow += 0;
            }else{
                shadow += 1;
            }
        }    
    }
    shadow  /= 9.0;

    #endif
    return shadow;
   
}

float SimpleShadow(vec4 fragPosLightSpace, sampler2D depthMap, float far_plane)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(depthMap, projCoords.xy).r; 
    // it is currently in linear range between [0,1]. Re-transform back to original value
    closestDepth *= far_plane;

    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // check whether current frag pos is in shadow
    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    return shadow;
}  

float ShadowCalculationCube(vec3 fragPos, vec3 lightPos, samplerCube depthMap, float far_plane, vec3 normal)
{
    // get vector between fragment position and light position
    vec3 fragToLight = fragPos - lightPos;
    // use the light to fragment vector to sample from the depth map    
    float closestDepth = texture(depthMap, fragToLight).r;

    //if(closestDepth <= 0)closestDepth = 1;

    // it is currently in linear range between [0,1]. Re-transform back to original value
    closestDepth *= far_plane;
    // now get current linear depth as the length between the fragment and light position
    float currentDepth = length(fragToLight);
    // now test for shadows
    float bias = 0.05; 
    //bias = max(0.01 * (1.0 - dot(normal,fragToLight)), 0.005);  
    float shadow = currentDepth -  bias > closestDepth ? 1.0 : 0.0;

    //float e = currentDepth + bias > closestDepth ? 1.0 : 0.0;
    fragColor = vec4(vec3(currentDepth-closestDepth),1);
    //fragColor = vec4(vec3(currentDepth-closestDepth+bias), 1.0);  
    //fragColor = vec4(vec3(closestDepth/far_plane), 1.0);  
    //fragColor = vec4(vec3(currentDepth/far_plane), 1.0);
    //fragColor = vec4(vec3(1-shadow),1);
    return shadow;
}
float EasyShadowCalculation(vec4 fragPosLightSpace, sampler2D shadowMap)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    
    // check whether current frag pos is in shadow
    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    return shadow;
}
void main()
{		
    vec4 CWorldPos = texture(positionMap,i.texCoords);
    vec3 WorldPos = CWorldPos.rgb;
    vec3 V = normalize(cameraPosition-WorldPos);

	//vec4 fragColor = texture(albedoMap,i.texCoords);
	vec3 N = normalize(texture(normalMap,i.texCoords).rgb);
    vec3 R = reflect(-V, N);   

	vec3 albedo = texture(albedoMap,i.texCoords).rgb;
	vec3 metalRoughness = texture(metalRoughness,i.texCoords).rgb;

    float ssao = texture(ssaoMap, i.texCoords).r;


    if(useSsao == false)ssao = 1;
    float ao = metalRoughness.r;

    ao *= ssao;

    float metallic = metalRoughness.b;
    float roughness = metalRoughness.g;
    
    //metallic = 1- metallic;
    //metallic = min(metallic,0.99);
    //roughness = max(roughness, 0.05);
    
    //fragColor = vec4(vec3(albedo),1);
    //return;


    //roughness = 0.05;
    //metallic = 0.90;
   

   
    vec3 F0 = vec3(0.04); 
    //albedo = vec3(1);
    F0 = mix(F0, albedo, metallic);
	
    



    //irradianceMap
    /*
    vec3 kS = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness); 
    vec3 kD = 1.0 - kS;
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse    = irradiance * albedo;
    vec3 ambient    = (kD * diffuse) * ao; 
    */
    
    vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);
    vec3 kS = F;
    vec3 kD = 1.0 - kS;
    vec3 kDO = kD;
    kD *= 1.0 - metallic;	  
  
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse    = irradiance * albedo;
  
    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilteredColor = textureLod(prefilterMap, R,  roughness * MAX_REFLECTION_LOD).rgb;

    
    //Why is the view suddenly negative here? I dont know


    //vec2 envBRDF  = texture(brdfLUT, vec2(max(dot(N, -V), 0.0), roughness)).rg;
    float NdotV = dot(N,V);
    vec2 envBRDF = texture2D(brdfLUT, vec2(NdotV, roughness)).xy;

    vec3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);
  
    //vec3 ambient = (kD * diffuse + specular*albedo*(1-kD)) * ao; 
    //vec3 ambient = (kD * diffuse + specular*(metallic)) * ao; 
    //vec3 ambient = (specular*albedo) * ao; 
    vec3 ambient = (kD * diffuse + specular*albedo) * ao; 
    //fragColor = vec4(specular*metallic*F+(1-metallic*F)*diffuse,1);
    
    



    vec3 testColor = vec3(0);
    //testColor = kDO*metallic*specular*albedo+kD*diffuse;
    //testColor = specular*albedo*metallic+kD*diffuse;
    //testColor = albedo*metallic;
    



    //testColor = specular+albedo*kD;
    
    testColor = vec3(ambient);

    //testColor = vec3(ssao);
    fragColor = vec4(testColor, 1);


    brightColor = vec4(0);
    //return;
    
    vec3 dirLightDirection = normalize(dirLight);

    float depth = texture(depthMap, i.texCoords).r;

    //vec4 world3 = WorldPosFromDepth(depth,i.texCoord);

    float shadow = CalculateShadow(WorldPos,N,dirLight);
    vec4 s = dirLightMatrix*vec4(WorldPos,1);

    //shadow = EasyShadowCalculation(s, dirLightShadowMap);
    //shadow = SimpleShadow(s, dirLightShadowMap,farPlaneDirLight);

    //vec3 CalcLight(vec3 N, vec3 L, vec3 V, float roughness, float metallic, vec3 F0, vec3 albedo, vec3 radiance)

    // reflectance equation
    vec3 Lo = vec3(0.0);
    

    Lo+=CalcLight(N,dirLightDirection,V,roughness,metallic,F0,albedo,dirLightColor)*(1.0 - shadow);
    
    roughness = max(0.03, roughness);
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
        /*
        uniform sampler3D pointLightShadow;
uniform float pointLightFar;
        */
        /*
        float pointShadow = 0;
        if(i == 2)
        {
        //float ShadowCalculationCube(vec3 fragPos, vec3 lightPos, sampler3D depthMap, float far_plane)
            pointShadow = ShadowCalculationCube(WorldPos, lightPositions[i], pointLightShadow, pointLightFar, N);

            //vec3 dif = WorldPos-lightPositions[i];
            //float closestDepth = texture(pointLightShadow, dif).r;
            //fragColor = vec4(vec3(closestDepth*pointLightFar-length(dif)), 1.0);  
            //fragColor = vec4(vec3(1-length(dif)),1)
            //fragColor = vec4(vec3(1-pointShadow),1);
            brightColor = vec4(0);
            //return;
            radiance *= 10;
            return;
        }
        */
        //Lo += (kD * albedo / PI + specular) * radiance * NdotL * (1-pointShadow); 
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
        //fragColor = vec4(F,1.0);
    }   
    vec3 emissive = texture(emissiveMap,i.texCoords).rgb;
    vec3 color = ambient + Lo + emissive;
   

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