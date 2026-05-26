using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : Y_PopupBase
{
    [SerializeField] Button m_ExitBtn;


    new public void Start()
    {
        base.Start();
        m_ExitBtn.onClick.AddListener(()=>
        {
            // 打算结算页面
        });
    }
}
