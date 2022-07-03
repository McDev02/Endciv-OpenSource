using UnityEngine;

namespace Endciv
{
	public class CartView : MonoBehaviour
	{
		[SerializeField] Transform cartTarget;
		[SerializeField] Transform[] rotatingWheels;
		[SerializeField] float wheelRadius = 1;
		[SerializeField] float wheelRadiusInv;
		public Vector3 rotationAxis = Vector3.right;

		Vector3 lastPosCart;
		Vector3[] lastPos;
		Vector2 lastForward;

		private void Awake()
		{
			wheelRadiusInv = 1f / wheelRadius;

			lastPos = new Vector3[rotatingWheels.Length];
			for (int i = 0; i < lastPos.Length; i++)
			{
				lastPos[i] = rotatingWheels[i].position;
			}
			lastForward = transform.forward.To2D();
			rotationAxis = rotationAxis.normalized;
			lastPosCart = cartTarget.position;
		}

		private void Update()
		{
			if (Time.deltaTime <= 0.0001f) return;

			for (int i = 0; i < rotatingWheels.Length; i++)
			{
				var trans = rotatingWheels[i];
				//diff as speed is wrong, we would need to use longtitudual speed of the wheel, but that isn't very noticable.
				var diff = (trans.position - lastPos[i]).magnitude;

				trans.Rotate(rotationAxis, Mathf.Rad2Deg * diff * wheelRadiusInv);

				lastPos[i] = trans.position;
			}

			transform.LookAt(lastPosCart, Vector3.up);
			lastPosCart = cartTarget.position;
		}
	}
}