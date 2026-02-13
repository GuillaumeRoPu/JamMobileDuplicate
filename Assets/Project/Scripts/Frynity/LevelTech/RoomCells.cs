using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;

namespace Frynity.Gen
{
    public enum FaceDoor
    {
        Top, Bottom, Left, Right
    }

    [System.Serializable]
    public struct CellDoor
    {
        [MinValue(0)] public int cellId;
        public FaceDoor face;

        public CellDoor(int cellId, FaceDoor faceDoor)
        {
            this.cellId = cellId;
            face = faceDoor;
        }
    }

    [System.Serializable]
    public struct RendererRoom
    {
        [MinValue(0)] public int cellId;
        public SpriteRenderer renderer;

        public RendererRoom(int cellId, SpriteRenderer renderer)
        {
            this.cellId = cellId;
            this.renderer = renderer;
        }
    }

    [System.Serializable]
    public struct PrefabsRoom
    {
        public int cellId;
        public GameObject prefab;
        public GameObject obj;

        public PrefabsRoom(int celldId, GameObject prefab, GameObject obj)
        {
            this.cellId = celldId;
            this.obj = obj;
            this.prefab = prefab;
        }
    }

    [System.Serializable]
    public struct Cell
    {
        [MinValue(0)] public int id;
        public Vector2Int localPos;

        public Cell(int id, Vector2Int localPos)
        {
            this.id = id;
            this.localPos = localPos;
        }
    }

    public class RoomCells : MonoBehaviour
    {
        [SerializeField] AssetList _assets;
        [SerializeField] float _cellSize;
        [SerializeField] List<RendererRoom> _renderers = new List<RendererRoom>();
        [SerializeField] List<PrefabsRoom> _prefabs = new List<PrefabsRoom>();
        [SerializeField] List<Cell> _localCells = new List<Cell>();
        [SerializeField] List<CellDoor> _localDoors = new List<CellDoor>();
        [SerializeField, HideInInspector] List<bool> _activeDoors = new List<bool>();

        public AssetList GetAssetList => _assets;
        public AssetList SetAssetList { set  { _assets = value; } }
        public float GetCellSize => _cellSize;
        public float SetCellSize { set { _cellSize = value; } }
        public List<RendererRoom> GetRenderers => _renderers;
        public List<PrefabsRoom> GetPrefabs => _prefabs;
        public List<Cell> GetCells => _localCells;
        public List<Cell> SetCells { set { _localCells = value; }  }
        public bool TileInCells(Vector2Int pos)
        {
            foreach (Cell tile in _localCells)
            {
                if (tile.localPos == pos) return true;
            }
            return false;
        }
        public List<CellDoor> GetDoors => _localDoors;
        public void SetDoors(List<CellDoor> doors)
        {
            _localDoors = doors;
            _activeDoors = new List<bool>();
            for (int i = 0; i < _localDoors.Count; i++)
            {
                _activeDoors.Add(true);
            }

        }

        public List<bool> GetActiveDoors => _activeDoors;

        public void CreateRoom(List<RendererRoom> renderers, List<PrefabsRoom> prefabs)
        {
            _renderers = renderers;

            _prefabs = prefabs;
        }

        public void ActiveDoor(int cell, FaceDoor face)
        {
            for (int i = 0; i < _localDoors.Count; i++)
            {
                if (_localDoors[i].cellId == cell)
                {
                    _activeDoors[i] = _localDoors[i].face == face;
                }
            }
        }

        public bool CellHaveDoor(int id)
        {
            bool value = false;
            foreach (CellDoor door in _localDoors)
            {
                value = door.cellId == id;
                if (value) break;
            }

            return value;
        }

        public void CreateWalls()
        {
            GameObject wallParent = new GameObject("Walls");
            wallParent.transform.SetParent(transform);
            Vector2Int cellRoomPos = Vector2Int.FloorToInt((Vector2)transform.position / GameManager.Of.Grid.TileSize);

            foreach (Cell tile in _localCells)
            {
                Sprite sprite = null;

                Vector2Int worldPos = cellRoomPos + tile.localPos;

                if (!TileInCells(tile.localPos + Vector2Int.right))
                {
                    bool flip = false;
                    if (GridManager.Tiles.ContainsKey(worldPos + Vector2Int.right))
                    {
                        sprite = GameManager.Of.SideInWall;
                        flip = true;
                    }
                    else
                    {
                        sprite = GameManager.Of.SideOutWall;
                    }
                    SpawnWall(sprite, (Vector2)(worldPos + Vector2Int.right) * GameManager.Of.Grid.TileSize + new Vector2(GameManager.Of.Grid.TileSize / 2, GameManager.Of.Grid.TileSize / 2 * 1.5f), 6, wallParent.transform, null, flip);
                }

                if (!TileInCells(tile.localPos + Vector2Int.left))
                {
                    bool flip = false;
                    if (GridManager.Tiles.ContainsKey(worldPos + Vector2Int.left))
                    {
                        sprite = GameManager.Of.SideInWall;
                    }
                    else
                    {
                        sprite = GameManager.Of.SideOutWall;
                        flip = true;
                    }
                    SpawnWall(sprite, (Vector2)(worldPos + Vector2Int.left) * GameManager.Of.Grid.TileSize + new Vector2(GameManager.Of.Grid.TileSize / 2, GameManager.Of.Grid.TileSize / 2 * 1.5f), 6, wallParent.transform, null, flip);
                }

                if (!TileInCells(tile.localPos + Vector2Int.up))
                {
                    if (GridManager.Tiles[worldPos].CellData.DoorPosition == Face.Up)
                    {
                        SpawnWall(_assets != null && _assets.DoorWalls.Length > 0 ? _assets.DoorWalls[Random.Range(0, _assets.DoorWalls.Length)] :
                        GameManager.Of.OutDoorWall[Random.Range(0, GameManager.Of.OutDoorWall.Length)],
                        (Vector2)worldPos * GameManager.Of.Grid.TileSize + new Vector2(0, GameManager.Of.Grid.TileSize), 5, wallParent.transform, GameManager.Of.Grid.GetTileAtPosition(worldPos + Vector2Int.up));
                    }
                    else
                    {
                        SpawnWall(_assets != null && _assets.Walls.Length > 0 ? _assets.Walls[Random.Range(0, _assets.Walls.Length)] :
                        GameManager.Of.OutWall[Random.Range(0, GameManager.Of.OutWall.Length)],
                        (Vector2)worldPos * GameManager.Of.Grid.TileSize + new Vector2(0, GameManager.Of.Grid.TileSize), 5, wallParent.transform, GameManager.Of.Grid.GetTileAtPosition(worldPos + Vector2Int.up));
                    }

                }

                if (!TileInCells(tile.localPos + Vector2Int.down))
                {
                    if (!GridManager.Tiles.ContainsKey(worldPos + Vector2Int.down) ||
                        !GridManager.Tiles[worldPos + Vector2Int.down].CellData.HasRoom)
                    {
                        if (GridManager.Tiles[worldPos].CellData.DoorPosition == Face.Down)
                        {
                            SpawnWall(GameManager.Of.OutDoorWall[Random.Range(0, GameManager.Of.OutDoorWall.Length)], (Vector2)worldPos * GameManager.Of.Grid.TileSize, 5, wallParent.transform, GameManager.Of.Grid.GetTileAtPosition(worldPos));
                        }
                        else
                        {
                            SpawnWall(GameManager.Of.OutWall[Random.Range(0, GameManager.Of.OutWall.Length)], (Vector2)worldPos * GameManager.Of.Grid.TileSize, 5, wallParent.transform, GameManager.Of.Grid.GetTileAtPosition(worldPos));
                        }
                    }
                }
            }
        }

        void SpawnWall(Sprite sprite, Vector2 cellPos, int layer, Transform parent, Tile tileWallPos = null, bool flipX = false)
        {
            GameObject go = new GameObject("Wall");
            go.transform.parent = gameObject.transform;
            go.transform.position = cellPos;
            go.transform.localScale = Vector3.one * GameManager.Of.Grid.TileSize;
            go.transform.SetParent(parent);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Playground";
            renderer.sortingOrder = layer;
            renderer.sprite = sprite;
            renderer.flipX = flipX;

            if (tileWallPos != null)
            {
                if (tileWallPos.CellData.HasRoom)
                    tileWallPos.GetRoomData().SetNewWall(renderer);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach (Cell cell in _localCells)
            {
                Gizmos.DrawWireCube((Vector2)transform.position + (Vector2)cell.localPos * _cellSize + new Vector2(_cellSize, _cellSize)/2, new Vector2(_cellSize, _cellSize));
                
            }

            Gizmos.color = Color.blueViolet;
            for (int i = 0; i < _localDoors.Count; i++)
            {
                if (_activeDoors[i])
                {
                    Vector2 pos = (Vector2)transform.position + (Vector2)_localCells[_localDoors[i].cellId].localPos * _cellSize + new Vector2(_cellSize, _cellSize) / 2;
                    pos += _localDoors[i].face == FaceDoor.Left ? new Vector2(-_cellSize / 2, 0) :
                        _localDoors[i].face == FaceDoor.Top ? new Vector2(0, _cellSize / 2) :
                        _localDoors[i].face == FaceDoor.Bottom ? new Vector2(0, -_cellSize / 2) : new Vector2(_cellSize / 2, 0);

                    Gizmos.DrawCube(pos, new Vector2(0.1f, 0.1f));
                }
            }
        }
    }
}