using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PrefabId
{
    ATTACHMENT_POINT,
}

[Serializable]
public struct PrefabEntry
{
    public PrefabId Id;
    public GameObject Prefab;
}

public class Prefabs : MonoBehaviour
{
    [SerializeField]
    List<PrefabEntry> _prefabEntries = new();

    private Dictionary<PrefabId, GameObject> _lookup = new();

    public static Prefabs Instance { get; private set; }

    private void Awake()
    {
        _lookup = _prefabEntries.ToDictionary(e => e.Id, e => e.Prefab);

        Instance = this;
    }

    public GameObject Get(PrefabId id) => _lookup[id];
}
