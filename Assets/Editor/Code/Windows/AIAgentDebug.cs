using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
	public class AIAgentDebug : EditorWindow
	{
		GameObject CurrentSelection;
		AIAgentFeatureBase Agent;

		// Add menu named "My Window" to the Window menu
		[MenuItem(EditorHelper.EditorToolsPath + "AI Agent Debug")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			AIAgentDebug window = (AIAgentDebug)GetWindow(typeof(AIAgentDebug));
			window.titleContent = new GUIContent("AI Agent Debug");
			window.Show();
		}

		void OnGUI()
		{
			CurrentSelection = Selection.activeGameObject;
			GUILayout.Label("AI Agent Debug", EditorStyles.boldLabel);
			Agent = null;
			if (CurrentSelection != null)
			{
				var view = CurrentSelection.GetComponent<EntityFeatureView>();

				if (view != null && view.Feature != null)
				{
					if (view.Feature.Entity.HasFeature<CitizenAIAgentFeature>())
						Agent = view.Feature.Entity.GetFeature<CitizenAIAgentFeature>();
				}
			}
			if (Agent == null)
			{
				GUILayout.Label("No AI Agent selected");
				return;
			}

			GUILayout.Space(5);
			GuiMain();
			Repaint();
		}

		void GuiMain()
		{
			var task = Agent.CurrentTask;
			if (task == null) { GUILayout.Label("CurrentTask: Has no task"); return; }
			else GUILayout.Label("CurrentTask: " + task.GetType().Name);

			GUILayout.Label("Sub State: " + task.CurrentSubState);
			var action = task.CurrentAction;
			if (action == null) GUILayout.Label("No Action");
			else
			{
				GUILayout.Label("CurrentAction: " + action.GetType().Name + " State: " + action.Status.ToString());
				task.CurrentAction.DrawUIDetails();
			}

			GUILayout.Space(5);
			GUILayout.Label("Task States");
			foreach (var state in task.StateTree.States)
			{
				GUILayout.Label(state.Key + ": " + state.Value.Status.ToString());
			}
		}

		void OnSelectionChange()
		{
			Repaint();
		}
	}
}