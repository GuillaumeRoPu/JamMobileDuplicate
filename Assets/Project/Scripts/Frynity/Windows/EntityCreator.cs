using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Frynity.Window
{
    public class EntityCreator : EditorWindow
    {

        Vector2 _scroll;

        #region Entity

        static string _entityName = string.Empty;
        static float _life = 1;
        static float _speed = 1;

        #endregion

        #region Item

        bool _showItem;
        static Sprite _itemSprite;
        static Animator _itemAnimator;

        #endregion

        #region Block

        bool _showBlock;
        static Sprite _blockSprite;
        static Animator _blockAnimator;

        #endregion

        #region Prefab Settings

        bool _settings;
        bool _showPath;
        static List<string> _selectedPath = new List<string>() {"Assets/"};
        static List<string> _pathNames = new List<string>() { "Path 1" };
        List<bool> _errorPath = new List<bool>() { false };

        #endregion


        private GUIStyle _backgroundStyle;
        private Texture2D _backgroundTexture;

        private void OnEnable()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
            _backgroundTexture.Apply();

            _backgroundStyle = new GUIStyle();
            _backgroundStyle.border = new RectOffset(2, 2, 2, 2);
            _backgroundStyle.normal.background = _backgroundTexture;
        }
        private void OnDisable()
        {
            DestroyImmediate(_backgroundTexture);
        }

        [MenuItem("Window/Frynity/Entity Creator")]
        public static void ShowWindow()
        {
            EditorWindow window =  GetWindow<EntityCreator>("Entity Creator");
            window.position = new Rect(0, 0, 650, 700);
            window.Show();
        }

        void OnGUI()
        {

            // Creator Area
            GUILayout.BeginArea(new Rect(0, 0, position.width/2, position.height));
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label("Welcome in the Entitys Creator by James CHEVAL !", EditorStyles.boldLabel);

            // Entity
            _entityName = EditorGUILayout.TextField("Entity Name", _entityName);

            _life = Mathf.Min(1, EditorGUILayout.FloatField("Life", _life));
            _speed = Mathf.Min(0, EditorGUILayout.FloatField("Speed", _speed));

            // Item
            float width = position.width / 2;
            _showItem = EditorGUILayout.BeginToggleGroup("Item", _showItem);
            if (_showItem)
            {
                EditorGUILayout.HelpBox("The block attributes will be selected if the item fields are empty.", MessageType.Info);

                _itemSprite = EditorGUILayout.ObjectField("Item Sprite", _itemSprite, typeof(Sprite), false) as Sprite;
                _itemAnimator = EditorGUILayout.ObjectField("Item Animator", _itemAnimator, typeof(Animator), false) as Animator;

            }
            EditorGUILayout.EndToggleGroup();

            // Block
            _showBlock = EditorGUILayout.BeginToggleGroup("Block", _showBlock);
            if (_showBlock)
            {
                _blockSprite = EditorGUILayout.ObjectField("Item Sprite", _blockSprite, typeof(Sprite), false) as Sprite;
                _blockAnimator = EditorGUILayout.ObjectField("Item Animator", _blockAnimator, typeof(Animator), false) as Animator;

            }
            EditorGUILayout.EndToggleGroup();

            // Prefab

            _settings = EditorGUILayout.BeginFoldoutHeaderGroup(_settings, "Prefab Settings");
            if (_settings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                _showPath = EditorGUILayout.Foldout(_showPath, "Paths", true);
                if (GUILayout.Button("+"))
                {
                    _selectedPath.Add("Assets/");
                    _pathNames.Add("Path " + _selectedPath.Count.ToString());
                }
                if (_selectedPath.Count > 1)
                {
                    if (GUILayout.Button("-"))
                    {
                        _selectedPath.RemoveAt(_selectedPath.Count - 1);
                        _pathNames.RemoveAt(_pathNames.Count - 1);
                    }
                }
                else
                {
                    EditorGUILayout.Space(20);
                }
                GUILayout.EndHorizontal();

                if (_selectedPath.Count != _errorPath.Count)
                    _errorPath = new List<bool>() { false };

                if (_showPath)
                {
                    for (int i = 0; i < _selectedPath.Count; i++)
                    {
                        GUILayout.BeginVertical(_backgroundStyle);

                        if (i == _errorPath.Count) _errorPath.Add(false);
                        if (_errorPath[i])
                        {
                            EditorGUILayout.HelpBox("Choose a path in Asset folder of the project", MessageType.Warning);
                        }

                        _pathNames[i] = EditorGUILayout.TextField(_pathNames[i]);

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Path : ", _selectedPath[i], EditorStyles.boldLabel);
                        if (GUILayout.Button("Choose Path"))
                        {
                            string _path;
                            _path = EditorUtility.OpenFolderPanel("Choose folder for create prefab", "Assets", "");

                            if (!string.IsNullOrEmpty(_path))
                            {
                                if (!_path.StartsWith(Application.dataPath))
                                {
                                    _errorPath[i] = true;
                                }
                                else
                                {
                                    _errorPath[i] = false;
                                    _selectedPath[i] = "Assets" + _path.Substring(Application.dataPath.Length);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }

                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Reset Prefab")))
            {
                _entityName = string.Empty;
                _life = 1;
                _itemSprite = null;
                _itemAnimator = null;
                _blockSprite = null;
                _blockAnimator = null;
            }
            if (GUILayout.Button(new GUIContent("Create Prefab")))
            {
                Debug.Log("Item created !");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Visual Area
            GUILayout.BeginArea(new Rect(position.width/2, 0, position.width / 2, position.height));

            GUILayout.EndArea();

        }
    }
}