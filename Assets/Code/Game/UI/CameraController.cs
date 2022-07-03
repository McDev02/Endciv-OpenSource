using System;
using UnityEngine;
namespace Endciv
{
	public class CameraController : MonoBehaviour
	{
		public Transform Gear;
		public Camera Camera;
		public CameraPreset Model;
		GameInputManager inputManager;
		public CameraPostProcessingSettings postProcessingSettings;

		public bool IsRunning { get; private set; }

		internal void SetBounds(Rect bounds)
		{
			Model.PosX.Min = bounds.xMin;
			Model.PosX.Max = bounds.xMax;
			Model.PosY.Min = bounds.yMin;
			Model.PosY.Max = bounds.yMax;
		}

		private Vector3 baseMousePos;
		private Vector3 oldMousePos;
		private Vector3 camPosition;
		private Vector3 rootPosition;
		private Quaternion tarRootRotation;

		private Quaternion curRootRotation;
		private Quaternion gearRotation;

		public int scrollBoarderThreshold = 5;
		public LayerMask collisionMask;

		Vector2 lastTouchAveragePosition;
		float lastTouchAverageDistance;
		int lastTouchCount;

		public void SetCamera(Camera cam)
		{
			Camera = cam;
			if (cam == null) return;

			cam.transform.SetParent(Gear);
			cam.transform.localPosition = camPosition;
			cam.transform.localRotation = Quaternion.identity;
		}

		public void Run()
		{
			IsRunning = true;
		}

		public void Setup(GameInputManager gameInputManager)
		{
			this.inputManager = gameInputManager;
			if (Gear == null)
				Debug.LogError("Gear Transform is not defined.");

			Model.PosX.Setup();
			Model.PosY.Setup();
			Model.Pitch.Setup();
			Model.Yaw.Setup();
			Model.Zoom.Setup();

			Model.PosX.SetRelative(0.5f, true);
			Model.PosY.SetRelative(0.5f, true);
			Model.Pitch.SetRelative(0.5f, true);
			Model.Yaw.SetRelative(0, true);
			Model.Zoom.SetRelative(0.5f, true);

			curRootRotation = tarRootRotation = Quaternion.AngleAxis(Model.Yaw.Target, Vector3.up);

			SetPosition(new Vector2(Model.PosX.Current, Model.PosY.Current), true);

			if (Camera != null)
				SetCamera(Camera);
		}

		void LateUpdate()
		{
			if (!IsRunning) return;

			var deltaTime = Main.unscaledDeltaTimeSafe;

			Vector3 pan = Vector3.zero;
			if (inputManager.IsGameInputAllowed)
			{
				var pan2D = HandleInput(deltaTime);
				pan = transform.localToWorldMatrix.MultiplyVector(pan2D.To3D()) * deltaTime;
			}

			Model.PosX.AddTarget(pan.x);
			Model.PosY.AddTarget(pan.z);

			Model.Pitch.Min = Mathf.Lerp(Model.minPitchByDistance.min, Model.minPitchByDistance.max, Model.Zoom.CurrentRelative);
			tarRootRotation = Quaternion.AngleAxis(Model.Yaw.Target, Vector3.up);

			//InterpolateValues
			Model.PosX.ApplyCurrent(deltaTime);
			Model.PosY.ApplyCurrent(deltaTime);
			Model.Pitch.ApplyCurrent(deltaTime);
			Model.Zoom.ApplyCurrent(deltaTime);
			curRootRotation = Quaternion.Lerp(curRootRotation, tarRootRotation, Model.Yaw.Adaption * deltaTime);

			//Apply values
			rootPosition.x = Model.PosX.Current;
			rootPosition.z = Model.PosY.Current;

			gearRotation = Quaternion.AngleAxis(Model.Pitch.Current, Vector3.right);
			camPosition.z = -Model.Zoom.Current;

			transform.localPosition = rootPosition;
			transform.localRotation = curRootRotation;
			Gear.localRotation = gearRotation;

			if (Camera != null)
				Camera.transform.localPosition = camPosition;

			oldMousePos = Input.mousePosition;
		}

		Vector2 HandleInput(float deltaTime)
		{
			Vector2 pan = Vector2.zero;

			var mousePos = Input.mousePosition;
			var mouseDiff = (oldMousePos - mousePos);

			if (Input.GetMouseButtonDown(1))
				baseMousePos = mousePos;

			//Rotation
			if (Input.GetMouseButton(2))
			{
				var t = deltaTime * 0.05f;
				Model.Yaw.AddTarget(mouseDiff.x * t * inputManager.camRotateSpeedX, false);
				Model.Pitch.AddTarget(mouseDiff.y * t * inputManager.camRotateSpeedY);
			}
			//Movement
			else if (Input.GetMouseButton(1))
			{
				var t = deltaTime * 0.2f;
				//diff.x = Mathf.Sign(diff.x) * Mathf.Clamp01(Mathf.Abs(diff.x));
				//diff.y = Mathf.Sign(diff.y) * Mathf.Clamp01(Mathf.Abs(diff.y));
				pan.x = mouseDiff.x * t * inputManager.camMoveSpeedX;
				pan.y = mouseDiff.y * t * inputManager.camMoveSpeedY;
			}

			pan = HandleKeyboardInput(pan, deltaTime);
			pan = HandleTouchInput(pan, deltaTime);

			//Handle Mouse Scrolling
#if !UNITY_EDITOR
			if (inputManager.EnableEdgeScroll)
			{
				if (Mathf.Abs(mousePos.x) <= scrollBoarderThreshold)
					pan.x -= 1f / 10f;
				if (Mathf.Abs(mousePos.x - Screen.width) <= scrollBoarderThreshold)
					pan.x += 1f / 10f;
				if (Mathf.Abs(mousePos.y) <= scrollBoarderThreshold)
					pan.y -= 1f / 10f;
				if (Mathf.Abs(mousePos.y - Screen.height) <= scrollBoarderThreshold)
					pan.y += 1f / 10f;
			}
#endif
			pan.x = Mathf.Clamp(pan.x, -1, 1);
			pan.y = Mathf.Clamp(pan.y, -1, 1);
			pan *= 1 + 2 * Model.Zoom.TargetRelative;

			//Sprint
			if (Input.GetKey(KeyCode.LeftShift))
				pan *= Model.sprintPanFactor;

			if (!inputManager.IsMouseOverUI())
				Model.Zoom.AddTarget(Input.GetAxisRaw("Mouse ScrollWheel") * -100 * deltaTime);


			return pan;
		}

		Vector2 HandleKeyboardInput(Vector2 pan, float deltaTime)
		{
			//Handle Keyboard Input
			if (Input.GetKey(KeyCode.W))
				pan.y += 1f / 10f;
			if (Input.GetKey(KeyCode.S))
				pan.y -= 1f / 10f;
			if (Input.GetKey(KeyCode.A))
				pan.x -= 1f / 10f;
			if (Input.GetKey(KeyCode.D))
				pan.x += 1f / 10f;

			//Zoom
			if (Input.GetKey(KeyCode.PageUp))
				Model.Zoom.AddTarget(-2 * deltaTime);
			if (Input.GetKey(KeyCode.PageDown))
				Model.Zoom.AddTarget(2 * deltaTime);

			//Rotation
			if (!inputManager.restrictCameraRotation)
			{
				if (Input.GetKey(KeyCode.LeftArrow))
					Model.Yaw.AddTarget(deltaTime * 0.35f, false);
				if (Input.GetKey(KeyCode.RightArrow))
					Model.Yaw.AddTarget(-deltaTime * 0.35f, false);

				if (Input.GetKey(KeyCode.UpArrow))
					Model.Pitch.AddTarget(deltaTime * 0.35f);
				if (Input.GetKey(KeyCode.DownArrow))
					Model.Pitch.AddTarget(-deltaTime * 0.35f);
			}
			return pan;
		}
		Vector2 HandleTouchInput(Vector2 pan, float deltaTime)
		{
			Vector2 averagePos;
			float averageDistance;
			if (Input.touchCount == 2)
			{
				var p1 = Input.GetTouch(0);
				var p2 = Input.GetTouch(1);

				averagePos = (p1.position + p2.position) / 2f;
				averageDistance = (p2.position - p1.position).magnitude;
			}
			else if (Input.touchCount >= 3)
			{
				var p1 = Input.GetTouch(0);
				var p2 = Input.GetTouch(1);
				var p3 = Input.GetTouch(2);

				averagePos = (p1.position + p2.position + p3.position) / 3f;
				averageDistance = ((p2.position - p1.position).magnitude + (p2.position - p3.position).magnitude) / 2f;
			}
			else
			{
				averagePos = Vector2.zero;
				averageDistance = 1;
			}

			if (Input.touchCount != lastTouchCount)
			{
				lastTouchAveragePosition = averagePos;
				lastTouchAverageDistance = averageDistance;
				Debug.Log("new touch: " + Input.touchCount.ToString());
			}


			//Pan and Zoom
			if (Input.touchCount >= 2)
			{
				var diff = (averagePos - lastTouchAveragePosition) * 100;

				////Rotate 
				//if (Input.touchCount >= 3)
				//{
				//	Model.Yaw.AddTarget(diff.x * deltaTime * inputManager.camSpeedX, false);
				//	Model.Pitch.AddTarget(diff.y * deltaTime * inputManager.camSpeedY);
				//}
				////Pan
				//else
				//{
				//	pan += diff * deltaTime;
				//}
				//Zoom
				Model.Zoom.AddTarget((lastTouchAverageDistance - averageDistance) * 0.2f * deltaTime);

			}

			lastTouchAveragePosition = averagePos;
			lastTouchAverageDistance = averageDistance;
			lastTouchCount = Input.touchCount;
			return pan;
		}

		public void SetPosition(Vector2 pos, bool force = false)
		{
			Model.PosX.Target = pos.x;
			Model.PosY.Target = pos.y;
			if (force)
			{
				Model.PosX.Current = Model.PosX.Target;
				Model.PosY.Current = Model.PosY.Target;
			}
		}
		/// <summary>
		/// Does not yet follow a unit but only sets position once
		/// </summary>
		internal void FollowUnit(BaseEntity unit)
		{
			SetPosition(unit.GetFeature<EntityFeature>().View.transform.position.To2D());
		}

		/// <summary>
		/// Does not yet follow a unit but only sets position once
		/// </summary>
		internal void FollowEntity(BaseEntity entity)
		{
			SetPosition(entity.GetFeature<EntityFeature>().View.transform.position.To2D());
		}
	}
}