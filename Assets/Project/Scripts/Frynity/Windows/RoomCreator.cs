using Frynity.Window;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Frynity.Window
{
    public class RoomCreator : EditorWindow
    {
        float _zoom = 100;
        static float _cellSize;

        // Objects

        enum ObjectType
        {
            Sprite, Prefab
        }

        bool _showObject;
        List<ObjectType> _objects = new List<ObjectType>();
        List<(Sprite sprite, Vector2 position, float size)> _sprites = new List<(Sprite, Vector2, float)>();
        List<bool> _spriteShow = new List<bool>();

        List<(GameObject prefab, Vector2 position, float size)> _prefabs = new List<(GameObject, Vector2, float)>();
        List<bool> _prefabShow = new List<bool>();

        // Styles
        Texture2D _backgroundTexture;
        GUIStyle _backgroundStyle;

        Texture2D _objectsTexture;
        GUIStyle _objectsStyle;

        private void OnEnable()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
            _backgroundTexture.Apply();

            _backgroundStyle = new GUIStyle();
            _backgroundStyle.border = new RectOffset(2, 2, 2, 2);
            _backgroundStyle.normal.background = _backgroundTexture;

            _objectsTexture = new Texture2D(1, 1);
            _objectsTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1));
            _objectsTexture.Apply();

            _objectsStyle = new GUIStyle();
            _objectsStyle.border = new RectOffset(2, 2, 2, 2);
            _objectsStyle.normal.background = _objectsTexture;
        }
        private void OnDisable()
        {
            DestroyImmediate(_backgroundTexture);
            DestroyImmediate(_objectsTexture);
        }

        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<RoomCreator>("Room Creator");
            window.position = new Rect(0, 0, 1000, 700);
            window.Show();
        }

        void OnGUI()
        {
            // Creator Area
            GUILayout.BeginArea(new Rect(0,0,position.width/3, position.height), _backgroundStyle);

            if (GUILayout.Button("Back"))
            {
                RoomGenerator.ShowWindow();
                Close();
            }


            _zoom = EditorGUILayout.Slider(_zoom, 0, 200);
            _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize);
            if (_cellSize < 1) _cellSize = 1;


            // Objects

            GUILayout.BeginHorizontal();
            _showObject = EditorGUILayout.BeginFoldoutHeaderGroup(_showObject, "Objects");
            if (GUILayout.Button("+"))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Sprite"), false, OnCreateObject, ObjectType.Sprite);
                menu.AddItem(new GUIContent("Prefab"), false, OnCreateObject, ObjectType.Prefab);

                menu.ShowAsContext();

            }
            GUILayout.Space(position.width / 3);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(_objectsStyle);
            if (_showObject)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Show All"))
                {
                    for (int i = 0; i < _spriteShow.Count; i++)
                    {
                        _spriteShow[i] = true;
                    }
                    for (int i = 0; i < _prefabShow.Count; i++)
                    {
                        _prefabShow[i] = true;
                    }
                }
                if (GUILayout.Button("Hide All"))
                {
                    for (int i = 0; i < _spriteShow.Count; i++)
                    {
                        _spriteShow[i] = false;
                    }

                    for (int i = 0; i < _prefabShow.Count; i++)
                    {
                        _prefabShow[i] = false;
                    }
                }
                GUILayout.EndHorizontal();

                for (int i = 0; i < _sprites.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    _spriteShow[i] = EditorGUILayout.Foldout(_spriteShow[i], _sprites[i].sprite == null ? "Sprite" : _sprites[i].sprite.name, true);
                    if (GUILayout.Button("-"))
                    {
                        _sprites.RemoveAt(i);
                        _spriteShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUILayout.Space(position.width/3 * 0.7f);
                    GUILayout.EndHorizontal();

                    if (_spriteShow[i])
                    {
                        Sprite newSprite = EditorGUILayout.ObjectField("Sprite", _sprites[i].sprite, typeof(Sprite), false) as Sprite;
                        Vector2 newPos = Vector2.zero;
                        float newSize = 1;
                        if (newSprite != null)
                        {
                            newPos = EditorGUILayout.Vector2Field("Position", _sprites[i].position);

                            newSize = EditorGUILayout.FloatField("Size", _sprites[i].size);
                            newSize = Mathf.Max(0, newSize);
                        }

                        _sprites[i] = (newSprite, newPos, newSize);
                        Rect rect = GUILayoutUtility.GetRect(float.MaxValue, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }
                    

                }

                

                for (int i = 0; i < _prefabs.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    _prefabShow[i] = EditorGUILayout.Foldout(_prefabShow[i], _prefabs[i].prefab == null ? "Prefab" : _prefabs[i].prefab.name, true);
                    if (GUILayout.Button("-"))
                    {
                        _prefabs.RemoveAt(i);
                        _prefabShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUILayout.Space(position.width / 3 * 0.7f);
                    GUILayout.EndHorizontal();

                    if (_prefabShow[i])
                    {
                        GameObject newObject = EditorGUILayout.ObjectField("Prefab", _prefabs[i].prefab, typeof(GameObject), false) as GameObject;
                        Vector2 newPos = Vector2.zero;
                        float newSize = 1;
                        if (newObject != null)
                        {
                            newPos = EditorGUILayout.Vector2Field("Position", _prefabs[i].position);

                            newSize = EditorGUILayout.FloatField("Size", _prefabs[i].size);
                            newSize = Mathf.Max(0, newSize);
                        }

                        _prefabs[i] = (newObject, newPos, newSize);
                        Rect rect = GUILayoutUtility.GetRect(float.MaxValue, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }
                        
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();

            // Visual Area
            GUILayout.BeginArea(new Rect(position.width / 3, 0, 2*position.width / 3, position.height));

            Vector2 centerPos = new Vector2(position.width / 3, position.height / 2);
            //Handles.DrawSolidRectangleWithOutline(new Rect(centerPos.x, centerPos.y, _cellSize * pixelPerUnit, _cellSize * pixelPerUnit), Color.clear, Color.green);

            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i].sprite == null) continue;

                Texture2D tex = _sprites[i].sprite.texture;
                GUI.DrawTexture(new Rect(centerPos + (_sprites[i].position - _sprites[i].size * (Vector2)_sprites[i].sprite.bounds.size/2) * _zoom, 
                    _sprites[i].size * _sprites[i].sprite.bounds.size * _zoom), tex);
            }

            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].prefab == null) continue;

                int count = 0;
                Vector2 parent = Vector2.zero;

                foreach (SpriteRenderer renderer in _prefabs[i].prefab.GetComponentsInChildren<SpriteRenderer>())
                {
                    Vector2 world = renderer.transform.position;
                    Vector2 scale = renderer.transform.lossyScale;

                    if (count == 1)
                    {
                        world.y *= -1;
                    }

                    Vector2 pos = _prefabs[i].position + world * _prefabs[i].size - scale * _prefabs[i].size * (Vector2)renderer.sprite.bounds.size / 2;
                    
                    Texture2D tex = renderer.sprite.texture;
                    GUI.DrawTexture(new Rect(centerPos + pos * _zoom,
                        _prefabs[i].size * scale * renderer.sprite.bounds.size * _zoom), tex);

                    if (count == 0)
                    {
                        count = 1;
                    }
                }
            }

            GUILayout.EndArea();
        }

        void OnCreateObject(object type)    
        {
            _objects.Add((ObjectType) type);
            _showObject = true;
            switch (type)
            {
                case ObjectType.Sprite:
                    _sprites.Add((null, Vector2.zero, 1));
                    _spriteShow.Add(false);
                    break;

                case ObjectType.Prefab:
                    _prefabs.Add((null, Vector2.zero, 1));
                    _prefabShow.Add(false);
                    break;
            }
        }
    }
}