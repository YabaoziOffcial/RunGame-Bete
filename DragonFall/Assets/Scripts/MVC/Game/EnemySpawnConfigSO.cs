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
    // 玩家超过该等级后停止刷新；-1 表示不限制结束等级
    public int endLevel = -1;
    // 本局经过多少秒后允许刷新
    public float unlockGameTime = 0f;
    // 本局超过多少秒后停止刷新；-1 表示不限制结束时间
    public float endGameTime = -1f;
    // 该敌人的刷新间隔
    public float spawnInterval = 1f;
    // 该规则最多生成的敌人总数；小于等于 0 表示不再生成
    public int count = 20;
}
