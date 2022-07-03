using UnityEngine;
using UnityEditor;
using System.IO;

namespace Endciv.Editor
{
    public static class EntityStaticDataCreator
    {
        [MenuItem("Assets/Create/StaticData/New Unit")]
        public static void CreateUnit()
        {
            string path = GetCurrentPath();
            var entity = ScriptableObject.CreateInstance<EntityStaticData>();

            entity.name = "New Unit.asset";
            string filePath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + entity.name);
            AssetDatabase.CreateAsset(entity, filePath);
            AssetDatabase.SaveAssets();
			EntityStaticDataEditor.AddFeature(entity, typeof(EntityFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(UnitFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(GridAgentStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(LivingBeingStaticData));			
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));            
        }

        [MenuItem("Assets/Create/StaticData/New Structure")]
        public static void CreateStructure()
        {
            string path = GetCurrentPath();
            var entity = ScriptableObject.CreateInstance<EntityStaticData>();

            entity.name = "New Structure.asset";
            string filePath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + entity.name);
            AssetDatabase.CreateAsset(entity, filePath);
            AssetDatabase.SaveAssets();
			EntityStaticDataEditor.AddFeature(entity, typeof(EntityFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(StructureFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(ConstructionStaticData));
			AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));
        }

        [MenuItem("Assets/Create/StaticData/New Resource Pile")]
        public static void CreateResourcePile()
        {
            string path = GetCurrentPath();
            var entity = ScriptableObject.CreateInstance<EntityStaticData>();

            entity.name = "New Resource Pile.asset";
            string filePath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + entity.name);
            AssetDatabase.CreateAsset(entity, filePath);
            AssetDatabase.SaveAssets();
			EntityStaticDataEditor.AddFeature(entity, typeof(EntityFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(StructureFeatureStaticData));
			EntityStaticDataEditor.AddFeature(entity, typeof(ResourcePileFeatureStaticData));
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));
        }

		[MenuItem("Assets/Create/StaticData/New Item")]
		public static void CreateItem()
		{
			string path = GetCurrentPath();
			var entity = ScriptableObject.CreateInstance<EntityStaticData>();

			entity.name = "New Item.asset";
			string filePath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + entity.name);
			AssetDatabase.CreateAsset(entity, filePath);
			AssetDatabase.SaveAssets();
			EntityStaticDataEditor.AddFeature(entity, typeof(ItemFeatureStaticData));					
			AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));
		}		

        private static string GetCurrentPath()
        {
            var path = "";
            var obj = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    return Path.GetDirectoryName(path);
                }
            }
            return path;
        }
    }
}
