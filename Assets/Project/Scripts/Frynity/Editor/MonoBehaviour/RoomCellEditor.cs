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

        private void OnEnable()
        {
            RoomCells room = (RoomCells)target;
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
                    _showCellFoldout[i] = EditorGUILayout.Foldout(_showCellFoldout[i], "Cell " + i, true);
                    if (_showCellFoldout[i])
                    {
                        for (int j = 0; j < room.GetDoors.Count; j++)
                        {
                            if (room.GetDoors[j].cell != i) continue;
                            room.GetActiveDoors[j] = EditorGUILayout.Toggle("Door", room.GetActiveDoors[j]);

                            if (room.GetActiveDoors[j])
                            {
                                room.ActiveDoor(room.GetDoors[j].cell, room.GetDoors[j].face);
                            }

                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Modify Prefab"))
            {
                RoomCreator.ShowWindow(room);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}