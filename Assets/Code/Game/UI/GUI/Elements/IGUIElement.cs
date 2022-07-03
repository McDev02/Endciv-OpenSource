using UnityEngine;
namespace Endciv
{
	public class GUIInteractable : MonoBehaviour
	{

		void OnMouseOver()
		{
			//If your mouse hovers over the GameObject with the script attached, output this message
			Debug.Log("Mouse is over GameObject.");
		}

		void OnMouseExit()
		{
			//The mouse is no longer hovering over the GameObject so output this message each frame
			Debug.Log("Mouse is no longer on GameObject.");
		}
	}
}