using UnityEngine;
using UnityEditor;


class TextureImportScripts : AssetPostprocessor
{
	void OnPreprocessTexture()
	{
		//Resource sprites
		if (assetPath.Contains("Content/GUI/Sprites/Resources/"))
		{
			TextureImporter textureImporter = (TextureImporter)assetImporter;
			textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.isReadable = true;
		}
		//General Sprites, if nothing other than Default has been specified
		if (assetPath.Contains("Content/GUI/Sprites/"))
		{
			TextureImporter textureImporter = (TextureImporter)assetImporter;
			if (textureImporter.textureType == TextureImporterType.Default)
				textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.sRGBTexture = true;
		}
		//ModularTextures Readable - Exclude already merged textures
		if (assetPath.Contains("Content/Models/") && !assetPath.Contains("MergedModels"))
		{
			TextureImporter textureImporter = (TextureImporter)assetImporter;
			textureImporter.isReadable = true;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
		}
	}
}