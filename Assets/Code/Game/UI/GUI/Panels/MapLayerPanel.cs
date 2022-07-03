using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Endciv
{
	public class MapLayerPanel : MonoBehaviour
	{
		[SerializeField] Color iconColorIdle;
		[SerializeField] Color iconColorSelected;

		[SerializeField] Sprite buttonSpriteIdle;
		[SerializeField] Sprite buttonSpriteSelected;

		[SerializeField] Button pollutionButton;
		[SerializeField] Button groundWaterButton;
		[SerializeField] Image pollutionIcon;
		[SerializeField] Image groundWaterIcon;
		GameGUIController gameGUIController;
		GameManager gameManager;

		internal void Setup(GameGUIController gameGUIController, GameManager gameManager)
		{
			this.gameGUIController = gameGUIController;
			this.gameManager = gameManager;

			gameManager.TerrainManager.terrainView.OnLayerChanged -= UpdateButtons;
			gameManager.TerrainManager.terrainView.OnLayerChanged += UpdateButtons;
		}

		public void OnToggleWater()
		{
			if (gameManager.TerrainManager.terrainView.LayerMode == TerrainView.ELayerView.GroundWater)
				gameManager.TerrainManager.terrainView.HideLayerMap();
			else
				gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.GroundWater);
			UpdateButtons();
		}
		public void OnTogglePollution()
		{
			if (gameManager.TerrainManager.terrainView.LayerMode == TerrainView.ELayerView.Pollution)
				gameManager.TerrainManager.terrainView.HideLayerMap();
			else
				gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.Pollution);
			UpdateButtons();
		}

		void UpdateButtons()
		{
			bool isPollution = gameManager.TerrainManager.terrainView.LayerMode == TerrainView.ELayerView.Pollution;
			pollutionButton.image.sprite = isPollution ? buttonSpriteSelected : buttonSpriteIdle;
			pollutionIcon.color = isPollution ? iconColorSelected : iconColorIdle;

			bool isGroundWater = gameManager.TerrainManager.terrainView.LayerMode == TerrainView.ELayerView.GroundWater;
			groundWaterButton.image.sprite = isGroundWater ? buttonSpriteSelected : buttonSpriteIdle;
			groundWaterIcon.color = isGroundWater ? iconColorSelected : iconColorIdle;
		}
	}
}