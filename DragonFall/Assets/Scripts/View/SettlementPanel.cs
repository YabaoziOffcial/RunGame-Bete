using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettlementPanel : Y_PanelBase
{
    [SerializeField] Button m_CompleteBtn;

    new public void Start()
    {
        base.Start();
        m_CompleteBtn.onClick.AddListener(()=>
        {
            // 关闭面板
            // UIManager.Instance.OpenUI<MainPanel>();
        });
    }
}
