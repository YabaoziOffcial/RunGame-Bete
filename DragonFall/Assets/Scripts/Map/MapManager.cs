using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapConfig m_Config;
    [SerializeField] float m_WallThickness = 1f;

    private List<MapChunk> m_Chunks = new List<MapChunk>();
    private List<GameObject> m_Walls = new List<GameObject>();
    private HashSet<int> m_CandidateSet = new HashSet<int>();
    private Transform m_MapRoot;

    public void Init()
    {
        if (m_MapRoot == null)
        {
            m_MapRoot = new GameObject("MapRoot").transform;
            m_MapRoot.SetParent(transform);
        }
        BuildEntireMap();
    }

    public void Clear()
    {
        for (int i = 0; i < m_Chunks.Count; i++)
        {
            if (m_Chunks[i] != null) Destroy(m_Chunks[i].gameObject);
        }
        m_Chunks.Clear();

        for (int i = 0; i < m_Walls.Count; i++)
        {
            if (m_Walls[i] != null) Destroy(m_Walls[i]);
        }
        m_Walls.Clear();
    }

    private void BuildEntireMap()
    {
        Clear();

        if (m_Config == null || m_Config.nodes == null || m_Config.nodes.Count == 0) return;

        for (int col = 0; col < m_Config.mapWidth; col++)
        {
            for (int row = 0; row < m_Config.mapHeight; row++)
            {
                PlaceChunkAt(col, row);
            }
        }

        if (m_Config.wallPrefab != null)
        {
            BuildWalls();
        }
    }

    private void PlaceChunkAt(int col, int row)
    {
        bool isSpawn = (col == 0 && row == 0);
        int nodeIndex = isSpawn && m_Config.spawnNodeIndex < m_Config.nodes.Count
            ? m_Config.spawnNodeIndex
            : PickNodeIndex(col, row);

        GameObject chunkGo = new GameObject($"Chunk_{col}_{row}");
        chunkGo.transform.SetParent(m_MapRoot);

        MapChunk chunk = chunkGo.AddComponent<MapChunk>();
        chunk.Init(m_Config.nodes[nodeIndex].prefab, nodeIndex, col, row);

        chunkGo.transform.position = new Vector3(col * chunk.ChunkWidth, row * chunk.ChunkHeight, 0f);

        m_Chunks.Add(chunk);
    }

    private void BuildWalls()
    {
        float totalWidth = 0f;
        float totalHeight = 0f;
        float cellWidth = 40f;
        float cellHeight = 40f;

        if (m_Chunks.Count > 0)
        {
            cellWidth = m_Chunks[0].ChunkWidth;
            cellHeight = m_Chunks[0].ChunkHeight;
        }

        totalWidth = m_Config.mapWidth * cellWidth;
        totalHeight = m_Config.mapHeight * cellHeight;
        float hw = m_WallThickness * 0.5f;

        CreateWall("Wall_Bottom", new Vector3(totalWidth * 0.5f, -hw, 0f), new Vector3(totalWidth + m_WallThickness * 2, m_WallThickness, 1f));
        CreateWall("Wall_Top", new Vector3(totalWidth * 0.5f, totalHeight + hw, 0f), new Vector3(totalWidth + m_WallThickness * 2, m_WallThickness, 1f));
        CreateWall("Wall_Left", new Vector3(-hw, totalHeight * 0.5f, 0f), new Vector3(m_WallThickness, totalHeight + m_WallThickness * 2, 1f));
        CreateWall("Wall_Right", new Vector3(totalWidth + hw, totalHeight * 0.5f, 0f), new Vector3(m_WallThickness, totalHeight + m_WallThickness * 2, 1f));
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = ObjectPool.GetObj(m_Config.wallPrefab, m_MapRoot);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        m_Walls.Add(wall);
    }

    private int PickNodeIndex(int col, int row)
    {
        m_CandidateSet.Clear();
        bool hasConstraint = false;

        TryApplyConstraint(col - 1, row, n => n.rightConnections, ref hasConstraint);
        TryApplyConstraint(col + 1, row, n => n.leftConnections, ref hasConstraint);
        TryApplyConstraint(col, row - 1, n => n.downConnections, ref hasConstraint);
        TryApplyConstraint(col, row + 1, n => n.upConnections, ref hasConstraint);

        if (hasConstraint && m_CandidateSet.Count > 0)
        {
            int idx = Random.Range(0, m_CandidateSet.Count);
            foreach (int n in m_CandidateSet)
            {
                if (idx-- == 0) return n;
            }
        }

        return Random.Range(0, m_Config.nodes.Count);
    }

    private void TryApplyConstraint(int col, int row, System.Func<ChunkNode, List<int>> getConnections, ref bool hasConstraint)
    {
        if (col < 0 || col >= m_Config.mapWidth || row < 0 || row >= m_Config.mapHeight) return;

        MapChunk neighbor = GetChunkAt(col, row);
        if (neighbor == null) return;

        List<int> connections = getConnections(m_Config.nodes[neighbor.NodeIndex]);
        if (connections == null || connections.Count == 0) return;

        if (!hasConstraint)
        {
            hasConstraint = true;
            m_CandidateSet.UnionWith(connections);
        }
        else
        {
            m_CandidateSet.IntersectWith(connections);
        }
    }

    private MapChunk GetChunkAt(int col, int row)
    {
        int index = row * m_Config.mapWidth + col;
        if (index < 0 || index >= m_Chunks.Count) return null;
        return m_Chunks[index];
    }
}
