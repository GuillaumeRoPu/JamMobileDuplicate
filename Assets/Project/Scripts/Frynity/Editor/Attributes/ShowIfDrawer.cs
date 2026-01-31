using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        bool shouldShow = ShowIf(property);

        if (shouldShow)
        {
            if (showIf.indentation)
                EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property, label, true);
            if (showIf.indentation)
                EditorGUI.indentLevel--;
        }
    }

    private bool ShowIf(SerializedProperty property)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty condition = FindConditionProperty(property, showIf.conditionField);
        bool shouldShow = false;

        if (condition != null)
        {
            if (condition.propertyType == SerializedPropertyType.Boolean)
                shouldShow = condition.boolValue;
            else if (condition.propertyType == SerializedPropertyType.Enum)
            {
                shouldShow = (int)GetValue(condition) == (int)showIf.conditionValue;
                if (!shouldShow)
                {
                    shouldShow = ((int)GetValue(condition) & (int)showIf.conditionValue) > 0;
                }

            }
            else if (showIf.conditionValue != null)
                shouldShow = GetValue(condition).Equals(showIf.conditionValue);
        }

        return shouldShow;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        bool shouldShow = ShowIf(property);

        return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
    }

    private SerializedProperty FindConditionProperty(SerializedProperty property, string conditionField)
    {
        string path = property.propertyPath;
        string conditionPath = path.Replace(property.name, conditionField);

        SerializedProperty condition = property.serializedObject.FindProperty(conditionPath);

        // Si rien trouvé, on tente avec le backing field généré par C#
        if (condition == null)
        {
            string backingFieldPath = path.Replace(property.name, $"<{conditionField}>k__BackingField");
            condition = property.serializedObject.FindProperty(backingFieldPath);
        }

        return condition;
    }

    private object GetValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.intValue;
            case SerializedPropertyType.Float:
                return prop.floatValue;
            case SerializedPropertyType.Boolean:
                return prop.boolValue;
            case SerializedPropertyType.Enum:
                return prop.enumValueFlag;
            case SerializedPropertyType.String:
                return prop.stringValue;
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue;
            default:
                return null;
        }
    }
}
