using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	[RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData), typeof(InventoryStaticData))]
	public class StorageStaticData : FeatureStaticData<StorageFeature>
	{
		[EnumMask]
		public EStoragePolicy Policy = EStoragePolicy.AllGoods;
		public bool EditPolicy;
		public bool IncludeForMaintenance = true;
		[Range(0, 5)]
		public int Priority = 1;
		public bool Protected;
	}
}