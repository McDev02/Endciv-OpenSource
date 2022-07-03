using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Endciv
{
	public class GUIConstructionListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		ConstructionMenu controller;
		int listID;
		public EntityStaticData data;
		public Button button;
		public Text title;
		public Image icon;
		public Transform resourcesContainer;

		[SerializeField] GUIResourceInfoEntry resourceListEntry;
		internal ResoruceListHelper resoruceListHelper;

		internal void Setup(ConstructionMenu controller, int id, EntityStaticData data, Sprite sprite)
		{
			this.controller = controller;
			this.data = data;

			listID = id;
			title.text = data.Name;

			float width = 200;
			if (sprite == null)
			{
				Color col = Color.white;
				col.a = 0;
				icon.color = col;
			}
			else
				width = sprite.rect.width;

			//Adjust width
			var size = icon.rectTransform.sizeDelta;
			size.x = width;
			icon.rectTransform.sizeDelta = size;

			var rect = (RectTransform)transform;
			size = rect.sizeDelta;
			size.x = width;
			rect.sizeDelta = size;


			icon.sprite = sprite;
			if (resoruceListHelper == null)
				resoruceListHelper = new ResoruceListHelper(resourceListEntry, resourcesContainer);
            var constructionData = data.GetFeature<ConstructionStaticData>();
			resoruceListHelper.UpdateResourceList(constructionData.Cost);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			controller.ShowDetails(listID);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			controller.HideDetails(listID);
		}
	}
}