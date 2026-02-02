using Frynity.Gen;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frynity.Window
{
    public class RoomCreator : EditorWindow
    {
        RoomCells _room = null;

        float _zoom = 100;
        static float _cellSize;

        Vector2 _scroll;

        // Objects

        enum ObjectType
        {
            Sprite, Prefab
        }

        bool _showObject;
        List<(Sprite sprite, Vector2 position, float size)> _sprites = new List<(Sprite, Vector2, float)>();
        List<bool> _spriteShow = new List<bool>();

        List<(GameObject prefab, Vector2 position, float size)> _prefabs = new List<(GameObject, Vector2, float)>();
        List<bool> _prefabShow = new List<bool>();

        // Cells
        bool _showCells;
        List<Vector2Int> _cells = new List<Vector2Int>();
        List<bool> _cellsShow = new List<bool>();
        bool _onOther;

        // Doors
        bool _showDoor;
        List<(int indexCellsWall, int cellId, FaceDoor face)> _doors = new List<(int, int, FaceDoor)>();
        List<bool> _doorShow = new List<bool>();

        // Settings
        bool _errorPath = false;
        static string _selectedPath;

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

        public static void ShowWindow(RoomCells room = null)
        {
            RoomCreator window = GetWindow<RoomCreator>("Room Creator");
            window.position = new Rect(0, 0, 1000, 700);
            window.Open(room);
            window.Show();
        }

        public void Open(RoomCells room)
        {
            if (room == null) return;

            _room = room;

            _cellSize = _room.GetCellSize;

            _showObject = true;
            _showCells = true;
            _showDoor = true;

            foreach (SpriteRenderer renderer in room.GetRenderers)
            {
                Vector2 pos = renderer.transform.position;
                pos.y *= -1;
                _sprites.Add((renderer.sprite, pos, renderer.transform.lossyScale.x));
                _spriteShow.Add(false);
            }

            foreach ((GameObject prefab, GameObject obj) prefab in room.GetPrefabs)
            {
                Vector2 pos = prefab.obj.transform.position;
                pos.y *= -1;
                _prefabs.Add((prefab.prefab, pos, prefab.obj.transform.lossyScale.x/ prefab.prefab.transform.lossyScale.x));
                _prefabShow.Add(false);
            }

            foreach (var cell in room.GetCells)
            {
                _cells.Add(cell.Value);
                _cellsShow.Add(false);
            }

            List<(int id, FaceDoor face)> cellsWithWallId = new List<(int, FaceDoor)>();
            for (int i = 0; i < _cells.Count; i++)
            {
                List<FaceDoor> faces = new List<FaceDoor>();
                bool wallRight = !_cells.Contains(_cells[i] + Vector2Int.right);
                bool wallLeft = !_cells.Contains(_cells[i] + Vector2Int.left);
                bool wallDown = !_cells.Contains(_cells[i] + Vector2Int.up);
                bool wallUp = !_cells.Contains(_cells[i] + Vector2Int.down);

                if (wallRight) faces.Add(FaceDoor.Right);
                if (wallDown) faces.Add(FaceDoor.Bottom);
                if (wallLeft) faces.Add(FaceDoor.Left);
                if (wallUp) faces.Add(FaceDoor.Top);

                foreach (FaceDoor face in faces)
                {
                    cellsWithWallId.Add((i, face));
                }
            }


            foreach ((int, FaceDoor) door in room.GetDoors)
            {
                int id = 0;
                foreach(var cell in cellsWithWallId)
                {
                    if (cell.id == door.Item1 && cell.face == door.Item2) break;
                    else id++;
                }
                _doors.Add((id, door.Item1, door.Item2));
                _doorShow.Add(false);
            }
        }

        void OnGUI()
        {
            // Creator Area
            GUILayout.BeginArea(new Rect(0,0, position.width/3, position.height), _backgroundStyle);
            float width = position.width / 3 * 0.95f;
            _scroll = GUILayout.BeginScrollView(_scroll);
            float viewWidth = position.width / 3 - 20f;
            GUILayout.BeginVertical(GUILayout.Width(viewWidth));

            if (GUILayout.Button("Back", GUILayout.MaxWidth(viewWidth)))
            {
                RoomGenerator.ShowWindow();
                Close();
            }


            _zoom = EditorGUILayout.Slider(_zoom, 0, 200, GUILayout.Width(width));
            _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize, GUILayout.Width(width));
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

            GUILayout.BeginVertical(_objectsStyle, GUILayout.MaxWidth(viewWidth));
            if (_showObject)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
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
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                    _spriteShow[i] = EditorGUILayout.Foldout(_spriteShow[i], _sprites[i].sprite == null ? "Sprite" : _sprites[i].sprite.name, true);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _sprites.RemoveAt(i);
                        _spriteShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
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
                        Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }

                }

                

                for (int i = 0; i < _prefabs.Count; i++)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                    _prefabShow[i] = EditorGUILayout.Foldout(_prefabShow[i], _prefabs[i].prefab == null ? "Prefab" : _prefabs[i].prefab.name, true);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _prefabs.RemoveAt(i);
                        _prefabShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
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
                        Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }
                        
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            _showCells = EditorGUILayout.BeginFoldoutHeaderGroup(_showCells, "Cells");
            if (GUILayout.Button("+"))
            {
                _cells.Add(Vector2Int.zero);
                _cellsShow.Add(false);
                _showCells = true;
            }
            GUILayout.Space(position.width / 3);
            GUILayout.EndHorizontal();


            if (_showCells)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                if (GUILayout.Button("Show All"))
                {
                    for (int i = 0; i < _cellsShow.Count; i++)
                    {
                        _cellsShow[i] = true;
                    }
                }
                if (GUILayout.Button("Hide All"))
                {
                    for (int i = 0; i < _cellsShow.Count; i++)
                    {
                        _cellsShow[i] = false;
                    }

                }
                GUILayout.EndHorizontal();
                for (int i = 0; i <  _cells.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    _cellsShow[i] = EditorGUILayout.Foldout(_cellsShow[i], "Cell " + i, true);
                    GUILayout.Space(30);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _cells.RemoveAt(i);
                        _cellsShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;

                    }
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();
                    if (_cellsShow[i])
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        if (GUILayout.Button("Up", GUILayout.Width(50))) // up = -1
                        {
                            _cells[i] += Vector2Int.down;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Left", GUILayout.Width(50)))
                        {
                            _cells[i] += Vector2Int.left;
                        }
                        if (GUILayout.Button("Right", GUILayout.Width(50)))
                        {
                            _cells[i] += Vector2Int.right;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        if (GUILayout.Button("Down", GUILayout.Width(50))) // Down = +1
                        {
                            _cells[i] += Vector2Int.up;
                        }
                        GUILayout.EndHorizontal();
                        Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }
                }

                EditorGUI.indentLevel--;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            _showDoor = EditorGUILayout.BeginFoldoutHeaderGroup(_showDoor, "Doors");
            if (GUILayout.Button("+"))
            {
                _doors.Add((0, 0, FaceDoor.Left));
                _doorShow.Add(false);
                _showDoor = true;

            }
            GUILayout.Space(position.width / 3);
            GUILayout.EndHorizontal();


            if (_showDoor)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                if (GUILayout.Button("Show All"))
                {
                    for (int i = 0; i < _doorShow.Count; i++)
                    {
                        _doorShow[i] = true;
                    }
                }
                if (GUILayout.Button("Hide All"))
                {
                    for (int i = 0; i < _doorShow.Count; i++)
                    {
                        _doorShow[i] = false;
                    }

                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < _doors.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    _doorShow[i] = EditorGUILayout.Foldout(_doorShow[i], "Door " + i, true);
                    GUILayout.Space(30);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _doors.RemoveAt(i);
                        _doorShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;

                    }
                    GUILayout.FlexibleSpace();

                    List<(int id, FaceDoor face)> cellsWithWallId = new List<(int, FaceDoor)>();
                    for(int j = 0; j < _cells.Count; j++)
                    {
                        List<FaceDoor> faces = new List<FaceDoor>();
                        bool wallRight = !_cells.Contains(_cells[j] + Vector2Int.right);
                        bool wallLeft = !_cells.Contains(_cells[j] + Vector2Int.left);
                        bool wallDown = !_cells.Contains(_cells[j] + Vector2Int.up);
                        bool wallUp = !_cells.Contains(_cells[j] + Vector2Int.down);

                        if (wallRight) faces.Add(FaceDoor.Right);
                        if (wallDown) faces.Add(FaceDoor.Bottom);
                        if (wallLeft) faces.Add(FaceDoor.Left);
                        if (wallUp) faces.Add(FaceDoor.Top);

                        foreach (FaceDoor face in faces)
                        {
                            cellsWithWallId.Add((j, face));
                        }
                    }

                    if (i >= cellsWithWallId.Count)
                    {
                        _doors.RemoveAt(i);
                        _doorShow.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }

                    int index = _doors[i].indexCellsWall;
                    for (int j = 0; j < _doors.Count; j++)
                    {
                        if (i == j) continue;
                        if (_doors[j] == (index, cellsWithWallId[index].id, cellsWithWallId[index].face))
                        {
                            index++;
                            if (index >= cellsWithWallId.Count) index = 0;
                            j = 0;
                        }
                    }

                    GUILayout.EndHorizontal();

                    if (_doorShow[i] && _doors.Count < cellsWithWallId.Count)
                    {
                        index = _doors[i].indexCellsWall;
                        if (GUILayout.Button("Next", GUILayout.MaxWidth(viewWidth)))
                        {
                            index++;
                            if (index >= cellsWithWallId.Count) index = 0;

                            while (_doors.Contains((index, cellsWithWallId[index].id, cellsWithWallId[index].face)))
                            {
                                index++;
                                if(index >= cellsWithWallId.Count) index = 0;
                            }
                        }
                        if (GUILayout.Button("Back", GUILayout.MaxWidth(viewWidth)))
                        {
                            index--;
                            if (index < 0) index = cellsWithWallId.Count - 1;

                            while (_doors.Contains((index, cellsWithWallId[index].id, cellsWithWallId[index].face)))
                            {
                                index--;
                                if (index < 0) index = cellsWithWallId.Count - 1;
                            }
                        }
                        Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                        EditorGUI.DrawRect(rect, Color.black);
                    }
                    int id = cellsWithWallId[index].id;
                    _doors[i] = (index, id, cellsWithWallId[index].face);
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(30);

            if (_errorPath)
            {
                EditorGUILayout.HelpBox("Choose a path in Asset folder of the project", MessageType.Warning);
            }

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
            EditorGUILayout.LabelField("Path : ", EditorStyles.boldLabel);
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
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField(_selectedPath, GUILayout.MaxWidth(viewWidth));

            if (_room == null)
            {
                if (GUILayout.Button("Create Prefab", GUILayout.MaxWidth(viewWidth)))
                {
                    if (_onOther)
                    {
                        Debug.LogError("You can't create prefab with two cell at the same positions.");
                    }
                    else
                    {
                        GameObject fab = new GameObject("Room");

                        // Sprites & Prefabs
                        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
                        if (_sprites.Count > 0)
                        {
                            GameObject skin = new GameObject("Skin");
                            for (int i = 0; i < _sprites.Count; i++)
                            {
                                GameObject sprite = new GameObject("Sprite " + i);
                                SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
                                renderer.sprite = _sprites[i].sprite;
                                renderer.sortingOrder = i;

                                Vector2 pos = _sprites[i].position;
                                pos.y *= -1;
                                sprite.transform.position = pos;
                                sprite.transform.localScale = new Vector2(_sprites[i].size, _sprites[i].size);
                                sprite.transform.SetParent(skin.transform);

                                renderers.Add(renderer);
                            }
                            skin.transform.SetParent(fab.transform);
                        }
                        

                        List<(GameObject prefab, GameObject obj)> prefabs = new List<(GameObject, GameObject)>();
                        if (_prefabs.Count > 0)
                        {
                            GameObject prefab = new GameObject("Prefabs");
                            for (int i = 0; i < _prefabs.Count; i++)
                            {
                                Vector2 pos = _prefabs[i].position;
                                pos.y *= -1;

                                GameObject obj = PrefabUtility.InstantiatePrefab(_prefabs[i].prefab) as GameObject;
                                obj.transform.position = pos;
                                obj.transform.localScale *= new Vector2(_prefabs[i].size, _prefabs[i].size);

                                int j = 0;
                                foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>())
                                {
                                    renderer.sortingOrder = j + _sprites.Count;
                                    j++;
                                }

                                obj.transform.SetParent(prefab.transform);

                                prefabs.Add((_prefabs[i].prefab, obj));
                            }
                            prefab.transform.SetParent(fab.transform);
                        }
                            

                        // Cells
                        RoomCells room = fab.AddComponent<RoomCells>();
                        Dictionary<int, Vector2Int> cells = new Dictionary<int, Vector2Int>();
                        // Set cell 0 to main cell at position (0,0)
                        for (int i = 0; i < _cells.Count; i++)
                        {
                            Vector2Int pos = _cells[i];
                            pos.y *= -1;
                            cells.Add(i, pos);
                        }

                        room.SetCells = cells;

                        List<(int, FaceDoor)> newDoors = new List<(int, FaceDoor)>();
                        for (int i = 0; i < _doors.Count; i++)
                        {
                            newDoors.Add((_doors[i].cellId, _doors[i].face));
                        }
                        room.SetDoors = newDoors;
                        room.SetCellSize = _cellSize;
                        room.CreateRoom(renderers, prefabs);

                        PrefabUtility.SaveAsPrefabAssetAndConnect(fab, _selectedPath, InteractionMode.AutomatedAction);
                        Debug.Log("Prefab created");
                    }
                }
            }
            else
            {
                
                if (GUILayout.Button("Modify Prefab", GUILayout.MaxWidth(viewWidth)))
                {
                    if (_onOther)
                    {
                        Debug.LogError("You can't modify prefab with two cell at the same positions.");
                    }
                    else
                    {
                        // Sprites
                        GameObject sprites = null;
                        bool nouv = false;
                        if (_room.GetRenderers.Count > 0)
                        {
                            sprites = _room.GetRenderers[0].transform.parent.gameObject;
                        }
                        else if (_sprites.Count > 0)
                        {
                            sprites = new GameObject("Sprites");
                            nouv = true;
                        }

                        while (_room.GetRenderers.Count > 0)
                        {
                            DestroyImmediate(_room.GetRenderers[0].gameObject);
                            _room.GetRenderers.RemoveAt(0);
                        }

                        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
                        for (int i = 0; i < _sprites.Count; i++)
                        {
                            GameObject sprite = new GameObject("Sprite " + i);
                            SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
                            renderer.sprite = _sprites[i].sprite;
                            renderer.sortingOrder = i;

                            Vector2 pos = _sprites[i].position;
                            pos.y *= -1;
                            sprite.transform.position = pos;
                            sprite.transform.localScale = new Vector2(_sprites[i].size, _sprites[i].size);

                            sprite.transform.SetParent(sprites.transform);
                            renderers.Add(renderer);
                        }
                        if (nouv)
                            sprites.transform.SetParent(_room.transform);

                        // prefabs
                        GameObject prefabs = null;
                        nouv = false;
                        List<(GameObject prefab, GameObject obj)> prefabsList = new List<(GameObject, GameObject)>();
                        if (_room.GetPrefabs.Count > 0)
                        {
                            prefabs = _room.GetPrefabs[0].obj.transform.parent.gameObject;
                        }
                        else if (_prefabs.Count > 0)
                        {
                            prefabs = new GameObject("Prefabs");
                            nouv = true;
                        }

                        while (_room.GetPrefabs.Count > 0)
                        {
                            DestroyImmediate(_room.GetPrefabs[0].obj);
                            _room.GetPrefabs.RemoveAt(0);
                        }

                        for (int i = 0; i < _prefabs.Count; i++)
                        {
                            Vector2 pos = _prefabs[i].position;
                            pos.y *= -1;
                            GameObject obj = PrefabUtility.InstantiatePrefab(_prefabs[i].prefab) as GameObject;
                            obj.transform.position = pos;
                            obj.transform.localScale *= new Vector2(_prefabs[i].size, _prefabs[i].size);

                            int j = 0;
                            foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>())
                            {
                                renderer.sortingOrder = j + _sprites.Count;
                                j++;
                            }

                            obj.transform.SetParent(prefabs.transform);
                            prefabsList.Add((_prefabs[i].prefab, obj));

                        }
                        if (nouv)
                            prefabs.transform.SetParent(_room.transform);

                        // Cells
                        Dictionary<int, Vector2Int> cells = new Dictionary<int, Vector2Int>();
                        // Set cell 0 to main cell at position (0,0)
                        for (int i = 0; i < _cells.Count; i++)
                        {
                            Vector2Int pos = _cells[i];
                            pos.y *= -1;
                            cells.Add(i, pos);
                        }

                        _room.SetCells = cells;

                        List<(int, FaceDoor)> newDoors = new List<(int, FaceDoor)>();
                        for (int i = 0; i < _doors.Count; i++)
                        {
                            newDoors.Add((_doors[i].cellId, _doors[i].face));
                        }
                        _room.SetDoors = newDoors;
                        _room.SetCellSize = _cellSize;
                        _room.CreateRoom(renderers, prefabsList);
                        Debug.Log("Prefab modified");
                    }
                }
            }


            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.EndArea();

            // Visual Area
            GUILayout.BeginArea(new Rect(position.width / 3, 0, 2*position.width / 3, position.height));

            Vector2 centerPos = new Vector2(position.width / 3, position.height / 2);

            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i].sprite == null) continue;

                Texture2D tex = _sprites[i].sprite.texture;
                Vector2 pivot = _sprites[i].sprite.pivot / (_sprites[i].sprite.pixelsPerUnit * (Vector2)_sprites[i].sprite.bounds.size);
                pivot.y = 1 - pivot.y;
                GUI.DrawTexture(new Rect(centerPos + (_sprites[i].position - (_sprites[i].size * (Vector2)_sprites[i].sprite.bounds.size * pivot)) * _zoom,
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

                    world.y *= -1;

                    if (count == 1)
                    {
                        world -= parent;
                    }

                    if (count == 0)
                    {
                        Vector2 origin = Vector2.zero;
                        if (_room)
                        {
                            origin = _room.transform.position;
                        }
                        count = 1;
                        parent = world - origin;
                        world = Vector2.zero;
                    }

                    
                    Vector2 pivot = renderer.sprite.pivot / (renderer.sprite.pixelsPerUnit * (Vector2)renderer.sprite.bounds.size);
                    pivot.y = 1 - pivot.y;

                    Vector2 pos = _prefabs[i].position + world * _prefabs[i].size - scale * _prefabs[i].size * (Vector2)renderer.sprite.bounds.size * pivot;

                    Texture2D tex = renderer.sprite.texture;
                    GUI.DrawTexture(new Rect(centerPos + pos * _zoom,
                        _prefabs[i].size * scale * renderer.sprite.bounds.size * _zoom), tex);
                }
            }

            for (int i = 0;  i < _cells.Count; i++)
            {
                _onOther = false;
                for (int j = 0; j < _cells.Count; j++)
                {
                    if (j != i)
                    {
                        if (_cells[j] == _cells[i])
                        {
                            _onOther = true;
                            break;
                        }
                    }
                }
                Handles.DrawSolidRectangleWithOutline(new Rect(centerPos.x + _cells[i].x * _cellSize * _zoom, centerPos.y + (_cells[i].y - _cellSize) * _cellSize * _zoom, _cellSize * _zoom - 1, _cellSize * _zoom - 1), Color.clear, _onOther ? Color.red : i == 0 ? Color.cyan : Color.green);
            }

            for (int i = 0; i < _doors.Count; i++)
            {
                Vector2 pos = (Vector2)_cells[_doors[i].cellId] * _cellSize;
                pos += _doors[i].face == FaceDoor.Top ? new Vector2(_cellSize / 2, -_cellSize) :
                    _doors[i].face == FaceDoor.Bottom ? new Vector2(_cellSize / 2, 0) :
                    _doors[i].face == FaceDoor.Right ? new Vector2(_cellSize, -_cellSize/2) : new Vector2(0, -_cellSize / 2);

                Handles.DrawSolidRectangleWithOutline(new Rect(centerPos + pos * _zoom - new Vector2(5, 5), new Vector2(10, 10)), Color.blueViolet, Color.cyan);
            }

            GUILayout.EndArea();
        }

        void OnCreateObject(object type)    
        {
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