using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class UI3DController : MonoBehaviour
	{
		[SerializeField] Canvas canvas;
		[SerializeField] UI3DFactory factory;
		private List<UI3DBase> currentElements = new List<UI3DBase>(32);

		GameInputManager gameInputManager;


		public void Run(GameInputManager gameInputManager, Camera camera)
		{
			this.gameInputManager = gameInputManager;
			canvas.worldCamera = camera;
		}

		public void UpdateElements()
		{
			var cam = gameInputManager.MainCamera.transform;
			for (int i = 0; i < currentElements.Count; i++)
			{
				UpdatePosition(currentElements[i], cam);
				currentElements[i].UpdateElement(cam.position);
			}
		}

		private void UpdatePosition(UI3DBase ui3DBase, Transform cam)
		{
			var trans = ui3DBase.transform;

			//var dist = Vector3.Distance(playerCamera.DroneCam.position, playerCamera.transform.position);
			//dist = Mathf.Clamp(dist, 1f, gameStetting.Gui3dMaxZoom);
			//dist *= gameStetting.Gui3dScale;

			if (ui3DBase.anchoredObject != null)
			{
				trans.position = ui3DBase.anchoredObject.position + new Vector3(0, ui3DBase.HeightOffset, 0);
			}
			else
			{
				trans.position = ui3DBase.anchoredPosition + new Vector3(0, ui3DBase.HeightOffset, 0);
			}

			Vector3 pos = trans.position + cam.forward;
			trans.LookAt(pos, cam.up);

			//trans.localScale = new Vector3(dist, dist, 1);
		}

		internal void RemoveElement(UI3DBase ui)
		{
			currentElements.Remove(ui);
		}

		internal void AddElement<T>(T ui) where T : UI3DBase
		{
			currentElements.Add(ui);
		}
	}
}