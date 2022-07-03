using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class ConstructionTest : MonoBehaviour
{
	Renderer renderer;
	[Range(0,1)]
	public float progress;

	private void Start()
	{
		renderer = GetComponent<Renderer>();
		if (renderer == null)
			enabled = false;
	}

	void Update()
	{
		var mat = renderer.sharedMaterial;
		mat.EnableKeyword("CONSTRUCTION");
		mat.SetFloat("_ConstructionProgress", 1f - progress);
	}
}