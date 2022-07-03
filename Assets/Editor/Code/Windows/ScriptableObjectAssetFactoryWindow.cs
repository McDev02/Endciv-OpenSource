using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
	internal class ScriptableObjectAssetFactoryWindow : EditorWindow
	{
		[MenuItem("Assets/Open ScriptableObject Creator", false, 0)]
		[MenuItem(EditorHelper.EditorToolsPath + "Open ScriptableObject Creator", false, 0)]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow<ScriptableObjectAssetFactoryWindow>(false, "ScriptableObject AssetFactory Window", true).Show();
		}

		private Vector2 m_ScrollView;
		private List<Type> m_TargetTypes = new List<Type>();
		private string m_TargetFolder = "Assets/";

		private void OnEnable()
		{
			m_TargetTypes.Clear();
			Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; ++i)
			{
				if (!assemblies[i].FullName.StartsWith("Assembly", StringComparison.InvariantCulture))
				{
					continue;
				}

				var types = assemblies[i].GetTypes();

				for (int j = 0; j < types.Length; j++)
				{
					if (types[j].IsSubclassOf(typeof(ScriptableObject))
						&& !types[j].IsSubclassOf(typeof(EditorWindow))
						&& !types[j].IsSubclassOf(typeof(UnityEditor.Editor))
						&& !types[j].IsAbstract)
					{
						m_TargetTypes.Add(types[j]);
					}
				}
			}
			m_TargetTypes.Sort((a, b) => a.Name.CompareTo(b.Name));

			OnSelectionChange();
		}

		private void OnSelectionChange()
		{
			var target = Selection.activeObject;
			m_TargetFolder = "Assets/";
			if (target != null)
			{
				var path = AssetDatabase.GetAssetPath(target);
				if (Directory.Exists(path))
				{
					m_TargetFolder = path;
				}
				else
				{
					m_TargetFolder = Path.GetDirectoryName(path);
				}
			}
			Repaint();
		}

		private void OnGUI()
		{
			GUILayout.Label("TargetFolder: " + m_TargetFolder);
			m_ScrollView = GUILayout.BeginScrollView(m_ScrollView);

			GUILayout.BeginVertical();
			for (int i = 0; i < m_TargetTypes.Count; i++)
			{
				var type = m_TargetTypes[i];

				GUILayout.BeginHorizontal(GUI.skin.box);
				GUILayout.Label(type.FullName);
				if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.Width(60)))
				{
					CreateAsset(type);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndScrollView();
		}

		private void CreateAsset(Type type)
		{
			var data = ScriptableObject.CreateInstance(type);
			var path = Path.Combine(m_TargetFolder, type.Name + ".asset");
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			//EditorUtility.DisplayDialog (
			//	"Create Asset",
			//	"Create ScriptableObject " + type.FullName + "\nIn " + path,
			//	"ok"
			//);

			Selection.activeObject = data;
		}
	}
}