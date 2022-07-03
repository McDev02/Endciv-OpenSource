using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIOptionsWindow : GUIAnimatedPanel
	{
		enum EState { General, Graphics, Audio, Input }

		[SerializeField] TabController tabController;
		[SerializeField] GUIOptionsGeneral PlayerPanel;
		[SerializeField] GUICanvasGroup InputPanel;
		[SerializeField] GUIOptionsAudio AudioPanel;
		[SerializeField] GUIOptionsGraphics GraphicsPanel;

		[SerializeField] MainGUIController MainController;

		internal void Setup(Main main)
		{
			PlayerPanel.Setup(main.generalSettingsManager);
			GraphicsPanel.Setup(main.graphicsManager);
			AudioPanel.Setup(main.audioManager);
		}

		public void ApplyValues()
		{
			switch ((EState)tabController.CurrentTab)
			{
				case EState.General:
					PlayerPanel.ApplyValues();
					break;
				case EState.Graphics:
					GraphicsPanel.ApplyValues();
					break;
				case EState.Audio:
					AudioPanel.ApplyValues();
					break;
				case EState.Input:
					break;
				default:
					break;
			}
		}

		public void DiscardValues()
		{
			switch ((EState)tabController.CurrentTab)
			{
				case EState.General:
					PlayerPanel.DiscardValues();
					break;
				case EState.Graphics:
					GraphicsPanel.DiscardValues();
					break;
				case EState.Audio:
					AudioPanel.DiscardValues();
					break;
				case EState.Input:
					break;
				default:
					break;
			}
		}
	}
}