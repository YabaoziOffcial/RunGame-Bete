using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Game/Enemy Spawn Config")]
public class EnemySpawnConfigSO : ScriptableObject
{
    public List<EnemySpawnRule> rules = new List<EnemySpawnRule>
    {
        new EnemySpawnRule()
    };
}

[System.Serializable]
public class EnemySpawnRule
{
    // Resources/Prefab/Enemy 下的敌人预制体名称，例如 Enemy_1
    public string enemyName = "Enemy_1";
    // 玩家达到该等级后允许刷新
    public int unlockLevel = 1;
    // 本局经过多少秒后允许刷新
    public float unlockGameTime = 0f;
    // 该敌人的刷新间隔
    public float spawnInterval = 1f;
    // 该规则下同类敌人的最大存活数量；小于等于 0 表示不限制
    public int maxAliveCount = 20;
}
