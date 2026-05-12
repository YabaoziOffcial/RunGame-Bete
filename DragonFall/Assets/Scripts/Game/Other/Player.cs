using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerData m_PlayerData = new PlayerData();
    public PlayerData PlayerData { get => m_PlayerData; private set => m_PlayerData = value; }

    public List<EquipBase> EquipList { get; private set; } = new List<EquipBase>();

    public void Start()
    {
        SetPlayerData(new PlayerData()
        {
            speed = 3f,
            attackBase = 10f,
            attackRate = 1f,
            defenseBase = 10f,
            hpBase = 100f,
        });

        AddEquip("Weapon_1");
    }
    void Update()
    {
        Move();
        UpdateEquip();
    }


    public void SetPlayerData(PlayerData playerData)
    {
        m_PlayerData = playerData;
    }

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

    public void UpgradeEquip(string id)
    {

    }


    public void UpdateEquip()
    {
        foreach (var equip in EquipList)
        {
            equip.OnEquipUpdate(this);
        }
    }

    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (Vector3)(input * PlayerData.speed * Time.deltaTime);
    }
}

[SerializeField]
public class PlayerData
{
    public float speed;     // 移动速度

    public float attackBase;// 攻击力
    public float attackRate;// 攻击比率

    public float defenseBase;// 防御力

    public float hpBase;// 生命值

    public float ExDrogRate; // 经验掉落率
}
