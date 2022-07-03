namespace Endciv
{
    [System.Serializable]
	public class BaseEntityReference
	{
		public string entityUID;
        public string type;

		public BaseEntityReference(string entityUID, string type)
		{
			this.entityUID = entityUID;
            this.type = type;
		}
	}	
}