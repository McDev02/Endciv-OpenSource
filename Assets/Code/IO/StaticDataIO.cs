using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Endciv
{
	public class StaticDataIO
	{
		public static StaticDataIO m_Instance;
		public static StaticDataIO Instance
		{
			get
			{
				if (m_Instance == null) m_Instance = new StaticDataIO();
				return m_Instance;
			}
		}

		const bool DebugAllReads = false;

		public string StaticDataPathSerializedObjects { get; private set; }
		public string StaticDataFolderSerializedObjects = "StaticData/";

		private Dictionary<string, StaticDataReaderBase> staticDataReaders;

		public bool IsRunning { get; private set; } = false;

		public StaticDataIO()
		{
			StaticDataPathSerializedObjects = "Assets/Content/Resources/" + StaticDataFolderSerializedObjects;
		}
		public void Run()
		{
			SetupStaticDataReaders();

			ReadAllScriptableObjects(false);
			IsRunning = true;
		}

		void SetupStaticDataReaders()
		{
			if (!Directory.Exists(StaticDataPathSerializedObjects))
				Directory.CreateDirectory(StaticDataPathSerializedObjects);
			staticDataReaders = new Dictionary<string, StaticDataReaderBase>();
			AddStaticDataReader<TraderStaticData>("Traders");
			AddStaticDataReader<NotificationStaticData>("Notifications");
			AddStaticDataReader<MilestoneStaticData>("Milestones");
		}

		void AddStaticDataReader<T>(string path)
			where T : BaseStaticData, new()
		{
			staticDataReaders.Add(path, new StaticDataReader<T>(this, path));
		}

		void ReadScriptableObjects<T>(string key, bool createInstance)
			where T : BaseStaticData, new()
		{
			((StaticDataReader<T>)staticDataReaders[key]).
				ReadScriptableObjects(DebugAllReads, createInstance);
		}

		void ReadAllScriptableObjects(bool createinstance)
		{			
			ReadScriptableObjects<TraderStaticData>("Traders", createinstance);
			ReadScriptableObjects<NotificationStaticData>("Notifications", createinstance);
			ReadScriptableObjects<MilestoneStaticData>("Milestones", createinstance);
		}

		public Dictionary<string, T> GetData<T>(string key)
			where T : BaseStaticData, new()
		{
			if (!IsRunning) Run();
			return ((StaticDataReader<T>)staticDataReaders[key]).GetStaticData();
		}

		public StaticDataReader<T> GetReader<T>(string key)
			where T : BaseStaticData, new()
		{
			if (!IsRunning) Run();
			return (StaticDataReader<T>)staticDataReaders[key];
		}

#if UNITY_EDITOR

		void WriteScriptableObjects<T>(string key)
			where T : BaseStaticData, new()
		{
			((StaticDataReader<T>)staticDataReaders[key]).
				WriteScriptableObjects();
		}

		internal void WriteAllScriptableObjects()
		{			
			WriteScriptableObjects<TraderStaticData>("Traders");
			WriteScriptableObjects<NotificationStaticData>("Notifications");
			WriteScriptableObjects<MilestoneStaticData>("Milestones");
		}
#endif
	}
}