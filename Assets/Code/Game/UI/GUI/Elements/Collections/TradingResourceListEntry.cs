using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	public class TradingResourceListEntry : TradingBaseListEntry
	{
		public override float BaseValue
		{
			get
			{
				return m_BaseValue;
			}
		}
		private float m_BaseValue;

		public override void Setup(int amount, string staticDataID, TradingWindow tradingWindow, bool useTooltip = true, params object[] args)
		{
			var factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			name = "entry_" + staticDataID;
			var iconID = factory.EntityStaticData[staticDataID].ID;
			var sprite = ResourceManager.Instance.GetIcon(iconID, EResourceIconType.General);
			icon.sprite = sprite;
			m_BaseValue = factory.GetStaticData<ItemFeatureStaticData>(staticDataID).Value;
			base.Setup(amount, staticDataID, tradingWindow, useTooltip, args);
		}

		public override bool Matches(TradingBaseListEntry entry)
		{
			return entry.id == id;
		}

		public override void DestroyResources()
		{
			int count = currentAmount;
			var storageSystem = Main.Instance.GameManager.SystemsManager.StorageSystem;
			while (count > 0)
			{
				foreach (var storage in storageSystem.GetAllStorages(SystemsManager.MainPlayerFaction))
				{
					for (int i = count - 1; i >= 0; i--)
					{
						int currentCount = Mathf.Min(InventorySystem.GetItemCount(storage.Inventory, id), count);
						if (currentCount <= 0)
							continue;
						var items = InventorySystem.WithdrawItems(storage.Inventory, id, currentCount);
						if (items == null)
							continue;
						count -= currentCount;
						if (count <= 0)
							break;
					}
					if (count <= 0)
						break;
				}
			}
		}

		public override void AcquireResources()
		{
			var gridMap = Main.Instance.GameManager.GridMap;
			var agricultureSystem = Main.Instance.GameManager.SystemsManager.AgricultureSystem;
			Vector2i spawnPosition;
			if (gridMap.GetPossitionNearPlayerTown(out spawnPosition))
			{
				spawnPosition = gridMap.MapCenteri;
				//Add seeds if it is a crop
				agricultureSystem.ChangeSeeds(id, currentAmount);
				//Place goods in world
				ResourcePileSystem.PlaceStoragePile(spawnPosition, id, currentAmount);
			}
			else
				Debug.LogError("Rare error, GetPossitionNearPlayerTown returned nothing");

		}

		public override void SetupTooltip()
		{
			string text;
			if (!LocalizationManager.GetTextSafely($"#Resources/{id}/short_info", out text))
			{
				text = LocalizationManager.GetText($"#Resources/{id}/name");
			}
			tooltip.text = text;
		}

		public ResourceStack ToResourceStack()
		{
			return new ResourceStack(id, currentAmount);
		}
	}

	public static class TradingResourceListEntryExtensions
	{
		public static List<ResourceStack> ToResourceStackList(this List<TradingBaseListEntry> entries)
		{
			var list = new List<ResourceStack>();
			foreach (var entry in entries)
			{
				if (entry is TradingResourceListEntry)
					list.Add((entry as TradingResourceListEntry).ToResourceStack());
			}
			return list;
		}
	}
}