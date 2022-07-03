using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GUIToggleGroup : ToggleGroup
{
	public delegate void ChangedEventHandler(int active);

	public event ChangedEventHandler OnChange;

	protected override void Start()
	{
		Setup();
	}

	public void Setup()
	{
		int id = 0;
		foreach (Transform transformToggle in gameObject.transform)
		{
			var toggle = transformToggle.gameObject.GetComponent<Toggle>();
			int tmp = id++;
			toggle.onValueChanged.AddListener((isSelected) =>
			{
				if (!isSelected)
				{
					return;
				}
				var activeToggle = Active();
				DoOnChange(tmp);
			});
		}
	}
	public Toggle Active()
	{
		return ActiveToggles().FirstOrDefault();
	}

	protected virtual void DoOnChange(int active)
	{
		var handler = OnChange;
		if (handler != null) handler(active);
	}
}