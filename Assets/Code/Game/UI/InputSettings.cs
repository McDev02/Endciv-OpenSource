using UnityEngine;

namespace Endciv
{
	/*public enum ActionKey
	{
		//Avoid to change the order. Append new actions at the end. Removing actions will mess up default assignments
		NotUsed = 0,
		RotateCameraCW,
		RotateCameraCCW,
		ZoomCameraIn,
		ZoomCameraOut,
		RotateBuildingCW,
		RotateBuildingCCW,
		ToggleMap,
		DemolishBuilding,
		TogglePause,
		GameSpeedSlower,
		GameSpeedFaster,
		GameSpeed1,
		GameSpeed2,
		GameSpeed3,
		GoToTownCenter,
		CycleUnitsWithCamera,
		CycleNotificationLocations,
		CameraToggleFollow,
		ToggleGUI,
		QuickSave,
		ToggleIngameMenu,
		OpenSaveScreen,
		OpenLoadScreen,
		ToggleFPSPanel,
		ToggleHelpScreen,
		CycleLayers,
		HideLayers,
	}*/

	public class InputSettings : ScriptableObject
	{
        public ActionInput[] actions;
    }
}
