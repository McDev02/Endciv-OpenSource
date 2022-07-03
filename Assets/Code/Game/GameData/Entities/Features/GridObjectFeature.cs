using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Entity component that moves on the grid
	/// </summary>
	public class GridObjectFeature : Feature<GridObjectSaveData>
	{
		Transform EntityTransform;
		public List<Vector2i> PartitionIDs = new List<Vector2i>();
		/// <summary>
		/// Runtime GridObjectData, not to confuse with StructureStaticData.GridObjectData
		/// This one contains rotation and translation
		/// </summary>
		public GridObjectFeatureStaticData StaticData { get; private set; }
		public GridObjectData GridObjectData { get; set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);				
			if(args != null)
			{
				var featureParams = (GridObjectFeatureParams)args;
				if(featureParams != null)
				{
					GridObjectData = new GridObjectData();
					GridObjectData.CopyFrom(featureParams.GridObjectData);
				}
			}
			StaticData = Entity.StaticData.GetFeature<GridObjectFeatureStaticData>();
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			EntityTransform = Entity.GetFeature<EntityFeature>().View.transform;
		}

		internal Vector2 GetPosition()
		{
			return EntityTransform.position.To2D();
		}

		public override ISaveable CollectData()
		{
			var data = new GridObjectSaveData();
			data.direction = (int)GridObjectData.Direction;
			data.gridObjectData = GridObjectData;
			return data;
		}

        public override void ApplyData(GridObjectSaveData data)
        {
            if (data.gridObjectData != null)
                GridObjectData = data.gridObjectData;			
			GridObjectData.Direction = (EDirection)data.direction;
        }
	}
}