using UnityEngine;

namespace Endciv
{
	public sealed class StructureFeatureView : FeatureView<StructureFeature>
	{
		public bool hideViewOnEnter = true;
		public WaypointPath path;
		public MeshRenderer animatedMesh;
		Material[] cachedMaterials;
		Material[] currentMaterials;

		public bool CanBeEntered
		{
			get
			{
				return path != null && path.points != null && path.points.Length > 0;
			}
		}

		public override void Setup(FeatureBase feature)
		{
			base.Setup(feature);
			if (animatedMesh != null)
			{
				cachedMaterials = animatedMesh.sharedMaterials;
				//Make material instances
				currentMaterials = animatedMesh.materials;
				for (int i = 0; i < currentMaterials.Length; i++)
				{
					currentMaterials[i].EnableKeyword("CONSTRUCTION");
				}
			}
		}

		public void UpdateConstructionView()
		{
			SetConstructionProgress(Feature.Entity.GetFeature<ConstructionFeature>().ConstructionProgress);
		}
		public void FinishConstructionSite()
		{
			SetConstructionProgress(1);
			if (animatedMesh != null)
				animatedMesh.sharedMaterials = cachedMaterials;
		}

		void SetConstructionProgress(float progress)
		{
			//Scale Animation
			if (currentMaterials == null)
			{
				var scale = transform.localScale;
				scale.y = Mathf.Lerp(0.1f, 1, progress);
				transform.localScale = scale;
			}
			//Material animation
			else
			{
				for (int i = 0; i < currentMaterials.Length; i++)
				{
					currentMaterials[i].SetFloat("_ConstructionProgress", 1f - progress);
				}
			}
		}

		public override void UpdateView()
		{
			
		}
	}
}