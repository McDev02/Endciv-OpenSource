using UnityEngine;
using UnityEngine.PostProcessing;

namespace Endciv
{
	public class CameraPostProcessingSettings : MonoBehaviour
	{
		public PostProcessingBehaviour postProcessingBehavior;
		public Smaa.SMAA smaa;
		//public CTAA_PC ctaa;
		//public SENaturalBloomAndDirtyLens bloom;
		public AdvancedUnsharpMask unsharpmask;
		//public AmplifyOcclusionEffect ambientOcclusion;

		public void SetPostExposure(float value)
		{
			var profile = postProcessingBehavior.profile;
			var gradingSettings = profile.colorGrading.settings;
			gradingSettings.basic.postExposure = value;

			profile.colorGrading.settings = gradingSettings;
			//postProcessingBehavior.profile = profile;
		}

		public void EnableSMAA(bool onOff)
		{
			smaa.enabled = onOff;
		}
		public void EnableTAA(bool onOff)
		{
			//ctaa.CTAA_Enabled = onOff;
			//unsharpmask.enabled = onOff;
		}
	}
}