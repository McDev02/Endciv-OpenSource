using UnityEngine;

namespace Endciv
{
	public class TutorialScenario : ScenarioBase
	{
		[SerializeField] StoryWindow introductionWindowPrefab;
		StoryWindow myIntroductionWindow;
		[SerializeField] StoryWindow endWindowPrefab;
		StoryWindow myEndWindow;
		CameraController cameraController;
		ProductionSystem productionSystem;

		CitizenAISystem citizenAISystem;
		ConstructionSystem constructionSystem;

		//CameraControll
		Vector3 startPos;
		float startYaw, startPitch;
		float startZoom;

		//Time
		TimeManager.EGameSpeed startTime;

		enum TutorialMilestones { camera, watercollector, timeadjustment, rations, shelter, production }

		public override void Run()
		{
			ConstructionSystem.OnBuildingPlaced += OnBuildingPlaced;
			ConstructionSystem.OnBuildingBuilt += OnBuildingBuilt;
			ConstructionSystem.OnBuildingDemolished += OnBuildingDemolished;

			myIntroductionWindow = Instantiate(introductionWindowPrefab, gameManager.GameGUIController.objectivesContainer, false);
			myIntroductionWindow.actionButtons[0].onClick.AddListener(OnStartTutorial);
			myIntroductionWindow.OnShow();

			cameraController = gameManager.CameraController;
			var sys = gameManager.SystemsManager;
			productionSystem = sys.ProductionSystem;
			citizenAISystem = sys.AIAgentSystem.CitizenAISystem;
			constructionSystem = sys.ConstructionSystem;

			citizenAISystem.OnRationsChanged += OnRationsChanged;
		}

		internal override void MilestoneChanged()
		{
			switch ((TutorialMilestones)currentMilestoneID)
			{
				case TutorialMilestones.camera:
					startPos = cameraController.transform.position;
					startYaw = cameraController.Model.Yaw.Target;
					startPitch = cameraController.Model.Pitch.Target;
					startZoom = cameraController.Model.Zoom.Target;
					break;
				case TutorialMilestones.watercollector:
					constructionSystem.AddTech(ETechnologyType.BuildingEnabled);
					break;
				case TutorialMilestones.timeadjustment:
					startTime = gameManager.timeManager.currentGameSpeed;
					break;
				case TutorialMilestones.shelter:
					break;
				case TutorialMilestones.production:
					break;
				default:
					myEndWindow = Instantiate(endWindowPrefab, gameManager.GameGUIController.objectivesContainer, false);
					break;
			}
		}

		internal override void UpdateGameLoop()
		{
			switch ((TutorialMilestones)currentMilestoneID)
			{
				case TutorialMilestones.camera:
					if ((cameraController.transform.position - startPos).sqrMagnitude > 0.01f)
						notificationSystem.SetVariable<bool>("tut_cameraMoved", true);
					if (Mathf.Abs(cameraController.Model.Yaw.Target - startYaw) > 0.01f && Mathf.Abs(cameraController.Model.Pitch.Target - startPitch) > 0.01f)
						notificationSystem.SetVariable<bool>("tut_cameraRotated", true);
					if (Mathf.Abs(cameraController.Model.Zoom.Target - startZoom) > 0.01f)
						notificationSystem.SetVariable<bool>("tut_cameraZoomed", true);
					break;
				case TutorialMilestones.watercollector:
					break;
				case TutorialMilestones.timeadjustment:
					if (startTime != gameManager.timeManager.currentGameSpeed)
					{
						startTime = gameManager.timeManager.currentGameSpeed;
						notificationSystem.SetVariable<bool>("tut_gameSpeedChanged", true);
					}
					break;
				case TutorialMilestones.shelter:
					break;
				case TutorialMilestones.production:
					for (int i = 0; i < productionSystem.Orders.Length; i++)
					{
						var order = productionSystem.Orders[i];
						if (order.StaticData.OutputResources.ResourceID == "mechanic_parts")
						{
							if (order.targetAmount >= 10)
								notificationSystem.SetVariable<bool>("tut_productionOrdered", true);
						}
					}

					break;
				default:
					break;
			}
		}

		public void OnStartTutorial()
		{
			if (milestones != null && milestones.Length > currentMilestoneID)
			{
				milestones[currentMilestoneID].Run();
				notificationSystem.StartMilestone(milestones[currentMilestoneID]);
			}
			MilestoneChanged();
			Destroy(myIntroductionWindow.gameObject);
		}

		void OnBuildingPlaced(string id)
		{
			if (id == "scrapyard")
				notificationSystem.SetVariable<bool>("isScrapyardPlaced", true);
			if (id == "watercollector_small" || id == "watercollector_medium")
				notificationSystem.SetVariable<bool>("tut_watercollectorPlaced", true);
		}

		void OnRationsChanged()
		{
			notificationSystem.SetVariable<float>("tut_rationsWater", citizenAISystem.waterConsumePortions);
			notificationSystem.SetVariable<float>("tut_rationsFood", citizenAISystem.nutritionConsumePortions);
		}

		void OnBuildingBuilt(string id)
		{
			if (id == "scrapyard")
				notificationSystem.SetVariable<bool>("isScrapyardBuilt", true);
			if (id == "storagetent")
				notificationSystem.SetVariable<bool>("tut_storageBuilt", true);
			if (id == "watercollector_small" || id == "watercollector_medium")
				notificationSystem.SetVariable<bool>("tut_watercollectorBuilt", true);
		}
		void OnBuildingDemolished(string id)
		{
		}
	}
}