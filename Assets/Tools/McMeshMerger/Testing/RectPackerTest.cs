using UnityEngine;
using System.Collections.Generic;

namespace McMeshMerger
{
	public class RectPackerTest : MonoBehaviour
	{
		[SerializeField] MeshRenderer texturePrefab;
		List<MeshRenderer> rectObjects;
		RectPacker rectPacker;

		int texWidth;
		int texHeight;

		void UpdateRectangleObjecs(List<MaterialData> materials)
		{
			int count = Mathf.Min(materials.Count, rectObjects.Count);
			for (int i = 0; i < count; i++)
			{
				var rect = materials[i].rect;
				rectObjects[i].transform.localScale = new Vector3(rect.width, 1, rect.height);
				rectObjects[i].transform.localPosition = new Vector3(rect.min.x, 0, rect.min.y);
			}
		}

		public void Recalculate(List<MaterialData> materials, int width, int height)
		{
			texWidth = width;
			texHeight = height;
			var max = Mathf.Max(texWidth, texHeight);
			transform.localScale = max > 0 ? (10f / max) * Vector3.one : Vector3.one;

			if (rectObjects == null)
				rectObjects = new List<MeshRenderer>();

			var rends = GetComponentsInChildren<MeshRenderer>();
			rectObjects.AddRange(rends);

			for (int i = 0; i < materials.Count; i++)
			{
				var mat = materials[i];
				if (rectObjects.Count <= i)
				{
					var rectObj = Instantiate(texturePrefab, transform);
					rectObj.name = $"Texture #{i}";
					rectObjects.Add(rectObj);									}
				//Apply material
				rectObjects[i].sharedMaterial = mat.material;
				rectObjects[i].gameObject.SetActive(true);
			}
			for (int i = materials.Count; i < rectObjects.Count; i++)
			{
				rectObjects[i].gameObject.SetActive(false);
			}

			if (rectPacker == null)
				rectPacker = new RectPacker();
			rectPacker.PackMaterials(width, height, materials);
			UpdateRectangleObjecs(materials);
		}

		void OnDrawGizmos()
		{
			var matx = transform.localToWorldMatrix;
			var a = matx.MultiplyPoint(new Vector3(0, 0, 0));
			var b = matx.MultiplyPoint(new Vector3(texWidth, 0, 0));
			var c = matx.MultiplyPoint(new Vector3(texWidth, 0, texHeight));
			var d = matx.MultiplyPoint(new Vector3(0, 0, texHeight));

			Gizmos.DrawLine(a, b);
			Gizmos.DrawLine(b, c);
			Gizmos.DrawLine(c, d);
			Gizmos.DrawLine(d, a);
		}
	}
}