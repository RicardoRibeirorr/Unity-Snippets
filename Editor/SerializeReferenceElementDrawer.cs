/**
 *  Drawer for classes and subclasses that use [SerializeReference] attribute.
 * 
 * 
 * @usage
 * 
 * Create classes and subclasses that inherit from a base class or interface, and use the [SerializeReference] 
 * attribute to allow polymorphic serialization.
 * example of drawerable class:
 * ```csharp
 *        [Serializable]
 *       public abstract class Behaviour
 *        {
 *           public string name;
 *        }
 *
 *        [Serializable]
 *        public class AttackBehaviour : Behaviour
 *        {
 *            public int damage;
 *        }
 *
 *        [Serializable]
 *        public class HealBehaviour : Behaviour
 *        {
 *            public int healAmount;
 *        }
 *
 *        [System.Serializable]
 *        public class BehaviourList
 *        {
 *            [SerializeReference]
 *            public List<Behaviour> behaviours = new();
 *        }
 ```
 * 
 * Usage of the drawer:
 * ```csharp
 *         [SerializeReference]
 *      public Behaviour behaviour; // Single polymorphic field
 * 
 *      [SerializeReference]
 *      public List<Behaviour> listBehaviours; // Nested list
 * 
 *      [SerializeReference]
 *       public List<BehaviourList> nestedBehaviours; // Nested list
 * ```
 * 
 * 
 * @author Ricardo Ribeiro - Fantasy2DRPG 
 * @url: https://github.com/RicardoRibeirorr/Unity-Snippets
 * 
 **/



using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomPropertyDrawer(typeof(object), true)]
public class SerializeReferenceElementDrawer : PropertyDrawer
{
    private static Dictionary<Type, Type[]> _typeCache = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.ManagedReference)
        {
            float dropdownHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.managedReferenceValue != null)
            {
                return dropdownHeight + EditorGUI.GetPropertyHeight(property, label, true);
            }
            return dropdownHeight;
        }

        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.ManagedReference)
        {
            DrawReferenceField(position, property, label);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    private void DrawReferenceField(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Type baseType = GetBaseTypeFromProperty(property);
        if (baseType == null)
        {
            EditorGUI.LabelField(position, label.text, "Cannot resolve type.");
            EditorGUI.EndProperty();
            return;
        }

        var currentType = property.managedReferenceValue?.GetType();
        var allTypes = GetAllDerivedTypes(baseType);

        // Insert placeholder
        string[] typeNames = new string[allTypes.Length + 1];
        typeNames[0] = "-- Select Type --";
        for (int i = 0; i < allTypes.Length; i++)
            typeNames[i + 1] = allTypes[i].Name;

        int currentIndex = currentType != null ? Array.IndexOf(allTypes, currentType) + 1 : 0;

        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect contentRect = new Rect(
            position.x,
            position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
            position.width,
            position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing
        );

        int selectedIndex = EditorGUI.Popup(dropdownRect, currentIndex, typeNames);
        if (selectedIndex != currentIndex)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = selectedIndex > 0 ? Activator.CreateInstance(allTypes[selectedIndex - 1]) : null;
            property.serializedObject.ApplyModifiedProperties();
        }

        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(contentRect, property, true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static Type[] GetAllDerivedTypes(Type baseType)
    {
        if (!_typeCache.TryGetValue(baseType, out var types))
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        .ToArray();
            _typeCache[baseType] = types;
        }
        return types;
    }

    private static Type GetBaseTypeFromProperty(SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;
        Type currentType = target.GetType();
        FieldInfo fieldInfo = null;

        string[] pathParts = property.propertyPath.Replace(".Array.data[", "[").Split('.');

        foreach (string part in pathParts)
        {
            if (part.Contains("["))
            {
                var arrayField = part.Substring(0, part.IndexOf("["));
                int index = int.Parse(part.Substring(part.IndexOf("[") + 1, part.IndexOf("]") - part.IndexOf("[") - 1));

                fieldInfo = currentType.GetField(arrayField, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo == null) return null;

                currentType = fieldInfo.FieldType.IsArray
                    ? fieldInfo.FieldType.GetElementType()
                    : fieldInfo.FieldType.GetGenericArguments().FirstOrDefault();
            }
            else
            {
                fieldInfo = currentType.GetField(part, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo == null) return null;

                currentType = fieldInfo.FieldType;
            }

            if (currentType == null)
                return null;
        }

        return currentType;
    }
}
