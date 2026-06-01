using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 局内总控制器：刷怪与对局流程、改 Model、发 GameEvents、统一 Open/Close UI。
/// 装备 Update 由 EquipManager + GameRoot 驱动，不在此 Update。
/// </summary>
public class GameController : YBZ.Design.Singleton<GameController>
{
    private GameModel m_Model;
    public GameModel Model => m_Model;
    public Player Player { get; private set; }
    public Transform PlayerTransform => Player != null ? Player.transform : null;
    public bool IsGameOver { get; private set; }

    private EnemySpawnConfigSO m_EnemySpawnConfig;
    private readonly List<EnemySpawnRuntime> m_EnemySpawnRuntimes = new List<EnemySpawnRuntime>();
    private bool m_UiEventsRegistered;

    /// <summary>GameRoot 入口：创建本局 Model（不开 UI，见 GameStart）。</summary>
    public void Init()
    {
        m_Model = new GameModel();
        m_Model.SetStartGameTime(Time.time);
        m_Model.SetStartTime(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    /// <summary>每帧刷怪（GameRoot.Update 调用）。</summary>
    public void Update()
    {
        if (IsGameOver) return;
        UpdateEnemySpawnRules();
    }

    #region UI 收口（唯一 Open/Close 与 HUD 刷新入口）

    /// <summary>打开主 HUD（UIManager 缓存复用）。</summary>
    public GamePanel OpenGamePanel()
    {
        return UIManager.Instance.OpenUI<GamePanel>();
    }

    /// <summary>升级时打开选技能/属性弹窗，并注入当前 PlayerStats。</summary>
    public SelectView OpenSelectView()
    {
        SelectView view = UIManager.Instance.OpenUI<SelectView>();
        if (view != null && m_Model != null)
        {
            view.Refresh(m_Model.Stats);
            view.ShowAvailableEquips();
        }
        return view;
    }

    /// <summary>关闭选技能弹窗（GameOver 等时机）。</summary>
    public void CloseSelectView()
    {
        UIManager.Instance.CloseUI<SelectView>();
    }

    /// <summary>打开结算界面。</summary>
    public GameOverView OpenGameOverView()
    {
        return UIManager.Instance.OpenUI<GameOverView>();
    }

    // 从 UIManager 缓存取已打开的 GamePanel，未打开则返回 null
    private GamePanel GetGamePanel()
    {
        if (UIManager.Instance.UICachas.TryGetValue(typeof(GamePanel), out ViewBase view))
        {
            return view as GamePanel;
        }
        return null;
    }

    // 开局或需要时全量刷新 HUD（经验条、击杀数、装备栏）
    private void RefreshGameHud()
    {
        GamePanel panel = GetGamePanel();
        if (panel == null || m_Model == null) return;

        panel.RefreshHud(GameExpSnapshot.FromModel(m_Model), m_Model.KillEnemyCount);
        panel.RefreshEquipIcons(EquipManager.Instance.Equips);
    }

    // 订阅局内 UI 事件：只做 View 刷新/开窗，不写 Model
    private void RegisterUiEventHandlers()
    {
        if (m_UiEventsRegistered) return;

        EventManager.AddListener(GameEvents.Id.PlayerProgressChanged, OnUiPlayerProgressChanged);
        EventManager.AddListener(GameEvents.Id.KillCountChanged, OnUiKillCountChanged);
        EventManager.AddListener(GameEvents.Id.LevelUp, OnUiLevelUp);
        EventManager.AddListener(GameEvents.Id.EquipListChanged, OnUiEquipListChanged);
        m_UiEventsRegistered = true;
    }

    // GameOver 时注销，避免重复监听与泄漏
    private void UnregisterUiEventHandlers()
    {
        if (!m_UiEventsRegistered) return;

        EventManager.RemoveListener(GameEvents.Id.PlayerProgressChanged, OnUiPlayerProgressChanged);
        EventManager.RemoveListener(GameEvents.Id.KillCountChanged, OnUiKillCountChanged);
        EventManager.RemoveListener(GameEvents.Id.LevelUp, OnUiLevelUp);
        EventManager.RemoveListener(GameEvents.Id.EquipListChanged, OnUiEquipListChanged);
        m_UiEventsRegistered = false;
    }

    // 经验/等级变化：增量刷新经验条
    private void OnUiPlayerProgressChanged(params object[] args)
    {
        GamePanel panel = GetGamePanel();
        if (panel == null) return;

        if (GameEvents.TryGetPlayerProgressChanged(args, out GameExpSnapshot snapshot))
        {
            panel.RefreshExpAndLevel(snapshot);
            return;
        }

        if (m_Model != null)
        {
            panel.RefreshExpAndLevel(GameExpSnapshot.FromModel(m_Model));
        }
    }

    // 击杀数变化：刷新 HUD 文本
    private void OnUiKillCountChanged(params object[] args)
    {
        GamePanel panel = GetGamePanel();
        if (panel == null) return;

        if (!GameEvents.TryGetKillCount(args, out int killCount) && m_Model != null)
        {
            killCount = m_Model.KillEnemyCount;
        }

        panel.RefreshKillCount(killCount);
    }

    // 升级：由 Controller 开 SelectView，View 不自行 SetActive 子节点
    private void OnUiLevelUp(params object[] args)
    {
        OpenSelectView();
    }

    // 装备列表变化：刷新 HUD 装备图标
    private void OnUiEquipListChanged(params object[] args)
    {
        GamePanel panel = GetGamePanel();
        if (panel == null) return;
        panel.RefreshEquipIcons(EquipManager.Instance.Equips);
    }

    #endregion

    // 读取刷怪配置表，生成本局每条规则的运行时状态
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

    // 逐条扣冷却、判条件，在屏幕外生成敌人
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

    // 根据等级、已玩时长判断该规则是否处于生效区间
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

    // 从对象池取敌人并放到摄像机视野外
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

    /// <summary>开局：刷怪、绑定玩家、开 HUD、注册 UI 事件。</summary>
    public void GameStart()
    {
        IsGameOver = false;
        InitEnemySpawnRuntimes();
        BindPlayer();

        OpenGamePanel();
        RegisterUiEventHandlers();
        RefreshGameHud();
    }

    // 查找场景 Player，重置 Stats，并交给 EquipManager 开局装备
    private void BindPlayer()
    {
        Player = GameObject.FindObjectOfType<Player>();
        if (Player == null) return;

        m_Model.Stats.ResetToDefault();
        Player.RefreshHpBar();
        EquipManager.Instance.StartSession(Player);
    }

    /// <summary>玩家死亡：停 UI 事件、关装备会话、打开结算 UI。</summary>
    public void GameOver()
    {
        Debug.Log("GameOver");
        if (!IsGameOver)
        {
            IsGameOver = true;
            UnregisterUiEventHandlers();
            CloseSelectView();
            EquipManager.Instance.EndSession();
            OpenGameOverView();
        }
    }

    /// <summary>敌人死亡上报：击杀数 + 经验球掉落 + 通知 HUD。</summary>
    public void OnEnemyKilled(Vector3 deathPosition)
    {
        if (m_Model == null) return;
        m_Model.AddKillEnemyCount();
        GameEvents.RaiseKillCountChanged(m_Model.KillEnemyCount);
        TryDropExAt(deathPosition);
    }

    /// <summary>玩家受伤上报：改 Stats.CurrentHp，归零则 GameOver。</summary>
    public void OnPlayerDamaged(float damage)
    {
        if (IsGameOver || Player == null || damage <= 0f) return;
        if (Player.ApplyDamage(damage)) return;
        GameOver();
    }

    /// <summary>拾取经验球：回收物体、加经验、发进度/升级事件。</summary>
    public void OnPlayerPickupEx(GameObject exObject)
    {
        if (exObject == null || !exObject.CompareTag(GameConst.ExTag)) return;

        int exp = ResolveExValue(exObject.name);
        ObjectPool.PushObj(exObject);
        OnCollectExp(exp);
    }

    /// <summary>经验掉落概率（读取 Stats.Greed）。</summary>
    public float GetPlayerExDropRate()
    {
        return m_Model != null ? m_Model.Stats.Greed : 0f;
    }

    // 改 Model 经验/等级，并广播 UI 事件（连升会多次 RaiseLevelUp）
    private void OnCollectExp(int exp)
    {
        if (m_Model == null || exp <= 0) return;

        int levelsGained = m_Model.AddExp(exp);
        GameEvents.RaisePlayerProgressChanged(m_Model);

        for (int i = 0; i < levelsGained; i++)
        {
            GameEvents.RaiseLevelUp(m_Model);
        }
    }

    // 按经验球预制体名称解析经验值
    private static int ResolveExValue(string exObjectName)
    {
        return exObjectName switch
        {
            "EX_1" => GameConst.ExExpValue,
            "EX_2" => GameConst.ExExpValue * 2,
            "EX_3" => GameConst.ExExpValue * 3,
            _ => GameConst.ExExpValue,
        };
    }

    // 按 Greed 概率在死亡位置生成经验球
    private void TryDropExAt(Vector3 position)
    {
        if (Random.value > GetPlayerExDropRate()) return;

        GameObject exPrefab = GameConst.GetExPrefab();
        if (exPrefab == null) return;

        GameObject ex = ObjectPool.GetObj(exPrefab);
        ex.transform.position = position;
    }

    // 在摄像机视野外四边随机一点作为刷怪出生点
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
        public int RemainingCount;

        public EnemySpawnRuntime(EnemySpawnRule rule)
        {
            Rule = rule;
            SpawnTimer = 0f;
            RemainingCount = rule.count;
        }
    }
}
