using UnityEngine;
using UnityEngine.UI;

// 升级选技能/属性面板：由 GameController 打开并传入数据刷新
public class SelectView : Y_PopupBase
{
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

    /// <summary>展示可选装备/技能列表（待接 CardManager 或 EquipManager 数据）。</summary>
    public void ShowAvailableEquips()
    {
    }
}
