using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaxValueAttribute))]
public class MaxValueEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MaxValueAttribute script = (MaxValueAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = Mathf.Min(property.floatValue, script.maxValue);
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = Mathf.Min(property.intValue, (int)script.maxValue);
        }

        EditorGUI.PropertyField(position, property, label, true);
    }
}
