using Frynity.ECS;
using Frynity.Heal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frynity.Window
{
    public class EntityCreator : EditorWindow
    {
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
        static string _selectedPath;
        bool _errorPath;

        #endregion


        [MenuItem("Window/Frynity/Entity Creator")]
        public static void ShowWindow()
        {
            EditorWindow window =  GetWindow<EntityCreator>("Entity Creator");
            window.position = new Rect(0, 0, 600, 700);
            window.Show();
        }

        void OnGUI()
        {

            // Creator Area
            GUILayout.BeginArea(new Rect(0, 0, position.width/2, position.height));
            GUILayout.Label("Welcome in the Entitys Creator by James CHEVAL !", EditorStyles.boldLabel);

            // Entity
            _entityName = EditorGUILayout.TextField("Entity Name", _entityName);

            _life = Mathf.Min(1, EditorGUILayout.FloatField("Life", _life));
            _speed = Mathf.Min(0, EditorGUILayout.FloatField("Speed", _speed));

            // Item
            float width = position.width / 2;
            _showItem = EditorGUILayout.BeginFoldoutHeaderGroup(_showItem, "Item");
            if (_showItem)
            {
                EditorGUILayout.HelpBox("The block attributes will be selected if the item fields are empty.", MessageType.Info);

                _itemSprite = EditorGUILayout.ObjectField("Item Sprite", _itemSprite, typeof(Sprite), false) as Sprite;
                _itemAnimator = EditorGUILayout.ObjectField("Item Animator", _itemAnimator, typeof(Animator), false) as Animator;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Block
            _showBlock = EditorGUILayout.BeginFoldoutHeaderGroup(_showBlock, "Block");
            if (_showBlock)
            {
                _blockSprite = EditorGUILayout.ObjectField("Item Sprite", _blockSprite, typeof(Sprite), false) as Sprite;
                _blockAnimator = EditorGUILayout.ObjectField("Item Animator", _blockAnimator, typeof(Animator), false) as Animator;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Prefab
            _settings = EditorGUILayout.BeginFoldoutHeaderGroup(_settings, "Prefab Settings");
            if (_settings)
            {
                EditorGUILayout.LabelField("Path : ", _selectedPath, EditorStyles.boldLabel);
                if (_errorPath)
                {
                    EditorGUILayout.HelpBox("Choose a path in Asset folder of the project", MessageType.Warning);
                }
                if (GUILayout.Button("Choose Path"))
                {
                    string _path;
                    _path = EditorUtility.OpenFolderPanel("Choose folder for create prefab", "Assets", "");

                    if (!string.IsNullOrEmpty(_path))
                    {
                        if (!_path.StartsWith(Application.dataPath))
                        {
                            _errorPath = true;
                        }
                        else
                        {
                            _errorPath = false;
                            _selectedPath = "Assets" + _path.Substring(Application.dataPath.Length);
                        }
                    }
                }
            }

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

            GUILayout.EndArea();

            // Visual Area
            GUILayout.BeginArea(new Rect(position.width/2, 0, position.width / 2, position.height));

            GUILayout.EndArea();

        }
    }
}