using Framework;
using System;
using Zev;

namespace Example
{
    class LightPassShader
    {
		private readonly ShaderProgram _lightPassDef;

		public ShaderProgram Shader => _lightPassDef;
		public LightPassShader(string ResourceDir)
        {
            _lightPassDef = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "lightPassDef"));
            CalculatePositions();
            /*
			uniform samplerCube prefilterMap;
			uniform sampler2D   brdfLUT;
			*/
            /*
			 * uniform sampler2D positionMap;
uniform sampler2D albedoMap;
uniform sampler2D metalRoughness;
uniform sampler2D normalMap;

uniform samplerCube envMap;
			 * 
			 * 
			 * */
            Console.WriteLine("LIGHT PASS");
            ShaderHelper.ShaderInfo(_lightPassDef);
        }

        public void CalculatePositions()
        {
            var vars = new ShaderProgramVariables(_lightPassDef);

            PosMap = vars.Get<UniformTexture>("positionMap");
            ViewNormalMap = vars.Get<UniformTexture>("positionRawMap");
            AlbedoMap = vars.Get<UniformTexture>("albedoMap");
            MetalRoughtnessMap = vars.Get<UniformTexture>("metalRoughness");
            NormalMap = vars.Get<UniformTexture>("normalMap");

            EnvMap = vars.Get<UniformCubeTexture>("envMap");
            DepthMap = vars.Get<UniformTexture>("depthMap");
            CameraPos = vars.Get<UniformVec3>("cameraPosition");
            EmissiveMap = vars.Get<UniformTexture>("emissiveMap");
            IrradianceMap = vars.Get<UniformCubeTexture>("irradianceMap");

            DirectionalLightDirection = vars.Get<UniformVec3>("dirLight");
            DirectionalLightShadowMap = vars.Get<UniformTexture>("dirLightShadowMap");
            DirectionalLightMatrix = vars.Get<UniformMatrix4>("dirLightMatrix");

            PrefilterSpecularMap = vars.Get<UniformCubeTexture>("prefilterMap");
            BRDFLUT = vars.Get<UniformTexture>("brdfLUT");

            PointLightMap = vars.Get<UniformCubeTexture>("pointLightShadow");

            /*
			 * uniform bool useSsao = false;
uniform sampler2D ssaoMap;

			 * 
			 * */
            UseSSAO = vars.Get<UniformBool>("useSsao");
            SsaoMap = vars.Get<UniformTexture>("ssaoMap");
        }

        public UniformTexture PosMap { get; internal set; }
        public UniformTexture AlbedoMap { get; internal set; }
        public UniformTexture MetalRoughtnessMap { get; internal set; }
        public UniformTexture NormalMap { get; internal set; }
        public UniformCubeTexture EnvMap { get; internal set; }
        public UniformTexture DepthMap { get; internal set; }
        public UniformVec3 CameraPos { get; internal set; }
        public UniformCubeTexture IrradianceMap { get; internal set; }

		public UniformVec3 DirectionalLightDirection { get; internal set; }
        public UniformTexture DirectionalLightShadowMap { get; internal set; }
        public UniformMatrix4 DirectionalLightMatrix { get; internal set; }
        public UniformCubeTexture PrefilterSpecularMap { get; internal set; }
        public UniformTexture BRDFLUT { get; internal set; }
        public UniformCubeTexture PointLightMap { get; private set; }
        public UniformBool UseSSAO { get; internal set; }
        public UniformTexture SsaoMap { get; internal set; }
        public UniformTexture ViewNormalMap { get; internal set; }

        public UniformTexture EmissiveMap { get; internal set; }
        public void Bind()
	{
		_lightPassDef.Bind();
	}
}
}
