using System.Collections.Generic;
namespace Endciv
{
	public class EntitySystem : BaseGameSystem
	{
		protected List<BaseEntity> Entities;

		public int EntityCount { get; private set; }
		SystemsManager Manager;
		int factions;

		public delegate void EntityRegister(BaseEntity entity);
		public event EntityRegister OnRegisterEntity;
		public event EntityRegister OnDeregisterEntity;

		public EntitySystem(int factions, SystemsManager manager) : base()
		{
			this.factions = factions;
			Manager = manager;
			Entities = new List<BaseEntity>(32);
			EntityCount = 0;
		}

		public override void UpdateGameLoop()
		{
		}

		private void OnEntityDies(BaseEntity entity)
		{
			/*
			var time = Manager.timeManager.CurrentTotalTick - entity.GetFeature<EntityFeature>().BornTimeTick;
			float days = time / (float)Manager.timeManager.dayTickLength;
			UnityEngine.Debug.Log($"Entity lived for: {time} Ticks ({days.ToString("0.0")} Days)");
			*/
			if (entity.HasFeature<CitizenAIAgentFeature>())
			{
				if (entity.factionID == SystemsManager.MainPlayerFaction)
					Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger("totalCitizenDead");
			}
			entity.Die();
		}

		public override void UpdateStatistics()
		{
		}

		public void KillEntity(BaseEntity entity)
		{
			if (entity.GetFeature<EntityFeature>().IsAlive)
				OnEntityDies(entity);
		}

		public void DestroyEntity(BaseEntity entity)
		{
			Manager.DeregisterEntity(entity);
			entity.Destroy();
		}

		public void AddDamage(BaseEntity entity, float amount)
		{
			var entityFeature = entity.GetFeature<EntityFeature>();
			entityFeature.Health.Value -= amount;
			if (entityFeature.IsAlive && entityFeature.Health.Value <= 0)
				OnEntityDies(entity);
		}

		internal virtual void RegisterEntity(BaseEntity Entity)
		{
			if (Entities.Contains(Entity))
				UnityEngine.Debug.LogError(typeof(BaseEntity).ToString() + " already registered.");
			else
			{
				Entities.Add(Entity);
				OnRegisterEntity?.Invoke(Entity);
			}
			EntityCount = Entities.Count;
		}

		internal virtual void DeregisterEntity(BaseEntity Entity)
		{
			if (!Entities.Contains(Entity))
				return;
			else
			{
				OnDeregisterEntity?.Invoke(Entity);
				Entities.Remove(Entity);
			}

			EntityCount = Entities.Count;
		}

		internal void SwitchFaction(BaseEntity entity, int faction)
		{
			if (faction < 0 || faction >= factions || entity.factionID == faction) return;
			entity.ChangeFaction(faction);
		}
	}
}