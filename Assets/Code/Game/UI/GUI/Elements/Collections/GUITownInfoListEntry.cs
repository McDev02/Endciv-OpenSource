using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Endciv
{
	public class GUITownInfoListEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image icon;
		public Image background;
		public Text amount;
		public Sprite hoverSprite;
		public EInfobarCategory category;

		private List<BaseEntity> entities;

		private int index = -1;
		[NonSerialized] public InfobarPanel controller;

		public void UpdateEntities(List<BaseEntity> entities)
		{
			this.entities = entities;
			amount.text = entities.Count.ToString();
			index = -1;
		}

		public void Reset()
		{
			entities = null;
			index = -1;
			amount.text = string.Empty;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			index++;
			if (index >= entities.Count)
			{
				index = 0;
			}
			Main.Instance.GameManager.CameraController.FollowEntity(entities[index]);
			Main.Instance.GameManager.UserToolSystem.SelectionTool.SelectEntity(entities[index]);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			background.overrideSprite = hoverSprite;
			controller.ShowInfo(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			background.overrideSprite = null;
			controller.HideInfo(this);
		}

		private void OnDestroy()
		{
			controller.HideInfo(this);
		}

		internal void Setup(object cateogry, InfobarPanel infobarPanel)
		{
			throw new NotImplementedException();
		}
	}
}