#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


/*******************************************************
 * 
 *  File:       DisabledAttributeDrawer.cs
 *  Description: Custom property drawer for the DisabledAttribute, which disables the GUI for the decorated property in the Unity Inspector.
 *  
 *  Folder:     Assets/Scripts/Editor (REQUIRED TO BE IN AN "Editor" FOLDER)
 *  
 *  Author:     RicardoRibeiroRR
 *  
 *******************************************************/

[CustomPropertyDrawer(typeof(DisabledAttribute))]
public class DisabledAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool oldState = GUI.enabled;
        GUI.enabled = false;

        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = oldState;
    }
}
#endif
