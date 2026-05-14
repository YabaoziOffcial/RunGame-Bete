using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : Y_PanelBase
{
    [SerializeField] Transform m_KillEnemyCountText, m_GameTimeText, m_LvText;

    [SerializeField] Slider m_HpSlider;

    private bool m_IsListeningPlayerExAndLvChanged;

    public void Start()
    {
        Show();
    }

    public override void Show()
    {
        base.Show();
        AddPlayerExAndLvListener();
        UpdatePlayerExAndLv();
    }

    public override void Close()
    {
        RemovePlayerExAndLvListener();
        base.Close();
    }

    protected override void OnDestroy()
    {
        RemovePlayerExAndLvListener();
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

    public void UpdatePlayerExAndLv(params object[] value)
    {
        GameModel model = GameController.Instance.Model;
        m_HpSlider.value = model.LevelUpExp > 0 ? (float)model.Exp / model.LevelUpExp : 0f;
        m_LvText.SetText($"Lv.{model.Level} {model.Exp}/{model.LevelUpExp}");
    }

    private void AddPlayerExAndLvListener()
    {
        if (m_IsListeningPlayerExAndLvChanged) return;

        EventManager.AddListener(GameConst.PlayerExAndLvChangedEvent, UpdatePlayerExAndLv);
        m_IsListeningPlayerExAndLvChanged = true;
    }

    private void RemovePlayerExAndLvListener()
    {
        if (!m_IsListeningPlayerExAndLvChanged) return;

        EventManager.RemoveListener(GameConst.PlayerExAndLvChangedEvent, UpdatePlayerExAndLv);
        m_IsListeningPlayerExAndLvChanged = false;
    }
}
