using System;
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

    // 当前已装备的装备列表
    public List<EquipBase> EquipList { get; private set; } = new List<EquipBase>();

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

        AddEquip("Weapon_1");
    }
    void Update()
    {
        Move();
        UpdateEquip();
    }


    // 设置玩家属性数据
    public void SetPlayerData(PlayerData playerData)
    {
        m_PlayerData = playerData;
    }

    // 根据装备类名创建并添加装备
    public void AddEquip(string id)
    {
        Type equipType = typeof(EquipBase).Assembly.GetType(id);
        if (equipType == null || equipType.IsAbstract || !typeof(EquipBase).IsAssignableFrom(equipType))
        {
            Debug.LogError($"未找到装备类: {id}");
            return;
        }
        EquipList.Add((EquipBase)Activator.CreateInstance(equipType));
    }

    // 升级指定装备，后续装备升级逻辑在这里扩展
    public void UpgradeEquip(string id)
    {

    }

    // 每帧更新所有装备逻辑
    public void UpdateEquip()
    {
        foreach (var equip in EquipList)
        {
            equip.OnEquipUpdate(this);
        }
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
