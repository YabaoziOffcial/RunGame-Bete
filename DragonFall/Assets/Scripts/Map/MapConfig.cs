using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Map/MapConfig")]
public class MapConfig : ScriptableObject
{
    public List<ChunkNode> nodes = new List<ChunkNode>();

    [Header("地图尺寸（块数）")]
    public int mapWidth = 10;
    public int mapHeight = 8;

    [Header("边界墙壁")]
    public GameObject wallPrefab;

    [Header("出生块")]
    public int spawnNodeIndex;
}

[System.Serializable]
public class ChunkNode
{
    public GameObject prefab;
    public Vector2 editorPosition;
    public List<int> upConnections = new List<int>();
    public List<int> downConnections = new List<int>();
    public List<int> leftConnections = new List<int>();
    public List<int> rightConnections = new List<int>();
}
