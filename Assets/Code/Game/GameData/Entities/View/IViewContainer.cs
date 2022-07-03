using UnityEngine;

namespace Endciv
{
	public interface IFeatureViewContainer
	{
		GameObject GetFeatureViewInstance(int variationID = -1);
		int GetNextViewID(int currentID);
		int GetRandomViewID();		
	}	
}
