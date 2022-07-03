using UnityEngine;

namespace Endciv
{
	public enum EUnitType { Citizen, Animal, Trader, Immigrant }

	[RequireFeature(typeof(EntityFeatureStaticData))]
	[EntityCategory("Units")]
	public class UnitFeatureStaticData : FeatureStaticData<UnitFeature>,
		IFeatureViewContainer
	{
		public MinMax adultMaleSizes;
		public MinMax adultFemaleSizes;
		public MinMax childMaleSizes;
		public MinMax childFemaleSizes;

		public EUnitType unitType;

		[SerializeField] public AiAgentSettings aiSettings;

		public GameObject[] views;

		public GameObject GetFeatureViewInstance(int variationID = -1)
		{
			if (views == null || views.Length <= 0)
				return null;
			return Instantiate(views[CivRandom.Range(0, views.Length)]);
		}

		public int GetNextViewID(int currentID)
		{
			if (views == null || views.Length <= 0)
				return -1;
			currentID++;
			if (currentID >= views.Length)
				currentID = 0;
			return currentID;
		}

		public int GetRandomViewID()
		{
			if (views == null || views.Length <= 0)
				return -1;
			return CivRandom.Range(0, views.Length);
		}
	}

}
