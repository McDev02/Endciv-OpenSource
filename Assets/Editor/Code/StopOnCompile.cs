using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
	[InitializeOnLoad]
	public class StopOnCompile
	{
		static StopOnCompile()
		{
			EditorApplication.update += EditorUpdate;
		}

		static void EditorUpdate()
		{
			if ((EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
				&& EditorApplication.isCompiling)
			{
				EditorApplication.isPlaying = false;

				Debug.Log("Unity is compiling\nExit play mode...");

				foreach (EditorWindow window in SceneView.sceneViews)
				{
					window.ShowNotification(new GUIContent("Unity is compiling\nExit play mode..."));
				}
			}
		}
	}
}