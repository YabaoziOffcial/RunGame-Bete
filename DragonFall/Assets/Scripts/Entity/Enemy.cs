using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>敌人实体：移动、受击、攻击玩家；死亡/掉落经 GameController 处理，不写 Model。</summary>
public class Enemy : MonoBehaviour
{
    // 自动补碰撞体时使用的默认半径
    private const float ColliderRadius = 0.5f;

    // 敌人基础属性数据
    [SerializeField] private EnemyData m_EnemyData = new EnemyData() { hp = 10f, attack = 10f, speed = 2f };

    // 对象池复用时用于重置生命值
    [SerializeField] float m_MaxHp;
    public float currentHP;
    public float attackInterval = 0.2f;
    private float m_AttackTimer;

    // 防止死亡逻辑重复触发
    private bool m_IsDead;
    private Rigidbody2D m_Rigidbody2D;
    private Animation m_MoveAnimation;
    private bool m_WasMoving;

    // 初始化物理组件和默认碰撞体
    private void Awake()
    {
        m_MaxHp = m_EnemyData.hp;
        m_MoveAnimation = GetComponent<Animation>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        if (m_Rigidbody2D == null)
        {
            m_Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }
        m_Rigidbody2D.gravityScale = 0f;

        if (GetComponent<Collider2D>() != null) return;

        CircleCollider2D collider2d = gameObject.AddComponent<CircleCollider2D>();
        collider2d.radius = ColliderRadius;
    }

    // 从对象池取出时重置状态
    private void OnEnable()
    {
        if (m_MaxHp > 0f)
        {
            m_EnemyData.hp = m_MaxHp;
        }
        m_IsDead = false;
        m_WasMoving = false;
        m_AttackTimer = 0f;
        ResetMoveAnimationToStart();
    }


    public void Update()
    {
        if (m_IsDead) return;
        if (m_AttackTimer > 0f)
        {
            m_AttackTimer -= Time.deltaTime;
        }
        MoveToPlayer();
    }


    // 设置敌人属性，并同步最大生命值
    public void SetEnemyData(EnemyData enemyData)
    {
        m_EnemyData = enemyData;
        m_MaxHp = m_EnemyData.hp;
    }

    /// <summary>受伤；HP 归零时上报 OnEnemyKilled（击杀数与经验球由 Controller 处理）。</summary>
    public float TakeDamage(float damage)
    {
        return TakeDamage(damage, transform.position);
    }

    // 承受伤害，并在指定命中位置显示伤害数字
    public float TakeDamage(float damage, Vector3 hitPosition)
    {
        if (m_IsDead) return 0f;
        if (damage <= 0f) return 0f;

        float actualDamage = Mathf.Min(damage, m_EnemyData.hp);
        if (actualDamage <= 0f) return 0f;

        m_EnemyData.hp -= actualDamage;
        ShowDamageNumber(actualDamage, hitPosition);
        if (m_EnemyData.hp <= 0f)
        {
            m_IsDead = true;
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.velocity = Vector2.zero;
            }
            ResetMoveAnimationToStart();
            GameController.Instance.OnEnemyKilled(transform.position);
            ObjectPool.PushObj(gameObject);
        }

        return actualDamage;
    }

    // 在敌人位置显示伤害数字
    private void ShowDamageNumber(float damage, Vector3 hitPosition)
    {
        GameObject damageNumberPrefab = GameConst.GetDamageNumberPrefab();
        if (damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumberPrefab is null");
            return;
        }

        Transform damageNumberParent = UIManager.Instance.canvasWorldTransform;
        if (damageNumberParent == null)
        {
            Debug.LogError("canvasWorldTransform 空引用");
            return;
        }

        GameObject damageNumberTextGo = ObjectPool.GetObj(damageNumberPrefab, damageNumberParent);
        DamageNumberText damageNumberText = damageNumberTextGo.GetComponent<DamageNumberText>();
        if (damageNumberText == null)
        {
            Debug.LogError("DamageNumberText 空引用");
            damageNumberText = damageNumberTextGo.AddComponent<DamageNumberText>();
        }
        damageNumberText.Init(damage, hitPosition);
    }

    // 持续朝玩家方向移动
    private void MoveToPlayer()
    {
        Transform playerTransform = GameController.Instance.PlayerTransform;
        if (playerTransform == null || m_Rigidbody2D == null)
        {
            SetMoveAnimationPlaying(false);
            return;
        }

        Vector2 direction = playerTransform.position - transform.position;
        if (direction.sqrMagnitude <= 0f)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            SetMoveAnimationPlaying(false);
            return;
        }

        m_Rigidbody2D.velocity = direction.normalized * m_EnemyData.speed;
        SetMoveAnimationPlaying(true);
    }

    // 播放移动的动画
    private void SetMoveAnimationPlaying(bool isMoving)
    {
        if (m_MoveAnimation == null) return;

        if (isMoving)
        {
            if (!m_WasMoving)
            {
                m_MoveAnimation.Play();
            }
        }
        else if (m_WasMoving)
        {
            ResetMoveAnimationToStart();
        }

        m_WasMoving = isMoving;
    }

    // 重置移动动画到开始状态
    private void ResetMoveAnimationToStart()
    {
        if (m_MoveAnimation == null || m_MoveAnimation.clip == null) return;

        string clipName = m_MoveAnimation.clip.name;
        AnimationState state = m_MoveAnimation[clipName];
        if (state == null) return;

        m_MoveAnimation.Play(clipName);
        state.enabled = true;
        state.weight = 1f;
        state.normalizedTime = 0f;
        state.time = 0f;
        m_MoveAnimation.Sample();
        m_MoveAnimation.Stop(clipName);
        m_WasMoving = false;
    }

    // 触发器持续攻击玩家
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(GameConst.PlayerTag)) return;
        Y_Debug.Log($"OnCollisionStay2D: {other.gameObject.name}");
        TryAttackPlayer();
    }

    // 碰撞器持续攻击玩家
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(GameConst.PlayerTag)) return;
        Y_Debug.Log($"OnCollisionStay2D: {collision.gameObject.name}");
        TryAttackPlayer();
    }

    // 接触玩家时上报伤害，由 GameController 改 Stats 并判定 GameOver
    private void TryAttackPlayer()
    {
        if (m_IsDead || m_AttackTimer > 0f) return;
        if (GameController.Instance.PlayerTransform == null) return;

        GameController.Instance.OnPlayerDamaged(m_EnemyData.attack);
        m_AttackTimer = attackInterval;
    }
}


[System.Serializable]
public class EnemyData
{
    public float hp; // 生命值
    public float speed; // 移动速度
    public float attack; // 攻击力
    public float defense; // 防御力
    public float attackInterval; // 攻击间隔
}
