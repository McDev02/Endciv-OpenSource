using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GridRectController : MonoBehaviour
	{
		public Image prefab;

		[SerializeField] private RectPreset constructionPreset;
		[SerializeField] private RectPreset destructionPreset;
		[SerializeField] private RectPreset graveyardPreset;
		[SerializeField] private RectPreset wastePreset;
		[SerializeField] private RectPreset storagePreset;
		[SerializeField] private RectPreset farmlandPreset;
		[SerializeField] private RectPreset waterPreset;

		WeatherSystem weatherSystem;

		[Serializable]
		public class RectPreset
		{
			public Color color;
			public Sprite icon;
		}

		[Serializable]
		public class RectEntry
		{
			public ConstructionFeature constructionFeature;
			public RectPreset preset;
			public Image image;
			public Image icon;
		}

		private List<RectEntry> rects = new List<RectEntry>();
		private Stack<RectEntry> rectPool = new Stack<RectEntry>();

		public void Run(WeatherSystem weatherSystem)
		{
			this.weatherSystem = weatherSystem;
			weatherSystem.OnGameLoopUpdate -= UpdateColors;
			weatherSystem.OnGameLoopUpdate += UpdateColors;
		}

		private void UpdateColors()
		{
			float value = Mathf.Clamp01(1f / weatherSystem.blendedWeather.cameraFX.LuminanceTonemapper);
			for (int i = 0; i < rects.Count; i++)
			{
				var rect = rects[i];
				var col = rect.preset.color * value;
				col.a = rect.preset.color.a;
				rect.image.color = col;
				rect.icon.color = col;
			}
		}

		public void AddRect(ConstructionFeature gridObject)
		{
			if (HasEntry(gridObject))
				return;
			var rect = GetRectEntry();
			var data = gridObject.Entity.GetFeature<GridObjectFeature>().GridObjectData;
			var gridRect = data.Rect;

			rect.constructionFeature = gridObject;

			float padding = gridObject.Entity.GetFeature<GridObjectFeature>().StaticData.GridRectPadding;
			var worldRect = Main.Instance.GameManager.GridMap.View.LocalToWorld(gridRect);

			if (data.EdgeIsWall)
				padding -= 0.25f;

			CivMath.RectExtend(ref worldRect, padding);

			rect.image.transform.localPosition = worldRect.position;
			rect.image.GetComponent<RectTransform>().sizeDelta = worldRect.size;
			rects.Add(rect);
			gridObject.OnConstructionStateChanged -= OnStateChanged;
			gridObject.OnConstructionStateChanged += OnStateChanged;
			OnStateChanged(gridObject, gridObject.ConstructionState);
		}

		private bool HasEntry(ConstructionFeature gridObject)
		{
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].constructionFeature == gridObject)
					return true;
			}
			return false;
		}

		private RectEntry GetRectEntry()
		{
			RectEntry rect = null;
			while (rect == null && rectPool.Count > 0)
			{
				rect = rectPool.Pop();
			}
			if (rect == null)
			{
				rect = new RectEntry();
				rect.image = Instantiate(prefab, transform);
				rect.icon = rect.image.transform.GetChild(0).GetComponent<Image>();
			}
			rect.image.gameObject.SetActive(true);
			return rect;
		}

		public void RemoveRect(ConstructionFeature gridObject)
		{
			gridObject.OnConstructionStateChanged -= OnStateChanged;

			RectEntry rect = null;
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].constructionFeature == gridObject)
				{
					rect = rects[i];
					rects.RemoveAt(i);
					break;
				}
			}
			if (rect == null)
				return;

			rect.image.gameObject.SetActive(false);
			rectPool.Push(rect);
		}

		private void OnStateChanged(ConstructionFeature gridObject, ConstructionSystem.EConstructionState state)
		{
			RectEntry rect = null;
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].constructionFeature == gridObject)
				{
					rect = rects[i];
					break;
				}
			}
			if (rect == null)
				return;

			rect.preset = constructionPreset;
			rect.image.gameObject.SetActive(true);
			switch (state)
			{
				case ConstructionSystem.EConstructionState.Ready:
					switch (gridObject.Entity.GetFeature<GridObjectFeature>().StaticData.GridRectType)
					{
						case EGridRectType.Storage:
							rect.preset = storagePreset;
							break;

						case EGridRectType.Farmland:
							rect.preset = farmlandPreset;
							break;

						case EGridRectType.Waste:
							rect.preset = wastePreset;
							break;

						case EGridRectType.Graveyard:
							rect.preset = graveyardPreset;
							break;

						case EGridRectType.Water:
							rect.preset = waterPreset;
							break;
						default:
							rect.image.gameObject.SetActive(false);
							break;
					}
					break;

				case ConstructionSystem.EConstructionState.Construction:
					rect.preset = constructionPreset;
					break;

				case ConstructionSystem.EConstructionState.Demolition:
					rect.preset = destructionPreset;
					break;
			}

			//Update color
			rect.image.color = rect.preset.color;
			rect.icon.color = rect.preset.color;
			rect.icon.sprite = rect.preset.icon;
		}
	}
}