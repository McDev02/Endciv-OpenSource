using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
	public class PathfindingDebugWindow : EditorWindow
	{
		// Add menu named "My Window" to the Window menu
		[MenuItem(EditorHelper.EditorToolsPath + "Pathfinding Debug")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			AIAgentDebug window = (AIAgentDebug)GetWindow(typeof(PathfindingDebugWindow));
			window.titleContent = new GUIContent("Pathfinding Debug");
			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Label("Pathfinding Debug", EditorStyles.boldLabel);

			var controller = PathfindingManager.Instance;
			if (controller == null)
			{
				GUILayout.Label("Game not running");
				return;
			}
			GUILayout.Label($"New Jobs: {controller.NewJobs}");
			GUILayout.Label($"Current Jobs: {controller.CurrentJobs}");
		}
	}
}