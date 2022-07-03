using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Rotates a Transform based on wind power
	/// </summary>
	public class WindRotatorView : MonoBehaviour
	{
		[SerializeField] float rotationSpeed = 5;
		[SerializeField] Transform rotator;
		[SerializeField] Vector3 axis = Vector3.up;
		WeatherSystem weatherSystem;

		void Start()
		{
			axis = Vector3.up;
			weatherSystem = Main.Instance.GameManager.SystemsManager.WeatherSystem;
		}		

		void Update()
		{
			var speed = rotationSpeed * weatherSystem.WindPower * Main.deltaTime;
			rotator.Rotate(axis * speed);
		}
	}
}