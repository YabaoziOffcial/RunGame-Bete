using UnityEngine;

public class MapChunk : MonoBehaviour
{
    public int NodeIndex { get; private set; }
    public int LogicalCol { get; private set; }
    public int LogicalRow { get; private set; }
    public float ChunkWidth { get; private set; }
    public float ChunkHeight { get; private set; }

    private GameObject m_ContentInstance;
    private MapChunkConfig m_Config;
    private IChunkResetable[] m_Resetables;

    public void Init(GameObject chunkPrefab, int nodeIndex, int logicalCol, int logicalRow)
    {
        NodeIndex = nodeIndex;

        if (m_ContentInstance != null)
        {
            if (m_Config != null && chunkPrefab != null && m_Config.gameObject.name == chunkPrefab.name)
            {
                SetPosition(logicalCol, logicalRow);
                ResetContent();
                return;
            }

            ObjectPool.PushObj(m_ContentInstance);
            m_ContentInstance = null;
        }

        m_ContentInstance = ObjectPool.GetObj(chunkPrefab, transform);
        m_ContentInstance.transform.localPosition = Vector3.zero;

        m_Config = m_ContentInstance.GetComponent<MapChunkConfig>();
        if (m_Config != null)
        {
            ChunkWidth = m_Config.chunkWidth;
            ChunkHeight = m_Config.chunkHeight;
            Transform root = m_Config.contentRoot != null ? m_Config.contentRoot : m_ContentInstance.transform;
            m_Resetables = root.GetComponentsInChildren<IChunkResetable>(true);
        }
        else
        {
            ChunkWidth = 40f;
            ChunkHeight = 40f;
            m_Resetables = new IChunkResetable[0];
        }

        SetPosition(logicalCol, logicalRow);
    }

    public void SetPosition(int col, int row)
    {
        LogicalCol = col;
        LogicalRow = row;
        transform.position = new Vector3(col * ChunkWidth, row * ChunkHeight, 0f);
    }

    public void ResetContent()
    {
        if (m_Resetables == null) return;
        for (int i = 0; i < m_Resetables.Length; i++)
        {
            if (m_Resetables[i] != null)
            {
                m_Resetables[i].OnChunkReset();
            }
        }
    }

    private void OnDestroy()
    {
        if (m_ContentInstance != null)
        {
            ObjectPool.PushObj(m_ContentInstance);
        }
    }
}
