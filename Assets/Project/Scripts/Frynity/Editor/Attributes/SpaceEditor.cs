using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SpaceAttribute))]
public class SpaceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SpaceAttribute space = (SpaceAttribute)attribute;
        Rect fieldRect = new Rect(
            position.x,
            position.y + space.width,
            position.width,
            EditorGUI.GetPropertyHeight(property, label, true)
        );
        EditorGUI.PropertyField(fieldRect, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        SpaceAttribute space = (SpaceAttribute)attribute;

        return EditorGUI.GetPropertyHeight(property, label, true) + space.width;
    }
}
