using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace Endciv.Editor
{
	[CustomEditor(typeof(EntityStaticData), true)]
	public class EntityStaticDataEditor : UnityEditor.Editor
	{
		private List<UnityEditor.Editor> featureEditors;
		private EntityStaticData context;
		private Type[] featureTypes;
		private HashSet<Type> containedTypes = new HashSet<Type>();
		private int selectedType = 0;

		private void OnEnable()
		{
			context = (EntityStaticData)target;
			RefreshEditors();
		}

		private void RefreshEditors()
		{
			featureEditors = new List<UnityEditor.Editor>();
			featureTypes = Assembly.GetAssembly(typeof(FeatureStaticDataBase)).
				GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(FeatureStaticDataBase))).
				ToArray();
			if (context.FeatureStaticData != null && context.FeatureStaticData.Count > 0)
			{
				foreach (var feature in context.FeatureStaticData)
				{
					featureEditors.Add(CreateEditor(feature));
					containedTypes.Add(feature.GetType());
				}
			}
			selectedType = 0;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			//Name is derriced from ScriptableObject asset, here we exposie it for copy paste.
			GUILayout.Label("Entity ID (Read only)");
            GUI.enabled = false;
			EditorGUILayout.TextField(context.name);
            GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			base.OnInspectorGUI();
			EditorGUILayout.Space();
			for (int i = 0; i < featureEditors.Count; i++)
			{
				var editor = featureEditors[i];

				var backgroundRect = EditorGUILayout.BeginVertical();
				EditorGUILayout.Space();
				GUI.Box(backgroundRect, "");
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(15);
				EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"{editor.target.GetType().ToString()}", EditorStyles.boldLabel);
				GUI.enabled = featureEditors[0] != editor;
				if (GUILayout.Button("\u25B2", EditorStyles.miniButton))
				{
					int oldIndex = featureEditors.IndexOf(editor);
					int newIndex = oldIndex - 1;
					var data = context.FeatureStaticData[oldIndex];
					context.FeatureStaticData.Remove(data);
					featureEditors.Remove(editor);
					featureEditors.Insert(newIndex, editor);
					context.FeatureStaticData.Insert(newIndex, data);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(context));
					break;
				}
				GUI.enabled = featureEditors[featureEditors.Count - 1] != editor;
				if (GUILayout.Button("\u25BC", EditorStyles.miniButton))
				{
					int oldIndex = featureEditors.IndexOf(editor);
					int newIndex = oldIndex + 1;
					var data = context.FeatureStaticData[oldIndex];
					context.FeatureStaticData.Remove(data);
					featureEditors.Remove(editor);
					featureEditors.Insert(newIndex, editor);
					context.FeatureStaticData.Insert(newIndex, data);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(context));
					break;
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				editor.OnInspectorGUI();
				EditorGUILayout.EndVertical();
				GUILayout.Space(15);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(15);
				if (GUILayout.Button("Remove Feature"))
				{
					var index = featureEditors.IndexOf(editor);
					var feature = context.FeatureStaticData[index];
					foreach (var feat in context.FeatureStaticData)
					{
						if (feat == feature)
							continue;
						var att = feat.GetType().GetAttribute<RequireFeatureAttribute>();
						if (att == null)
							continue;
						if (att.requiredTypes == null || att.requiredTypes.Length <= 0)
							continue;
						foreach (var subType in att.requiredTypes)
						{
							if (subType == feature.GetType())
							{
								string currentFeatureName = feature.GetType().ToString();
								currentFeatureName = currentFeatureName.Substring(currentFeatureName.LastIndexOf('.') + 1);
								string derivedFeatureName = feat.GetType().ToString();
								derivedFeatureName = derivedFeatureName.Substring(derivedFeatureName.LastIndexOf('.') + 1);
								EditorUtility.DisplayDialog("Cannot remove Feature", "Feature " + currentFeatureName + " cannot be removed as it is required by Feature " + derivedFeatureName + ".", "Cancel");
								return;
							}
						}
					}
					context.FeatureStaticData.RemoveAt(index);
					containedTypes.Remove(feature.GetType());
					DestroyImmediate(feature, true);
					featureEditors.RemoveAt(index);
					BroadcastStaticDataChanged(context);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(context));
					break;
				}
				GUILayout.Space(15);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			List<string> validTypeNames = new List<string>();
			List<Type> validTypes = new List<Type>();
			foreach (var type in featureTypes)
			{
				if (containedTypes.Contains(type))
					continue;
				validTypes.Add(type);
				validTypeNames.Add(type.ToString());
			}
			if (validTypes.Count <= 0)
				return;
			EditorGUILayout.Space();
			var addFeatureRect = EditorGUILayout.BeginHorizontal();
			GUI.Box(addFeatureRect, "");
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			float height = EditorStyles.popup.fixedHeight;
			EditorStyles.popup.fixedHeight = 18.3f;
			var rect = EditorGUILayout.GetControlRect();
			rect.y += 1;
			selectedType = EditorGUI.Popup(rect, selectedType, validTypeNames.ToArray());
			EditorStyles.popup.fixedHeight = height;
			if (GUILayout.Button("Add Feature"))
			{
				AddFeature(context, validTypes[selectedType]);
				RefreshEditors();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			var types = GetMissingTypes(context);
			if(types != null && types.Count > 0)
			{
				EditorGUILayout.Space();
				string txt = "\nRequired Features Missing!\n\nThe following features are missing from this entity:\n\n";
				foreach(var type in types)
				{
					txt += "- "+type.ToString() + "\n";
				}
				EditorGUILayout.HelpBox(txt, MessageType.Warning);
			}
			
		}

		public static void AddFeature(EntityStaticData entity, Type type)
		{
			var feature = CreateInstance(type);
            ((FeatureStaticDataBase)feature).entity = entity;
			feature.name = type.ToString();
			feature.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(feature, entity);
			if (entity.FeatureStaticData == null)
				entity.FeatureStaticData = new List<FeatureStaticDataBase>();
			entity.FeatureStaticData.Add((FeatureStaticDataBase)feature);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));

			var att = type.GetAttribute<RequireFeatureAttribute>();
			if (att == null)
			{
				BroadcastStaticDataChanged(entity);
				return;
			}
			var types = att.requiredTypes;
			if (types == null || types.Length <= 0)
			{
				BroadcastStaticDataChanged(entity);
				return;
			}
			foreach (var subType in types)
			{
				if (!typeof(FeatureStaticDataBase).IsAssignableFrom(subType))
				{
					continue;
				}

				if (entity.HasFeature(subType))
				{
					continue;
				}
				AddFeature(entity, subType);
			}
			BroadcastStaticDataChanged(entity);
		}

		public static void BroadcastStaticDataChanged(EntityStaticData entity)
		{
			foreach(var data in entity.FeatureStaticData)
			{
				data.OnFeatureStaticDataChanged();
			}
		}

		public static List<Type> GetMissingTypes(EntityStaticData entity)
		{
			List<Type> typeList = new List<Type>();
			foreach (var feature in entity.FeatureStaticData)
			{
				var type = feature.GetType();
				var att = type.GetAttribute<RequireFeatureAttribute>();
				if (att == null)
				{
					continue;
				}
				var types = att.requiredTypes;
				if (types == null || types.Length <= 0)
				{
					continue;
				}
				foreach (var subType in types)
				{
					if (!typeof(FeatureStaticDataBase).IsAssignableFrom(subType))
					{
						continue;
					}

					if (!entity.HasFeature(subType))
					{
						typeList.Add(subType);
					}
				}
			}
			return typeList;
		}
	}
}