using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class BaseFeatureInfoPanel : ContentPanel
	{
		protected StringBuilder stringBuilder;
		protected BaseEntity entity;
		protected GameGUIController controller;

		public Text entityName;
		public GUIProgressBar entityHealthbar;
		bool invincible;

		[SerializeField] ColorPool healthColors;
		
		public virtual void Setup(GameGUIController controller, BaseEntity entity)
		{
			stringBuilder = new StringBuilder();
			this.controller = controller;
			this.entity = entity;
			invincible = entity.GetFeature<EntityFeature>().StaticData.Invincible;
			if (entityHealthbar != null) entityHealthbar.gameObject.SetActive(!invincible);


			UpdateData();
		}

		public void LocateEntity()
		{
			controller.LocateEntity(entity);
		}

		public void DeselectEntity()
		{
			controller.OnDeselectEntity();
		}

		public override void UpdateData()
		{
			if (entity == null)
			{
				OnClose();
			}
			else
			{
				entityName.text = entity.GetFeature<EntityFeature>().EntityName;
				if (!invincible && entityHealthbar != null)
					UpdateBar(entityHealthbar, entity.GetFeature<EntityFeature>().Health.Progress, false);
			}
		}

		protected void UpdateBar(GUIProgressBar bar, float need, bool isNeed = true)
		{
			if (!isNeed)
				need = need * 2 - 1;

			if (need > 0)
				bar.progressBar.color = Color.Lerp(healthColors.GetColor(1), healthColors.GetColor(0), need);
			else
				bar.progressBar.color = Color.Lerp(healthColors.GetColor(1), healthColors.GetColor(2), -need);

			bar.Value = (need + 1) * 0.5f;
		}

		protected void UpdateBar(GUIProgressBar bar, float need, float progress, bool isNeed = true)
		{
			if (!isNeed)
				need = need * 2 - 1;

			if (need > 0)
				bar.progressBar.color = Color.Lerp(healthColors.GetColor(1), healthColors.GetColor(0), need);
			else
				bar.progressBar.color = Color.Lerp(healthColors.GetColor(1), healthColors.GetColor(2), -need);

			bar.Value = progress;
		}
	}
}