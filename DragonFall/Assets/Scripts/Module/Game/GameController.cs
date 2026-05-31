using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : YBZ.Design.Singleton<GameController>
{
    private GameModel m_Model;
    public GameModel Model => m_Model;
    public Player Player { get; private set; }

    private EnemySpawnConfigSO m_EnemySpawnConfig;
    private readonly List<EnemySpawnRuntime> m_EnemySpawnRuntimes = new List<EnemySpawnRuntime>();


    public void Init()
    {
        m_Model = new GameModel();
        m_Model.SetStartGameTime(Time.time);
        m_Model.SetStartTime(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        InitEnemySpawnRuntimes();
        Player = GameObject.FindObjectOfType<Player>();
    }


    public void Update()
    {
        UpdateEnemySpawnRules();
    }

    // 将配置表转换为本局运行时刷怪状态
    private void InitEnemySpawnRuntimes()
    {
        m_EnemySpawnConfig = ResourceManager.Instance.LoadRes<EnemySpawnConfigSO>(PathConst.GetEnemySpawnConfigPath("Level1"));
        m_EnemySpawnRuntimes.Clear();
        if (m_EnemySpawnConfig == null || m_EnemySpawnConfig.rules == null) return;

        for (int i = 0; i < m_EnemySpawnConfig.rules.Count; i++)
        {
            EnemySpawnRule rule = m_EnemySpawnConfig.rules[i];
            if (rule == null || string.IsNullOrEmpty(rule.enemyName)) continue;
            m_EnemySpawnRuntimes.Add(new EnemySpawnRuntime(rule));
        }
    }

    // 逐条更新刷怪规则：条件满足、数量未耗尽、冷却结束后生成
    private void UpdateEnemySpawnRules()
    {
        for (int i = 0; i < m_EnemySpawnRuntimes.Count; i++)
        {
            EnemySpawnRuntime runtime = m_EnemySpawnRuntimes[i];

            if (!CanSpawn(runtime.Rule)) continue;
            if (runtime.RemainingCount <= 0) continue;

            runtime.SpawnTimer -= Time.deltaTime;
            if (runtime.SpawnTimer > 0f) continue;

            SpawnEnemy(runtime);
            runtime.SpawnTimer = runtime.Rule.spawnInterval;
        }
    }

    // 判断当前等级和游戏时间是否处于规则允许的生成区间
    private bool CanSpawn(EnemySpawnRule rule)
    {
        if (rule == null || m_Model == null) return false;

        float gameTime = Time.time - m_Model.StartGameTime;
        if (m_Model.Level < rule.unlockLevel) return false;
        if (rule.endLevel >= 0 && m_Model.Level > rule.endLevel) return false;
        if (gameTime < rule.unlockGameTime) return false;
        if (rule.endGameTime >= 0f && gameTime > rule.endGameTime) return false;

        return true;
    }

    // 按指定规则生成敌人，并扣减该规则剩余生成数量
    private void SpawnEnemy(EnemySpawnRuntime runtime)
    {
        if (runtime == null || runtime.Rule == null || Camera.main == null) return;

        if (runtime.EnemyPrefab == null)
        {
            runtime.EnemyPrefab = ResourceManager.Instance.LoadRes<GameObject>(PathConst.GetEnemyPrefabPath(runtime.Rule.enemyName));
        }
        if (runtime.EnemyPrefab == null) return;

        GameObject enemy = ObjectPool.GetObj(runtime.EnemyPrefab);
        enemy.transform.position = GetRandomSpawnPosition();
        runtime.RemainingCount--;
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        UIManager.Instance.OpenUI<GameOverView>(); 
    }

    // 在摄像机视野外四周随机取一个出生点
    private Vector3 GetRandomSpawnPosition()
    {
        Camera mainCamera = Camera.main;
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        Vector3 cameraPosition = mainCamera.transform.position;

        int side = Random.Range(0, 4);
        float x = Random.Range(cameraPosition.x - halfWidth, cameraPosition.x + halfWidth);
        float y = Random.Range(cameraPosition.y - halfHeight, cameraPosition.y + halfHeight);

        switch (side)
        {
            case 0:
                y = cameraPosition.y + halfHeight + GameConst.EnemySpawnPadding;
                break;
            case 1:
                y = cameraPosition.y - halfHeight - GameConst.EnemySpawnPadding;
                break;
            case 2:
                x = cameraPosition.x - halfWidth - GameConst.EnemySpawnPadding;
                break;
            default:
                x = cameraPosition.x + halfWidth + GameConst.EnemySpawnPadding;
                break;
        }

        return new Vector3(x, y, 0f);
    }


    // 单条刷怪规则在本局中的运行时状态
    private class EnemySpawnRuntime
    {
        // 原始配置规则
        public EnemySpawnRule Rule { get; private set; }
        // 当前规则距离下一次生成的倒计时
        public float SpawnTimer;
        // 该规则对应的敌人预制体缓存
        public GameObject EnemyPrefab;
        // 该规则本局剩余可生成数量
        public int RemainingCount;

        public EnemySpawnRuntime(EnemySpawnRule rule)
        {
            Rule = rule;
            SpawnTimer = 0f;
            RemainingCount = rule.count;
        }
    }
}
