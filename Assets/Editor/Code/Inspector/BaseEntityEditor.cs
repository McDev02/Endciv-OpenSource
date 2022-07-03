using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Endciv.Editor
{
	[CustomEditor(typeof(EntityFeatureView), true)]
	public sealed class BaseEntityEditor : UnityEditor.Editor
	{
		private BaseEntity Context { get; set; }
		private Dictionary<string, FeatureEditorBase> FeatureEditors { get; set; }
		List<string> missingFeatureEditors;
		private Dictionary<string, string> AssemblyReferences { get; set; }

		private void OnEnable()
		{
			Context = ((EntityFeatureView)target).Feature.Entity;
			PopulateAssemblyReferences();
			BuildFeatureEditors();
		}

		private void PopulateAssemblyReferences()
		{
			AssemblyReferences = new Dictionary<string, string>();
			var types = GetType().Assembly.GetTypes().Where(t => t.IsClass && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(FeatureEditor<>) && !t.IsAbstract);
	
			foreach (var type in types)
			{
				var args = type.BaseType.GetGenericArguments();
				if (args.Length <= 0)
					continue;
				var featureType = args[0];
				AssemblyReferences.Add(featureType.ToString(), type.ToString());
			
			}
		}

		private void BuildFeatureEditors()
		{
			FeatureEditors = new Dictionary<string, FeatureEditorBase>();
			missingFeatureEditors = new List<string>();
			foreach (var pair in Context.Features)
			{
				if (!AssemblyReferences.ContainsKey(pair.Key.ToString()))
				{
					missingFeatureEditors.Add(pair.Key.ToString());
					continue;
				}
				var editor = (FeatureEditorBase)Activator.CreateInstance(Type.GetType(AssemblyReferences[pair.Key.ToString()]));
				editor.SetFeature(pair.Value);
				FeatureEditors.Add(pair.Key.ToString(), editor);
				editor.OnEnable();
			}
		}

		public override void OnInspectorGUI()
		{
			//Entity
			EditorGUILayout.LabelField("Object ID: " + Context.IDString);
			EditorGUILayout.LabelField("Object UID " + Context.UID);

			EditorGUILayout.BeginVertical("Box");

			EditorGUILayout.LabelField("Features: " + Context.FeatureCount, EditorStyles.boldLabel);
			EditorGUILayout.Space();
			//Features
			foreach (var editor in FeatureEditors)
			{
				EditorGUILayout.BeginVertical("Box");
				var featureName = editor.Key.Substring(editor.Key.LastIndexOf('.') + 1);
				EditorGUILayout.LabelField(Regex.Replace(featureName, "((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", " $1"), EditorStyles.boldLabel);
				editor.Value.OnGUI();
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			for (int i = 0; i < missingFeatureEditors.Count; i++)
			{
				GUILayout.Label(missingFeatureEditors[i]);
			}

			EditorGUILayout.EndVertical();
			Repaint();
		}
	}
}