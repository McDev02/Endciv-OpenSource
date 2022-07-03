using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
	public class WeatherEditor : EditorWindow
	{
		WeatherSystem weatherSystem;
		float windAngle;
		// Add menu named "My Window" to the Window menu
		[MenuItem(EditorHelper.EditorToolsPath + "Weather Editor")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			WeatherEditor window = (WeatherEditor)EditorWindow.GetWindow(typeof(WeatherEditor));
			window.titleContent = new GUIContent("Weather Editor");
			window.Show();
		}

		void OnGUI()
		{
			if (Main.Instance == null || Main.Instance.GameManager == null || !Main.Instance.GameManager.IsRunning)
			{
				weatherSystem = null;
				GUILayout.Label("Game not running");
				return;
			}
			weatherSystem = Main.Instance.GameManager.SystemsManager.WeatherSystem;
			GuiMain();
		}

		void GuiMain()
		{
			var w = weatherSystem;

			GUILayout.Label("Update Simulation");
			w.updateSymulation = EditorGUILayout.Toggle(w.updateSymulation);

			GUILayout.Label("Cloudiness");
			w.Cloudiness = GUILayout.HorizontalSlider(w.Cloudiness, 0, 1);
			GUILayout.Label("Rainfall");
			w.Rainfall = GUILayout.HorizontalSlider(w.Rainfall, 0, 1);
			GUILayout.Label("Snowfall");
			w.Snowfall = GUILayout.HorizontalSlider(w.Snowfall, 0, 1);
			GUILayout.Label("Wetness");
			w.Wetness = GUILayout.HorizontalSlider(w.Wetness, 0, 1);
			GUILayout.Label("Snow");
			w.Snowlevel = GUILayout.HorizontalSlider(w.Snowlevel, 0, 1);
			GUILayout.Space(8);
			GUILayout.Label("Wind Power");
			w.WindPower = GUILayout.HorizontalSlider(w.WindPower, 0, 1);
			GUILayout.Label("Wind Turbulance");
			w.WindTurbulance = GUILayout.HorizontalSlider(w.WindTurbulance, 0, 1);
			GUILayout.Label($"Wind Direction: {w.WindDirection.ToString()}");
		}
	}
}