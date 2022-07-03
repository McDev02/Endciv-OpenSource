using UnityEngine;
namespace Endciv
{
	public abstract class EntityFactory : MonoBehaviour, IExiting
	{
		public SystemsManager systemsManager;
		static int EntityIDCounter;
#if UNITY_EDITOR
		static protected Transform EntityRoot;
#endif
		public virtual void Setup(SystemsManager systemsManager)
		{
			this.systemsManager = systemsManager;
#if UNITY_EDITOR 
			if (EntityRoot == null)
			{
				EntityRoot = new GameObject().transform;
				EntityRoot.name = "Entities";
			}
#endif
		}

		public BaseEntity CreateBaseEntity(int faction, EntityStaticData data, SystemsManager systemsManager, GameObject entityObject = null, Transform root = null)
		{
			if (entityObject == null)
				entityObject = new GameObject();

			if (root != null)
				entityObject.transform.SetParent(root);
#if UNITY_EDITOR
			else
				entityObject.transform.SetParent(EntityRoot);
#endif
			var view = entityObject.AddComponent<EntityFeatureView>();

			var entity = new BaseEntity();
			entity.Initialize(data, systemsManager);
			var entityFeature = new EntityFeature();
			entityFeature.Setup(entity);
			entityFeature.SetView(view);
			view.Feature = entityFeature;
			entity.AttachFeature(entityFeature);
			entity.SetID(EntityIDCounter++);
			entity.factionID = faction;

			view.name = "Entity " + entity.ID.ToString();
			return entity;
		}

		public void OnExit()
		{
			EntityIDCounter = 0;
		}
	}
}