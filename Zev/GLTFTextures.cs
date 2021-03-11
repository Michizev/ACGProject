using System;
using System.Collections.Generic;
using System.IO;

namespace Example
{
    class GLTFTextures
    {
		GltfModel _model { get; }
		string rootPath;

		public Dictionary<int, Framework.Texture> TextureToTextureHandle = new Dictionary<int, Framework.Texture>();
        public GLTFTextures(GltfModel model, string rootPath)
        {
            _model = model;
            this.rootPath = rootPath;
        }

        public void Load()
        {
			var imageNamesPath = new Dictionary<string, string>();
			if (_model.Gltf.Images == null) return;
			foreach (var i in _model.Gltf.Images)
			{
				var name = i.Name;
				if(i.Name == null)
                {
					name = i.Uri;
                }


				imageNamesPath.Add(name, i.Uri);
			}

			//var imgs = new Dictionary<int,ImageMagick.MagickImage>();
			int textureID = 0;
			foreach (var t in _model.Gltf.Textures)
			{
				Console.WriteLine(t.Name + " " + t.Source);
				if (t.Source is not null)
				{
					var imgName = _model.Gltf.Images[(int)t.Source].Name;
					if (imgName == null) imgName = _model.Gltf.Images[(int)t.Source].Uri;
					var path = rootPath + '\\' + imageNamesPath[imgName];
					Console.WriteLine(path);
					using var s = File.OpenRead(path);

					//Console.WriteLine(s.CanRead);

					//imgs.Add((int)t.Source, new ImageMagick.MagickImage(path));

				
						var tex = Framework.TextureLoader.Load(s);
					tex.Function = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
						TextureToTextureHandle.Add(textureID++, tex);
					
				}

			}
		}

    }
}
