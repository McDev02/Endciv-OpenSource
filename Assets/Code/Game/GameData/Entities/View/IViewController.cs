namespace Endciv
{
	public interface IViewController
	{
		int CurrentViewID { get; set; }
		void SetView(FeatureViewBase view);
		void ShowView();
		void HideView();
		void UpdateView();
		void SelectView();
		void DeselectView();
	}

	public interface IViewController<T> : IViewController
		where T : FeatureViewBase
	{
		
	}
}
