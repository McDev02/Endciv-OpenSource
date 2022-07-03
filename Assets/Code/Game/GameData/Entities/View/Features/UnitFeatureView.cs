using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Endciv
{
	public enum EAnimationState
	{
		None,
		Idle,
		Walking,
		Working,
		HammeringStanding,
		HammeringKneeling,
		Sleeping,
		CarryWalking,
		CarryIdle,
		PutDown,
		PickUp
	}

	public class UnitFeatureView : FeatureView<UnitFeature>
	{
		GridMap gridMap;

		public SkinnedMeshRenderer SkinnedMesh;
		public Animator Animator;
		private Action<EAnimationState> stateEndCallback;

		[SerializeField] LineRenderer LineRenderer;
		Animator animator;
		private EAnimationState currentState = EAnimationState.Idle;
		const float heightOffset = 0.05f;
		Vector3 offset = new Vector3(0, heightOffset, 0);
		private List<string> parameterList;

		private const float HIDDEN_ANIMATION_TIME = 1f;

		private ModelFactory modelFactory;
		private Dictionary<string, GameObject> objectPool;
		[SerializeField] Transform objectHolder;

		private Dictionary<EAnimationState, Action> stateActionTable;

		public override void Setup(FeatureBase feature)
		{
			base.Setup(feature);
			float sizeVariation = UnitSystem.HumanSizeFactor;

			switch (Feature.Age)
			{
				case ELivingBeingAge.Child:
					if (Feature.Gender == ELivingBeingGender.Male)
						sizeVariation *= Feature.StaticData.childMaleSizes.GetRandom();
					else
						sizeVariation *= Feature.StaticData.childFemaleSizes.GetRandom();
					break;

				case ELivingBeingAge.Adult:
					if (Feature.Gender == ELivingBeingGender.Male)
						sizeVariation *= Feature.StaticData.adultMaleSizes.GetRandom();
					else
						sizeVariation *= Feature.StaticData.adultFemaleSizes.GetRandom();
					break;
			}
			transform.localScale = Vector3.one * sizeVariation;

			modelFactory = Main.Instance.GameManager.Factories.ModelFactory;
			objectPool = new Dictionary<string, GameObject>();
			SetupStateActionTable();

			if (Feature.IsCarrying)
				currentState = EAnimationState.CarryIdle;
			else
				currentState = EAnimationState.Idle;
			stateActionTable[currentState].Invoke();
			gridMap = Main.Instance.GameManager.GridMap;
			animator = GetComponent<Animator>();
			if (animator != null)
			{
				parameterList = new List<string>();
				foreach (var parameter in animator.parameters)
				{
					parameterList.Add(parameter.name);
				}
			}
			ShowHide(true);
		}

		private void SetupStateActionTable()
		{
			stateActionTable = new Dictionary<EAnimationState, Action>();

			stateActionTable.Add(EAnimationState.None, HideCurrentObject);
			stateActionTable.Add(EAnimationState.Idle, HideCurrentObject);
			stateActionTable.Add(EAnimationState.Walking, HideCurrentObject);
			stateActionTable.Add(EAnimationState.Sleeping, HideCurrentObject);
			stateActionTable.Add(EAnimationState.Working, HideCurrentObject);

			stateActionTable.Add(EAnimationState.CarryIdle, ShowInventoryObject);
			stateActionTable.Add(EAnimationState.CarryWalking, ShowInventoryObject);
			stateActionTable.Add(EAnimationState.PutDown, ShowInventoryObject);
			stateActionTable.Add(EAnimationState.PickUp, ShowInventoryObject);

			//Add show hammer option for the two hammering states
			//once we get hammer views
		}

		public void ShowObject(string objectName)
		{
			if (objectHolder == null)
			{
				return;
			}
			if (objectHolder.childCount > 0)
			{
				if (objectHolder.GetChild(0).gameObject.name == objectName)
					return;
				HideCurrentObject();
			}
			var obj = GetObject(objectName);
			obj.transform.parent = objectHolder;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localEulerAngles = Vector3.zero;
		}

		public void HideCurrentObject()
		{
			if (objectHolder == null)
			{
				return;
			}
			if (objectHolder.childCount <= 0)
			{
				return;
			}
			var obj = objectHolder.GetChild(0).gameObject;
			obj.transform.parent = transform;
			obj.SetActive(false);
			objectPool.Add(obj.name, obj);
		}

		private GameObject GetObject(string objectName)
		{
			GameObject obj = null;
			if (objectPool.ContainsKey(objectName))
			{
				obj = objectPool[objectName];
				objectPool.Remove(objectName);
				obj.gameObject.SetActive(true);
			}
			else
			{
				obj = modelFactory.GetModelObject(objectName);
			}
			return obj;
		}

		public bool ShowWaypoint;

		public void UpdateWaypoint()
		{
			if (!ShowWaypoint)
			{
				HideWaypointPath();
				return;
			}

			LineRenderer.enabled = true;
			var destination = Feature.Entity.GetFeature<GridAgentFeature>().Destination;
			Vector3[] positions;
			Vector3 pos;
			var type = destination.Type;

#if !USE_FLOWFIELDS
			type = Location.EDestinationType.Waypoint;
#endif
			switch (type)
			{
				case Location.EDestinationType.Position:
					pos = gridMap.View.GetTileWorldPosition(destination.Index).To3D(heightOffset);
					positions = new Vector3[] { transform.position + offset, pos };
					break;
				case Location.EDestinationType.Structure:
					pos = destination.Structure.GetFeature<EntityFeature>().View.transform.position + offset;
					positions = new Vector3[] { transform.position + offset, pos };
					break;
				case Location.EDestinationType.Multiple:
					//Not fully supported, should draw lines to every target?
					pos = gridMap.View.GetTileWorldPosition(destination.Index).To3D(heightOffset);
					positions = new Vector3[] { transform.position + offset, pos };
					break;
				case Location.EDestinationType.Waypoint:
					int k = 1;
					positions = new Vector3[(destination.Positions.Length - destination.currentPositionID) + 1];
					for (int i = destination.currentPositionID; i < destination.Positions.Length; i++)
					{
						positions[k++] = destination.Positions[i].To3D(heightOffset);
					}
					positions[0] = transform.position + offset;
					break;
				default:
					positions = new Vector3[0];
					break;
			}
			LineRenderer.positionCount = positions.Length;
			LineRenderer.SetPositions(positions);
		}

		public void UpdateWaypointPosition()
		{
			if (!ShowWaypoint)
			{
				HideWaypointPath();
				return;
			}

			LineRenderer.enabled = true;
			LineRenderer.SetPosition(0, Feature.Entity.GetFeature<EntityFeature>().View.transform.position + offset);
		}

		internal void HideWaypointPath()
		{
			if (LineRenderer != null)
				LineRenderer.enabled = false;
		}

		public void SwitchAnimationState(EAnimationState state, float animatorSpeed = 1f, System.Action<EAnimationState> callback = null)
		{
			if (!IsVissible)
			{
				StartCoroutine(FakeAnimationHidden(state, callback));
				return;
			}
			if (animator == null)
			{
				//Debug.LogError("No animator attached to unit " + name);
				return;
			}
			animator.speed = animatorSpeed;
			if (state == currentState)
			{
				if (callback != null)
					callback.Invoke(state);
				return;
			}
			if (!parameterList.Contains(state.ToString()))
			{
				Debug.Log("State " + state.ToString() + " not found for unit " + name);
				return;
			}
			ResetAnimatorTriggers();
			animator.SetTrigger(state.ToString());
			currentState = state;
			stateEndCallback += callback;
			if (stateActionTable.ContainsKey(currentState))
				stateActionTable[currentState].Invoke();
			else
				stateActionTable[EAnimationState.None].Invoke();
		}

		private IEnumerator FakeAnimationHidden(EAnimationState state, System.Action<EAnimationState> callback = null)
		{
			currentState = state;
			if (callback != null)
			{
				yield return new WaitForSeconds(HIDDEN_ANIMATION_TIME);
				callback.Invoke(currentState);
			}
		}

		public void ExecuteStateCallback(int state)
		{
			if (stateEndCallback != null)
			{
				stateEndCallback.Invoke((EAnimationState)state);
			}
		}

		public void UnregisterCallback(System.Action<EAnimationState> callback)
		{
			stateEndCallback -= callback;
		}

		private void ResetAnimatorTriggers()
		{
			foreach (var parameter in animator.parameters)
			{
				if (parameter.type == AnimatorControllerParameterType.Trigger)
					animator.ResetTrigger(parameter.name);
			}
		}

		public void ShowInventoryObject()
		{
			ShowObject("transportBox");
		}

		public override void UpdateView()
		{

		}
	}
}