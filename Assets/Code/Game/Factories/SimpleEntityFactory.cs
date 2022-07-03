using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public sealed class SimpleEntityFactory
	{
		private Transform masterRoot;
		private Dictionary<string, Transform> entityRoots;
		private Dictionary<string, int> entityCounters;

		private SystemsManager systemsManager;

		//Eventually every entity's static data will be in this dictionary
		//(structures, units, traders, items, notifications, etc)
		public Dictionary<string, EntityStaticData> EntityStaticData { get; private set; }

		//Static Data IDs, classified by Feature type
		private Dictionary<Type, HashSet<string>> StaticDataIDReferences { get; set; }
		//Feature Static Data classified by type and ID
		private Dictionary<Type, Dictionary<string, FeatureStaticDataBase>> FeatureStaticData { get; set; }
		private Dictionary<Type, Dictionary<string, FeatureStaticDataBase>> FeatureStaticDataByFeature { get; set; }

		/// <summary>
		/// Loads and configures StaticData resources and Lookup Tables
		/// </summary>
		/// <param name="staticDataPath">Path of StaticData instances in Resources Folder</param>
		/// <param name="systemsManager">Systems Manager reference for BaseEntity dependency injection</param>
		public void Setup(string staticDataPath, SystemsManager systemsManager)
		{
			this.systemsManager = systemsManager;

			//Static data loading
			EntityStaticData = new Dictionary<string, EntityStaticData>();
			StaticDataIDReferences = new Dictionary<Type, HashSet<string>>();
			FeatureStaticData = new Dictionary<Type, Dictionary<string, FeatureStaticDataBase>>();
			FeatureStaticDataByFeature = new Dictionary<Type, Dictionary<string, FeatureStaticDataBase>>();

			entityRoots = new Dictionary<string, Transform>();
			entityCounters = new Dictionary<string, int>();

			var data = Resources.LoadAll<EntityStaticData>(staticDataPath);
			foreach (var entry in data)
			{
				entry.Init();
				//Populate General Pool
				EntityStaticData.Add(entry.ID, entry);

				foreach (var feature in entry.FeatureStaticData)
				{
					var type = feature.GetType();
					var featureType = type.BaseType.GetGenericArguments()[0];
					if (!StaticDataIDReferences.ContainsKey(type))
					{
						StaticDataIDReferences.Add(type, new HashSet<string>());
						FeatureStaticData.Add(type, new Dictionary<string, FeatureStaticDataBase>());
						FeatureStaticDataByFeature.Add(featureType, new Dictionary<string, FeatureStaticDataBase>());
					}
					StaticDataIDReferences[type].Add(entry.ID);
					FeatureStaticData[type].Add(entry.ID, feature);
					FeatureStaticDataByFeature[featureType].Add(entry.ID, feature);
				}
			}

			if (Application.isPlaying)
			{
				masterRoot = new GameObject("Entities").transform;
				RegisterRoot("Base Entities");
			}
		}

		private void RegisterRoot(string rootName)
		{
			var root = new GameObject(rootName).transform;
			root.parent = masterRoot;
			entityRoots.Add(rootName, root);
			entityCounters.Add(rootName, 0);
		}

		/// <summary>
		/// Returns a List of IDs for all Entity Static Data containing 
		/// feature static data of type T
		/// </summary>
		/// <typeparam name="T">Derived FeatureStaticDataBase</typeparam>
		/// <returns></returns>
		public HashSet<string> GetStaticDataIDList<T>()
			where T : FeatureStaticDataBase
		{
			if (!StaticDataIDReferences.ContainsKey(typeof(T)))
				return new HashSet<string>();
			return StaticDataIDReferences[typeof(T)];
		}

		/// <summary>
		/// Retuns FeatureStaticData instance of specified FeatureStaticData type,
		/// of specific entry ID
		/// </summary>
		/// <typeparam name="T">Derived FeatureStaticDataBase</typeparam>
		/// <param name="id">Entity Static Data ID</param>
		/// <returns>FeatureStaticData instance</returns>
		public T GetStaticData<T>(string id)
			where T : FeatureStaticDataBase
		{
			return (T)FeatureStaticData[typeof(T)][id];
		}

		/// <summary>
		/// Returns StaticData of provided Runtime Feature type and id
		/// </summary>
		/// <typeparam name="T">StaticData type to return</typeparam>
		/// <param name="id">Entity ID</param>
		/// <param name="featureType">Runtime feature type</param>
		/// <returns></returns>
		public T GetStaticDataByFeature<T>(string id, Type featureType)
			where T : FeatureStaticDataBase
		{
			return (T)FeatureStaticDataByFeature[featureType][id];
		}

		/// <summary>
		/// Creates runtime BaseEntity instance by referencing
		/// EntityStaticData properties by ID
		/// </summary>
		/// <param name="entityID">Associated EntityStaticData ID</param>
		/// <returns>Runtime BaseEntity with attached Features</returns>
		public BaseEntity CreateInstance(string entityID, string guid = null, FactoryParams args = null)
		{
			var entity = new BaseEntity();

			var entityStaticData = EntityStaticData[entityID];
			entity.Initialize(entityStaticData, systemsManager);
			foreach (var featureStaticData in entityStaticData.FeatureStaticData)
			{
				//Create feature by static data call
				var feature = featureStaticData.GetRuntimeFeature();
				feature.AutoRun = featureStaticData.autoRun;
				//Setup is abstract, the implementation's code is executed
				var featureType = feature.GetType();
				if (args != null && args.HasParams(featureType))
					feature.Setup(entity, args.GetParams(featureType));
				else
					feature.Setup(entity);
				entity.AttachFeature(feature);
			}
			if (entity.ViewControllers.Count > 0)
			{
				GenerateEntityViews(entity);
			}
			if (!string.IsNullOrEmpty(guid))
			{
				entity.SetEntityGuid(Guid.Parse(guid));
			}
			entity.Run();
			return entity;
		}

		/// <summary>
		/// Loops over all registered ViewControllers,
		/// dynamically generates views for each one, 
		/// and assigns them to the corresponding features
		/// </summary>
		/// <param name="entity">Runtime BaseEntity</param>
		private void GenerateEntityViews(BaseEntity entity)
		{
			//Entity that manages views must always contain EntityFeature
			if (!entity.HasFeature<EntityFeature>())
				throw new InvalidOperationException
					("Cannot generate views for entities without EntityFeatureStaticData!");

			//Create the view of the entity feature dynamically, which serves as the parent 
			//of all other view features
			var entityFeature = entity.GetFeature<EntityFeature>();
			var root = new GameObject();

			//entity category
			var rootCategory = GetFirstEntityCategory(entity.StaticData);
			if (!entityRoots.ContainsKey(rootCategory))
				RegisterRoot(rootCategory);
			var entityRoot = entityRoots[rootCategory];
			root.transform.SetParent(entityRoot);
			var entityView = root.AddComponent<EntityFeatureView>();
			entityFeature.SetView(entityView);
			entityView.Feature = entityFeature;
			root.name = $"#{entityCounters[rootCategory]} {entity.StaticData.ID}";
			entityCounters[rootCategory]++;

			//Loop through all view controllers
			foreach (var viewController in entity.ViewControllers)
			{
				//Possibly redundant casting check
				var feature = (FeatureBase)viewController;
				if (feature == null)
					continue;

				//We skip the entity feature that already has a setup view
				if (feature == entityFeature)
					continue;

				//Check if feature's static data has assigned views				
				var staticData = FeatureStaticDataByFeature[feature.GetType()][entity.StaticData.ID];
				if (!(staticData is IFeatureViewContainer))
				{
					continue;
				}

				//Static data provides an instance of the view				
				var viewModel = ((IFeatureViewContainer)staticData).GetFeatureViewInstance(viewController.CurrentViewID);
				if (viewModel == null)
					throw new NullReferenceException($"View Model of {entity.StaticData.name} is not assigned. Fix this in Data and assign Dummy at least.");

				//Set the instance's parent to be the root view
				viewModel.transform.parent = root.transform;
				viewModel.transform.localPosition = Vector3.zero;
				viewModel.transform.localEulerAngles = Vector3.zero;

				var views = viewModel.GetComponents<FeatureViewBase>();
				if (views != null && views.Length > 0)
				{
					foreach (var view in views)
					{
						var viewType = GetFirstGenericType(view.GetType());
						if (viewType.GenericTypeArguments.Length <= 0)
							continue;
						var featureType = viewType.GenericTypeArguments[0];
						if (!entity.Features.ContainsKey(featureType))
							continue;
						var viewFeature = entity.Features[featureType];
						var controller = viewFeature as IViewController;
						if (controller == null)
							continue;
						controller.SetView(view);
						view.Setup(viewFeature);
					}
				}

				var flexible = viewModel.GetComponent<FlexibleStructureView>();
				if (flexible != null && entity.HasFeature<GridObjectFeature>())
				{
					//Caution about this GridObjectData!!!
					var gridObjectData = entity.GetFeature<GridObjectFeature>().GridObjectData;
					flexible.flexibleBase.localScale = new Vector3(gridObjectData.Rect.Width, 1, gridObjectData.Rect.Length) * GridMapView.GridTileFactor;
				}
			}
		}

		/// <summary>
		/// Scans the Type's ancestors and returns the 
		/// first ancestor with Generic Arguments, or MonoBehaviour 
		/// if none is found
		/// </summary>
		/// <param name="type">Type to lookup</param>
		/// <returns>Generic Ancestor or MonoBehaviour</returns>
		private Type GetFirstGenericType(Type type)
		{
			Type genericType = type.BaseType;
			while (genericType != typeof(MonoBehaviour) && genericType.GenericTypeArguments.Length <= 0)
				genericType = genericType.BaseType;
			return genericType;
		}

		private string GetFirstEntityCategory(EntityStaticData staticData)
		{
			foreach (var feature in staticData.FeatureStaticData)
			{
				var att = feature.GetType().GetAttribute<EntityCategoryAttribute>();
				if (att == null)
				{
					continue;
				}
				if (string.IsNullOrEmpty(att.categoryName))
					continue;
				return att.categoryName;
			}
			return "Base Entities";
		}
	}
}
