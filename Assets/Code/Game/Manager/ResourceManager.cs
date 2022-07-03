using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

namespace Endciv
{
	public enum EResourceIconType
	{
		General,
		Building,
		Notification,
		Unit
	}

	public enum EIconSize
	{
		Small,
		Normal,
		Large
	}

	public class ResourceManager : ResourceSingleton<ResourceManager>
	{
		private const string DEFAULT_ICON = "default-icon";
		private const string DEFAULT_ICON_SMALL = "default-icon_s";
		private const string DEFAULT_ICON_LARGE = "default-icon_l";
		private const int MAX_ATLAS_SIZE = 1024;
		public TMP_SpriteAsset SpriteAsset;

		public string resourceIconPath;
		public string buildingIconPath;
		public string notificationIconPath;
		public string unitIconPath;
		public string textSpritesPath;
		public int textSpritesYOffset;

		public Dictionary<string, Sprite> resourceIcons;
		public Dictionary<string, Sprite> buildingIcons;
		public Dictionary<string, Sprite> notificationIcons;
		public Dictionary<string, Sprite> unitIcons;

		public void Initialize()
		{
			if (!LoadIcons())
			{
				Debug.Log("General Icons failed to load.");
			}
			else
			{
				Debug.Log("General Icons loaded. Count " + resourceIcons.Count + ".");
			}
			if (!LoadBuildingIcons())
			{
				Debug.Log("Building Icons failed to load.");
			}
			else
			{
				Debug.Log("Building Icons loaded. Count " + buildingIcons.Count + ".");
			}
			if (!LoadNotificationIcons())
			{
				Debug.Log("Notification Icons failed to load.");
			}
			else
			{
				Debug.Log("Notification Icons loaded. Count " + notificationIcons.Count + ".");
			}
			if (!LoadUnitIcons())
			{
				Debug.Log("Unit Icons failed to load.");
			}
			else
			{
				Debug.Log("Unit Icons loaded. Count " + unitIcons.Count + ".");
			}
		}

		private bool LoadIcons()
		{
			resourceIcons = LoadIconsAtPath(resourceIconPath);
			return resourceIcons != null;
		}

		private bool LoadBuildingIcons()
		{
			buildingIcons = LoadIconsAtPath(buildingIconPath);
			return buildingIcons != null;
		}

		private bool LoadNotificationIcons()
		{
			notificationIcons = LoadIconsAtPath(notificationIconPath);
			return notificationIcons != null;
		}

		private bool LoadUnitIcons()
		{
			unitIcons = LoadIconsAtPath(unitIconPath);
			return unitIcons != null;
		}

#if UNITY_EDITOR
		private void BuildSpriteAssets()
		{
			SpriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
			GenerateSpriteAssetsForPath(resourceIconPath);
			GenerateSpriteAssetsForPath(buildingIconPath);
			GenerateSpriteAssetsForPath(notificationIconPath);
			GenerateSpriteAssetsForPath(textSpritesPath, textSpritesYOffset);
			UnityEditor.EditorUtility.SetDirty(SpriteAsset);
		}
#endif
		private Dictionary<string, Sprite> LoadIconsAtPath(string path)
		{
			var dict = new Dictionary<string, Sprite>();
			if (string.IsNullOrEmpty(path))
				return null;
			var textures = Resources.LoadAll<Sprite>(path);
			foreach (var texture in textures)
			{
				dict.Add(texture.name, texture);
			}
			return dict;
		}

		public Sprite GetIcon(string ID, EResourceIconType type, EIconSize size = EIconSize.Normal)
		{
			string path = string.Empty;
			string iconID = ID;
			if (size == EIconSize.Small)
				iconID = ID + "_s";
			if (size == EIconSize.Large)
				iconID = ID + "_l";
			switch (type)
			{
				case EResourceIconType.General:
					if (resourceIcons.ContainsKey(iconID))
						return resourceIcons[iconID];
					else if (ID != iconID && resourceIcons.ContainsKey(ID))
					{
						return resourceIcons[ID];
					}
					else if (size == EIconSize.Normal && resourceIcons.ContainsKey(DEFAULT_ICON))
					{
						return resourceIcons[DEFAULT_ICON];
					}
					else if (size == EIconSize.Large && resourceIcons.ContainsKey(DEFAULT_ICON_LARGE))
					{
						return resourceIcons[DEFAULT_ICON_LARGE];
					}
					else if (resourceIcons.ContainsKey(DEFAULT_ICON_SMALL))
					{
						return resourceIcons[DEFAULT_ICON_SMALL];
					}
					else
					{
						return null;
					}

				case EResourceIconType.Building:
					if (buildingIcons.ContainsKey(iconID))
						return buildingIcons[iconID];
					else if (ID != iconID && buildingIcons.ContainsKey(ID))
					{
						return buildingIcons[ID];
					}
					else if (size == EIconSize.Normal && buildingIcons.ContainsKey(DEFAULT_ICON))
					{
						return buildingIcons[DEFAULT_ICON];
					}
					else if (buildingIcons.ContainsKey(DEFAULT_ICON_SMALL))
					{
						return buildingIcons[DEFAULT_ICON_SMALL];
					}
					else
					{
						return null;
					}

				case EResourceIconType.Notification:
					if (notificationIcons.ContainsKey(iconID))
						return notificationIcons[iconID];
					else if (ID != iconID && notificationIcons.ContainsKey(ID))
					{
						return notificationIcons[ID];
					}
					else if (size == EIconSize.Normal && notificationIcons.ContainsKey(DEFAULT_ICON))
					{
						return notificationIcons[DEFAULT_ICON];
					}
					else if (notificationIcons.ContainsKey(DEFAULT_ICON_SMALL))
					{
						return notificationIcons[DEFAULT_ICON_SMALL];
					}
					else
					{
						return null;
					}

				case EResourceIconType.Unit:
					if (unitIcons.ContainsKey(iconID))
						return unitIcons[iconID];
					else if (ID != iconID && unitIcons.ContainsKey(ID))
					{
						return unitIcons[ID];
					}
					else if (size == EIconSize.Normal && unitIcons.ContainsKey(DEFAULT_ICON))
					{
						return unitIcons[DEFAULT_ICON];
					}
					else if (unitIcons.ContainsKey(DEFAULT_ICON_SMALL))
					{
						return unitIcons[DEFAULT_ICON_SMALL];
					}
					else
					{
						return null;
					}
				default:
					return null;
			}
		}

#if UNITY_EDITOR
		void GenerateSpriteAssetsForPath(string path, int yOffset = 0)
		{
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Path is null");
				return;
			}

			var textures = Resources.LoadAll<Texture2D>(path).ToList();
			int counter = 0;
			while (textures != null && textures.Count > 0)
			{
				var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
				spriteAsset.name = path + "_" + counter;
				UnityEditor.AssetDatabase.CreateAsset(spriteAsset, "Assets/Content/GUI/Sprites/Resources/TextSpriteAssets/" + spriteAsset.name + ".asset");
				int maxArea = MAX_ATLAS_SIZE * MAX_ATLAS_SIZE;
				float area = 0f;
				List<Texture2D> atlasTextures = new List<Texture2D>();
				for (int i = textures.Count - 1; i >= 0; i--)
				{
					var texture = textures[i];
					area += texture.width * texture.height;
					if (area > maxArea)
						break;
					atlasTextures.Add(texture);
					textures.Remove(texture);
				}
				int side = NextPowerOfTwo((int)Mathf.Ceil(Mathf.Sqrt(area)));
				var atlas = new Texture2D(side, side);
				Debug.Log("Atlas generated for resources at path " + path + " with size " + side);
				var data = atlas.PackTextures(atlasTextures.ToArray(), 0);
				spriteAsset.spriteSheet = atlas;
				atlas.hideFlags = HideFlags.HideInHierarchy;
				UnityEditor.AssetDatabase.AddObjectToAsset(atlas, spriteAsset);
				List<TMP_Sprite> spriteInfoList = new List<TMP_Sprite>();

				for (int i = 0; i < atlasTextures.Count; i++)
				{
					TMP_Sprite sprite = new TMP_Sprite();

					sprite.id = i;
					sprite.name = path + "/" + atlasTextures[i].name;
					sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(sprite.name);

					int unicode = 0;
					int indexOfSeperator = sprite.name.IndexOf('-');
					if (indexOfSeperator != -1)
						unicode = TMP_TextUtilities.StringHexToInt(sprite.name.Substring(indexOfSeperator + 1));
					else
						unicode = TMP_TextUtilities.StringHexToInt(sprite.name);

					sprite.unicode = unicode;

					sprite.x = data[i].x * atlas.width;
					sprite.y = data[i].y * atlas.height;
					sprite.width = data[i].width * atlas.width;
					sprite.height = data[i].height * atlas.height;

					//Calculate sprite pivot position
					sprite.pivot = new Vector2(0f, 0f);

					// Properties the can be modified
					sprite.xAdvance = sprite.width;
					sprite.scale = 1.0f;
					sprite.xOffset = 0 - (sprite.width * sprite.pivot.x);
					sprite.yOffset = sprite.height - (sprite.height * sprite.pivot.y) + yOffset;

					spriteInfoList.Add(sprite);
				}
				spriteAsset.spriteInfoList = spriteInfoList;

				Shader shader = Shader.Find("TextMeshPro/Sprite");
				Material material = new Material(shader);
				material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);
				spriteAsset.material = material;
				material.hideFlags = HideFlags.HideInHierarchy;
				UnityEditor.AssetDatabase.AddObjectToAsset(material, spriteAsset);
				spriteAsset.UpdateLookupTables();
				SpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
				counter++;
			}
		}

		[UnityEditor.MenuItem("Endciv/Bake Atlas Textures")]
		static void BakeAtlasTexturesEditor()
		{
			Instance.BuildSpriteAssets();
		}
#endif

		public int NextPowerOfTwo(int number)
		{
			if (number < 0)
				return 0;
			number--;
			number |= number >> 1;
			number |= number >> 2;
			number |= number >> 4;
			number |= number >> 8;
			number |= number >> 16;
			return number + 1;
		}

		public Vector2 GetSpriteSize(string spritePath)
		{
			foreach (var asset in SpriteAsset.fallbackSpriteAssets)
			{
				var spriteInfo = asset.spriteInfoList.FirstOrDefault(x => x.name == spritePath);
				if (spriteInfo == null)
					continue;
				return new Vector2(spriteInfo.width, spriteInfo.height);

			}
			return Vector2.zero;
		}
	}
}