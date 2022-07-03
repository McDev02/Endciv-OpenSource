using System.Collections.Generic;

namespace Endciv
{
	public enum EResourceType { Undefined, Material, Food, Item }
	public enum EItemType { Tool, Weapon }

	/// <summary>
	/// Defines a stack of resources
	/// </summary>
	[System.Serializable]
	public class ResourceStack
	{
		public int Amount;
		[StaticDataID("StaticData/SimpleEntities/Items")]
		public string ResourceID;   //Can be any type of resource, unique

		public ResourceStack(string id, int amount)
		{
			ResourceID = id;
			Amount = amount;
		}
	}	

	public static class ResourceStackExtensions
    {
        public static ResourceStack[] ToResourceStackArray(this Dictionary<string, int> dict)
        {
            var arr = new ResourceStack[dict.Count];
            int i = 0;
            foreach (var pair in dict)
            {
                arr[i] = new ResourceStack(pair.Key, pair.Value);
                i++;
            }
            return arr;
        }

		public static List<ResourceStack> ToResourceStackList(this Dictionary<string, int> dict)
		{
			var arr = new List<ResourceStack>(dict.Count);			
			foreach (var pair in dict)
			{
				arr.Add(new ResourceStack(pair.Key, pair.Value));				
			}
			return arr;
		}
	}		
}