using UnityEngine;

namespace Endciv
{
	public sealed class PowerSourceCombustionFeatureView : FeatureView<PowerSourceCombustionFeature>
	{
		[SerializeField] MeshRenderer flameRenderer;
		Material localFlameMaterial;
		[SerializeField] Light light;
		float lightStartingValue;

		public override void Setup(FeatureBase feature)
		{
			if (flameRenderer != null)
				localFlameMaterial = flameRenderer.material;

			lightStartingValue = light.intensity;

			base.Setup(feature);			
		}

		public override void UpdateView()
		{
			//Apply data		
			var vec = localFlameMaterial.GetVector("_Sheets");
			vec.w = CivMath.LerpStep(vec.w, Feature.powerOutput, 0.1f * Time.deltaTime);
			localFlameMaterial.SetVector("_Sheets", vec);

			light.intensity = vec.w * lightStartingValue;
			light.gameObject.SetActive(light.intensity > 0);
		}

		private void Update()
		{
			if (IsRunning)
				UpdateView();
		}
	}
}