using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Endciv
{
	public sealed class BaseEntity : ISaveable, IRunLogic, IUI3DController, IViewController
	{
		//Unique ID, used for assigning references during Save/Load
		public Guid UID { get; set; } = Guid.Empty;

		public int ID { get; private set; }
		public string IDString { get { return ID.ToString(); } }

		public int factionID;

		public bool IsRunning { get; private set; }

		public bool isDestroyed;

		private UI3DNeeds m_needsInfo;
		public UI3DNeeds NeedsInfo
		{
			get
			{
				if (m_needsInfo == null)
				{
					m_needsInfo = UI3DFactory.Instance.GetUI3DNeeds(this);
				}
				return m_needsInfo;
			}
			set
			{
				m_needsInfo = value;
			}
		}

		public bool NeedsInfoExists
		{
			get
			{
				return m_needsInfo != null;
			}
		}

		public Dictionary<Type, FeatureBase> Features { get; private set; }
		public List<IUI3DController> UI3DControllers { get; private set; }
		public List<IViewController> ViewControllers { get; private set; }
		public int FeatureCount { get { return Features == null ? 0 : Features.Count; } }

		public EntityStaticData StaticData { get; private set; }

		public SystemsManager systemsManager;
		public InventoryFeature Inventory { get; private set; }

		public void RefreshUI3D()
		{
			foreach (var controller in UI3DControllers)
				controller.RefreshUI3D();
		}

		public void SetID(int id)
		{
			ID = id;
		}

		public void Initialize(EntityStaticData staticData, SystemsManager systemsManager)
		{
			StaticData = staticData;
			Features = new Dictionary<Type, FeatureBase>(4);
			UI3DControllers = new List<IUI3DController>(4);
			ViewControllers = new List<IViewController>(4);

			this.systemsManager = systemsManager;

			if (UID == Guid.Empty)
				this.SetRandomEntityGuid();
		}

		public void Dispose()
		{
			Main.Instance.GameManager.SystemsManager.DeregisterEntity(this);
		}

		public void Run()
		{
			systemsManager.RegisterEntity(this);
			if (IsRunning)
			{
				Debug.LogError("Entity already running!");
				return;
			}
			IsRunning = true;
			var count = FeatureCount;
			var keys = Features.Keys.ToArray();
			foreach (var key in keys)
			{
				var feature = Features[key];
				if (feature.AutoRun)
					feature.Run(systemsManager);
			}
		}

		public void Stop()
		{
			if (!IsRunning)
			{
				Debug.LogError("Entity already stopped!");
				return;
			}

			IsRunning = false;
			var count = FeatureCount;
			var keys = Features.Keys.ToArray();
			foreach (var key in keys)
			{
				var feature = Features[key];
				feature.Stop();
			}
		}

		#region IViewController

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view) { }

		public void ShowView()
		{
			foreach (var view in ViewControllers)
			{
				view.ShowView();
			}
		}

		public void HideView()
		{
			foreach (var view in ViewControllers)
			{
				view.HideView();
			}
		}

		public void UpdateView()
		{
			foreach (var view in ViewControllers)
			{
				view.UpdateView();
			}
		}

		public void SelectView()
		{
			foreach (var view in ViewControllers)
			{
				view.SelectView();
			}
		}

		public void DeselectView()
		{
			foreach (var view in ViewControllers)
			{
				view.DeselectView();
			}
		}
		#endregion

		public bool HasFeature<T>() where T : FeatureBase
		{
			return Features != null && Features.ContainsKey(typeof(T));
		}

		public bool HasFeature(Type t)
		{
			return Features != null && Features.ContainsKey(t);
		}

		public T GetFeature<T>() where T : FeatureBase
		{
#if _DEBUG || UNITY_EDITOR
			try
			{
				return (T)Features[typeof(T)];
			}
			catch (Exception e)
			{
				var txt = new System.Text.StringBuilder();
				foreach (var feature in Features.Values)
				{
					txt.AppendLine(feature.GetType().ToString());
				}
				string outName = UID.ToString();
				if (Features != null && Features.ContainsKey(typeof(EntityFeature)))
				{
					outName = ((EntityFeature)Features[typeof(EntityFeature)]).EntityName;
				}
				Debug.LogError($"Feature of Type {typeof(T).ToString()} could not be found. Entity ({outName}) only has:\n{txt}");

				return null;
			}
#endif
			return (T)Features[typeof(T)];
		}

		public FeatureBase GetFeature(Type type)
		{
#if _DEBUG || UNITY_EDITOR
			try
			{
				return Features[type];
			}
			catch (Exception e)
			{
				var txt = new System.Text.StringBuilder();
				foreach (var feature in Features.Values)
				{
					txt.AppendLine(feature.GetType().ToString());
				}
				string outName = UID.ToString();
				if (Features != null && Features.ContainsKey(typeof(EntityFeature)))
				{
					outName = ((EntityFeature)Features[typeof(EntityFeature)]).EntityName;
				}
				Debug.LogError($"Feature of Type {type.ToString()} could not be found. Entity ({outName}) only has:\n{txt}");

				return null;
			}
#endif
			return Features[type];
		}

		public AIAgentFeatureBase GetFirstAIFeature()
		{
			foreach (var feature in Features.Values)
			{
				if (feature is AIAgentFeatureBase)
				{
					return (AIAgentFeatureBase)feature;
				}

			}
			return null;
		}

		public bool AttachFeature(FeatureBase feature)
		{
			var type = feature.GetType();
			if (HasFeature(type))
			{
				Debug.LogError("Feature of type " + CivHelper.GetTypeName(type) + " is already attached.");
				return false;
			}

			Features.Add(feature.GetType(), feature);
			var controller = feature as IUI3DController;
			if (controller != null && !UI3DControllers.Contains(controller))
				UI3DControllers.Add(controller);

			var view = feature as IViewController;
			if (view != null && !ViewControllers.Contains(view))
				ViewControllers.Add(view);

			//Assign defaults
			if (feature is InventoryFeature) Inventory = feature as InventoryFeature;

			//Run if Entity is running
			if (IsRunning) feature.Run(systemsManager);

			return true;
		}

		public T RemoveFeature<T>() where T : FeatureBase
		{
			var type = typeof(T);
			if (Features != null && Features.ContainsKey(type))
			{
				if (Features[type].IsRunning)
					Features[type].Stop();
				var feature = Features[type];
				Features.Remove(type);

				var controller = feature as IUI3DController;
				if (controller != null && UI3DControllers.Contains(controller))
					UI3DControllers.Remove(controller);

				var view = feature as IViewController;
				if (view != null && ViewControllers.Contains(view))
					ViewControllers.Remove(view);

				return (T)feature;
			}
			Debug.LogError("Feature of type " + type.ToString() + " not found.");
			return null;
		}
		
		public ISaveable CollectData()
		{
			EntitySaveData data = new EntitySaveData();
			//Base data		
			data.id = StaticData.ID;
			data.UID = UID.ToString();
			data.factionID = factionID;

			//Features
			data.featureSaveData = new Dictionary<string, object>(FeatureCount);
			foreach (var pair in Features)
			{
				var saveData = pair.Value.CollectData();
				if (saveData == null)
					continue;
				var type = saveData.GetType();
				data.featureSaveData.Add(type.ToString(), saveData);
			}
			return data;
		}

		public void ApplySaveData(ISaveable data)
		{
			var entityData = (EntitySaveData)data;
			if (entityData == null)
				return;
			UID = Guid.Parse(entityData.UID);
			factionID = entityData.factionID;
			this.SetEntityGuid(UID);

			foreach (var pair in Features)
			{
				if (pair.Value is AIAgentFeatureBase)
				{
					//Will be loaded afterwards through the AI System
					continue;
				}
				var saveType = pair.Value.GetType().BaseType.GetGenericArguments()[0];
				var featureData = entityData.GetSaveData(saveType);
				if (featureData == null)
					continue;
				pair.Value.ApplySaveData(featureData);
			}
		}

		public void Destroy()
		{
			if (isDestroyed)
				return;
			Stop();
			foreach (var pair in Features)
			{
				pair.Value.Destroy();
			}
			Dispose();
			if (m_needsInfo != null)
			{
				UI3DFactory.Instance.Recycle(m_needsInfo);
			}
			isDestroyed = true;
		}

		public void ChangeFaction(int faction)
		{
			var oldFaction = factionID;
			factionID = faction;
			var keys = Features.Keys.ToArray();
			foreach (var key in keys)
			{
				Features[key].OnFactionChanged(oldFaction);
			}
		}

		public void Die()
		{
			GetFeature<EntityFeature>().Health.Value = 0;
			GetFeature<EntityFeature>().IsAlive = false;
			Stop();
		}

		#region Disposable
		public static bool operator ==(BaseEntity x, BaseEntity y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (ReferenceEquals(x, null))
			{
				if (ReferenceEquals(y, null))
				{
					return true;
				}
				else
				{
					return y.isDestroyed;
				}
			}
			if (ReferenceEquals(y, null))
			{
				if (ReferenceEquals(x, null))
				{
					return true;
				}
				else
				{
					return x.isDestroyed;
				}
			}
			return x.Equals(y);
		}

		public static bool operator !=(BaseEntity x, BaseEntity y)
		{
			return !(x == y);
		}

		public override bool Equals(object obj)
		{
			if (obj == null && isDestroyed)
				return true;
			if (obj == null && !isDestroyed)
				return false;
			return ReferenceEquals(obj, this);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion
	}
}