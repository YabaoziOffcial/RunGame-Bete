using UnityEngine;

// 游戏模块常量和通用资源入口
public class GameConst
{
    // 敌人预制体 Resources 路径
    public const string EnemyPrefabPath = "Prefab/Enemy/Enemy_1";
    // 经验物体预制体 Resources 路径
    public const string ExPrefabPath = "Prefab/Ex/EX_1";
    // 伤害数字预制体 Resources 路径
    public const string DamageNumberPrefabPath = "Prefab/UI/DamageNumberText";
    // 敌人生成间隔
    public const float EnemySpawnInterval = 1f;
    // 敌人在屏幕外生成时的边距
    public const float EnemySpawnPadding = 1f;
    // 单个 EX 提供的经验值
    public const int ExExpValue = 10;
    // 基础升级经验，每升一级额外增加同等需求
    public const int BaseLevelUpExp = 100;

    public const string PlayerTag = "Player";
    public const string EnemyTag = "Enemy";
    public const string ExTag = "EX";

    // 局内 UI 事件请使用 GameEvents.Id / GameEvents.Raise*（以下为兼容别名，勿在新代码中使用）
    public const string CollectExEvent = GameEvents.Id.PlayerProgressChanged;
    public const string KillEnemyCountChangedEvent = GameEvents.Id.KillCountChanged;
    public const string LevelUpEvent = GameEvents.Id.LevelUp;
    public const string PlayerEquipChangedEvent = GameEvents.Id.EquipListChanged;

    // 缓存 EX 预制体，避免重复加载
    private static GameObject m_ExPrefab;
    // 缓存伤害数字预制体，避免重复加载
    private static GameObject m_DamageNumberPrefab;

    // 获取 EX 预制体
    public static GameObject GetExPrefab()
    {
        if (m_ExPrefab == null)
        {
            m_ExPrefab = ResourceManager.Instance.LoadRes<GameObject>(ExPrefabPath);
        }
        return m_ExPrefab;
    }

    // 获取伤害数字预制体
    public static GameObject GetDamageNumberPrefab()
    {
        if (m_DamageNumberPrefab == null)
        {
            m_DamageNumberPrefab = ResourceManager.Instance.LoadRes<GameObject>(DamageNumberPrefabPath);
        }
        return m_DamageNumberPrefab;
    }
}
