#version 430 core

out vec4 fragColor;

in Data
{
	vec2 texCoords;
} i;

uniform sampler2D scene;
uniform sampler2D bloomScene;
uniform float exposure;


// Filmic Tonemapping Operators http://filmicworlds.com/blog/filmic-tonemapping-operators/
vec3 tonemapFilmic(vec3 x) {
  vec3 X = max(vec3(0.0), x - 0.004);
  vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
  return pow(result, vec3(2.2));
}

float tonemapFilmic(float x) {
  float X = max(0.0, x - 0.004);
  float result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
  return pow(result, 2.2);
}
// Unreal 3, Documentation: "Color Grading"
// Adapted to be close to Tonemap_ACES, with similar range
// Gamma 2.2 correction is baked in, don't use with sRGB conversion!
vec3 unreal(vec3 x) {
  return x / (x + 0.155) * 1.019;
}

float unreal(float x) {
  return x / (x + 0.155) * 1.019;
}
// Narkowicz 2015, "ACES Filmic Tone Mapping Curve"
vec3 aces(vec3 x) {
  const float a = 2.51;
  const float b = 0.03;
  const float c = 2.43;
  const float d = 0.59;
  const float e = 0.14;
  return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

float aces(float x) {
  const float a = 2.51;
  const float b = 0.03;
  const float c = 2.43;
  const float d = 0.59;
  const float e = 0.14;
  return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

//Gamma sRGB
const float gamma = 2.2;

void main()
{
	
    vec3 hdrColor = texture(scene, i.texCoords).rgb;      
    vec3 bloomColor = texture(bloomScene, i.texCoords).rgb;
    hdrColor += bloomColor; // additive blending


    // tone mapping after the images are combined!
    //vec3 result = vec3(1.0) - exp(-hdrColor * exposure);

    vec3 result = vec3(0);
    //result = aces(hdrColor*exposure);
    //result = aces(hdrColor)*exposure;
    //Already has gamma
    result = aces(hdrColor*exposure);
    

    // also gamma correct while we're at it       
    result = pow(result, vec3(1.0 / gamma));
    

    fragColor = vec4(result, 1.0);
    
}
