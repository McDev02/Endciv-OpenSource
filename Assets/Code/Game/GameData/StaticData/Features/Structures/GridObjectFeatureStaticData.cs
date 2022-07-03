using UnityEngine;

namespace Endciv
{
	public class GridObjectFeatureStaticData : FeatureStaticData<GridObjectFeature>
	{
		//Todo: Store occupation cells and entrance points? Or calculate.

		public GridObjectData GridObjectData;   //[NonSerialized]  only when making an instance

		public EGridRectType GridRectType;

		public bool GridIsFlexible = false;
		public int SizeX;
		public int SizeY;

		/// <summary>
		/// Defines the min and max size of one edge in grids which are valid
		/// </summary>
		public MinMax FlexibleSize;
		/// <summary>
		/// Defines the min and max area (width * length) which are valid
		/// </summary>
		public MinMax FlexibleArea;

		public int GridOffsetX;
		public int GridOffsetY;

		public float GridRectPadding;
		public float VisualPadding;

		public float Passability = 0;
		public bool Enclosed = true;

		public override void Init()
		{
			GridObjectData.Rect = new RectBounds(0, 0, SizeX, SizeY);
			if (SizeX <= 0 || SizeY <= 0)
				Debug.LogError("Size of GridObjet is 0");
		}

		internal void SetSize(int x, int y)
		{
			SizeX = x;
			SizeY = y;
			Init();
		}		
	}
}
