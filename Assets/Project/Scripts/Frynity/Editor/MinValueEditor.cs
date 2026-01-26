using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinValueAttribute))]
public class MinValueEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinValueAttribute script = (MinValueAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = Mathf.Max(property.floatValue, script.minValue);
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = Mathf.Max(property.intValue, (int)script.minValue);
        }

            EditorGUI.PropertyField(position, property, label, true);
    }
}
