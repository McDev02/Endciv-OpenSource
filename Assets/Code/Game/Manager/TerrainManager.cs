using UnityEngine;
namespace Endciv
{
	public class TerrainManager : MonoBehaviour
	{
		GameManager GameManager;
        public TerrainGenerationManager terrainGenerator;
        public TerrainFactory factory;
		public TerrainView terrainView;
		public GridMap GridMap { get; private set; }
		public LayerMask TerrainLayer;
		public LayerMask TerrainLayerInv { get; private set; }

		private void Awake()
		{
			TerrainLayerInv = 1 << TerrainLayer;
		}

		public void Setup(GameManager gameManager, GridMap gridMap)
		{
			GameManager = gameManager;
			GridMap = gridMap;

            factory.Setup(gridMap);
		}

        public void GenerateView()
        {
            terrainView.Setup(this, GridMap);
        }
	}
}