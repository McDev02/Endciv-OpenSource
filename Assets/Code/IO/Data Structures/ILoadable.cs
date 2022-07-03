namespace Endciv
{/// <summary>
 /// Basic interface for all classes that handle their own data loading 
 /// </summary>
    public interface ILoadable
    {
        void ApplySaveData(object data);
    }

    public interface ILoadable<T> where T : ISaveable
    {
        void ApplySaveData(T data);
    }
}