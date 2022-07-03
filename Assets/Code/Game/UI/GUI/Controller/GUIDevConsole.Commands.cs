using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public partial class GUIDevConsole : GUIAnimatedPanel
	{
		#region Command Registration
		void RegisterCommands()
		{
			Register
				(
					new Property<bool>
					(
						"stacktrace",
						"Add to the unity log the stacktrace",
						() => m_AddLogStacktrace,
						(v) => m_AddLogStacktrace = v
					)
				);
			RegisterCommand("set_time", "Set Time", SetTime,
				new IArgument[] { new DefaultArgument<string>("Time Value", "pause, 1x, 3x, 5x") });
			RegisterCommand("add_days", "Add Days", AddDays,
				new IArgument[] { new DefaultArgument<int>("Time Value", "Amount of days to add") });
			RegisterCommand("spawn_immigrants", "Spawn Immigrants", SpawnImmigrants);
			RegisterCommand("spawn_trader", SpawnTrader);
			RegisterCommand("unlock_all_technologies", UnlockAllTechnology);
			//RegisterCommand("spawn_humans", "Spawn Humans", SpawnHumans, 
			//    new IArgument[] { new DefaultArgument<int>("Count", "Number of humans to spawn.") });
		}
		#endregion

		#region Command Implementation
		private void AddDays(object[] parameters)
		{
			if (Main.Instance.GameManager == null)
			{
				WriteLineIntoBuffer("Cannot change time in main menu.");
				return;
			}
			if (Main.Instance.GameManager.timeManager == null)
			{
				WriteLineIntoBuffer("Time manager is not set up. Aborting.");
				return;
			}
			int days = (int)parameters[0];
			if (days >= 0)
			{
				Main.Instance.GameManager.timeManager.AddDays(days);
			}
		}

		private void SetTime(object[] timeScale)
		{
			if (Main.Instance.GameManager == null)
			{
				WriteLineIntoBuffer("Cannot change time in main menu.");
				return;
			}
			if (Main.Instance.GameManager.timeManager == null)
			{
				WriteLineIntoBuffer("Time manager is not set up. Aborting.");
				return;
			}
			var time = timeScale[0].ToString().ToLower();
			switch (time)
			{
				case "pause":
					Main.Instance.GameManager.timeManager.SetTimeSpeed(TimeManager.EGameSpeed.Pause);
					break;

				case "1x":
					Main.Instance.GameManager.timeManager.SetTimeSpeed(TimeManager.EGameSpeed._1x);
					break;

				case "3x":
					Main.Instance.GameManager.timeManager.SetTimeSpeed(TimeManager.EGameSpeed._3x);
					break;

				case "5x":
					Main.Instance.GameManager.timeManager.SetTimeSpeed(TimeManager.EGameSpeed._5x);
					break;

				default:
					WriteLineIntoBuffer(time + " is an invalid time argument. Valid arguments: pause, x1, x3, x5");
					return;
			}
			WriteLineIntoBuffer("Time set to " + time + ".");
		}

		private void SpawnHumans(object[] count)
		{
			if (Main.Instance.GameManager == null)
			{
				WriteLineIntoBuffer("Cannot spawn humans in main menu.");
				return;
			}
			if (Main.Instance.GameManager.UserToolSystem == null)
			{
				WriteLineIntoBuffer("User Tool System is not set up. Aborting.");
				return;
			}
			var number = (int)count[0];
			if (number <= 0)
				return;
			Main.Instance.GameManager.UserToolSystem.PlaceUnit("human", number, 3);
			string targetName = "human";
			if (number > 1)
				targetName += "s";
			WriteLineIntoBuffer("Click anywhere to place " + number + " " + targetName);
		}

		private void SpawnImmigrants(object args)
		{
			if (Main.Instance.GameManager == null)
			{
				WriteLineIntoBuffer("Cannot spawn immigrants in main menu.");
				return;
			}
			if (Main.Instance.GameManager.SystemsManager.NpcSpawnSystem == null)
			{
				WriteLineIntoBuffer("Town AI System is not set up. Aborting.");
				return;
			}
			Main.Instance.GameManager.SystemsManager.NpcSpawnSystem.AddImmigrationGroup();
			WriteLineIntoBuffer("Created a new immigration group.");
		}

		private void UnlockAllTechnology(object args)
		{
			Main.Instance.UnlockAllTech = true;
		}
		private void SpawnTrader(object args)
		{
			if (Main.Instance.GameManager == null)
			{
				WriteLineIntoBuffer("Cannot spawn trader in main menu.");
				return;
			}
			if (Main.Instance.GameManager.SystemsManager.NpcSpawnSystem == null)
			{
				WriteLineIntoBuffer("Town AI System is not set up. Aborting.");
				return;
			}
			Main.Instance.GameManager.SystemsManager.NpcSpawnSystem.ResetTraderCounter();
			WriteLineIntoBuffer("Spawned new trader.");
		}
		#endregion
	}

}
