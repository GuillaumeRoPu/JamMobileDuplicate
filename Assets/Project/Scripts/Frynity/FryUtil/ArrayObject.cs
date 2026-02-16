using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrayObject : MonoBehaviour
{
    [Flags]
    private enum Axis
    {
        X = 1 << 0, Y = 2 << 1, Z = 4 << 2
    }

    [SerializeField] Axis axis;
    [SerializeField] GameObject _toCopy;

    [SerializeField, MinValue(0)] int _count;
    [SerializeField, MinValue(0)] float _spacing;

    [SerializeField] Material[] _materials;
    [SerializeField] bool _reverseMaterials;

    List<GameObject> _objects = new List<GameObject>();
    int _materialIndex;
    bool _indexReversing;

    int AddMaterialIndex
    {
        get => 0;
        set
        {
            if (_reverseMaterials)
            {
                if (_indexReversing)
                    _materialIndex -= value;
                else
                    _materialIndex += value;

                if (_materialIndex >= _materials.Length)
                {
                    _materialIndex -= value * 2;
                    _indexReversing = true;
                }
                else if (_materialIndex < 0)
                {
                    _materialIndex += value * 2;
                    _indexReversing = false;
                }

            }
            else
            {
                _materialIndex += value;
                if (_materialIndex >= _materials.Length)
                    _materialIndex -= _materials.Length;
            }
        }
    }


    [Button("Generate")]
    public void CreateArray()
    {
        DestroyObjects();
        for (int i = 0; i < _count; i++)
        {
            _objects.Add(Instantiate(_toCopy, transform));

            if (_materials.Length > 0)
            {
                if (_materials[_materialIndex])
                    _objects[i].GetComponent<MeshRenderer>().material = _materials[_materialIndex];
            }

            float distance = _objects[i].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * _objects[i].transform.localScale.x;
            Vector3 newPos = transform.position;

            if (axis.HasFlag(Axis.X))
            {
                newPos.x += (distance + _spacing) * (i - _count / 2);
                if (_count % 2 == 0)
                    newPos.x += (distance + _spacing) / 2;
            }
            if (axis.HasFlag(Axis.Y))
            {
                newPos.y += (distance + _spacing) * (i - _count / 2);
                if (_count % 2 == 0)
                    newPos.y += (distance + _spacing) / 2;
            }
            if (axis.HasFlag(Axis.Z))
            {
                newPos.z += (distance + _spacing) * (i - _count / 2);
                if (_count % 2 == 0)
                    newPos.z += (distance + _spacing) / 2;
            }

            _objects[i].transform.position = newPos;
            AddMaterialIndex++;
        }
    }

    [Button("Destroy")]
    private void DestroyObjects()
    {
        _materialIndex = 0;
        int objCount = _objects.Count;
        for (int i = 0; i < objCount; i++)
        {
            DestroyImmediate(_objects[i]);
        }
        _objects.Clear();
    }
}
