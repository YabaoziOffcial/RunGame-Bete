using UnityEngine;

public class MapChunkConfig : MonoBehaviour
{
    [Tooltip("地图块宽度（世界单位）")]
    public float chunkWidth = 40f;
    [Tooltip("地图块高度（世界单位）")]
    public float chunkHeight = 40f;
    [Tooltip("可重置物体的根节点（空则用自身 Transform）")]
    public Transform contentRoot;
}
