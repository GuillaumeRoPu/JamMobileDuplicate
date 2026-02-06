using Frynity.Window;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frynity.Gen
{
    [CustomEditor(typeof(RoomCells))]
    public class RoomCellEditor : Editor
    {

        bool _cells;
        List<bool> _showCellFoldout = new List<bool>();

        SerializedProperty doors;

        private void OnEnable()
        {
            RoomCells room = (RoomCells)target;

            doors = serializedObject.FindProperty("_activeDoors");
        }

        public override void OnInspectorGUI()
        {

            RoomCells room = (RoomCells)target;

            serializedObject.Update();

            if (_showCellFoldout.Count != room.GetCells.Count)
            {
                _showCellFoldout = new List<bool>();
                for (int i = 0; i < room.GetCells.Count; i++)
                {
                    _showCellFoldout.Add(false);
                }
            }

            _cells = EditorGUILayout.BeginFoldoutHeaderGroup(_cells, "Cells");
            if (_cells)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < room.GetCells.Count; i++)
                {
                    if (!room.CellHaveDoor(room.GetCells[i].id)) continue;
                    _showCellFoldout[i] = EditorGUILayout.Foldout(_showCellFoldout[i], "Cell " + i, true);
                    if (_showCellFoldout[i])
                    {
                        for (int j = 0; j < room.GetDoors.Count; j++)
                        {
                            if (room.GetDoors[j].cellId != i) continue;

                            SerializedProperty elementProp = doors.GetArrayElementAtIndex(j);
                            EditorGUILayout.PropertyField(elementProp, new GUIContent("Door"));

                            if (elementProp.boolValue)
                            {
                                for (int a = 0; a < room.GetDoors.Count; a++)
                                {
                                    if (room.GetDoors[a].cellId == room.GetCells[i].id)
                                    {
                                        SerializedProperty element = doors.GetArrayElementAtIndex(a);
                                        element.boolValue = a == j;
                                    }
                                    
                                }
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();


            if (GUILayout.Button("Modify Prefab"))
            {
                RoomCreator.ShowWindow(room);
            }

            base.OnInspectorGUI();

        }
    }

}