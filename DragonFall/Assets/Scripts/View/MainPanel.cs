using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 主 HUD：仅负责显示，由 GameController 驱动刷新与开关
public class MainPanel : Y_PanelBase
{
    [SerializeField] Button m_CardBtn, m_StartBtn, m_SettingBtn, m_ExitBtn;

    new public void Start()
    {
        base.Start();
        m_StartBtn.onClick.AddListener(OnStartClick);
        m_SettingBtn.onClick.AddListener(OnSettingClick);
        m_ExitBtn.onClick.AddListener(OnExitClick);
    }

    public void RefreshMainPanel()
    {

    }

    public void OnStartClick()
    {
        // 关闭主菜单，启动对局（GameStart 内部会打开 GamePanel 并开始刷怪）

        GameController.Instance.GameStart();
    }

    public void OnCardClick()
    {
        // 打开卡牌界面
        // UIManager.Instance.OpenUI<CardPanel>();
    }

    public void OnSettingClick()
    {
        // UIManager.Instance.OpenUI<SettingPanel>();
    }

    public void OnExitClick()
    {
        Application.Quit();
    }


}
