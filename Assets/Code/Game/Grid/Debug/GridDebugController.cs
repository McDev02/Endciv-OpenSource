using UnityEngine;
namespace Endciv
{
	[RequireComponent(typeof(GridMap))]
	public class GridDebugController : MonoBehaviour
	{
		private BaseGrid Grid;
		[SerializeField] GridDebugView View;

		public void SetGrid(BaseGrid grid, bool force = false)
		{
			if (!force && Grid == grid)
				return;

			Grid = grid;

			if (Grid != null)
			{
				View.Setup(Grid);
				View.DrawGrid();
			}
		}

		internal void Redraw()
		{
			View.Clear();
			View.DrawGrid();
		}
	}
}