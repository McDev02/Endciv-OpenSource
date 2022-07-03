using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Endciv.Editor
{

	public class FontFinderWindow : EditorWindow
	{
		private class FontData
		{
			public UnityEngine.Object font;
			public List<GameObject> assignedObjects;
			public string guiString;
			public bool isUnfold;

			public FontData(UnityEngine.Object font, GameObject obj)
			{
				this.font = font;
				assignedObjects = new List<GameObject>() { obj };
				guiString = "";
			}

		}

		private Dictionary<UnityEngine.Object, FontData> fontsFound = new Dictionary<UnityEngine.Object, FontData>();

		private bool isWorking = false;

		[MenuItem(EditorHelper.EditorToolsPath + "Helper/Font Finder", false)]
		public static void Open()
		{
			GetWindow<FontFinderWindow>(false, "Font Finder", true).Show();
		}


		private void OnGUI()
		{
			EditorGUI.BeginDisabledGroup(isWorking);
			if (GUILayout.Button("Find Fonts in Scene", EditorStyles.toolbarButton))
			{
				FindFontsInScene();
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(16);

			if (isWorking)
			{
				GUILayout.Label($"Searching... ({fontsFound.Count})");
			}

			StringBuilder builder = new StringBuilder();
			var fonts = fontsFound.Values.ToList();
			if (fonts.Count > 0)
			{
				for (int i = 0; i < fonts.Count; i++)
				{
					var font = fonts[i];
					font.isUnfold = GUILayout.Toggle(font.isUnfold, font.guiString);

					if (font.isUnfold)
					{
						builder.Clear();
						for (int j = 0; j < font.assignedObjects.Count; j++)
						{
							builder.Append(" - ");
							builder.AppendLine(font.assignedObjects[j].name);
						}
						GUILayout.Label(builder.ToString());
					}
				}
			}
			else
				GUILayout.Label("No Fonts Found");
		}

		private void FindFontsInScene()
		{
			var scene = SceneManager.GetActiveScene();
			var roots = scene.GetRootGameObjects();

			fontsFound.Clear();

			FindFontsInSceneRoutine(roots);

			foreach (var fontInfo in fontsFound.Values)
			{
				fontInfo.guiString = $"{fontInfo.font.name}: ({fontInfo.assignedObjects.Count})";
			}
		}

		private void FindFontsInSceneRoutine(GameObject[] roots)
		{
			isWorking = true;

			foreach (var root in roots)
			{
				IterrateOverElement(root.transform);
			}

			isWorking = false;
		}

		private void IterrateOverElement(Transform trans)
		{
			CollectFontOfElement(trans.gameObject);

			for (int i = 0; i < trans.childCount; i++)
			{
				IterrateOverElement(trans.GetChild(i));
			}
		}
		private void CollectFontOfElement(GameObject obj)
		{
			var text = obj.GetComponent<UnityEngine.UI.Text>();
			if (text != null)
			{
				if(text.font!=null)
					RegisterFont(text.font, obj);
			}
			var tmp = obj.GetComponent<TextMeshPro>();
			if(tmp != null)
			{
				if (tmp.font != null)
					RegisterFont(tmp.font, obj);
			}
		}

		private void RegisterFont(UnityEngine.Object font, GameObject obj)
		{
			if (fontsFound.ContainsKey(font))
			{
				fontsFound[font].assignedObjects.Add(obj);
			}
			else
			{
				fontsFound.Add(font, new FontData(font, obj));
			}
		}
	}
}