using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : YBZ.Design.Singleton<GameController>
{
    private GameModel m_Model;
    public GameModel Model => m_Model;
    public Player Player { get; private set; }

    private float m_EnemySpawnTimer;
    private GameObject m_EnemyPrefab;
    private EnemySpawnConfigSO m_EnemySpawnConfig;
    private readonly List<EnemySpawnRuntime> m_EnemySpawnRuntimes = new List<EnemySpawnRuntime>();


    public void Init()
    {
        m_Model = new GameModel();
        m_Model.SetStartGameTime(Time.time);
        m_Model.SetStartTime(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        m_EnemySpawnConfig = ResourceManager.Instance.LoadRes<EnemySpawnConfigSO>(PathConst.GetEnemySpawnConfigPath());
        InitEnemySpawnRuntimes();
        m_EnemyPrefab = ResourceManager.Instance.LoadRes<GameObject>(PathConst.GetEnemyPrefabPath("Enemy_1"));
        m_EnemySpawnTimer = 0f;
        Player = GameObject.FindObjectOfType<Player>();
    }


    public void Update()
    {
        if (m_EnemySpawnRuntimes.Count > 0)
        {
            UpdateEnemySpawnRules();    // 
            return;
        }

        m_EnemySpawnTimer -= Time.deltaTime;
        if (m_EnemySpawnTimer > 0f) return;
        SpawnEnemy();
        m_EnemySpawnTimer = GameConst.EnemySpawnInterval;
    }

    private void InitEnemySpawnRuntimes()
    {
        m_EnemySpawnRuntimes.Clear();
        if (m_EnemySpawnConfig == null || m_EnemySpawnConfig.rules == null) return;

        for (int i = 0; i < m_EnemySpawnConfig.rules.Count; i++)
        {
            EnemySpawnRule rule = m_EnemySpawnConfig.rules[i];
            if (rule == null || string.IsNullOrEmpty(rule.enemyName)) continue;

            m_EnemySpawnRuntimes.Add(new EnemySpawnRuntime(rule));
        }
    }

    private void UpdateEnemySpawnRules()
    {
        for (int i = 0; i < m_EnemySpawnRuntimes.Count; i++)
        {
            EnemySpawnRuntime runtime = m_EnemySpawnRuntimes[i];
            runtime.CleanupInactiveEnemies();

            if (!CanSpawn(runtime.Rule)) continue;
            if (runtime.Rule.maxAliveCount > 0 && runtime.AliveCount >= runtime.Rule.maxAliveCount) continue;

            runtime.SpawnTimer -= Time.deltaTime;
            if (runtime.SpawnTimer > 0f) continue;

            SpawnEnemy(runtime);
            runtime.SpawnTimer = runtime.Rule.spawnInterval;
        }
    }

    private bool CanSpawn(EnemySpawnRule rule)
    {
        if (rule == null || m_Model == null) return false;

        float gameTime = Time.time - m_Model.StartGameTime;
        return m_Model.Level >= rule.unlockLevel && gameTime >= rule.unlockGameTime;
    }

    private void SpawnEnemy()
    {
        if (m_EnemyPrefab == null || Camera.main == null) return;

        GameObject enemy = ObjectPool.GetObj(m_EnemyPrefab);
        enemy.transform.position = GetRandomSpawnPosition();
    }

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
        runtime.AliveEnemies.Add(enemy);
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        UIManager.Instance.OpenUI<GameOverView>(); 
    }

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


    private class EnemySpawnRuntime
    {
        public EnemySpawnRule Rule { get; private set; }
        public float SpawnTimer;
        public GameObject EnemyPrefab;
        public readonly List<GameObject> AliveEnemies = new List<GameObject>();
        public int AliveCount => AliveEnemies.Count;

        public EnemySpawnRuntime(EnemySpawnRule rule)
        {
            Rule = rule;
            SpawnTimer = 0f;
        }

        public void CleanupInactiveEnemies()
        {
            for (int i = AliveEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = AliveEnemies[i];
                if (enemy == null || !enemy.activeInHierarchy)
                {
                    AliveEnemies.RemoveAt(i);
                }
            }
        }
    }
}
