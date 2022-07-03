using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endciv
{
	[Serializable]
	public class GameInputManagerSaveData : ISaveable
	{
		public ActionInput[] actionInputs;

		public ISaveable CollectData()
		{
			if (Main.Instance.gameInputManager == null)
				return null;
			return Main.Instance.gameInputManager.CollectData();
		}
	}

	public class GameInputManager : MonoBehaviour, ISaveable, ILoadable<GameInputManagerSaveData>
	{
		[SerializeField] EventSystem currentEventSystem;
		[SerializeField] InputSettings defaultInputSettings;
		GameManager GameManager;
		TerrainManager TerrainManager;

		GeneralSettingsManager generalSettingsManager;

		public CameraController CameraController { get; private set; }
		public Camera MainCamera { get; private set; }

		public bool MouseOnTerrain { get; private set; }

		public bool EnableEdgeScroll { get { return generalSettingsManager.enableEdgeScroll; } }
		public float inputSensitivity { get { return generalSettingsManager.mouseSensitivity; } }
		public float camMoveSpeedX { get { return generalSettingsManager.camMoveInvertX ? -generalSettingsManager.mouseSensitivity : generalSettingsManager.mouseSensitivity; } }
		public float camMoveSpeedY { get { return generalSettingsManager.camMoveInvertY ? -generalSettingsManager.mouseSensitivity : generalSettingsManager.mouseSensitivity; } }
		public float inputRotationSensitivity { get { return generalSettingsManager.mouseRotationSensitivity; } }
		public float camRotateSpeedX { get { return generalSettingsManager.mouseSensitivity * (generalSettingsManager.camMoveInvertX ? generalSettingsManager.mouseRotationSensitivity : -generalSettingsManager.mouseRotationSensitivity); } }
		public float camRotateSpeedY { get { return generalSettingsManager.mouseSensitivity * (generalSettingsManager.camMoveInvertY ? -generalSettingsManager.mouseRotationSensitivity : generalSettingsManager.mouseRotationSensitivity); } }

		public bool enableTouchControll { get; private set; }
		public bool enableMouseControll { get; private set; }

		public Pointer Pointer1 { get; private set; }
		public Pointer Pointer2 { get; private set; }
		public Pointer Pointer3 { get; private set; }

		private Vector3 currentTerrainPosition;

		public bool hasActiveInputElement;
		public bool gameInputShouldBeAllowed;
		public bool IsGameInputAllowed { get { return gameInputShouldBeAllowed; } }    //WHy that? && !hasActiveInputElement
		public bool restrictCameraRotation;

		public Vector2 scaledScreenBounds;

		public Action OnCanvasScaleChanged;
		public float UIScale = 1;
		public float UIScaleInv = 1;

		private ActionInput[] actionInputs;
		private Dictionary<string, KeyCode[]> actionReference;

		public class Pointer
		{
			public bool enabled;
			/// <summary>
			/// Whether this is a touch point or mouse input
			/// </summary>
			public bool isTouch;

			public bool isDragging;
			public bool isActive;
			public bool isOnTerrain;
			public Vector3 TerrainPosition;
			public Vector2i GridIndex;
			public Vector3 TerrainPositionBase;
			public Vector3 TerrainPositionDelta;
			public Vector2i GridIndexBase;
			public Vector3 mousePos;
			public Vector3 baseMousePos;
			public bool Released;
			internal bool releasedDrag;
		}

		bool IsReady;

		public void Awake()
		{
			Pointer1 = new Pointer();
			Pointer2 = new Pointer();
			Pointer3 = new Pointer();

			if (Main.Instance != null)
			{
				Main.Instance.generalSettingsManager.OnCanvasScaleChanged -= OnResizeScreen;
				Main.Instance.generalSettingsManager.OnCanvasScaleChanged += OnResizeScreen;
			}
		}

		public void SetupGameMode(GameManager gameManager, TerrainManager terrainManager, CameraController cameraController)
		{
			if (currentEventSystem == null)
				currentEventSystem = FindObjectOfType<EventSystem>();
			if (currentEventSystem == null)
				Debug.LogError("No EventSystem found!");

			GameManager = gameManager;
			if (Main.Instance != null)
				generalSettingsManager = Main.Instance.generalSettingsManager;
			TerrainManager = terrainManager;
			CameraController = cameraController;
			MainCamera = cameraController.Camera;
			IsReady = true;
			gameInputShouldBeAllowed = true;
			if (actionInputs == null && defaultInputSettings != null)
			{
				actionInputs = defaultInputSettings.actions;
				SetupActionReference();
			}

		}

		public void SetupActionReference()
		{
			if (actionInputs == null)
				return;
			actionReference = new Dictionary<string, KeyCode[]>(actionInputs.Length);
			foreach (var action in actionInputs)
			{
				if (actionReference.ContainsKey(action.actionName))
					continue;
				if (string.IsNullOrEmpty(action.actionName))
					continue;
				if (action.keys == null || action.keys.Length <= 0)
					continue;
				actionReference.Add(action.actionName, action.keys);
			}
		}

		public void Stop()
		{
			IsReady = false;
		}

		void OnResizeScreen(float scale)
		{
			UIScale = scale;
			UIScaleInv = 1f / UIScale;
			scaledScreenBounds = new Vector2(Screen.width, Screen.height) * UIScaleInv;
			OnCanvasScaleChanged?.Invoke();
		}


		public bool IsMouseOverUI()
		{
			return currentEventSystem.IsPointerOverGameObject();
		}

		private void Update()
		{
			if (!IsReady) return;

			//Check active UI element and see if it should block game controlls
			GameObject obj = currentEventSystem.currentSelectedGameObject;
			if (obj != null)
			{
				var inputField = obj.GetComponent<InputField>();
				if (inputField != null && inputField.isFocused) hasActiveInputElement = true;
				else hasActiveInputElement = false;
			}
			else hasActiveInputElement = false;

			CalculateRaycasts();
		}

		void CalculateRaycasts()
		{
			Ray ray; RaycastHit hit;
			const int distance = 500;

			//Terrain
			ray = MainCamera.ScreenPointToRay(Input.mousePosition);

			MouseOnTerrain = Physics.Raycast(ray, out hit, distance, TerrainManager.TerrainLayer);
			if (MouseOnTerrain)
			{
				currentTerrainPosition = hit.point;
				Pointer1.isOnTerrain = true;
				Pointer1.mousePos = Input.mousePosition;
				Pointer1.TerrainPositionDelta = currentTerrainPosition - Pointer1.TerrainPosition;
				Pointer1.TerrainPosition = currentTerrainPosition;
				Pointer1.GridIndex = new Vector2i(Mathf.FloorToInt(currentTerrainPosition.x * GridMapView.InvTileSize), Mathf.FloorToInt(currentTerrainPosition.z * GridMapView.InvTileSize));
			}
			else
				Pointer1.isOnTerrain = false;

			Pointer1.enabled = true;
			Pointer1.releasedDrag = false;

			if (Input.GetMouseButton(0))
			{
				Pointer1.isActive = true;
			}
			else
			{
				Pointer1.isActive = false;
			}

			if (!Pointer1.isDragging)
			{
				Pointer1.GridIndexBase = Pointer1.GridIndex;
				Pointer1.TerrainPositionBase = Pointer1.TerrainPosition;
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (!IsMouseOverUI())
					Pointer1.isDragging = true;
				Pointer1.releasedDrag = false;
				if (MouseOnTerrain)
				{
					Pointer1.TerrainPositionDelta = Vector3.zero;
					Pointer1.baseMousePos = Pointer1.mousePos;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				if (Pointer1.isDragging) Pointer1.releasedDrag = true;
				Pointer1.isDragging = false;
				Pointer2.enabled = false;
			}
		}

		public bool GetActionDown(string actionName)
		{
			if (actionReference == null)
				return false;
			if (!actionReference.ContainsKey(actionName))
				return false;
			foreach (var key in actionReference[actionName])
			{
				if (!Input.GetKeyDown(key))
					return false;
			}
			return true;
		}

		public bool GetActionUp(string actionName)
		{
			if (actionReference == null)
				return false;
			if (!actionReference.ContainsKey(actionName))
				return false;
			foreach (var key in actionReference[actionName])
			{
				if (!Input.GetKeyUp(key))
					return false;
			}
			return true;
		}

		public bool GetAction(string actionName)
		{
			if (actionReference == null)
				return false;
			if (!actionReference.ContainsKey(actionName))
				return false;
			foreach (var key in actionReference[actionName])
			{
				if (!Input.GetKey(key))
					return false;
			}
			return true;
		}

		public ISaveable CollectData()
		{
			var actionInputBundle = new GameInputManagerSaveData();
			actionInputBundle.actionInputs = actionInputs;
			return actionInputBundle;
		}

		public void ApplySaveData(GameInputManagerSaveData data)
		{
			if (data == null)
				return;
			actionInputs = data.actionInputs;
			SetupActionReference();
		}
	}
}