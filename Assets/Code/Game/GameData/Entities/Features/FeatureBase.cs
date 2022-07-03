namespace Endciv
{
    public abstract class FeatureBase : ISaveable
    {
		public bool AutoRun { get; set; }
        public bool IsRunning { get; private set; }
        public BaseEntity Entity { get; private set; }
        public int FactionID => Entity.factionID;
        public SystemsManager SystemsManager { get; private set; }

        protected bool isDestroyed = false;

		/// <summary>
		/// Defines the relative power output of this feature. Can be manipulated by power sources. Is 1 if no power is required.
		/// </summary>
		public float effectivity = 1;
		
		public virtual void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			Entity = entity;
			SystemsManager = entity.systemsManager;
		}

        public virtual void Run(SystemsManager manager)
        {
            //UnityEngine.Debug.Log("Run Feature: " + ToString());
            if (IsRunning) UnityEngine.Debug.LogError("Feature already running!");
            IsRunning = true;
        }

        public virtual void Stop()
        {
            if (!IsRunning)
                return;
            IsRunning = false;
        }

        public virtual void Destroy()
        {
            isDestroyed = true;
            if(IsRunning)
                Stop();
        }

        public virtual void OnFactionChanged(int oldFaction) { }

        public static bool operator ==(FeatureBase x, FeatureBase y)
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

        public static bool operator !=(FeatureBase x, FeatureBase y)
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

        public abstract ISaveable CollectData();
        public abstract void ApplySaveData(object data);
    }

    public abstract class Feature<T> : FeatureBase,
        ISaveable, ILoadable
        where T : ISaveable
    {           
        public override void ApplySaveData(object data)
        {
            if (data == null)
                return;
            var saveData = (T)data;            
            if (saveData == null)
                return;
            ApplyData(saveData);
        }

        public abstract void ApplyData(T data);        
    }
}