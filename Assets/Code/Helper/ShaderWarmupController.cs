using UnityEngine;
using System.Collections;

public class ShaderWarmupController : MonoBehaviour
{
	[SerializeField] Material endcivStandardMaterial;
	[SerializeField] Material endcivTerrainMaterial;

	// Use this for initialization
	IEnumerator Start()
	{
		yield return null;
		yield return null;
		endcivStandardMaterial.EnableKeyword("CONSTRUCTION");
		endcivTerrainMaterial.EnableKeyword("BLEND_ON");
		yield return null;
		yield return null;
		Destroy(gameObject);
	}
}
