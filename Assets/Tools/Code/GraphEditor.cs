using UnityEngine;
using System.Collections.Generic;
namespace Endciv
{
	public class GraphEditor : MonoBehaviour
	{
		[SerializeField] LineRenderer lineRenderer;
		List<float> values = new List<float>();
		[SerializeField] RectTransform scale;

		public float ScaleX = 1;
		public float ScaleY = 1;


		public void SetValues(float[] values)
		{
			this.values.Clear();
			for (int i = 0; i < values.Length; i++)
			{
				this.values.Add(values[i]);
			}
			UpdateGraph();
		}

		void UpdateGraph()
		{
			var rect = scale.rect.size;
			Vector3[] positions = new Vector3[values.Count];
			for (int i = 0; i < values.Count; i++)
			{
				positions[i] = new Vector3(i * rect.x * ScaleX, values[i] * rect.y * ScaleY, 0);
			}
			lineRenderer.positionCount = positions.Length;
			lineRenderer.SetPositions(positions);
		}
	}
}