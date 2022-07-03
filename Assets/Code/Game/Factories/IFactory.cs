using UnityEngine;
namespace Endciv
{
public interface IFactory
	{
		Transform GetModelObject(string modelID, int variationID = -1);
		Transform GetRandomModelObject(string modelID, out int variationID);
		int GetNextViewID(string modelID, int currentID);
		int GetRandomViewID(string modelID);
	}
}