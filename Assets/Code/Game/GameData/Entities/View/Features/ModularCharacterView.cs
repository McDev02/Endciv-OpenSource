using UnityEngine;

namespace Endciv
{
	public class ModularCharacterView : UnitFeatureView
	{
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;

		public override void Setup(FeatureBase feature)
		{
			base.Setup(feature);
			CharacterMeshFactory.Instance.GenerateModel(this, Feature.Gender, Feature.Age);											
		}

	}
}