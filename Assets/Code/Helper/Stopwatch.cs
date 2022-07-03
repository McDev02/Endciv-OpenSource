using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Endciv
{
    public class Stopwatch
	{
		struct LogPair
		{
			public TimeSpan Elapsed;
			public string RoundName;

			public LogPair(TimeSpan elapsed, string name)
			{
				Elapsed = elapsed;
				RoundName = name;
			}
		}

		System.Diagnostics.Stopwatch m_Watch;
		System.Diagnostics.Stopwatch m_RoundWatch;

		TimeSpan m_RoundElapsed;
		TimeSpan m_TotalElapsed;

		string watchName;

		public TimeSpan TotalElapsed { get { return m_TotalElapsed; } }
		public TimeSpan RoundElapsed { get { return m_RoundElapsed; } }

		List<LogPair> m_Logs;
		bool m_Paused;

		public Stopwatch() : this("Stopwatch") { }

		public Stopwatch(string name)
		{
			m_TotalElapsed = new TimeSpan();
			m_Watch = System.Diagnostics.Stopwatch.StartNew();
			m_RoundWatch = System.Diagnostics.Stopwatch.StartNew();
			watchName = name;
			m_Logs = new List<LogPair>();
		}

		public void Start()
		{
			m_Watch.Start();
			m_RoundWatch.Start();
		}

		public void Stop()
		{
			m_Watch.Stop();
			m_RoundWatch.Stop();
		}

		public void Reset()
		{
			m_Watch.Reset();
			m_RoundWatch.Reset();
			m_TotalElapsed = new TimeSpan();
		}

		public TimeSpan Round()
		{
			m_RoundWatch.Stop();
			m_RoundElapsed = m_RoundWatch.Elapsed;
			m_TotalElapsed += m_RoundElapsed;
			m_RoundWatch.Reset();
			m_RoundWatch.Start();

			return m_RoundElapsed;
		}

		internal void LogRound()
		{
			LogRound(watchName);
		}
		internal void LogRoundMilliseconds()
		{
			LogRoundMilliseconds(watchName);
		}
		internal void LogRound(string message,bool inMilliseconds=true)
		{
			var elapsed = Round();
			m_Logs.Add(new LogPair(elapsed, message));
			if(inMilliseconds)
			Logger.Log(message + ": " + elapsed.TotalMilliseconds.ToString());
			else
			UnityEngine.Debug.Log(message + ": " + elapsed.ToString());
		}
		internal void LogRoundMilliseconds(string message)
		{
			var elapsed = Round();
			m_Logs.Add(new LogPair(elapsed, message));
			UnityEngine.Debug.Log(message + ": " + elapsed.TotalMilliseconds.ToString()+"ms");
		}
		internal void LogTotal(string message)
		{
			Stop();
			var elapsed = m_Watch.Elapsed;
			m_Logs.Add(new LogPair(elapsed, message));
			UnityEngine.Debug.Log(message + ": " + elapsed.ToString());
		}

		internal void Write()
		{
			string filename = "Stopwatch_" + watchName;
			filename += "_" + DateTime.Now.ToFileTime();
			filename += ".xml";
			string path = "C:/Stopwatch/";
			UnityEngine.Debug.Log("Write Stopwatch File: " + (path + filename));
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			FileStream stream;
			if (!File.Exists(path + filename))
				stream = File.Create(path + filename);
			else
				stream = File.OpenWrite(path + filename);

			XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
			writer.WriteStartDocument(true);
			{
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 2;

				writer.WriteStartElement("Stopwatch benchmark");
				{
					for (int i = 0; i < m_Logs.Count; i++)
					{
						writer.WriteStartElement(m_Logs[i].RoundName);
						writer.WriteValue(m_Logs[i].Elapsed.ToString());
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
			writer.WriteEndDocument();
			writer.Close();
			stream.Close();
			UnityEngine.Debug.Log("Stopwatch file generated!");
		}
	}
}