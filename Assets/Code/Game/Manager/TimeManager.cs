using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Endciv
{
	public enum ETimeScaleMode
	{
		Scaled,
		Unscaled,
		UnscaledUnpaused
	}

	public enum EDaytime { Night, Morning, Noon, Evening }
	public enum ESeason { Spring, Summer, Fall, Winter }

	public class TimeManager : ISaveable, ILoadable<TimeManagerSaveData>
	{
		private class TimedEvent
		{
			public float interval;
			public float nextInterval;
			public bool oneShot;
			public ETimeScaleMode timeScaleMode;
			public Action OnIntervalReached;

			public TimedEvent(float interval, float currentTime, bool oneShot, ETimeScaleMode timeScaleMode, System.Action OnIntervalReached)
			{
				this.interval = interval;
				nextInterval = currentTime + interval;
				this.timeScaleMode = timeScaleMode;
				this.oneShot = oneShot;
				this.OnIntervalReached = OnIntervalReached;
			}
		}

		public enum EGameSpeed { Pause, Half, _1x, _2x, _3x, _5x, _10x, _20x, _50x }
		GameManager gameManager;
		WorldData worldData;

		public EGameSpeed currentGameSpeed { get; private set; }
		private EGameSpeed lastGameSpeed;

		//Listeners
		public Action OnTickChanged;
		//public ListenerCall OnMinuteChanged;
		//public ListenerCall OnHourChanged;
		public Action OnDayChanged;

		public bool PauseFreeze { get; private set; }

		public float CurrentTickProgress { get; private set; }
		private float currentDaytimeProgress;
		public float CurrentDaytimeProgress
		{
			get { return currentDaytimeProgress; }
			private set
			{
				//int oldminutes = Minutes;
				//int oldhours = Hours;
				currentDaytimeProgress = value;
				//float time = currentDaytimeProgress * 24;
				//Hours = (int)time;
				//Minutes = (int)((time - Hours) * 60);
				//if (oldminutes != Minutes) OnMinuteChanged?.Invoke();
				//if (oldhours != Hours) OnHourChanged?.Invoke();
			}
		}

		public float MinutesPerTick { get; private set; }
		public float TickPerMinute { get; private set; }

		public EDaytime CurrentDaytime;
		public ESeason CurrentSeason;
		public BlendedTimeline<DaytimePreset> DaytimeTimeline { get; }
		public BlendedTimeline<SeasonPreset> SeasonTimeline { get; }

		//Total ticks since game start
		public int CurrentTotalTick { get; private set; }
		public float CurrentTotalTickFloat { get { return CurrentTotalTick + CurrentTickProgress; } }
		//Current Tick of the day, will be reset at midnight
		private int currentDayTick;
		public int CurrentDayTick
		{
			get { return currentDayTick; }
			private set
			{
				if (currentDayTick != value)
					OnTickChanged?.Invoke();
				currentDayTick = value;
			}
		}
		public float CurrentDayTickFloat { get { return CurrentDayTick + CurrentTickProgress; } }

		/// <summary>
		/// Length of a day in ticks
		/// </summary>
		public int dayTickLength { get; private set; }

		/// <summary>
		/// 1 / dayTickLength
		/// </summary>
		public float dayTickFactor { get; private set; }

		/// <summary>
		/// Total elapsed days
		/// </summary>
		public int totalDays { get; private set; }
		/// <summary>
		/// Total elapsed days in floating value
		/// </summary>
		public float totalDaysFloat { get { return totalDays + CurrentDaytimeProgress; } }
		public bool IsGamePaused { get { return currentGameSpeed == EGameSpeed.Pause; } }

		public bool HasTimeChanged { get { return lastGameSpeed != currentGameSpeed; } }

		private float unscaledUnpausedTimer = 0f;

		private List<TimedEvent> timedEvents = new List<TimedEvent>();

		public TimeManager(GameManager gameManager, WorldData worldData)
		{
			this.gameManager = gameManager;
			this.worldData = worldData;

			//Setup World Data
			float relativeTotal = worldData.morningRelativeLength + worldData.noonRelativeLength + worldData.eveningRelativeLength + worldData.nightRelativeLength;
			worldData.morningWeather.Length = Mathf.Round(GameConfig.Instance.DayTickLength * worldData.morningRelativeLength / relativeTotal);
			worldData.noonWeather.Length = Mathf.Round(GameConfig.Instance.DayTickLength * worldData.noonRelativeLength / relativeTotal);
			worldData.eveningWeather.Length = Mathf.Round(GameConfig.Instance.DayTickLength * worldData.eveningRelativeLength / relativeTotal);
			worldData.nightWeather.Length = Mathf.Round(GameConfig.Instance.DayTickLength * worldData.nightRelativeLength / relativeTotal);

			worldData.dayTickLength = (int)(worldData.morningWeather.Length + worldData.noonWeather.Length + worldData.eveningWeather.Length + worldData.nightWeather.Length);

			relativeTotal = worldData.springRelativeDays + worldData.summerRelativeDays + worldData.fallRelativeDays + worldData.winterRelativeDays;
			worldData.springData.Length = Mathf.Round(GameConfig.Instance.YearDayLength * worldData.springRelativeDays / relativeTotal);
			worldData.summerData.Length = Mathf.Round(GameConfig.Instance.YearDayLength * worldData.summerRelativeDays / relativeTotal);
			worldData.fallData.Length = Mathf.Round(GameConfig.Instance.YearDayLength * worldData.fallRelativeDays / relativeTotal);
			worldData.winterData.Length = Mathf.Round(GameConfig.Instance.YearDayLength * worldData.winterRelativeDays / relativeTotal);

			worldData.yearDayLength = (int)(worldData.springData.Length + worldData.summerData.Length + worldData.fallData.Length + worldData.winterData.Length);

			//Timelines
			DaytimeTimeline = new BlendedTimeline<DaytimePreset>(0.75f);
			SeasonTimeline = new BlendedTimeline<SeasonPreset>(0.5f);

			DaytimeTimeline.AddNode(worldData.nightWeather);
			DaytimeTimeline.AddNode(worldData.morningWeather);
			DaytimeTimeline.AddNode(worldData.noonWeather);
			DaytimeTimeline.AddNode(worldData.eveningWeather);

			SeasonTimeline.AddNode(worldData.springData);
			SeasonTimeline.AddNode(worldData.summerData);
			SeasonTimeline.AddNode(worldData.fallData);
			SeasonTimeline.AddNode(worldData.winterData);

			//Other data
			dayTickLength = worldData.dayTickLength; //GameConfig.Instance.DayTickLength;
			dayTickFactor = 1f / dayTickLength;
			MinutesPerTick = (24 * 60) / (float)dayTickLength;
			TickPerMinute = 1f / MinutesPerTick;

			OnTickChanged += MyOnTickChanged;

			totalDays = 1;
			SetDaytime(0.24f);
			PauseGame();
		}

		void MyOnTickChanged()
		{
			CurrentDaytime = (EDaytime)DaytimeTimeline.GetID(CurrentDayTickFloat);
			CurrentSeason = (ESeason)SeasonTimeline.GetID(totalDaysFloat);
		}

		/// <summary>
		/// Used to register timed callback events   
		/// Will update the interval with new value if event is already registered
		/// Can either be called one time or repeatedly        
		/// </summary>
		/// <param name="interval">Time in seconds between callback invoke</param>
		/// <param name="callback">Supplied callback</param>
		/// <param name="oneShot">Will the event Deregister automatically after first invoke</param>
		/// <param name="timeScaleMode">How TimeScale and Game Pause affect the interval iteration</param>
		public void RegisterTimedEvent(float interval, System.Action callback, ETimeScaleMode timeScaleMode, bool oneShot = false)
		{
			if (callback == null)
				return;
			if (interval <= 0)
			{
				Debug.LogError("Timed event may not have 0 or negative interval value.");
				return;
			}
			float currentTime = 0f;
			switch (timeScaleMode)
			{
				case ETimeScaleMode.Scaled:
					currentTime = Time.time;
					break;

				case ETimeScaleMode.Unscaled:
					currentTime = Time.unscaledTime;
					break;

				case ETimeScaleMode.UnscaledUnpaused:
					currentTime = unscaledUnpausedTimer;
					break;
			}

			var ev = timedEvents.FirstOrDefault(x => x.OnIntervalReached == callback);
			if (ev != null)
			{
				ev.interval = interval;
				ev.nextInterval = currentTime + interval;
				ev.oneShot = oneShot;
				return;
			}
			timedEvents.Add(new TimedEvent(interval, currentTime, oneShot, timeScaleMode, callback));
		}

		/// <summary>
		/// Used to remove registered callback events
		/// Does nothing if callback is null or doesn't exist
		/// </summary>
		/// <param name="callback"></param>
		public void UnregisterTimedEvent(Action callback)
		{
			var ev = timedEvents.FirstOrDefault(x => x.OnIntervalReached == callback);
			if (ev != null)
			{
				timedEvents.Remove(ev);
			}
		}

		internal int GetDaysSince(int bornTimeTick)
		{
			return (int)((CurrentTotalTick - bornTimeTick) * dayTickFactor);
		}

		public void TogglePauseGame()
		{
			if (currentGameSpeed == EGameSpeed.Pause)
				UnpauseGame();
			else PauseGame();
		}
		public void PauseGame(bool forceUntilRelief = false)
		{
			SetTimeSpeed(EGameSpeed.Pause);
			if (forceUntilRelief)
				PauseFreeze = true;
		}
		public void UnpauseGame(bool forceUntilRelief = false)
		{
			if (forceUntilRelief)
				PauseFreeze = false;
			if (!PauseFreeze)
			{
				SetLastTimeSpeed();
#if UNITY_EDITOR
				if (gameManager != null)
#endif
					gameManager.UnsavedGameChanges = true;
			}
		}

		public void SetDaytime(int hours, int minutes = 0)
		{
			hours = Mathf.Clamp(hours * 60, 0, 24 * 60);
			minutes = Mathf.Clamp(minutes, 0, 60);
			CurrentDayTick = (int)((hours + minutes) / (24f * 60f) * dayTickLength);
			CurrentDaytimeProgress = CurrentDayTick / (float)dayTickLength + dayTickFactor * CurrentTickProgress;
		}

		public void SetDaytime(float daytimeProgress)
		{
			daytimeProgress = daytimeProgress % 1;
			CurrentTickProgress = 0;
			CurrentDayTick = (int)(dayTickLength * daytimeProgress);
			CurrentDaytimeProgress = CurrentDayTick / (float)dayTickLength + dayTickFactor * CurrentTickProgress;
		}

		public void NextGameTick()
		{
			CurrentDayTick++;
			CurrentTotalTick++;
			if (CurrentDayTick >= dayTickLength)
			{
				CurrentDayTick = 0;
				NextDay();
			}
			CurrentDaytimeProgress = CurrentDayTick / (float)dayTickLength + dayTickFactor * CurrentTickProgress;
		}

		void NextDay()
		{
			totalDays++;
			OnDayChanged?.Invoke();
		}

		internal void AddDays(int days)
		{
			totalDays += days;
			OnDayChanged?.Invoke();
		}


		/// <summary>
		/// Restore last choosen game speed or set to normal if paused
		/// </summary>
		public void SetLastTimeSpeed(bool NoPause = true)
		{
			if (PauseFreeze) return;
			var tmp = lastGameSpeed;
			if (NoPause && tmp == EGameSpeed.Pause) tmp = EGameSpeed._1x;

			lastGameSpeed = currentGameSpeed;
			currentGameSpeed = tmp;
			Time.timeScale = GetGameSpeeFactor(currentGameSpeed);
		}
		public void SetTimeSpeed(EGameSpeed speed)
		{
			if (PauseFreeze) return;
			//Debug.Log("Set time speed: " + speed.ToString());
			if (currentGameSpeed != speed)
				lastGameSpeed = currentGameSpeed;
			currentGameSpeed = speed;
			Time.timeScale = GetGameSpeeFactor(currentGameSpeed);
		}

		float GetGameSpeeFactor(EGameSpeed speed)
		{
			switch (speed)
			{
				case EGameSpeed.Pause: return 0;
				case EGameSpeed.Half: return 0.5f;
				case EGameSpeed._1x: return 1f;
				case EGameSpeed._2x: return 2f;
				case EGameSpeed._3x: return 3f;
				case EGameSpeed._5x: return 5f;
				case EGameSpeed._10x: return 10f;
				case EGameSpeed._20x: return 20f;
				case EGameSpeed._50x: return 50f;
			}
			return 1;
		}

		internal void SetTickProgress(float timer)
		{
			CurrentTickProgress = timer;
			CurrentDaytimeProgress = CurrentDayTick / (float)dayTickLength + dayTickFactor * CurrentTickProgress;
			if (currentGameSpeed != EGameSpeed.Pause)
			{
				unscaledUnpausedTimer += Time.unscaledDeltaTime;
			}

			//Reverse lookup to remove non existent events
			for (int i = timedEvents.Count - 1; i >= 0; i--)
			{
				var ev = timedEvents[i];
				if (ev == null)
				{
					timedEvents.RemoveAt(i);
					continue;
				}
				bool canExecute = false;
				float currentTime = 0f;
				switch (ev.timeScaleMode)
				{
					case ETimeScaleMode.Scaled:
						canExecute = ev.nextInterval <= Time.time;
						currentTime = Time.time;
						break;

					case ETimeScaleMode.Unscaled:
						canExecute = ev.nextInterval <= Time.unscaledTime;
						currentTime = Time.unscaledTime;
						break;

					case ETimeScaleMode.UnscaledUnpaused:
						canExecute = ev.nextInterval <= unscaledUnpausedTimer;
						currentTime = unscaledUnpausedTimer;
						break;
				}
				if (canExecute)
				{
					ev.nextInterval = ev.interval + currentTime;
					ev.OnIntervalReached?.Invoke();
					if (ev.oneShot)
					{
						timedEvents.RemoveAt(i);
					}
				}
			}
		}

		void PrepareWorldData()
		{

		}

		public ISaveable CollectData()
		{
			var data = new TimeManagerSaveData();
			data.currentDay = totalDays;
			data.currentDayTick = CurrentDayTick;
			data.currentDaytimeProgress = CurrentDaytimeProgress;
			data.currentTickProgress = CurrentTickProgress;
			data.currentTotalTick = CurrentTotalTick;
			data.dayTickFactor = dayTickFactor;
			data.dayTickLength = dayTickLength;
			return data;
		}

		public void ApplySaveData(TimeManagerSaveData data)
		{
			if (data == null)
				return;
			totalDays = data.currentDay;
			CurrentDayTick = data.currentDayTick;
			CurrentDaytimeProgress = data.currentDaytimeProgress;
			CurrentTickProgress = data.currentTickProgress;
			CurrentTotalTick = data.currentTotalTick;
			dayTickFactor = data.dayTickFactor;
			dayTickLength = data.dayTickLength;
		}
	}
}