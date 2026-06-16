using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : Y_PopupBase
{
    [SerializeField] Button m_ExitBtn, m_RestartBtn;


    new public void Start()
    {
        base.Start();
        m_ExitBtn.onClick.AddListener(()=>
        {
            // 打算结算页面
        });

        m_RestartBtn.onClick.AddListener(()=>
        {
            // 重新开始
        });
    }
}
