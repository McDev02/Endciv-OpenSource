using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Endciv
{
	public class TimeMenu : MonoBehaviour
	{
		bool isRunning;
		bool isBound;
		TimeManager timeManager;
		WeatherSystem weatherSystem;
		GeneralSettingsManager generalSettingsManager;
		CitizenAISystem aiSystem; //Temporarily

		[SerializeField] TabController timeButtons;
		[SerializeField] Text dayLabel;
		[SerializeField] Text timeLabel;
		[SerializeField] Text daytimeLabel;
		[SerializeField] Text seasonLabel;
		[SerializeField] Text rainLabel;
		[SerializeField] Text temperatureLabel;
		[SerializeField] Text sheduleLabel;

		TimeManager.EGameSpeed selectedSpeed;

		//Last frame Data
		CitizenShedule.ESheduleType lastSheduleState;
		EDaytime lastDaytime;
		ESeason lastSeason;
		int lastTemperature;
		int lastDays;


		const TimeManager.EGameSpeed NightGameSpeed = TimeManager.EGameSpeed._20x;

		public void Setup(TimeManager timeManager, WeatherSystem weatherSystem, CitizenAISystem aiSystem)
		{
			generalSettingsManager = Main.Instance.generalSettingsManager;
			this.timeManager = timeManager;
			this.weatherSystem = weatherSystem;
			this.aiSystem = aiSystem;
		}

		public void Run()
		{
			isRunning = true;
			//Force update correct time	
			selectedSpeed = TimeManager.EGameSpeed._1x;
			Update();

			if (!isBound)
				timeButtons.OnToggleChanged += OnTimeButton;
			isBound = true;

			timeManager.OnTickChanged -= RedrawUI;
			timeManager.OnTickChanged += RedrawUI;

			//Setup first time
			daytimeLabel.text = LocalizationManager.GetText($"#General/Time/{lastDaytime}");
			seasonLabel.text = LocalizationManager.GetText($"#General/Time/{timeManager.CurrentSeason}");
			lastTemperature = -999;
			lastDays = -999;
		}

		private void OnDestroy()
		{
			timeManager.OnTickChanged -= RedrawUI;
		}

		private void OnTimeButton(int id)
		{
			if (timeManager.PauseFreeze) return;

			switch (id)
			{
				case 0:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed.Pause);
					break;
				case 1:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._1x);
					break;
				case 2:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._3x);
					break;
				case 3:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._5x);
					break;
				case 4:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._10x);
					break;
				case 5:
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._20x);
					break;
				default:
					break;
			}
		}

		void RedrawUI()
		{
			//Daytime
			if (lastDaytime != timeManager.CurrentDaytime)
			{
				lastDaytime = timeManager.CurrentDaytime;
				daytimeLabel.text = LocalizationManager.GetText($"#General/Time/{lastDaytime}");
			}
			//timeLabel.text = $"{timeManager.Hours.ToString("00")}:{timeManager.Minutes.ToString("00")}";
			//timeLabel.text = (int)(timeManager.CurrentDaytimeProgress * 100) + "%";

			//Season
			if (lastSeason != timeManager.CurrentSeason)
			{
				seasonLabel.text = LocalizationManager.GetText($"#General/Time/{timeManager.CurrentSeason}");
				lastSeason = timeManager.CurrentSeason;
			}
			//Current Day
			if (lastDays != timeManager.totalDays)
			{
				lastDays = timeManager.totalDays;
				var singular = LocalizationManager.ETextVersion.Singular;// lastDays == 1 ? LocalizationManager.ETextVersion.Singular : LocalizationManager.ETextVersion.Plural;
				dayLabel.text = $"{LocalizationManager.GetText("#General/day", singular)} {timeManager.totalDays.ToString()}";
			}

			//Temperature
			if (generalSettingsManager.degreeFahrenheit)
			{
				var temp = Mathf.RoundToInt(weatherSystem.TemperatureF);
				if (lastTemperature != temp)
				{
					temperatureLabel.text = $"{temp}{LocalizationManager.GetText("#General/DegreeFahrenheit")}";
					lastTemperature = temp;
				}
			}
			else
			{
				var temp = Mathf.RoundToInt(weatherSystem.Temperature);
				if (lastTemperature != temp)
				{
					temperatureLabel.text = $"{temp}{LocalizationManager.GetText("#General/DegreeCelsius")}";
					lastTemperature = temp;
				}
			}
		}

		private void Update()
		{
			if (!isRunning) return;
			
			var sheduleState = aiSystem.generalSheduleState;

			if (timeManager.currentGameSpeed != NightGameSpeed && !timeManager.IsGamePaused && sheduleState == CitizenShedule.ESheduleType.Sleep)
				timeManager.SetTimeSpeed(NightGameSpeed);
			if (lastSheduleState != sheduleState)
			{
				sheduleLabel.text = LocalizationManager.GetText($"#General/Shedule/{sheduleState}");
				if (lastSheduleState == CitizenShedule.ESheduleType.Sleep)
					timeManager.SetLastTimeSpeed(true);
			}
			lastSheduleState = sheduleState;

			if (rainLabel != null) rainLabel.text = Mathf.RoundToInt(weatherSystem.Rainfall * 100).ToString() + "%";
			//Button view update
			if (selectedSpeed != timeManager.currentGameSpeed)
			{
				selectedSpeed = timeManager.currentGameSpeed;

				switch (selectedSpeed)
				{
					case TimeManager.EGameSpeed.Pause:
						timeButtons.SelectTab(0); break;
					case TimeManager.EGameSpeed.Half:
						timeButtons.SelectTab(-1); break;
					case TimeManager.EGameSpeed._1x:
						timeButtons.SelectTab(1); break;
					case TimeManager.EGameSpeed._2x:
						timeButtons.SelectTab(-1); break;
					case TimeManager.EGameSpeed._3x:
						timeButtons.SelectTab(2); break;
					case TimeManager.EGameSpeed._5x:
						timeButtons.SelectTab(3); break;
					case TimeManager.EGameSpeed._10x:
						timeButtons.SelectTab(4); break;
					default:
						break;
				}
			}
		}
	}
}