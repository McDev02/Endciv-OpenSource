namespace Endciv
{
    [System.Serializable]
    public class MetaData : ISaveable
    {
        public string name;
        public string date;
        public int saveVersion;

		public bool IsDeprecated { get { return saveVersion < SaveFileConverter.lowestSupportedSaveVersion; } }

        public ISaveable CollectData()
        {
            return this;
        }
    }
}