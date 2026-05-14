using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家控制、装备更新和经验拾取入口
public class Player : MonoBehaviour
{
    // 经验物体使用的 Tag
    private const string ExTag = "EX";

    // 玩家基础属性数据
    [SerializeField] PlayerData m_PlayerData = new PlayerData();
    public PlayerData PlayerData { get => m_PlayerData; private set => m_PlayerData = value; }

    [Header("Equip")]
    // 默认武器配置；为空时仍按类名添加 Weapon_1
    [SerializeField] private WeaponConfigSO m_DefaultWeaponConfig;

    // 玩家装备管理器，负责装备添加、升级和生命周期更新
    private EquipManager m_EquipManager;
    // 当前已装备的只读列表，供 UI 或其它系统查询
    public IReadOnlyList<EquipBase> EquipList => m_EquipManager != null ? m_EquipManager.Equips : null;

    private void Awake()
    {
        // 在玩家初始化时创建装备管理器，后续装备都通过它管理
        m_EquipManager = new EquipManager(this);
    }

    // 初始化玩家默认属性和初始装备
    public void Start()
    {
        SetPlayerData(new PlayerData()
        {
            speed = 3f,
            attackBase = 10f,
            attackRate = 1f,
            defenseBase = 10f,
            hpBase = 100f,
            ExDropRate = 0.3f,
        });

        AddEquip("Weapon_Magic");
    }

    void Update()
    {
        Move();
        UpdateEquip();
    }

    private void FixedUpdate()
    {
        // 物理相关装备逻辑统一放到固定帧入口
        m_EquipManager.FixedUpdateAll();
    }

    // 设置玩家属性数据
    public void SetPlayerData(PlayerData playerData)
    {
        m_PlayerData = playerData;
    }

    // 根据装备类名创建并添加装备
    public EquipBase AddEquip(string id)
    {
        return m_EquipManager.AddEquip(id);
    }

    // 根据配置创建并添加装备
    public EquipBase AddEquip(WeaponConfigSO config)
    {
        return m_EquipManager.AddEquip(config);
    }

    // 升级指定装备，后续装备升级逻辑在这里扩展
    public bool UpgradeEquip(string id)
    {
        return m_EquipManager.UpgradeEquip(id);
    }

    // 每帧更新所有装备逻辑，由装备管理器分发到具体装备
    public void UpdateEquip()
    {
        m_EquipManager.UpdateAll();
    }

    private void OnDestroy()
    {
        // 玩家销毁时让装备释放运行时状态
        m_EquipManager.Clear();
    }

    // 根据输入移动玩家
    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (Vector3)(input * PlayerData.speed * Time.deltaTime);
    }

    // 触发器拾取经验物体
    private void OnTriggerEnter2D(Collider2D other)
    {
        CollectEx(other.gameObject);
    }

    // 碰撞器拾取经验物体
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Y_Debug.Log($"OnCollisionEnter2D: {collision.gameObject.name}");
        CollectEx(collision.gameObject);
    }

    // 收集 EX 经验物体并回收到对象池
    private void CollectEx(GameObject ex)
    {
        if (ex == null || !ex.CompareTag(ExTag)) return;

        int exp = ex.name switch
        {
            "EX_1" => GameConst.ExExpValue,
            "EX_2" => GameConst.ExExpValue * 2,
            "EX_3" => GameConst.ExExpValue * 3,
            _ => GameConst.ExExpValue,
        };
        GameController.Instance.Model.AddExp(exp);
        ObjectPool.PushObj(ex);
        EventManager.SendEvent(GameConst.PlayerExAndLvChangedEvent);
    }
}

[System.Serializable]
public class PlayerData
{
    public float speed;     // 移动速度

    public float attackBase;// 攻击力
    public float attackRate;// 攻击比率

    public float defenseBase;// 防御力

    public float hpBase;// 生命值

    public float ExDropRate; // 经验掉落率
}
