using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_1_Bullet : MonoBehaviour
{
    private Vector2 m_Direction;
    private float m_Speed;
    private float m_LifeTimer;

    public void Init(Vector2 direction, float speed, float lifeTime)
    {
        m_Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        m_Speed = speed;
        m_LifeTimer = lifeTime;
    }

    private void Update()
    {
        transform.position += (Vector3)(m_Direction * m_Speed * Time.deltaTime);
        m_LifeTimer -= Time.deltaTime;
        if (m_LifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}