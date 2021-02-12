#version 430 core

layout (location = 0) out vec3 Position;
layout (location = 1) out vec3 Normal;
layout (location = 2) out vec4 AlbedoSpec;
layout (location = 3) out vec4 MetalRoughness;

in Data
{
	vec2 texCoords;
	vec3 normal;
	vec3 position;
    mat3 TBN;
	vec3 tangent;
} i;


uniform sampler2D albedoMap;
uniform sampler2D metalRoughnessMap;
uniform sampler2D normalMap;
uniform float normalStrength = 0.00001;

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


void main()
{    
    // store the fragment position vector in the first gbuffer texture
    Position = i.position;

    // also store the per-fragment normals into the gbuffer
    vec3 realNormal = normalize(i.normal);
    vec3 normaltex = texture(normalMap, i.texCoords).rgb;
	vec3 normal = normaltex * 2.0 - 1.0;   
	//Normal = normalize(i.TBN * normal); 

    if(i.texCoords.x > 0.1f)
    {
        //Normal = normalize(i.normal);
    }else{
        
    }
    //Normal = normalize(i.TBN*normaltex);
    Normal = i.TBN*normal;

    //Normal = CalcBumpedNormal();
    // and the diffuse per-fragment color
    AlbedoSpec.rgb = texture(albedoMap, i.texCoords).rgb;
    AlbedoSpec.a = 1;

    MetalRoughness.rgb = texture(metalRoughnessMap,i.texCoords).rgb;
    MetalRoughness.a = 1;
    // store specular intensity in gAlbedoSpec's alpha component
    //AlbedoSpec.a = texture(specularTexture, i.texCoords).r;
}  