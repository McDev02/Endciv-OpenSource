using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// By http://totallynotaliens.com/index.php?site=blog&postnr=205
/// </summary>

public static class MoveArrayElementEditor
{
	[InitializeOnLoadMethod]
	static void Start()
	{
		// For more information visit: https://docs.unity3d.com/ScriptReference/EditorApplication-contextualPropertyMenu.html
		EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
	}

	static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
	{
		if (property == null) return;

		// Path of an array is "PropertyName.Array.data[arraylength]", so let's look if the property is an array
		string propertyPath = property.propertyPath;
		if (!propertyPath.Contains(".Array.data[") || !propertyPath.EndsWith("]")) return;

		// Split the property path and find out the array length
		string[] fullPathSplit = propertyPath.Split('.');
		string ending = fullPathSplit[fullPathSplit.Length - 1];
		int index = 0;
		if (!int.TryParse(ending.Replace("data[", "").Replace("]", ""), out index)) return;

		// Rebuild the path without the ".Array.data[arraylength]" stuff
		string pathToArray = string.Empty;
		for (int i = 0; i < fullPathSplit.Length - 2; i++)
		{
			if (i < fullPathSplit.Length - 3)
			{
				pathToArray = string.Concat(pathToArray, fullPathSplit[i], ".");
			}
			else
			{
				pathToArray = string.Concat(pathToArray, fullPathSplit[i]);
			}
		}

		// Get the serialized target object and the property of the array with the path
		Object targetObject = property.serializedObject.targetObject;
		SerializedObject serializedTargetObject = new SerializedObject(targetObject);

		SerializedProperty serializedArray = serializedTargetObject.FindProperty(pathToArray);
		int arrayLength = serializedArray.arraySize;

		if (serializedArray == null) return;

		// Show context menu entry only if the user can move it up or down
		if (index > 0)
		{
			menu.AddItem(new GUIContent("Move Up (Index - 1)"), false,
						  () => MoveDown(serializedTargetObject, serializedArray, index));
		}

		if (index < arrayLength - 1)
		{
			menu.AddItem(new GUIContent("Move Down (Index + 1)"), false,
						  () => MoveUp(serializedTargetObject, serializedArray, index));
		}
	}

	static public void MoveDown(SerializedObject target, SerializedProperty array, int index)
	{
		array.MoveArrayElement(index, index - 1);
		target.ApplyModifiedProperties();
	}

	static public void MoveUp(SerializedObject target, SerializedProperty array, int index)
	{
		array.MoveArrayElement(index, index + 1);
		target.ApplyModifiedProperties();
	}
}