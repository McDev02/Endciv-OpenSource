using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class HardwareBenchmark : SceneSingleton<HardwareBenchmark>
	{
		string m_FilePrefix = "benchmark";
		string m_Path = "benchmarks/";

		int interval = 10;
		bool IsRunning;
		Coroutine m_BenchmarkRoutine;

		struct BenchmarkEntry
		{
			public float FPSCPUMin;
			public float FPSCPUMax;
			public float FPSCPUAvr;

			public float FPSGPUMin;
			public float FPSGPUMax;
			public float FPSGPUAvr;
		}
		struct FrameBenchmarkEntry
		{
			public float FPSCPU;
			public float FPSGPU;
		}

		List<BenchmarkEntry> m_BenchmarkList;
		FrameBenchmarkEntry[] m_FrameBenchmark;

		void Start()
		{
			m_Path = Directory.GetParent(Application.dataPath) + "/" + m_Path;
		}

		protected override void OnDestroy()
		{
			StopBenchmark();
			base.OnDestroy();
		}

		IEnumerator PerformanceBenchmark()
		{
			int counter = 0;
			while (IsRunning)
			{
				if (counter < interval)
				{
					var frame = m_FrameBenchmark[counter];
					//frame.FPSCPU = HUDFPS.Instance.FPSCPU;
					//frame.FPSGPU = HUDFPS.Instance.FPSGPU;
					m_FrameBenchmark[counter] = frame;
					counter++;
				}
				else
				{
					counter = 0;
					BenchmarkEntry entry = new BenchmarkEntry();
					for (int i = 0; i < m_FrameBenchmark.Length; i++)
					{
						var frame = m_FrameBenchmark[i];
						entry.FPSCPUAvr += frame.FPSCPU;
						entry.FPSGPUAvr += frame.FPSGPU;
						entry.FPSCPUMin = Mathf.Min(entry.FPSCPUMin, frame.FPSCPU);
						entry.FPSCPUMax = Mathf.Max(entry.FPSCPUMax, frame.FPSCPU);
						entry.FPSGPUMin = Mathf.Min(entry.FPSGPUMin, frame.FPSGPU);
						entry.FPSGPUMax = Mathf.Max(entry.FPSGPUMax, frame.FPSGPU);
					}
					entry.FPSCPUAvr /= (float)m_FrameBenchmark.Length;
					entry.FPSGPUAvr /= (float)m_FrameBenchmark.Length;
					m_BenchmarkList.Add(entry);
				}

				yield return new WaitForSeconds(1);
			}
			yield return null;
		}

		public void StopBenchmark()
		{
			if (IsRunning)
			{
				IsRunning = false;
				StopCoroutine(m_BenchmarkRoutine);
				WriteBenchmark();
			}
		}

		public void StartBenchmark()
		{
			//if (!HUDFPS.Instance.enabled)
			//	HUDFPS.Instance.enabled = true;

			m_BenchmarkList = new List<BenchmarkEntry>();
			m_FrameBenchmark = new FrameBenchmarkEntry[interval];
			for (int i = 0; i < m_FrameBenchmark.Length; i++)
			{
				m_FrameBenchmark[i] = new FrameBenchmarkEntry();
			}

			IsRunning = true;
			m_BenchmarkRoutine = StartCoroutine(PerformanceBenchmark());
		}

		void WriteBenchmark()
		{
			string filename = m_FilePrefix;
			filename += "_" + DateTime.Now.ToFileTime();
			filename += ".xml";

			Debug.Log("Write Benchmark File: " + (m_Path + filename));
			if (!Directory.Exists(m_Path))
				Directory.CreateDirectory(m_Path);

			FileStream stream;
			if (!File.Exists(m_Path + filename))
				stream = File.Create(m_Path + filename);
			else
				stream = File.OpenWrite(m_Path + filename);

			XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
			writer.WriteStartDocument(true);
			{
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 2;

				writer.WriteStartElement("System Info");
				{
					writer.WriteStartElement("Operating System");
					writer.WriteValue(SystemInfo.operatingSystem);
					writer.WriteEndElement();

					writer.WriteStartElement("DeviceUID");
					writer.WriteValue(SystemInfo.deviceUniqueIdentifier);
					writer.WriteEndElement();
					writer.WriteStartElement("DeviceModel");
					writer.WriteValue(SystemInfo.deviceModel);
					writer.WriteEndElement();
					writer.WriteStartElement("Processors");
					writer.WriteValue(SystemInfo.processorCount);
					writer.WriteEndElement();
					writer.WriteStartElement("Processortype");
					writer.WriteValue(SystemInfo.processorType);
					writer.WriteEndElement();
					writer.WriteStartElement("SystemMemory");
					writer.WriteValue(SystemInfo.systemMemorySize);
					writer.WriteEndElement();

					writer.WriteStartElement("GPU-ID");
					writer.WriteValue(SystemInfo.graphicsDeviceID.ToString());
					writer.WriteEndElement();
					writer.WriteStartElement("GPUName");
					writer.WriteValue(SystemInfo.graphicsDeviceName);
					writer.WriteEndElement();
					writer.WriteStartElement("GPUType");
					writer.WriteValue(SystemInfo.graphicsDeviceType.ToString());
					writer.WriteEndElement();
					writer.WriteStartElement("GPUVendor");
					writer.WriteValue(SystemInfo.graphicsDeviceVendor);
					writer.WriteEndElement();
					writer.WriteStartElement("GPUVersion");
					writer.WriteValue(SystemInfo.graphicsDeviceVersion);
					writer.WriteEndElement();
					writer.WriteStartElement("GPUMemory");
					writer.WriteValue(SystemInfo.graphicsMemorySize);
					writer.WriteEndElement();
					writer.WriteStartElement("GPUShaderLevel");
					writer.WriteValue(SystemInfo.graphicsShaderLevel);
					writer.WriteEndElement();
					writer.WriteStartElement("GPUmaxTextureSize");
					writer.WriteValue(SystemInfo.maxTextureSize);
					writer.WriteEndElement();
				}

				writer.WriteStartElement("Performance");
				{
					BenchmarkEntry averageEntry = new BenchmarkEntry();
					for (int i = 0; i < m_BenchmarkList.Count; i++)
					{
						var frame = m_BenchmarkList[i];
						averageEntry.FPSCPUAvr += frame.FPSCPUAvr;
						averageEntry.FPSGPUAvr += frame.FPSGPUAvr;

						averageEntry.FPSCPUMin = Mathf.Min(averageEntry.FPSCPUMin, frame.FPSCPUMin);
						averageEntry.FPSCPUMax = Mathf.Max(averageEntry.FPSCPUMax, frame.FPSCPUMax);
						averageEntry.FPSGPUMin = Mathf.Min(averageEntry.FPSGPUMin, frame.FPSGPUMin);
						averageEntry.FPSGPUMax = Mathf.Max(averageEntry.FPSGPUMax, frame.FPSGPUMax);
					}
					averageEntry.FPSCPUAvr /= m_BenchmarkList.Count;
					averageEntry.FPSGPUAvr /= m_BenchmarkList.Count;

					writer.WriteStartElement("Average Data");
					{
						writer.WriteStartElement("CPU FPS Average");
						writer.WriteValue(averageEntry.FPSCPUAvr.ToString("0.00"));
						writer.WriteEndElement();
						writer.WriteStartElement("CPU FPS Min");
						writer.WriteValue(averageEntry.FPSCPUMin.ToString("0.00"));
						writer.WriteEndElement();
						writer.WriteStartElement("CPU FPS Max");
						writer.WriteValue(averageEntry.FPSCPUMax.ToString("0.00"));
						writer.WriteEndElement();

						writer.WriteStartElement("GPU FPS Average");
						writer.WriteValue(averageEntry.FPSGPUAvr.ToString("0.00"));
						writer.WriteEndElement();
						writer.WriteStartElement("GPU FPS Min");
						writer.WriteValue(averageEntry.FPSGPUMin.ToString("0.00"));
						writer.WriteEndElement();
						writer.WriteStartElement("GPU FPS Max");
						writer.WriteValue(averageEntry.FPSGPUMax.ToString("0.00"));
						writer.WriteEndElement();
					}
					writer.WriteEndElement();

					for (int i = 0; i < m_BenchmarkList.Count; i++)
					{
						var frame = m_BenchmarkList[i];
						writer.WriteStartElement("Data " + i);
						{
							writer.WriteStartElement("CPU FPS Average");
							writer.WriteValue(frame.FPSCPUAvr.ToString("0.00"));
							writer.WriteEndElement();
							writer.WriteStartElement("CPU FPS Min");
							writer.WriteValue(frame.FPSCPUMin.ToString("0.00"));
							writer.WriteEndElement();
							writer.WriteStartElement("CPU FPS Max");
							writer.WriteValue(frame.FPSCPUMax.ToString("0.00"));
							writer.WriteEndElement();

							writer.WriteStartElement("GPU FPS Average");
							writer.WriteValue(frame.FPSGPUAvr.ToString("0.00"));
							writer.WriteEndElement();
							writer.WriteStartElement("GPU FPS Min");
							writer.WriteValue(frame.FPSGPUMin.ToString("0.00"));
							writer.WriteEndElement();
							writer.WriteStartElement("GPU FPS Max");
							writer.WriteValue(frame.FPSGPUMax.ToString("0.00"));
							writer.WriteEndElement();
						}
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
			writer.WriteEndDocument();
			writer.Close();
			stream.Close();
			UnityEngine.Debug.Log("Benchmark file generated!");
		}

		/*
		if (Input.GetKeyDown(KeyCode.G))
		{
			string filePath = GameSettings.Instance.m_DataPath + "Endciv_Data/output_log.txt";
		filePath = filePath.Replace('/', '\\');
			string command = string.Format("/select,\"{0}\"", filePath);
		System.Diagnostics.Process.Start("explorer.exe", command);
			UnityEngine.Debug.Log("Explorer: " + command);
		}*/
	}
}