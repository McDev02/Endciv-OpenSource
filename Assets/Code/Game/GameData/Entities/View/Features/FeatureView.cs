using UnityEngine;
using cakeslice;
using McLOD;

namespace Endciv
{
	public abstract class FeatureViewBase : MonoBehaviour, IRunLogic
	{
		public abstract void Setup(FeatureBase feature);
		public bool IsRunning { get; protected set; }

		public abstract void UpdateView();

		public bool IsVissible { get; protected set; }

		public abstract void ShowHide(bool vissible);
		public abstract void OnViewSelected();
		public abstract void OnViewDeselected();

		public virtual void Dispose()
		{
			IsRunning = false;
		}
	}

	public abstract class FeatureView<T> : FeatureViewBase 
		where T : FeatureBase
	{
		public T Feature { get; set; }		

		protected Outline[] outlines;
		protected IMcLODEntity myLOD;

		public override void Setup(FeatureBase feature)
		{
            Feature = (T)feature;
			outlines = GetComponentsInChildren<Outline>(true);
			myLOD = GetComponent<IMcLODEntity>();

			IsRunning = true;
			OnViewDeselected();

			UpdateView();
		}

		public override void ShowHide(bool vissible)
		{
			IsVissible = vissible;
			if (myLOD != null) McLOD.McLOD.ShowHideEntity(myLOD, vissible);

			//if(renderers == null || renderers.Length <= 0)
			//    renderers = GetComponentsInChildren<Renderer>().Where(x => !(x is LineRenderer) && x.enabled && x.gameObject.activeInHierarchy).ToArray();
			//foreach (var rend in renderers)
			//{
			//    rend.enabled = vissible;
			//}
			if (Feature.Entity.NeedsInfoExists)
                Feature.Entity.NeedsInfo.gameObject.SetActive(vissible);
		}		

		public override void OnViewSelected()
		{
			if (outlines != null && outlines.Length > 0)
			{
				foreach (var outline in outlines)
				{
					outline.enabled = true;
				}
			}
		}

		public override void OnViewDeselected()
		{
			if (outlines != null && outlines.Length > 0)
			{
				foreach (var outline in outlines)
				{
					outline.enabled = false;
				}
			}
		}
	}
}