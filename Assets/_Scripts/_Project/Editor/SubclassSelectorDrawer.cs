#if UNITY_EDITOR
using System;
using System.Linq;
using Game.Foundation;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect dropdownRect = new Rect(
            position.x + EditorGUIUtility.labelWidth, 
            position.y, 
            position.width - EditorGUIUtility.labelWidth, 
            EditorGUIUtility.singleLineHeight);

        string currentTypeName = property.managedReferenceValue?.GetType().Name ?? "None";
        
        if (GUI.Button(dropdownRect, currentTypeName, EditorStyles.popup))
        {
            ShowTypeMenu(property);
        }

        EditorGUI.PropertyField(position, property, label, true);

        EditorGUI.EndProperty();
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () => ApplyType(property, null));

        Type baseType = GetBaseType(property);
        if (baseType != null)
        {
            var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (Type type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), property.managedReferenceValue?.GetType() == type, () => ApplyType(property, type));
            }
        }
        menu.ShowAsContext();
    }

    private void ApplyType(SerializedProperty property, Type targetType)
    {
        property.managedReferenceValue = targetType == null ? null : Activator.CreateInstance(targetType);
        property.serializedObject.ApplyModifiedProperties();
    }

    private Type GetBaseType(SerializedProperty property)
    {
        string[] parts = property.managedReferenceFieldTypename.Split(' ');
        if (parts.Length == 2)
        {
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }
        return null;
    }
}
#endif