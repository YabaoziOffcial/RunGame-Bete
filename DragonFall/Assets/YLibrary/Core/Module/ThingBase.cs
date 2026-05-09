using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ThingBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("是否可交互"), Space(10)]
    public bool interactable = true; // 是否可交互

    public Action<ThingBase, PointerEventData> OnDownCallBack;

    public Action<ThingBase, PointerEventData> OnClickCallBack;

    public Action<ThingBase, PointerEventData> OnPointerUpCallBack;

    public Action<ThingBase, PointerEventData> OnPointerEnterCallBack;

    public Action<ThingBase, PointerEventData> OnPointerExitCallBack;

    public Action<ThingBase, PointerEventData> OnBeginDragCallBack;

    public Action<ThingBase, PointerEventData> OnDragCallBack;

    public Action<ThingBase, PointerEventData> OnEndDragCallBack;

    public Action<ThingBase, Collider2D> OnTriggerEnter2DCallBack;

    public Action<ThingBase, Collider2D> OnTriggerExit2DCallBack;

    public Action<ThingBase, Collider2D> OnTriggerStayCallBack;

    public Action<ThingBase, Collision2D> OnCollisionEnter2DCallBack;

    public Action<ThingBase, Collision2D> OnCollisionExit2DCallBack;

    public Action<ThingBase, Collision2D> OnCollisionStay2DCallBack;

    public Action<ThingBase> OnBecameVisibleCallBack;

    public Action<ThingBase> OnBecameInvisibleCallBack;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnDownCallBack?.Invoke(this, eventData);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnClickCallBack?.Invoke(this, eventData);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnPointerUpCallBack?.Invoke(this, eventData);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnPointerEnterCallBack?.Invoke(this, eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnPointerExitCallBack?.Invoke(this, eventData);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnBeginDragCallBack?.Invoke(this, eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnDragCallBack?.Invoke(this, eventData);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!IsPointerAllowed(eventData)) return;
        OnEndDragCallBack?.Invoke(this, eventData);
    }

    #region  触发器和碰撞器
    public virtual void OnTriggerEnter2D(Collider2D other) { OnTriggerEnter2DCallBack?.Invoke(this, other); }

    public virtual void OnTriggerExit2D(Collider2D other) { OnTriggerExit2DCallBack?.Invoke(this, other); }

    public virtual void OnTriggerStay2D(Collider2D other) { OnTriggerStayCallBack?.Invoke(this, other); }

    public virtual void OnCollisionEnter2D(Collision2D other) { OnCollisionEnter2DCallBack?.Invoke(this, other); }

    public virtual void OnCollisionExit2D(Collision2D other) { OnCollisionExit2DCallBack?.Invoke(this, other); }

    public virtual void OnCollisionStay2D(Collision2D other) { OnCollisionStay2DCallBack?.Invoke(this, other); }
    #endregion

    #region 可见性
    public void OnBecameVisible() { OnBecameVisibleCallBack?.Invoke(this); }

    public void OnBecameInvisible() { OnBecameInvisibleCallBack?.Invoke(this); }

    #endregion

    // 如果想要实现近距离渲染，请选择LOD组件实现, 并且通过Shader实现广告牌, 最终在对固定的按键监听，实现点击效果

    #region 回调
    // 安全防线，防止内存泄漏，
    public void ClearCallbacks()
    {
        OnDownCallBack = null;
        OnClickCallBack = null;
        OnPointerUpCallBack = null;
        OnPointerEnterCallBack = null;
        OnPointerExitCallBack = null;
        OnBeginDragCallBack = null;
        OnDragCallBack = null;
        OnEndDragCallBack = null;
        OnTriggerEnter2DCallBack = null;
        OnTriggerExit2DCallBack = null;
        OnTriggerStayCallBack = null;
        OnCollisionEnter2DCallBack = null;
        OnCollisionExit2DCallBack = null;
        OnCollisionStay2DCallBack = null;
        OnBecameVisibleCallBack = null;
        OnBecameInvisibleCallBack = null;
    }
    #endregion

    #region 销毁
    protected virtual void OnDestroy()
    {
        ClearCallbacks();
    }
    #endregion

    #region 判断是否允许点击
    private bool IsPointerAllowed(PointerEventData eventData)
    {
        if (!interactable) return false;
        // 其他的判断逻辑
        return true;
    }
    #endregion


}
