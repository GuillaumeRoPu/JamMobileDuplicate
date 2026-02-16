using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyIfAttribute))]
public class ReadOnlyIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        ReadOnlyIfAttribute readOnlyIf = (ReadOnlyIfAttribute)attribute;

        SerializedProperty condition = FindConditionProperty(property, readOnlyIf.condition);

        bool shouldShow = false;

        if (condition != null && condition.propertyType == SerializedPropertyType.Boolean)
        {
            shouldShow = condition.boolValue;
        }

        if (shouldShow)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
        else
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
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
}
