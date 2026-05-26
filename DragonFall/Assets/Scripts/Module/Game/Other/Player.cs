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

    // 玩家当前的生命值
    public float currentHp;
    public Transform BulletPool, PlayerHP, PlayerHPFill;

    private Animation m_MoveAnimation;
    private bool m_WasMoving;

    // 当前已装备的只读列表，供 UI 或其它系统查询
    public IReadOnlyList<EquipBase> EquipList => EquipManager.Instance.Equips;

    private void Awake()
    {
        // 在玩家初始化时设置装备管理器宿主，后续装备都通过单例管理
        EquipManager.Instance.Init(this);
        m_MoveAnimation = GetComponent<Animation>();
    }

    // 初始化玩家默认属性和初始装备
    public void Start()
    {
        float maxHp = m_PlayerData.hpBase; // 最大生命值（每次需要重新计算）
        float hpFill = currentHp / maxHp;
        PlayerHPFill.SetFillAmount(hpFill);

        SetPlayerData(new PlayerData()
        {
            speed = 1f,
            attackBase = 10f,
            attackRate = 1f,
            defenseBase = 10f,
            hpBase = 100f,
            ExDropRate = 0.3f,
        });

        // AddEquip("Weapon_Magic");
        // AddEquip("Weapon_Dart");
        AddEquip("Weapon_Sword");
    }

    void Update()
    {
        Move();
        UpdateEquip();
    }

    private void FixedUpdate()
    {
        // 物理相关装备逻辑统一放到固定帧入口
        EquipManager.Instance.FixedUpdateAll();
    }

    // 设置玩家属性数据
    public void SetPlayerData(PlayerData playerData)
    {
        m_PlayerData = playerData;
    }

    // 根据装备类名创建并添加装备
    public void AddEquip(string key)
    {
        EquipManager.Instance.AddEquip(key);
        EventManager.SendEvent(GameConst.PlayerEquipChangedEvent);
    }

    // 根据配置创建并添加装备
    public EquipBase AddEquip(WeaponConfigSO config)
    {
        return EquipManager.Instance.AddEquip(config);
    }

    // 升级指定装备，后续装备升级逻辑在这里扩展
    public bool UpgradeEquip(string id)
    {
        return EquipManager.Instance.UpgradeEquip(id);
    }

    // 每帧更新所有装备逻辑，由装备管理器分发到具体装备
    public void UpdateEquip()
    {
        EquipManager.Instance.UpdateAll();
    }

    private void OnDestroy()
    {
        // 玩家销毁时让装备释放运行时状态
        EquipManager.Instance.Clear();
    }

    // 根据输入移动玩家
    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (Vector3)(input * PlayerData.speed * Time.deltaTime);

        bool isMoving = input.sqrMagnitude > 0f;

        if (m_MoveAnimation == null) return;
        if (isMoving)
        {
            if (!m_WasMoving)
            {
                m_MoveAnimation.Play();
            }
        }
        else if (m_WasMoving)
        {
            ResetMoveAnimationToStart();
        }

        m_WasMoving = isMoving;
    }

    private void ResetMoveAnimationToStart()
    {
        if (m_MoveAnimation == null || m_MoveAnimation.clip == null) return;
        string clipName = m_MoveAnimation.clip.name;
        AnimationState state = m_MoveAnimation[clipName];
        if (state == null) return;
        m_MoveAnimation.Play(clipName);
        state.enabled = true;
        state.weight = 1f;
        state.normalizedTime = 0f;
        state.time = 0f;
        m_MoveAnimation.Sample();
        m_MoveAnimation.Stop(clipName);
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

    public float ExDropRate; // 经验掉落率 贪婪因子
}
