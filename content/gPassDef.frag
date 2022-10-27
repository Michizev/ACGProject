#version 430 core

layout (location = 0) out vec4 Position;
layout (location = 1) out vec3 Normal;
layout (location = 2) out vec4 AlbedoSpec;
layout (location = 3) out vec4 MetalRoughness;
layout (location = 4) out vec3 ViewNormal;
layout (location = 5) out vec4 ViewPos;
layout (location = 6) out vec4 Emissive;

in Data
{
	vec2 texCoords;
	vec3 normal;
	vec4 position;
    mat3 TBN;
	vec3 tangent;
    vec3 tangentPos;
    vec3 tangentCameraPos;
    vec3 positionRaw;
    vec3 viewNormal;
    vec4 viewPos;
} i;


uniform float heightScale = 1.0;
uniform vec3 cameraPos;

uniform sampler2D albedoMap;
uniform bool hasAO;
uniform sampler2D metalRoughnessMap;
uniform sampler2D normalMap;
uniform float normalStrength = 0.00001;

uniform sampler2D emissiveMap;
uniform bool hasEmissive = false;
uniform float emissiveIntensitiy = 2.0f;
uniform mat4 model;
uniform mat4 view;

//Code from http://ogldev.atspace.co.uk/www/tutorial26/tutorial26.html
vec3 CalcBumpedNormal()
{

    vec3 Normal = normalize(i.normal);
    vec3 Tangent = normalize(i.tangent);
    //Calc TBN
    Tangent = normalize(Tangent - dot(Tangent, Normal) * Normal);
    vec3 Bitangent = cross(Tangent, Normal);
    mat3 TBN = mat3(Tangent, Bitangent, Normal);

    vec3 BumpMapNormal = texture(normalMap, i.texCoords).xyz;
    BumpMapNormal = 2.0 * BumpMapNormal - vec3(1.0, 1.0, 1.0);
    vec3 NewNormal;
    
    NewNormal = TBN * BumpMapNormal;
    NewNormal = normalize(NewNormal);
    return NewNormal;
}

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    float height =  texture(metalRoughnessMap, texCoords).b;    
    vec2 p = viewDir.xy / viewDir.z * (height * heightScale);
    return texCoords - p;    
}

vec2 SteepParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    // number of depth layers
    const float numLayers = 10;
    // calculate the size of each layer
    float layerDepth = 1.0 / numLayers;
    // depth of current layer
    float currentLayerDepth = 0.0;
    // the amount to shift the texture coordinates per layer (from vector P)
    vec2 P = viewDir.xy * heightScale; 
    vec2 deltaTexCoords = P / numLayers;   

    // get initial values
    vec2  currentTexCoords     = texCoords;
    float currentDepthMapValue = texture(metalRoughnessMap, currentTexCoords).b;
  
    while(currentLayerDepth < currentDepthMapValue)
    {
        // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
        // get depthmap value at current texture coordinates
        currentDepthMapValue = texture(metalRoughnessMap, currentTexCoords).b;  
        // get depth of next layer
        currentLayerDepth += layerDepth;  
    }

    return currentTexCoords;
}
const float gamma = 2.2;
void main()
{   
    ViewPos = i.viewPos;

    vec3 viewDir   = normalize(i.tangentPos - i.tangentCameraPos);

    //vec3 viewDir   = normalize(i.positionRaw - cameraPos);

    
    //vec3 viewDir   = normalize(i.position - cameraPos);
    vec2 texCoords = ParallaxMapping(i.texCoords,  viewDir);

    float height =  texture(metalRoughnessMap, texCoords).b;  
    height = 0;
    float v = height * 1 - 1;
    texCoords = i.texCoords + (viewDir.xy * v);

    texCoords = i.texCoords;

    if(hasEmissive){
        Emissive = textureLod(emissiveMap, texCoords,0) * emissiveIntensitiy;
    }else{
        Emissive = vec4(0);
    }
    // store the fragment position vector in the first gbuffer texture
    Position = i.position;

    // also store the per-fragment normals into the gbuffer
    //Normal Mapping code
    //vec3 realNormal = normalize(i.normal);
    ViewNormal = i.viewNormal;

    vec3 normaltex = texture(normalMap, texCoords).rgb;
	vec3 normal = normaltex * 2.0 - 1.0;   
    Normal = normalize(i.TBN*normal);

    // and the diffuse per-fragment color
    vec4 tex = texture(albedoMap, texCoords).rgba;
    AlbedoSpec = vec4(pow(tex.rgb,vec3(gamma)),tex.a);

    MetalRoughness.rgb = texture(metalRoughnessMap, texCoords).rgb;
    if(!hasAO) MetalRoughness.r = 1;
    MetalRoughness.a = 1;
}  