namespace Endciv
{
	/// <summary>
	/// AI Action
	/// </summary>
    public abstract class AIActionBase : ISaveable
    {
        public enum EStatus { Started, Failure, Success, Running }
        public EStatus Status;

        public abstract void OnStart();
        public abstract void Update();
        public abstract void Reset();
        public abstract ISaveable CollectData();
        public abstract void ApplyData(object data);
#if UNITY_EDITOR
        public abstract void DrawUIDetails();
#endif
    }

    public abstract class AIAction<T> : AIActionBase, ILoadable<T>
        where T : ActionSaveData
	{
        public override void ApplyData(object data)
        {
            if (data == null)
                return;
            var saveData = (T)data;
            if (saveData == null)
                return;
            ApplySaveData(saveData);
        }

        public abstract void ApplySaveData(T data);
    }
}