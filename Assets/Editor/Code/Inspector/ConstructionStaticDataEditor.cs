using UnityEditor;

namespace Endciv.Editor
{
	[CustomEditor(typeof(ConstructionStaticData))]
	public class ConstructionStaticDataEditor : UnityEditor.Editor
	{
		private SimpleEntityFactory factory;

		private void OnEnable()
		{
			factory = new SimpleEntityFactory();
			factory.Setup("StaticData", null);
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			var myTarget = (ConstructionStaticData)target;


			float cost = 0;
			float mass = 0;
			int i = 0;
			if (myTarget.Cost != null)
			{
				foreach (var item in myTarget.Cost)
				{
					var data = factory.GetStaticData<ItemFeatureStaticData>(item.ResourceID);
					if (data != null)
					{
						if (item.Amount < 0)
							EditorGUILayout.HelpBox($"Item #{i}: The amount ({item.Amount}) is negative.", MessageType.Warning);
						else
						{
							cost += item.Amount * data.Value;
							mass += item.Amount * data.Mass;
						}
					}
					else
					{
						EditorGUILayout.HelpBox($"Item #{i} is undefined!", MessageType.Warning);
					}

					i++;
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Material Value: {cost.ToString("0.#")}", EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"Mass: {cost.ToString("0.#")}", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
		}
	}
}