using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectView : Y_PopupBase
{

    #region Player Values
    [SerializeField] Text m_MaxHpText, m_HealText, m_VampireText, m_DefenseText, m_MoveSpeedText;
    [SerializeField] Text m_StrengthText, m_BarrageSpeedText, m_BarrageDurationText, m_AttackRangeText;
    [SerializeField] Text m_BarrageCDText, m_BarrageCountText, m_ReliveNumberText, m_PickupRangeText;
    [SerializeField] Text m_LuckText, m_GrowthText, m_GreedText, m_CurseText;
    [SerializeField] Text m_ReselectText, m_SkipText, m_ExcludeText;
    #endregion

    public override void Show()
    {
        base.Show();
        SetValues();
        ShowOAvailableEquip();
    }

    // 显示当前游戏内的各项属性
    public void SetValues()
    {
        m_MaxHpText.text = GameController.Instance.Model.MaxHp.ToString();
        m_HealText.text = GameController.Instance.Model.Heal.ToString();
        m_VampireText.text = GameController.Instance.Model.Vampire.ToString();
        m_DefenseText.text = GameController.Instance.Model.Defense.ToString();
        m_MoveSpeedText.text = GameController.Instance.Model.MoveSpeed.ToString();
        m_StrengthText.text = GameController.Instance.Model.Strength.ToString();
        m_BarrageSpeedText.text = GameController.Instance.Model.BarrageSpeed.ToString();
        m_BarrageDurationText.text = GameController.Instance.Model.BarrageDuration.ToString();
        m_AttackRangeText.text = GameController.Instance.Model.AttackRange.ToString();
        m_BarrageCDText.text = GameController.Instance.Model.BarrageCD.ToString();
        m_BarrageCountText.text = GameController.Instance.Model.BarrageCount.ToString();
        m_ReliveNumberText.text = GameController.Instance.Model.ReliveNumber.ToString();
        m_PickupRangeText.text = GameController.Instance.Model.PickupRange.ToString();
        m_LuckText.text = GameController.Instance.Model.Luck.ToString();
        m_GrowthText.text = GameController.Instance.Model.Growth.ToString();
        m_GreedText.text = GameController.Instance.Model.Greed.ToString();
        m_CurseText.text = GameController.Instance.Model.Curse.ToString();
        m_ReselectText.text = GameController.Instance.Model.Reselect.ToString();
        m_SkipText.text = GameController.Instance.Model.Skip.ToString();
        m_ExcludeText.text = GameController.Instance.Model.Exclude.ToString();
    }

    // 显示可以获取的装备
    public void ShowOAvailableEquip()
    {
        
    }
}
