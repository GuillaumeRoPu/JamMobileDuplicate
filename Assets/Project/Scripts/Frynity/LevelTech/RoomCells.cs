using Frynity.Window;
using System.Collections.Generic;
using UnityEngine;

namespace Frynity.Gen
{
    public enum FaceDoor
    {
        Top, Bottom, Left, Right
    }

    public class RoomCells : MonoBehaviour
    {
        float _cellSize;
        List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        List<(GameObject prefab, GameObject obj)> _objects = new List<(GameObject, GameObject)>();
        Dictionary<int, Vector2Int> _localCells = new Dictionary<int, Vector2Int>();
        List<(int cellId, FaceDoor face)> _localDoors = new List<(int, FaceDoor)>();
        List<bool> _activeDoors = new List<bool>();

        public float GetCellSize => _cellSize;
        public float SetCellSize { set { _cellSize = value; } }
        public List<SpriteRenderer> GetRenderers => _renderers;
        public List<(GameObject prefab, GameObject obj)> GetPrefabs => _objects;
        public Dictionary<int, Vector2Int> GetCells => _localCells;
        public Dictionary<int, Vector2Int> SetCells { set { _localCells = value; }  }
        public List<(int cell, FaceDoor face)> GetDoors => _localDoors;
        public List<(int, FaceDoor)> SetDoors 
        { 
            set 
            { 
                _localDoors = value; 
                _activeDoors = new List<bool>();
                for (int i = 0; i < _localDoors.Count; i++)
                {
                    _activeDoors.Add(true);
                }
            } 
        }

        public List<bool> GetActiveDoors => _activeDoors;

        public void CreateRoom(List<SpriteRenderer> renderers, List<(GameObject, GameObject)> prefabs)
        {
            _renderers = renderers;
            _objects = prefabs;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var cell in _localCells)
            {
                Gizmos.DrawWireCube((Vector2)transform.position + (Vector2)cell.Value * _cellSize + new Vector2(_cellSize, _cellSize)/2, new Vector2(_cellSize, _cellSize));
                
            }

            Gizmos.color = Color.blueViolet;
            for (int i = 0; i < _localDoors.Count; i++)
            {
                if (_activeDoors[i])
                {
                    Vector2 pos = (Vector2)transform.position + (Vector2)_localCells[_localDoors[i].cellId] * _cellSize + new Vector2(_cellSize, _cellSize) / 2;
                    pos += _localDoors[i].face == FaceDoor.Left ? new Vector2(-_cellSize / 2, 0) :
                        _localDoors[i].face == FaceDoor.Top ? new Vector2(0, _cellSize / 2) :
                        _localDoors[i].face == FaceDoor.Bottom ? new Vector2(0, -_cellSize / 2) : new Vector2(_cellSize / 2, 0);

                    Gizmos.DrawCube(pos, new Vector2(0.1f, 0.1f));
                }
            }
        }
    }
}