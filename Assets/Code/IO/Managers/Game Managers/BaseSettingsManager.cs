using UnityEngine;

namespace Endciv
{
    public abstract class BaseSettingsManager<T> : MonoBehaviour where T : ISaveable
    {
        protected Main main;
        public T CurrentSettings { get; protected set; }
        public T TmpSettings { get; protected set; }

		protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public abstract void Setup(Main main);

        public virtual void DiscardTemporaryValues() { }
        public virtual void ApplyTemporaryValues(bool checkSaftey = true, bool writeToDisk = true) { }
        public virtual void StoreTemporaryValues() { }

        protected abstract void ValidateTemporaryData();
        protected abstract void UpdateSettings();
    }
}