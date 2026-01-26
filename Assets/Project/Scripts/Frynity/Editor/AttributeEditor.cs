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

        // Récupère toutes les méthodes décorées avec [Button]
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
            if (buttonAttr == null) continue;

            string label = buttonAttr.text ?? ObjectNames.NicifyVariableName(method.Name);

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

        serializedObject.ApplyModifiedProperties();

    }
}