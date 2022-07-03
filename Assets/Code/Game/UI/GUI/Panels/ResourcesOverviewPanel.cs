using UnityEngine;
using UnityEngine.UI;
using System.Text;
namespace Endciv
{
	public class ResourcesOverviewPanel : ContentPanel
	{
		[SerializeField] Text LableResources;
		[SerializeField] Text LableFood;
		[SerializeField] Text LableItems;

		public override void UpdateData()
		{
			var str = new StringBuilder();
			var stats = GameStatistics.InventoryStatistics;

			//Resources
			str.Append("TotalResources: ");
            var res = stats.TotalItems - stats.TotalWaste;
			str.AppendLine(res.ToString());

			//Waste
			str.Append("Waste: ");
			str.AppendLine(stats.TotalWaste.ToString());

			LableResources.text = str.ToString();
			str.Length = 0;

			//Food
			str.Append("TotalFood: ");
			str.AppendLine(stats.TotalFood.ToString());

			str.Append("Nutrition: ");
			str.AppendLine(stats.Nutrition.ToString("0.00"));
			str.Append("Water: ");
			str.AppendLine(stats.Water.ToString("0.00"));

			LableFood.text = str.ToString();
			str.Length = 0;

			//Items
			str.Append("TotalItems: ");
			str.AppendLine(stats.TotalItems.ToString());

			str.Append("Tools: ");
			str.AppendLine(stats.Tools.ToString());
			str.Append("Weapons: ");
			str.AppendLine(stats.Weapons.ToString());

			LableItems.text = str.ToString();
			str.Length = 0;
		}
	}
}