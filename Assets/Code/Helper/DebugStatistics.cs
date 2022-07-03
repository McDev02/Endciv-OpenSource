using System.Collections.Generic;
namespace Endciv
{
	public static class DebugStatistics
	{
		private static Dictionary<string, StopwatchEntry> Stopwatches = new Dictionary<string, StopwatchEntry>(8);

		internal static void RegisterNewStopwatch(string name, int rounds)
		{
			if (!Stopwatches.ContainsKey(name))
				Stopwatches.Add(name, new StopwatchEntry(rounds));
		}

		internal static void RestartWatch(string name)
		{
			if (!Stopwatches.ContainsKey(name))
			{
				UnityEngine.Debug.LogError("Stopwatch \"" + name + "\" not found");
				return;
			}
			var entry = Stopwatches[name];
			entry.watch.Reset();
			entry.watch.Start();
			entry.LastRound = 0;
		}

		internal static void CountRound(string name)
		{
			if (!Stopwatches.ContainsKey(name))
			{
				UnityEngine.Debug.LogError("Stopwatch \"" + name + "\" not found");
				return;
			}
			var entry = Stopwatches[name];
			entry.RecordRound();
			entry.watch.Start();
		}

		internal static double CountRoundAndStop(string name)
		{
			if (!Stopwatches.ContainsKey(name))
			{
				UnityEngine.Debug.LogError("Stopwatch \"" + name + "\" not found");
				return 0;
			}
			var entry = Stopwatches[name];
			return entry.RecordRound();
		}

		internal static double GetAverageTime(string name, int round = 0)
		{
			if (!Stopwatches.ContainsKey(name))
			{
				UnityEngine.Debug.LogError("Stopwatch \"" + name + "\" not found");
				return 0;
			}
			return Stopwatches[name].AverageTimes[round];
		}
	}

	class StopwatchEntry
	{
		public Stopwatch watch;
		public int Rounds;
		public int LastRound;

		public double[] AverageTimes;
		public double[] LastTimes;

		public StopwatchEntry(int rounds = 1)
		{
			watch = new Stopwatch();
			Rounds = rounds;
			LastRound = 0;
			AverageTimes = new double[Rounds];
			LastTimes = new double[Rounds];
		}

		public double RecordRound()
		{
			var time = watch.Round();
			var total = time.TotalMilliseconds;
			AddTime(LastRound++, total);
			return total;
		}

		public void AddTime(int round, double time)
		{
			LastTimes[round] = time;
			AverageTimes[round] = CivMath.dLerp(AverageTimes[round], time, 1.0 / (double)LastRound);
		}
	}
}