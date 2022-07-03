using UnityEngine;
using System;
using Object = System.Object;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Endciv
{
	/// <summary>
	/// Base manager to which all other Data Managers extend. 
	/// Contains commonly used properties like file paths, and 
	/// serialization/deserialization methods for manipulating game data.
	/// </summary>
	public class BaseDataManager
	{
		public const string FILETYPE = ".dat";

		protected string RootFolder
		{
			get
			{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				path = Directory.GetParent(path).ToString();
				path += "\\LocalLow\\Crowbox\\Endciv\\";
				return path;
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Crowbox/Endciv/";
#endif
			}
		}

		protected bool WriteToDisk(Object obj, string path, bool isReadable)
		{
			try
			{	
				using(FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
				{
					var formatter = new BinaryFormatter();
					formatter.Serialize(fs, obj);
				}											
				return true;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				return false;
			}
		}

		protected bool LoadFromDisk<T>(string path, out T data) where T : ISaveable
		{
			data = default(T);
			if (!File.Exists(path))
				return false;

			bool hasLoaded = false;			
			try
			{
				using(FileStream fs = new FileStream(path, FileMode.Open))
				{					
					var formatter = new BinaryFormatter();
					data = (T)formatter.Deserialize(fs);					
					hasLoaded = true;
				}				
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);				
				hasLoaded = false;				
			}
			return hasLoaded;
		}
	}
}