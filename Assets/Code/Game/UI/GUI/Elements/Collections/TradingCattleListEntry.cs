using UnityEngine.UI;
using System.Linq;

namespace Endciv
{
	public class TradingCattleListEntry : TradingBaseListEntry
	{
		public Text nameLabel;
		public Text ageLbl;
		ELivingBeingAge age;

		CattleStaticData cattleStaticData;

		public override float BaseValue
		{
			get
			{
				return cattleStaticData.entity.GetFeature<ItemFeatureStaticData>().Value;
			}
		}

		public override void Setup(int amount, string staticDataID, TradingWindow tradingWindow, bool useTooltip = true, params object[] args)
		{
			var staticData = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<CattleStaticData>(staticDataID);			
			cattleStaticData = staticData;
			nameLabel.text = LocalizationManager.GetText($"#Animals/{cattleStaticData.entity.ID}/name_p");
			name = "entry_" + cattleStaticData.entity.ID;
			icon.sprite = ResourceManager.Instance.GetIcon(staticData.entity.ID, EResourceIconType.Unit);

			age = (ELivingBeingAge)args[0];
			ageLbl.text = age.ToString();

			base.Setup(amount, staticDataID, tradingWindow, useTooltip, args);
		}

		public override bool Matches(TradingBaseListEntry entry)
		{
			return entry.id == id && entry.args != null && (ELivingBeingAge)entry.args[0] == age;
		}

		public override void DestroyResources()
		{
			var pastureSystem = Main.Instance.GameManager.SystemsManager.PastureSystem;
			int count = currentAmount;
			foreach (var pasture in pastureSystem.FeaturesByFaction[SystemsManager.MainPlayerFaction])
			{
				var cattles = pasture.Cattle.Where(x => x.Entity.StaticData.ID == id && x.Entity.GetFeature<LivingBeingFeature>().age == age);
				if (cattles.Count() <= 0)
					continue;
				foreach (var cattle in cattles)
				{
					cattle.Entity.Destroy();
					count--;
					if (count <= 0)
						return;
				}
			}
		}

		public override void AcquireResources()
		{
			var gridMap = Main.Instance.GameManager.GridMap;
			var factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			int count = currentAmount;
			while (count > 0)
			{
				Vector2i spawnPosition;
				if (gridMap.GetPossitionNearPlayerTown(out spawnPosition))
				{
					spawnPosition = gridMap.MapCenteri;
					var pos = gridMap.View.LocalToWorld(spawnPosition).To3D();
					var factoryParams = new FactoryParams();
					factoryParams.SetParams
						(
							new GridAgentFeatureParams()
							{
								Position = pos
							},
							new EntityFeatureParams()
							{
								FactionID = SystemsManager.MainPlayerFaction
							},
							new UnitFeatureParams()
							{
								Age = age
							}
						);
					factory.CreateInstance(id, null, factoryParams);
				}
				else
					UnityEngine.Debug.LogError("Rare error, GetPossitionNearPlayerTown returned nothing");
				count--;
			}
		}

		public override void SetupTooltip()
		{
			string text;
			if (!LocalizationManager.GetTextSafely($"#UI/Game/TradingWindow/CattleInfo/{cattleStaticData.entity.ID}", out text))
			{
				text = LocalizationManager.GetText($"#Animals/pig/name/{cattleStaticData.entity.ID}");
			}
			tooltip.text = text;
		}		
	}	
}