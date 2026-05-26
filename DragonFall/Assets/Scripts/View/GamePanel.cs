using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : Y_PanelBase
{
    [SerializeField] Transform m_KillEnemyCountText, m_GameTimeText, m_LvText;

    [SerializeField] Slider m_HpSlider;
    [SerializeField] List<Transform> m_EquipTemplates; // 装备模版 用来显示装备的icon

    #region Player Values
    [SerializeField] Text m_MaxHpText, m_HealText, m_VampireText, m_DefenseText, m_MoveSpeedText;
    [SerializeField] Text m_StrengthText, m_BarrageSpeedText, m_BarrageDurationText, m_AttackRangeText;
    [SerializeField] Text m_BarrageCDText, m_BarrageCountText, m_ReliveNumberText, m_TelekinesisText;
    [SerializeField] Text m_LuckText, m_GrowthText, m_GreedText, m_CurseText;
    [SerializeField] Text m_ReselectText, m_SkipText, m_ExcludeText;
    #endregion

    [SerializeField] Transform m_SelectPanel;

    private new void Awake()
    {

    }

    private new void Start()
    {
        Show();
    }

    public override void Show()
    {
        base.Show();
        EventManager.AddListener(GameConst.CollectExEvent, UpdatePlayerExAndLv);
        EventManager.AddListener(GameConst.PlayerEquipChangedEvent, UpdatePlayerEquip);
        UpdatePlayerExAndLv();
        UpdatePlayerEquip();

        m_SelectPanel.SetActive(false);

        GameController.Instance.Model.LevelUpCallBack += () =>
        {
            m_SelectPanel.SetActive(true);
            // 更新数值
        };
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Update()
    {
        if (GameController.Instance.Model.KillEnemyCountChanged)
        {
            m_KillEnemyCountText.SetText(GameController.Instance.Model.KillEnemyCount.ToString());
            GameController.Instance.Model.KillEnemyCountChanged = false;
        }
    }

    public void FixedUpdate()
    {

        float gameTime = Time.time - GameController.Instance.Model.StartGameTime;
        int totalSeconds = Mathf.FloorToInt(gameTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        m_GameTimeText.SetText($"{minutes:00}:{seconds:00}");
    }


    // 更新玩家经验值和等级
    public void UpdatePlayerExAndLv(params object[] value)
    {
        GameModel model = GameController.Instance.Model;
        m_HpSlider.value = model.LevelUpExp > 0 ? (float)model.Exp / model.LevelUpExp : 0f;
        m_LvText.SetText($"Lv.{model.Level} {model.Exp}/{model.LevelUpExp}");
    }

    // 更新玩家装备图标
    private void UpdatePlayerEquip(params object[] value)
    {
        IReadOnlyList<EquipBase> equipList = EquipManager.Instance.Equips;
        Debug.Log($"equipList.Count: {equipList.Count}");
        int equipCount = Mathf.Min(equipList.Count, m_EquipTemplates.Count);
        for (int i = 0; i < equipCount; i++)
        {
            EquipData equipData = equipList[i].EquipData;
            Sprite icon = equipData.iconSprite;
            if (icon == null && !string.IsNullOrEmpty(equipData.iconPath))
            {
                icon = ResourceManager.Instance.LoadRes<Sprite>(equipData.iconPath);
            }
            if (icon == null && !string.IsNullOrEmpty(equipData.name))
            {
                icon = EquipConst.GetWeaponIconPath(equipData.name);
            }

            m_EquipTemplates[i].Find("Icon").SetSprite(icon);
        }
    }
}
