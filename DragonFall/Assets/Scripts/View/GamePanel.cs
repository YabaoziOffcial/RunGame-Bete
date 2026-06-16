using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 主 HUD：仅负责显示，由 GameController 驱动刷新与开关
public class GamePanel : Y_PanelBase
{
    [SerializeField] Transform m_KillEnemyCountText, m_GameTimeText, m_LvText;

    [SerializeField] Slider m_HpSlider;
    [SerializeField] List<EquipUnit> m_EquipUnits;

    /// <summary>全量刷新经验条、等级文本与击杀数（开局由 Controller 调用）。</summary>
    public void RefreshHud(GameExpSnapshot expSnapshot, int killCount)
    {
        RefreshExpAndLevel(expSnapshot);
        m_KillEnemyCountText.SetText(killCount.ToString());
    }

    /// <summary>仅刷新经验条与等级（响应 PlayerProgressChanged）。</summary>
    public void RefreshExpAndLevel(GameExpSnapshot snapshot)
    {
        m_HpSlider.value = snapshot.LevelUpExp > 0 ? (float)snapshot.Exp / snapshot.LevelUpExp : 0f;
        m_LvText.SetText($"Lv.{snapshot.Level} {snapshot.Exp}/{snapshot.LevelUpExp}");
    }

    /// <summary>仅刷新击杀数文本。</summary>
    public void RefreshKillCount(int killCount)
    {
        m_KillEnemyCountText.SetText(killCount.ToString());
    }

    /// <summary>按当前装备列表刷新 HUD 装备图标槽。</summary>
    public void RefreshEquipIcons(IReadOnlyList<EquipBase> equipList)
    {
        int equipCount = Mathf.Min(equipList.Count, m_EquipUnits.Count);
        for (int i = 0; i < equipCount; i++)
        {
            EquipData equipData = equipList[i].EquipData;
            Sprite icon = equipData.iconSprite;

            if (icon == null && equipData.weaponConfig != null)
            {
                icon = equipData.weaponConfig.iconSprite;
            }

            if (icon == null && !string.IsNullOrEmpty(equipData.iconPath))
            {
                icon = ResourceManager.Instance.LoadRes<Sprite>(equipData.iconPath);
            }

            if (icon == null && !string.IsNullOrEmpty(equipData.name))
            {
                icon = EquipConst.GetWeaponIconPath(equipData.name);
            }

            m_EquipUnits[i].equipIcon.sprite = icon != null ? icon : EquipConst.DefaultIcon;
        }
    }

    // 本局已玩时长：只读 Model.StartGameTime，不订阅事件
    private void FixedUpdate()
    {
        GameModel model = GameController.Instance?.Model;
        if (model == null) return;

        float gameTime = Time.time - model.StartGameTime;
        int totalSeconds = Mathf.FloorToInt(gameTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        m_GameTimeText.SetText($"{minutes:00}:{seconds:00}");
    }
}
