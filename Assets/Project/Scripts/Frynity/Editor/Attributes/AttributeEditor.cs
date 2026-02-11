using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class AttributeEditor : Editor
{

    // Dictionnaire pour garder l’état de chaque groupe
    Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // FOLDOUT \\

        // Récupère tous les champs sérialisés
        var property = serializedObject.GetIterator();
        property.NextVisible(true); // ignorer script

        // Crée un mapping : groupe -> liste de propriétés
        var groups = new Dictionary<string, List<SerializedProperty>>();

        do
        {
            var field = property.serializedObject.targetObject.GetType()
                .GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            string groupName = null;

            if (field != null)
            {
                var attr = field.GetCustomAttribute<FoldoutAttribute>();
                if (attr != null)
                    groupName = attr.label;
            }

            // Pas de groupe → dessiner directement
            if (groupName == null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
            else
            {
                if (!groups.ContainsKey(groupName))
                    groups[groupName] = new List<SerializedProperty>();

                groups[groupName].Add(property.Copy());
            }
        } while (property.NextVisible(false));

        // Dessine les groupes
        foreach (var kvp in groups)
        {
            string groupName = kvp.Key;
            var props = kvp.Value;

            if (!foldoutStates.ContainsKey(groupName))
                foldoutStates[groupName] = false;

            foldoutStates[groupName] = EditorGUILayout.Foldout(foldoutStates[groupName], groupName, true);

            if (foldoutStates[groupName])
            {
                EditorGUI.indentLevel++;
                foreach (var p in props)
                    EditorGUILayout.PropertyField(p, true);
                EditorGUI.indentLevel--;
            }

        }

        // BUTTON \\

        var targetObj = target;
        var type = targetObj.GetType();

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
            var showIfAttr = method.GetCustomAttribute<ButtonShowIfAttribute>();
            if (buttonAttr == null) continue;

            string label = buttonAttr.text ?? ObjectNames.NicifyVariableName(method.Name);

            bool show = true;
            if (showIfAttr != null)
            {
                show = ShowIf(showIfAttr);
            }

            if (!show) continue;
            if (GUILayout.Button(label))
            {
                // Vérifie que la méthode n’a pas de paramètres
                if (method.GetParameters().Length == 0)
                {
                    Undo.RecordObject(targetObj, $"Invoke {method.Name}");
                    method.Invoke(targetObj, null);
                    EditorUtility.SetDirty(targetObj);
                }
                else
                {
                    Debug.LogWarning($"[Button] Ignoré : {method.Name} a des paramètres");
                }
            }
        }

        // LINK \\

        property = serializedObject.GetIterator();
        property.NextVisible(true); // ignorer script

        // Crée un mapping : groupe -> liste de propriétés
        do
        {
            var field = property.serializedObject.targetObject.GetType()
                .GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null)
            {
                var attr = field.GetCustomAttribute<LinkAttribute>();
            }

        } while (property.NextVisible(false));

        serializedObject.ApplyModifiedProperties();

    }

    private bool ShowIf(ButtonShowIfAttribute attr)
    {
        SerializedProperty condition = serializedObject.FindProperty(attr.conditionField);
        bool shouldShow = false;

        if (condition != null)
        {
            if (condition.propertyType == SerializedPropertyType.Boolean)
                shouldShow = condition.boolValue;
            else if (condition.propertyType == SerializedPropertyType.Enum)
            {
                shouldShow = (int)GetValue(condition) == (int)attr.conditionValue;
                if (!shouldShow)
                {
                    shouldShow = ((int)GetValue(condition) & (int)attr.conditionValue) > 0;
                }

            }
            else if (attr.conditionValue != null)
                shouldShow = GetValue(condition).Equals(attr.conditionValue);
        }

        return shouldShow;
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