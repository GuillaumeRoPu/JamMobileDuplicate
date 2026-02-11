using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LinkAttribute))]
public class LinkEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (GUILayout.Button("Link"))
        {
            var obj = Selection.objects;

            EditorGUILayout.PropertyField
        }

        EditorGUI.PropertyField(position, property, label, true);
    }
}