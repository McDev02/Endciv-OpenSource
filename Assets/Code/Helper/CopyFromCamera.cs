using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CopyFromCamera : MonoBehaviour
{
	[SerializeField] Camera reference;
	Camera cam;
	// Use this for initialization
	void Awake()
	{
		cam = GetComponent<Camera>();
		if (reference == null || cam == null)
		{
			enabled = false;
			return;
		}

		cam.nearClipPlane = reference.nearClipPlane;
		cam.farClipPlane = reference.farClipPlane;
		if (cam.enabled)
		{
			cam.enabled = false;
			cam.enabled = true;
		}
	}

	// Update is called once per frame
	void LateUpdate()
	{
		cam.fieldOfView = reference.fieldOfView;
	}
}