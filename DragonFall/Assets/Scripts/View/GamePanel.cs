using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePanel : Y_PanelBase
{
    [SerializeField] Transform m_KillEnemyCountText, m_GameTimeText;


    public override void Show()
    {
        base.Show();
    }

    public  void Update() {
        if (GameController.Instance.Model.KillEnemyCountChanged)
        {
            m_KillEnemyCountText.SetText(GameController.Instance.Model.KillEnemyCount.ToString());
            GameController.Instance.Model.KillEnemyCountChanged = false;
        }
    }

    public  void FixedUpdate() {
        
        float gameTime = Time.time - GameController.Instance.Model.StartGameTime;
        int totalSeconds = Mathf.FloorToInt(gameTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        m_GameTimeText.SetText($"{minutes:00}:{seconds:00}");
    }
}
