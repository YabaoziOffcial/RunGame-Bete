using UnityEngine;

// 游戏模块常量和通用资源入口
public class GameConst
{
    // 敌人预制体 Resources 路径
    public const string EnemyPrefabPath = "Prefab/Enemy/Enemy_1";
    // 经验物体预制体 Resources 路径
    public const string ExPrefabPath = "Prefab/Ex/EX_1";
    // 敌人生成间隔
    public const float EnemySpawnInterval = 1f;
    // 敌人在屏幕外生成时的边距
    public const float EnemySpawnPadding = 1f;
    // 单个 EX 提供的经验值
    public const int ExExpValue = 10;
    // 基础升级经验，每升一级额外增加同等需求
    public const int BaseLevelUpExp = 100;

    // 缓存 EX 预制体，避免重复加载
    private static GameObject m_ExPrefab;

    // 获取 EX 预制体
    public static GameObject GetExPrefab()
    {
        if (m_ExPrefab == null)
        {
            m_ExPrefab = ResourceManager.Instance.LoadRes<GameObject>(ExPrefabPath);
        }
        return m_ExPrefab;
    }
}
