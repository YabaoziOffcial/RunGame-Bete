using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 敌人受伤时显示的伤害数字
public class DamageNumberText : MonoBehaviour
{
    private const float LifeTime = 1f;

    private Text m_Text;
    private float m_LifeTimer;

    private void Awake()
    {
        m_Text = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        m_LifeTimer -= Time.deltaTime;
        if (m_LifeTimer <= 0f)
        {
            ObjectPool.PushObj(gameObject);
        }
    }

    // 显示伤害数字，并根据伤害值调整字号和颜色
    public void Init(float damage, Vector3 worldPosition)
    {
        if (m_Text == null) m_Text = GetComponentInChildren<Text>();

        if (m_Text != null)
        {
            m_Text.text = Mathf.CeilToInt(damage).ToString();
            ApplyStyle(damage);
        }

        SetWorldPosition(worldPosition);
        m_LifeTimer = LifeTime;
    }

    private void ApplyStyle(float damage)
    {
        if (damage >= 50f)
        {
            m_Text.fontSize = 52;
            m_Text.color = new Color(1f, 0.15f, 0.05f, 1f);
        }
        else if (damage >= 20f)
        {
            m_Text.fontSize = 44;
            m_Text.color = new Color(1f, 0.65f, 0.05f, 1f);
        }
        else
        {
            m_Text.fontSize = 36;
            m_Text.color = Color.white;
        }
    }

    // 世界空间 Canvas 下直接使用命中点作为显示位置
    private void SetWorldPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        transform.localRotation = Quaternion.identity;
    }
}
