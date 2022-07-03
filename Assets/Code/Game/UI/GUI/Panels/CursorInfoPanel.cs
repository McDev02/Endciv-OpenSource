using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class CursorInfoPanel : ContentPanel
	{
		[SerializeField] Text Lable;
		private GameInputManager gameInputManager;
		private GridMap gridMap;
		StringBuilder stringBuilder = new StringBuilder();
		Vector2i lastMousePos;

		public void Setup(GameInputManager gameInputManager, GridMap gridMap)
		{
			this.gameInputManager = gameInputManager;
			this.gridMap = gridMap;
		}

		public override void UpdateData()
		{
			stringBuilder.Length = 0;
			var pointer = gameInputManager.Pointer1;
			if (pointer != null && lastMousePos != pointer.GridIndex)
			{
				var data = gridMap.Data;
				stringBuilder.Append("X: ");
				stringBuilder.Append(pointer.GridIndex.X.ToString());
				stringBuilder.Append(" | Y: ");
				stringBuilder.AppendLine(pointer.GridIndex.Y.ToString());
				if (gridMap.Grid.IsInRange(pointer.GridIndex))
				{
					stringBuilder.Append("Occupied: ");
					stringBuilder.AppendLine(gridMap.IsOccupied(pointer.GridIndex, false).ToString());

					stringBuilder.Append("StayFree: ");
					stringBuilder.AppendLine(gridMap.IsStayFree(pointer.GridIndex).ToString());

					stringBuilder.Append("Passable: ");
					data.passability[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();

					stringBuilder.Append("<size=8>\n</size>");
					stringBuilder.Append("Faction ID: ");
					stringBuilder.AppendLine(data.factionID[pointer.GridIndex.X, pointer.GridIndex.Y].ToString());

					stringBuilder.Append("Group ID: ");
					stringBuilder.AppendLine(gridMap.GetGroupID(pointer.GridIndex).ToString());

					stringBuilder.Append("<size=8>\n</size>");
					stringBuilder.Append("Pollution: ");
					data.pollution[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();
					stringBuilder.Append("City Density: "); data.cityDensity[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();
					stringBuilder.Append("Open Area: "); data.openArea[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();

					stringBuilder.Append("<size=8>\n</size>");
					stringBuilder.Append("Fertile land: "); data.fertileLand[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();

					stringBuilder.Append("Waste: "); data.waste[pointer.GridIndex.X, pointer.GridIndex.Y].ToStringDecimal(2, stringBuilder);
					stringBuilder.AppendLine();
				}
				Lable.text = stringBuilder.ToString();
			}
			lastMousePos = pointer.GridIndex;
		}
	}
}