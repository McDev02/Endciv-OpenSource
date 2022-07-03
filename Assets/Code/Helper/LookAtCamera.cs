using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class LookAtCamera : MonoBehaviour
	{
		[SerializeField] bool _2d;
		Transform cam;

		private void Start()
		{
			cam = Main.Instance.GameManager.CameraController.Camera.transform;
		}
		// Update is called once per frame
		void Update()
		{
			var diff = (transform.position - cam.position).To2D();
			transform.LookAt(transform.position + Vector3.down, cam.up.To2D().To3D());
		}
	}
}