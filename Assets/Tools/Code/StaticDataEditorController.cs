using UnityEngine;
using System.Collections.Generic;
namespace Endciv
{
	public class StaticDataEditorController : MonoBehaviour
	{
		[SerializeField] StaticDataEditorWindow EditorWindow;

		Dictionary<string, EntityStaticData> StructureData;

		private void Awake()
		{
		}

		private void Start()
		{
			EditorWindow.UpdateDataList(StaticDataIO.Instance.GetData<EntityStaticData>("Structures"));
		}
	}
}