using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class StartSceneDefinition : EditorWindow
{
	void OnGUI()
	{
		// Use the Object Picker to select the start SceneAsset
		EditorSceneManager.playModeStartScene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Start Scene"), EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);
		GUILayout.Label("When defined this scene is being loaded when pressing the Start Button");
	}

	void SetPlayModeStartScene(string scenePath)
	{
		SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		if (myWantedStartScene != null)
			EditorSceneManager.playModeStartScene = myWantedStartScene;
		else
			Debug.Log("Could not find Scene " + scenePath);
	}

	[MenuItem("Endciv/Tools/Define Start Scene")]
	static void Open()
	{
		GetWindow<StartSceneDefinition>();
	}
}