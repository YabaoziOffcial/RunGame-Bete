using UnityEngine;

/// <summary>玩家实体：移动、受击表现、碰撞上报；不写 Model，经 GameController 处理玩法。</summary>
public class Player : ThingBase
{
    public Transform BulletPool, PlayerHPFill;

    private Animation m_MoveAnimation;
    private bool m_IsMoving;

    private PlayerStats Stats => GameController.Instance.Model.Stats;

    private void Awake()
    {
        interactable = false;
        RegisterCollisionCallbacks();
        m_MoveAnimation = GetComponent<Animation>();
    }

    private void RegisterCollisionCallbacks()
    {
        OnTriggerEnter2DCallBack = OnTriggerEnterHandler;
        OnCollisionEnter2DCallBack = OnCollisionEnterHandler;
    }

    // 触发器：经验球等，上报 Controller
    private void OnTriggerEnterHandler(ThingBase thing, Collider2D other)
    {
        GameController.Instance.OnPlayerPickupEx(other.gameObject);
    }

    // 碰撞体：与触发器共用拾取逻辑
    private void OnCollisionEnterHandler(ThingBase thing, Collision2D collision)
    {
        GameController.Instance.OnPlayerPickupEx(collision.gameObject);
    }

    /// <summary>由 GameController 在 Stats 重置后刷新血条。</summary>
    public void RefreshHpBar()
    {
        if (PlayerHPFill == null) return;
        float maxHp = Stats.MaxHp;
        float hpFill = maxHp > 0f ? Stats.CurrentHp / maxHp : 0f;
        PlayerHPFill.SetFillAmount(Mathf.Clamp01(hpFill));
    }

    void Update()
    {
        Move();
    }

    /// <summary>应用伤害并更新血条。返回 true 表示仍存活。</summary>
    public bool ApplyDamage(float damage)
    {
        if (damage <= 0f) return true;

        Stats.ApplyDamage(damage);
        RefreshHpBar();

        if (Stats.IsAlive) return true;

        ObjectPool.PushObj(gameObject);
        return false;
    }

    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (Vector3)(input * Stats.MoveSpeed * Time.deltaTime);

        bool isMoving = input.sqrMagnitude > 0f;

        if (m_MoveAnimation == null) return;
        if (isMoving)
        {
            if (!m_IsMoving)
            {
                m_MoveAnimation.Play();
            }
        }
        else if (m_IsMoving)
        {
            ResetMoveAnimationToStart();
        }

        m_IsMoving = isMoving;
    }

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
    }
}
