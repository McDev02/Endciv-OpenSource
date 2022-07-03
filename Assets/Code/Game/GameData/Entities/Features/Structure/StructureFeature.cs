namespace Endciv
{
	public class StructureFeature :
		Feature<StructureFeatureSaveData>,
		IViewController<StructureFeatureView>
	{
		public StructureFeatureView View { get; private set; }

		public StructureFeatureStaticData StaticData { get; private set; }				

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<StructureFeatureStaticData>();
			var featureParams = (StructureFeatureParams)args;
			if (featureParams != null)
				CurrentViewID = featureParams.CurrentViewID;
		}

		#region IViewController
		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (StructureFeatureView)view;
		}

		public void ShowView()
		{
			if (View != null)
			{
				View.ShowHide(true);
			}
		}

		public void HideView()
		{
			if (View != null)
			{
				View.ShowHide(false);
			}
		}

		public void UpdateView()
		{
			if (View != null)
			{
				View.UpdateView();
			}
		}

		public void SelectView()
		{
			if (View != null)
			{
				View.OnViewSelected();
			}
		}

		public void DeselectView()
		{
			if (View != null)
			{
				View.OnViewDeselected();
			}
		}
		#endregion

		public override void Stop()
		{
			base.Stop();
			SystemsManager.StructureSystem.DeregisterFeature(this);

		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.StructureSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.StructureSystem.RegisterFeature(this);
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			SystemsManager.StructureSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new StructureFeatureSaveData();
			return data;
		}

		public override void ApplyData(StructureFeatureSaveData data)
		{

		}
	}
}