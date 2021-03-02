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
			var vars = new ShaderProgramVariables(_lightPassDef);

			PosMap = vars.Get<UniformTexture>("positionMap");
			PositionRawMap = vars.Get<UniformTexture>("positionRawMap");
			AlbedoMap = vars.Get<UniformTexture>("albedoMap");
			MetalRoughtnessMap = vars.Get<UniformTexture>("metalRoughness");
			NormalMap = vars.Get<UniformTexture>("normalMap");

			EnvMap = vars.Get<UniformCubeTexture>("envMap");
			DepthMap = vars.Get<UniformTexture>("depthMap");
			CameraPos = vars.Get<UniformVec3>("cameraPosition");

			IrradianceMap = vars.Get<UniformCubeTexture>("irradianceMap");

			DirectionalLightDirection = vars.Get<UniformVec3>("dirLight");
			DirectionalLightShadowMap = vars.Get<UniformTexture>("dirLightShadowMap");
			DirectionalLightMatrix = vars.Get<UniformMatrix4>("dirLightMatrix");
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

        public UniformTexture PosMap { get; }
        public UniformTexture AlbedoMap { get; }
        public UniformTexture MetalRoughtnessMap { get; }
        public UniformTexture NormalMap { get; }
        public UniformCubeTexture EnvMap { get; }
        public UniformTexture DepthMap { get; }
        public UniformVec3 CameraPos { get; internal set; }
        public UniformCubeTexture IrradianceMap { get; }

		public UniformVec3 DirectionalLightDirection { get; }
        public UniformTexture DirectionalLightShadowMap { get; }
        public UniformMatrix4 DirectionalLightMatrix { get; }
        public UniformTexture PositionRawMap { get; internal set; }

        public void Bind()
	{
		_lightPassDef.Bind();
	}
}
}
