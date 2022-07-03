using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class ToolbarPanel : MonoBehaviour
	{
		[SerializeField] Color BaseColorNormal;
		[SerializeField] Color BaseColorHighlighted;
		[SerializeField] Color ActiveColorNormal;
		[SerializeField] Color ActiveColorHighlighted;

		[SerializeField] Button btn_peopleStats;
		[SerializeField] Button btn_occupations;
		[SerializeField] Button btn_resourceManagement;
		[SerializeField] Button btn_rationManagement;
		[SerializeField] Button btn_tradingWindow;
		[SerializeField] Button btn_production;
		[SerializeField] Button btn_feedback;

		GameManager gameManager;
		MainGUIController mainGUIController;
		GameGUIController gameGUIController;
		ConstructionSystem constructionSystem;

		//Window panels
		public OccupationPanel occupationPanel;
		public CitizenOverviewPanel citizenOverviewPanel;
		public ResourcesManagementPanel productionWindow;
		public RationsPanel rationsWindow;

		public void Setup(MainGUIController mainGUIController, GameGUIController gameGUIController, ConstructionSystem constructionSystem)
		{
			gameManager = gameGUIController.gameManager;
			this.mainGUIController = mainGUIController;
			this.gameGUIController = gameGUIController;
			this.constructionSystem = constructionSystem;

			productionWindow.Setup(gameManager.SystemsManager.ProductionSystem);
			occupationPanel.Setup(gameManager.SystemsManager.AIAgentSystem.CitizenAISystem,gameManager.SystemsManager.ConstructionSystem);
			citizenOverviewPanel.Setup(gameManager.SystemsManager.UnitSystem, gameManager.CameraController, gameManager.UserToolSystem.SelectionTool);
			rationsWindow.Setup(gameManager.SystemsManager.AIAgentSystem.CitizenAISystem, gameManager.Factories.SimpleEntityFactory);

			constructionSystem.OnTechChanged -= UpdateTech;
			constructionSystem.OnTechChanged += UpdateTech;
			InitializeWindowListeners();

			UpdateTech();
			UpdateWindowButtons();
		}

		private void UpdateTech()
		{
			btn_production.gameObject.SetActive(constructionSystem.HasTech(ETechnologyType.Production));

			btn_resourceManagement.gameObject.SetActive(false);
			btn_tradingWindow.gameObject.SetActive(false);
		}

		void UpdateWindowButtons()
		{
			UpdateButton(btn_peopleStats, citizenOverviewPanel.IsVisible);
			UpdateButton(btn_occupations, occupationPanel.IsVisible);
			//UpdateBUtton(btn_resourceManagement, mainGUIController.r .IsVisible);
			UpdateButton(btn_rationManagement, rationsWindow.IsVisible);
			UpdateButton(btn_production, productionWindow.IsVisible);
			UpdateButton(btn_feedback, mainGUIController.feedbackWindow.IsVisible);
			UpdateTech();
		}


		public void ShowHideResourcesManagementWindow()
		{
		}
		public void ShowHideRationsWindow()
		{
			rationsWindow.OnToggleActive();
		}
		public void ShowHideProductionsWindow()
		{
			productionWindow.OnToggleActive();
		}
		public void ShowHideOccupationsWindow()
		{
			occupationPanel.OnToggleActive();
		}
		public void ShowHideCitizensOverviewWindow()
		{
			citizenOverviewPanel.OnToggleActive();
		}
		public void ShowHideFeedbackWindow()
		{
			mainGUIController.feedbackWindow.OnToggleActive();
		}

		void UpdateButton(Button btn, bool active)
		{
			var cols = btn.colors;
			cols.normalColor = active ? ActiveColorNormal : BaseColorNormal;
			cols.highlightedColor = active ? ActiveColorHighlighted : BaseColorHighlighted;
			btn.colors = cols;
		}

		void InitializeWindowListeners()
		{
			occupationPanel.OnWindowOpened -= UpdateWindowButtons;
			citizenOverviewPanel.OnWindowOpened -= UpdateWindowButtons;
			productionWindow.OnWindowOpened -= UpdateWindowButtons;
			mainGUIController.feedbackWindow.OnWindowOpened -= UpdateWindowButtons;

			occupationPanel.OnWindowOpened += UpdateWindowButtons;
			citizenOverviewPanel.OnWindowOpened += UpdateWindowButtons;
			productionWindow.OnWindowOpened += UpdateWindowButtons;
			mainGUIController.feedbackWindow.OnWindowOpened += UpdateWindowButtons;

			occupationPanel.OnWindowClosed -= UpdateWindowButtons;
			citizenOverviewPanel.OnWindowClosed -= UpdateWindowButtons;
			productionWindow.OnWindowClosed -= UpdateWindowButtons;
			mainGUIController.feedbackWindow.OnWindowClosed -= UpdateWindowButtons;

			occupationPanel.OnWindowClosed += UpdateWindowButtons;
			citizenOverviewPanel.OnWindowClosed += UpdateWindowButtons;
			productionWindow.OnWindowClosed += UpdateWindowButtons;
			mainGUIController.feedbackWindow.OnWindowClosed += UpdateWindowButtons;
		}

		internal void Run()
		{
			productionWindow.Run();
			occupationPanel.Run();
			citizenOverviewPanel.Run();
		}
	}
}