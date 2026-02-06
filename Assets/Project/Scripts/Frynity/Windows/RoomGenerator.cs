

#if UNITY_EDITOR
using Frynity.Window;
using UnityEditor;
using UnityEngine;

namespace Frynity.Window
{

    public class RoomGenerator : EditorWindow
    {
        public static float cellSize = 1;


        [MenuItem("Window/Frynity/Room Generator")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<RoomGenerator>("Room Generator");
            window.position = new Rect(0, 0, 650, 700);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Room gen not work yet", MessageType.Warning);
            if (GUILayout.Button("Room Creator"))
            {
                RoomCreator.ShowWindow();
                Close();
            }
        }
    }
}
#endif