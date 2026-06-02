using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 升级选技能/属性面板：由 GameController 打开并传入数据刷新
public class SelectView : Y_PopupBase
{
    [SerializeField] List<GameObject> m_EquipUnitTemplates = new List<GameObject>(); // 已放置 + 对象池取出的 EquipUnit 缓存
    [SerializeField] Transform m_EquipUnitRoot;                                        // EquipUnit 挂载父节点
    [SerializeField] GameObject m_EquipUnitPrefab;  

    #region Player Values
    [SerializeField] Text m_MaxHpText, m_HealText, m_VampireText, m_DefenseText, m_MoveSpeedText;
    [SerializeField] Text m_StrengthText, m_BarrageSpeedText, m_BarrageDurationText, m_AttackRangeText;
    [SerializeField] Text m_BarrageCDText, m_BarrageCountText, m_ReliveNumberText, m_PickupRangeText;
    [SerializeField] Text m_LuckText, m_GrowthText, m_GreedText, m_CurseText;
    [SerializeField] Text m_ReselectText, m_SkipText, m_ExcludeText;
    #endregion

    /// <summary>展示当前玩家属性（由 GameController.OpenSelectView 注入，不直接读 Model）。</summary>
    public void Refresh(PlayerStats stats)
    {
        m_MaxHpText.text = stats.MaxHp.ToString();
        m_HealText.text = stats.Heal.ToString();
        m_VampireText.text = stats.Vampire.ToString();
        m_DefenseText.text = stats.Defense.ToString();
        m_MoveSpeedText.text = stats.MoveSpeed.ToString();
        m_StrengthText.text = stats.Strength.ToString();
        m_BarrageSpeedText.text = stats.BarrageSpeed.ToString();
        m_BarrageDurationText.text = stats.BarrageDuration.ToString();
        m_AttackRangeText.text = stats.AttackRange.ToString();
        m_BarrageCDText.text = stats.BarrageCD.ToString();
        m_BarrageCountText.text = stats.BarrageCount.ToString();
        m_ReliveNumberText.text = stats.ReliveNumber.ToString();
        m_PickupRangeText.text = stats.PickupRange.ToString();
        m_LuckText.text = stats.Luck.ToString();
        m_GrowthText.text = stats.Growth.ToString();
        m_GreedText.text = stats.Greed.ToString();
        m_CurseText.text = stats.Curse.ToString();
        m_ReselectText.text = stats.Reselect.ToString();
        m_SkipText.text = stats.Skip.ToString();
        m_ExcludeText.text = stats.Exclude.ToString();
    }

    /// <summary>展示可选装备列表，复用 m_EquipUnitTemplates 缓存并隐藏多余项。</summary>
    public void ShowAvailableEquips()
    {
        List<EquipBase> availableEquips = EquipManager.Instance.GetAvailableEquips();
        int showCount = availableEquips.Count;

        if (!BindEquipUnits(showCount)) return;

        for (int i = 0; i < showCount; i++)
        {
            GameObject unitGo = m_EquipUnitTemplates[i];
            if (unitGo == null) continue;

            EquipUnit equipUnit = unitGo.GetComponent<EquipUnit>();
            if (equipUnit == null) continue;

            EquipBase equip = availableEquips[i];
            WeaponConfigSO config = equip.EquipData?.weaponConfig;
            if (config != null)
            {
                equipUnit.Refresh(config);
            }
            else
            {
                equipUnit.Refresh(equip.EquipData);
            }
        }
    }

    // 关闭面板时隐藏全部 EquipUnit，保留在缓存列表中
    public override void UnLoad()
    {
        HideExtraEquipUnits(0);
        base.UnLoad();
    }

    // 确保 EquipUnit 数量足够，并关闭多余项
    private bool BindEquipUnits(int showCount)
    {
        ResolveEquipUnitRefs();
        if (m_EquipUnitPrefab == null)
        {
            Debug.LogError("SelectView: 未配置 EquipUnit 模板，请在 m_EquipUnitTemplates 或 m_EquipUnitPrefab 中指定。");
            return false;
        }

        EnsureEquipUnitCount(showCount);
        HideExtraEquipUnits(showCount);
        return true;
    }

    // 缓存不足时从 ObjectPool 取新 EquipUnit 并加入列表
    private void EnsureEquipUnitCount(int count)
    {
        while (m_EquipUnitTemplates.Count < count)
        {
            GameObject unit = ObjectPool.GetObj(m_EquipUnitPrefab, m_EquipUnitRoot);
            m_EquipUnitTemplates.Add(unit);
        }
    }

    // 仅显示前 showCount 个，其余 SetActive(false)
    private void HideExtraEquipUnits(int showCount)
    {
        for (int i = 0; i < m_EquipUnitTemplates.Count; i++)
        {
            GameObject unit = m_EquipUnitTemplates[i];
            if (unit == null) continue;
            unit.SetActive(i < showCount);
        }
    }

    // 自动补全父节点与对象池模板（优先使用 Inspector 已填项）
    private void ResolveEquipUnitRefs()
    {
        if (m_EquipUnitRoot == null)
        {
            for (int i = 0; i < m_EquipUnitTemplates.Count; i++)
            {
                if (m_EquipUnitTemplates[i] == null) continue;
                m_EquipUnitRoot = m_EquipUnitTemplates[i].transform.parent;
                break;
            }
        }

        if (m_EquipUnitPrefab == null)
        {
            for (int i = 0; i < m_EquipUnitTemplates.Count; i++)
            {
                if (m_EquipUnitTemplates[i] == null) continue;
                m_EquipUnitPrefab = m_EquipUnitTemplates[i];
                break;
            }
        }
    }


    // 装备点击事件（若按钮走 SelectView 转发）
    public void OnEquipUnitClick(EquipBase equip)
    {
        if (equip == null || equip.EquipData == null) return;
        if (EquipManager.Instance.HasEquip(equip.EquipData.className))
        {
            EquipManager.Instance.UpgradeEquip(equip.EquipData.className);
        }
        else
        {
            EquipManager.Instance.AddEquip(equip.EquipData.className);
        }
        GameController.Instance.CompleteCurrentLevelUpSelection();
    }
}
