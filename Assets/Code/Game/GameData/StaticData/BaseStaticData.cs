using UnityEngine;
namespace Endciv
{
	public class BaseStaticData : ScriptableObject
	{
		public string ID { get { return name; } }
		public string ModelID;
		
		public virtual void Init() { }
	}
}