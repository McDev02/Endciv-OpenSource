using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Endciv
{
	[RequireComponent(typeof(FlexibleStructureView))]
	public sealed class StorageAreaView : FeatureView<StorageFeature>
	{
		private ResourcePileView[,] resourcePiles;
		private SimpleEntityFactory factory;
		private GridMap gridMap;

		Perlin perlin;

		private Dictionary<EStoragePolicy, List<ResourcePileView>> pilePool;

		private float loadPerTile;
		private bool DoUpdateView;

		public override void Setup(FeatureBase feature)
		{
			perlin = new Perlin(CivRandom.Range(0, 99999));
			base.Setup(feature);
			gridMap = Main.Instance.GameManager.GridMap;
			pilePool = new Dictionary<EStoragePolicy, List<ResourcePileView>>();
			Feature.Entity.Inventory.OnInventoryChanged += OnInventoryChanged;
			var rect = Feature.Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect;
			resourcePiles = new ResourcePileView[rect.Width, rect.Length];
			factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			loadPerTile = 1f / (resourcePiles.GetLength(0) * resourcePiles.GetLength(1));
			UpdateView();
		}

		private void OnInventoryChanged(InventoryFeature inventory)
		{
			DoUpdateView = true;
		}

		private void Update()
		{
			if (DoUpdateView)
				UpdateView();
			DoUpdateView = false;
		}

		public override void UpdateView()
		{
			if (resourcePiles == null)
				return;
			CleanupPiles();
			var table = InventorySystem.CalculateLoadByPolicy(Feature.Entity.Inventory);
			var list = SortPoliciesByLoad(table);
			var splitList = SplitPoliciesByTileCapacity(list);
			int index = 0;
			int width = resourcePiles.GetLength(0);
			int height = resourcePiles.GetLength(1);
			int size = width * height;
			foreach (var policy in splitList)
			{
				if (index >= size)
					break;
				int x = index % width;
				int y = index / width;
				index++;
				PlacePileOnGrid(policy, x, y);
			}
		}

		private void PlacePileOnGrid(EStoragePolicy policy, int x, int y)
		{
			var obj = GetResourcePile(policy);

			var gridObject = Feature.Entity.GetFeature<GridObjectFeature>().GridObjectData;
			var rect = gridObject.Rect;

			var posID = rect.Minimum + new Vector2i(x, y);
			var pos = gridMap.View.GetTileWorldPosition(posID).To3D(Feature.Entity.GetFeature<EntityFeature>().View.transform.position.y);
			obj.transform.position = pos;
			float t = 54.393257f;
			var noise = 0.5f + perlin.Noise(0.23f + x * t, 0.23f + y * t);
			var dir = Mathf.Floor(noise * 4) % 4;
			obj.transform.localRotation = Quaternion.AngleAxis(dir * 90, Vector3.up);
			resourcePiles[x, y] = obj;
		}

		private void CleanupPiles()
		{
			for (int i = 0; i < resourcePiles.GetLength(0); i++)
			{
				for (int j = 0; j < resourcePiles.GetLength(1); j++)
				{
					if (resourcePiles[i, j] == null)
						continue;
					RemoveResourcePile(resourcePiles[i, j]);
					resourcePiles[i, j] = null;
				}
			}
		}

		private ResourcePileView GetResourcePile(EStoragePolicy policy)
		{
			ResourcePileView pile = null;
			if (pilePool.ContainsKey(policy) && pilePool[policy].Count > 0)
			{
				while (pilePool[policy].Count > 0 && pile == null)
				{
					pile = pilePool[policy][0];
					pilePool[policy].RemoveAt(0);
				}
				if (pilePool[policy].Count <= 0)
					pilePool.Remove(policy);
			}
			if (pile == null)
			{
				var obj = factory.EntityStaticData[ResourcePileSystem.GetStoragePileIDByPolicy(policy)].
					GetFeature<ResourcePileFeatureStaticData>().GetFeatureViewInstance();
				obj.transform.parent = transform;
				pile = obj.gameObject.AddComponent<ResourcePileView>();
				pile.storagePolicy = policy;
			}
			pile.gameObject.SetActive(true);
			return pile;
		}

		private void RemoveResourcePile(ResourcePileView pile)
		{
			pile.gameObject.SetActive(false);
			if (!pilePool.ContainsKey(pile.storagePolicy))
			{
				pilePool.Add(pile.storagePolicy, new List<ResourcePileView>());
			}
			if (!pilePool[pile.storagePolicy].Contains(pile))
				pilePool[pile.storagePolicy].Add(pile);
		}

		private List<KeyValuePair<EStoragePolicy, float>> SortPoliciesByLoad
			(Dictionary<EStoragePolicy, float> table)
		{
			var list = table.ToList();
			list.Sort((pairA, pairB) => pairB.Value.CompareTo(pairA.Value));
			return list;
		}

		private List<EStoragePolicy> SplitPoliciesByTileCapacity
			(List<KeyValuePair<EStoragePolicy, float>> table)
		{
			var list = new List<EStoragePolicy>();
			foreach (var pair in table)
			{
				int counts = (int)(pair.Value / loadPerTile);
				float rem = pair.Value - (counts * loadPerTile);
				if (rem > 0f)
					counts++;
				for (int i = 0; i < counts; i++)
				{
					list.Add(pair.Key);
				}
			}
			return list;
		}
	}

}
