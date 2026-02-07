
#if UNITY_EDITOR
using Frynity.Gen;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Frynity.Window
{
    public class RoomCreator : EditorWindow
    {
        struct ObjectTypeC
        {
            public int cellId;
            public ObjectType type;

            public ObjectTypeC(int cellId, ObjectType type)
            {
                this.cellId = cellId;
                this.type = type;
            }
        }

        struct SpriteAsset
        {
            public int cellId;
            public Sprite sprite;

            public SpriteAsset(int cellId, Sprite sprite)
            {
                this.cellId = cellId;
                this.sprite = sprite;
            }
        }

        RoomCells _room = null;

        float _zoom = 100;
        static float _cellSize;

        Vector2 _scroll;

        // Objects

        AssetList _assets;
        enum ObjectType
        {
            Sprite, Prefab
        }

        List<bool> _showObjects = new List<bool>();
        List<(int cellId, Sprite sprite, Vector2 position, float size)> _sprites = new List<(int, Sprite, Vector2, float)>();
        List<bool> _spriteShow = new List<bool>();

        List<(int cellId, GameObject prefab, Vector2 position, float size)> _prefabs = new List<(int, GameObject, Vector2, float)>();
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
        string _roomName = "";

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
            _assets = _room.GetAssetList;

            _showCells = true;
            _showDoor = true;
            foreach (var cell in room.GetCells)
            {
                Vector2Int pos = cell.localPos;
                pos.y *= -1;
                _cells.Add(pos);
                _cellsShow.Add(false);
                _showObjects.Add(false);
            }

            foreach (RendererRoom renderer in room.GetRenderers)
            {
                Vector2 posSprite = renderer.renderer.transform.position;
                posSprite.y *= -1;
                _sprites.Add((renderer.cellId, renderer.renderer.sprite, posSprite, renderer.renderer.transform.lossyScale.x));
                _spriteShow.Add(false);
            }

            foreach (var pref in room.GetPrefabs)
            {
                Vector2 posPrefab = pref.obj.transform.position;
                posPrefab.y *= -1;
                _prefabs.Add((pref.cellId, pref.prefab, posPrefab, pref.obj.transform.lossyScale.x / pref.prefab.transform.lossyScale.x));
                _prefabShow.Add(false);
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


            foreach (CellDoor door in room.GetDoors)
            {
                int id = 0;
                foreach(var cell in cellsWithWallId)
                {
                    if (cell.id == door.cellId && cell.face == door.face) break;
                    else id++;
                }
                if (id < cellsWithWallId.Count)
                {
                    _doors.Add((id, door.cellId, door.face));
                    _doorShow.Add(false);
                }
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
            _assets = EditorGUILayout.ObjectField("Asset List", _assets, typeof(AssetList), false, GUILayout.Width(width)) as AssetList;
            _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize, GUILayout.Width(width));
            if (_cellSize < 1) _cellSize = 1;

            // Cells
            GUILayout.BeginHorizontal();
            _showCells = EditorGUILayout.BeginFoldoutHeaderGroup(_showCells, "Cells");
            if (GUILayout.Button("+")) // Ajout de cell
            {
                _cells.Add(Vector2Int.zero);
                _cellsShow.Add(false);
                _showCells = true;
                _showObjects.Add(false);

                if (_assets)
                {
                    _sprites.Add((_cells.Count - 1, _assets.Grounds[Random.Range(0, _assets.Grounds.Length)] , Vector2.zero, 1));
                    _spriteShow.Add(false);
                }
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
                        for (int j = 0; j < _sprites.Count; j++)
                        {
                            if (_sprites[j].cellId == i)
                            {
                                _sprites.RemoveAt(j);
                                j--;
                            }
                        }

                        for (int j = 0; j < _prefabs.Count; j++)
                        {
                            if (_prefabs[j].cellId == i)
                            {
                                _prefabs.RemoveAt(j);
                                j--;
                            }
                        }

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
                        if (i == 0)
                        {
                            _cells[i] = Vector2Int.zero;
                        }
                        else
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

                            for (int j = 0; j < _sprites.Count; j++)
                            {
                                if (_sprites[j].cellId == i)
                                    UpdateSprite(i, j, _sprites[j].sprite, _sprites[j].position, _sprites[j].size, false);
                            }
                        }
                        
                        // Objects

                        GUILayout.BeginHorizontal();
                        _showObjects[i] = EditorGUILayout.Foldout(_showObjects[i], "Objects", true);
                        GUILayout.Space(30);
                        if (GUILayout.Button("+"))
                        {
                            GenericMenu menu = new GenericMenu();

                            if (_assets)
                            {
                                menu.AddItem(new GUIContent("Prefab"), false, OnCreateObject, new ObjectTypeC(i, ObjectType.Prefab));
                                foreach (var tile in _assets.Tiles)
                                {
                                    menu.AddItem(new GUIContent(tile.name), false, OnCreateSprite, new SpriteAsset(i, tile));
                                }
                                foreach (var prop in _assets.Props)
                                {
                                    menu.AddItem(new GUIContent(prop.name), false, OnCreateSprite, new SpriteAsset(i, prop));
                                }
                            }
                            else
                            {
                                menu.AddItem(new GUIContent("Sprite"), false, OnCreateObject, new ObjectTypeC(i, ObjectType.Sprite));
                                menu.AddItem(new GUIContent("Prefab"), false, OnCreateObject, new ObjectTypeC(i, ObjectType.Prefab));
                            }

                            menu.ShowAsContext();

                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical(_objectsStyle, GUILayout.MaxWidth(viewWidth));
                        if (_showObjects[i])
                        {
                            EditorGUI.indentLevel++;

                            GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                            if (GUILayout.Button("Show All"))
                            {
                                for (int j = 0; j < _spriteShow.Count; j++)
                                {
                                    _spriteShow[j] = true;
                                }
                                for (int j = 0; j < _prefabShow.Count; j++)
                                {
                                    _prefabShow[j] = true;
                                }
                            }
                            if (GUILayout.Button("Hide All"))
                            {
                                for (int j = 0; j < _spriteShow.Count; j++)
                                {
                                    _spriteShow[j] = false;
                                }

                                for (int j = 0; j < _prefabShow.Count; j++)
                                {
                                    _prefabShow[j] = false;
                                }
                            }
                            GUILayout.EndHorizontal();

                            for (int j = 0; j < _sprites.Count; j++)
                            {
                                if (_sprites[j].cellId != i) continue;
                                GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                                _spriteShow[j] = EditorGUILayout.Foldout(_spriteShow[j], _sprites[j].sprite == null ? "Sprite" : _sprites[j].sprite.name, true);
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    _sprites.RemoveAt(j);
                                    _spriteShow.RemoveAt(j);
                                    j--;
                                    GUILayout.EndHorizontal();
                                    continue;
                                }
                                GUILayout.EndHorizontal();

                                if (_spriteShow[j])
                                {
                                    Sprite newSprite = EditorGUILayout.ObjectField("Sprite", _sprites[j].sprite, typeof(Sprite), false) as Sprite;
                                    Vector2 newPos = Vector2.zero;
                                    float newSize = 1;

                                    UpdateSprite(i, j, newSprite, newPos, newSize);
                                    
                                    Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                                    EditorGUI.DrawRect(rect, Color.black);
                                }
                            }


                            for (int j = 0; j < _prefabs.Count; j++)
                            {
                                if (_prefabs[j].cellId != i) continue;
                                GUILayout.BeginHorizontal(GUILayout.MaxWidth(viewWidth));
                                _prefabShow[j] = EditorGUILayout.Foldout(_prefabShow[j], _prefabs[j].prefab == null ? "Prefab" : _prefabs[j].prefab.name, true);
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    _prefabs.RemoveAt(j);
                                    _prefabShow.RemoveAt(j);
                                    j--;
                                    GUILayout.EndHorizontal();
                                    continue;
                                }
                                GUILayout.EndHorizontal();

                                if (_prefabShow[j])
                                {
                                    GameObject newObject = EditorGUILayout.ObjectField("Prefab", _prefabs[j].prefab, typeof(GameObject), false) as GameObject;
                                    Vector2 newPos = Vector2.zero;
                                    float newSize = 1;
                                    if (newObject != null)
                                    {
                                        newPos = EditorGUILayout.Vector2Field("Position", _prefabs[j].position);

                                        newSize = EditorGUILayout.FloatField("Size", _prefabs[j].size);
                                        newSize = Mathf.Max(0, newSize);
                                    }

                                    _prefabs[j] = (i, newObject, newPos, newSize);
                                    Rect rect = GUILayoutUtility.GetRect(viewWidth, 2);
                                    EditorGUI.DrawRect(rect, Color.black);
                                }
                            }

                            EditorGUI.indentLevel--;
                        }
                        GUILayout.EndVertical();
                    }

                    if (_cells.Count > 1)
                    {
                        Rect box = GUILayoutUtility.GetRect(position.width / 3, 2);
                        EditorGUI.DrawRect(box, Color.black);
                    }

                }
                EditorGUI.indentLevel--;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(30);


            List<(int id, FaceDoor face)> cellsWithWallId = new List<(int, FaceDoor)>();
            for (int j = 0; j < _cells.Count; j++)
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

            for (int i = 0; i < _doors.Count; i++)
            {
               
                int index = _doors[i].indexCellsWall;
                for (int j = 0; j < _doors.Count; j++)
                {
                    if (i == j) continue;

                    if (index >= cellsWithWallId.Count)
                    {
                        _doors.RemoveAt(j);
                        index--;
                        if (index < 0) index = cellsWithWallId.Count - 1;
                        continue;
                    }

                    if (_doors[j] == (index, cellsWithWallId[index].id, cellsWithWallId[index].face))
                    {
                        index++;
                        if (index >= cellsWithWallId.Count) index = 0;
                        j = 0;
                    }
                }

                int id = cellsWithWallId[index].id;
                _doors[i] = (index, id, cellsWithWallId[index].face);
            }

            // Doors

            GUILayout.BeginHorizontal();
            _showDoor = EditorGUILayout.BeginFoldoutHeaderGroup(_showDoor, "Doors");
            if (GUILayout.Button("+"))
            {
                if (_doors.Count < cellsWithWallId.Count)
                {
                    _doors.Add((0, 0, FaceDoor.Left));
                    _doorShow.Add(false);
                    _showDoor = true;
                }
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

                    int index = _doors[i].indexCellsWall;

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
            EditorGUILayout.LabelField("Path : ", EditorStyles.boldLabel, GUILayout.MaxWidth(viewWidth));
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
            _roomName = EditorGUILayout.TextField("Room Name", _roomName, GUILayout.MaxWidth(viewWidth));

            if (_room == null)
            {
                if (GUILayout.Button("Create Prefab", GUILayout.MaxWidth(viewWidth)))
                {
                    if (_onOther)
                    {
                        Debug.LogError("You can't create prefab with two cell at the same positions.");
                    }
                    else if (_selectedPath == null && _roomName == "")
                    {
                        Debug.LogError("Select path won't be null");
                    }
                    else
                    {
                        GameObject fab = new GameObject("Room");

                        // Sprites & Prefabs
                        List<RendererRoom> renderers = new List<RendererRoom>();
                        if (_sprites.Count > 0)
                        {
                            GameObject skin = new GameObject("Skin");
                            for (int i = 0; i < _sprites.Count; i++)
                            {
                                GameObject sprite = new GameObject("Sprite " + i);
                                SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
                                renderer.sprite = _sprites[i].sprite;
                                renderer.sortingLayerName = "Room";
                                renderer.sortingOrder = i;

                                Vector2 pos = _sprites[i].position;
                                pos.y *= -1;
                                sprite.transform.position = pos;
                                sprite.transform.localScale = new Vector2(_sprites[i].size, _sprites[i].size);
                                sprite.transform.SetParent(skin.transform);

                                renderers.Add(new RendererRoom(_sprites[i].cellId, renderer));
                            }
                            skin.transform.SetParent(fab.transform);
                        }


                        List<PrefabsRoom> prefabs = new List<PrefabsRoom>();
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

                                prefabs.Add(new PrefabsRoom(_prefabs[i].cellId, _prefabs[i].prefab, obj));
                            }
                            prefab.transform.SetParent(fab.transform);
                        }


                        // Cells
                        RoomCells room = fab.AddComponent<RoomCells>();
                        List<Cell> cells = new List<Cell>();
                        // Set cell 0 to main cell at position (0,0)
                        for (int i = 0; i < _cells.Count; i++)
                        {
                            Vector2Int pos = _cells[i];
                            pos.y *= -1;
                            cells.Add(new Cell(i, pos));
                        }

                        room.SetCells = new List<Cell>(cells);
                        room.SetAssetList = _assets;

                        List<CellDoor> newDoors = new List<CellDoor>();
                        for (int i = 0; i < _doors.Count; i++)
                        {
                            newDoors.Add(new CellDoor(_doors[i].cellId, _doors[i].face));
                        }
                        room.SetDoors(new List<CellDoor>(newDoors));
                        room.SetCellSize = _cellSize;
                        room.CreateRoom(new List<RendererRoom>(renderers), new List<PrefabsRoom>(prefabs));

                        string localPath = _selectedPath + "/" + _roomName + ".prefab";

                        PrefabUtility.SaveAsPrefabAssetAndConnect(room.gameObject, AssetDatabase.GenerateUniqueAssetPath(localPath), InteractionMode.AutomatedAction);
                        Debug.Log("Prefab created");
                        Close();
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

                        string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_room.gameObject);

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null)
                        {
                            assetPath = stage.assetPath;
                        }

                        if (string.IsNullOrEmpty(assetPath))
                        {
                            assetPath = AssetDatabase.GetAssetPath(_room.gameObject);
                        }

                        if (string.IsNullOrEmpty(assetPath))
                        {
                            Debug.LogError("Impossible de trouver le chemin du prefab !");
                            return;
                        }

                        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                        RoomCells roomRoot = prefabRoot.GetComponent<RoomCells>();


                        // Sprites
                        GameObject sprites = null;
                        bool nouv = false;
                        if (roomRoot.GetRenderers.Count > 0)
                        {
                            sprites = roomRoot.GetRenderers[0].renderer.transform.parent.gameObject;
                        }
                        else if (_sprites.Count > 0)
                        {
                            sprites = new GameObject("Sprites");
                            nouv = true;
                        }

                        while (roomRoot.GetRenderers.Count > 0)
                        {
                            DestroyImmediate(roomRoot.GetRenderers[0].renderer.gameObject);
                            roomRoot.GetRenderers.RemoveAt(0);
                        }

                        List<RendererRoom> renderers = new List<RendererRoom>();
                        for (int i = 0; i < _sprites.Count; i++)
                        {
                            GameObject sprite = new GameObject("Sprite " + i);
                            SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
                            renderer.sprite = _sprites[i].sprite;
                            renderer.sortingLayerName = "Room";
                            renderer.sortingOrder = i;

                            Vector2 pos = _sprites[i].position;
                            pos.y *= -1;
                            sprite.transform.position = pos;
                            sprite.transform.localScale = new Vector2(_sprites[i].size, _sprites[i].size);

                            sprite.transform.SetParent(sprites.transform);
                            renderers.Add(new RendererRoom(_sprites[i].cellId, renderer));
                        }
                        if (nouv)
                            sprites.transform.SetParent(prefabRoot.transform);

                        // prefabs
                        GameObject prefabs = null;
                        nouv = false;
                        List<PrefabsRoom> prefabsList = new List<PrefabsRoom>();
                        if (roomRoot.GetPrefabs.Count > 0)
                        {
                            prefabs = roomRoot.GetPrefabs[0].obj.transform.parent.gameObject;
                        }
                        else if (_prefabs.Count > 0)
                        {
                            prefabs = new GameObject("Prefabs");
                            nouv = true;
                        }

                        while (roomRoot.GetPrefabs.Count > 0)
                        {
                            DestroyImmediate(roomRoot.GetPrefabs[0].obj);
                            roomRoot.GetPrefabs.RemoveAt(0);
                        }

                        if (_prefabs.Count == 0)
                        {
                            if (prefabs != null)
                            {
                                DestroyImmediate(prefabs);
                            }
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
                            prefabsList.Add(new PrefabsRoom(_prefabs[i].cellId, _prefabs[i].prefab, obj));

                        }
                        if (nouv)
                            prefabs.transform.SetParent(prefabRoot.transform);

                        // Cells
                        List<Cell> cells = new List<Cell>();
                        // Set cell 0 to main cell at position (0,0)
                        for (int i = 0; i < _cells.Count; i++)
                        {
                            Vector2Int pos = _cells[i];
                            pos.y *= -1;
                            cells.Add(new Cell(i, pos));
                        }

                        roomRoot.SetCells = new List<Cell>(cells);

                        List<CellDoor> newDoors = new List<CellDoor>();
                        for (int i = 0; i < _doors.Count; i++)
                        {
                            newDoors.Add(new CellDoor(_doors[i].cellId, _doors[i].face));
                        }
                        roomRoot.SetDoors(new List<CellDoor>(newDoors));
                        roomRoot.SetCellSize = _cellSize;
                        roomRoot.CreateRoom(new List<RendererRoom>(renderers), new List<PrefabsRoom>(prefabsList));

                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                        
                        Debug.Log("Prefab modified");
                        Close();
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
            ObjectTypeC jsp = (ObjectTypeC)type;
            _showObjects[jsp.cellId] = true;
            switch (jsp.type)
            {
                case ObjectType.Sprite:
                    _sprites.Add((jsp.cellId, null, Vector2.zero, 1));
                    _spriteShow.Add(false);
                    break;

                case ObjectType.Prefab:
                    _prefabs.Add((jsp.cellId, null, Vector2.zero, 1));
                    _prefabShow.Add(false);
                    break;
            }
        }

        void OnCreateSprite(object sp)
        {
            SpriteAsset spriteAsset = (SpriteAsset)sp;
            _showObjects[spriteAsset.cellId] = true;
            _sprites.Add((spriteAsset.cellId, spriteAsset.sprite, Vector2.zero, 1));
            _spriteShow.Add(false);
        }

        void UpdateSprite(int cellIndex, int spriteIndex, Sprite newSprite, Vector2 newPos, float newSize, bool show = true)
        {
            if (newSprite != null)
            {
                if (show)
                    newPos = EditorGUILayout.Vector2Field("Position", _sprites[spriteIndex].position);
                newPos.x = Mathf.Clamp(newPos.x, _cells[cellIndex].x * _cellSize, _cells[cellIndex].x * _cellSize + _cellSize - newSprite.bounds.size.x * _sprites[spriteIndex].size);
                newPos.y = Mathf.Clamp(newPos.y, _cells[cellIndex].y * _cellSize - _cellSize + newSprite.bounds.size.y * _sprites[spriteIndex].size, _cells[cellIndex].y * _cellSize);

                if (show)
                    newSize = EditorGUILayout.FloatField("Size", _sprites[spriteIndex].size);
                newSize = Mathf.Clamp(newSize, 0, _cellSize / Mathf.Max(newSprite.bounds.size.x, newSprite.bounds.size.y));
            }

            _sprites[spriteIndex] = (cellIndex, newSprite, newPos, newSize);
        }
    }
}
#endif