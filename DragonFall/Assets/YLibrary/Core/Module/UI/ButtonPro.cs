using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine;


/// <summary>
/// 自己重写的 Button 按钮
/// 1、单击
/// 2、双击
/// 3、长按
/// </summary>    
public class ButtonPro : Selectable, IPointerClickHandler, ISubmitHandler, IPointerExitHandler
{
    public ELongPressType longPressType = ELongPressType.Single;
    public float RepeatDuration = 0.5f;

    [Serializable]
    /// <summary>
    /// Function definition for a button click event.
    /// </summary>
    public class ButtonClickedEvent : UnityEvent { }

    // Event delegates triggered on click.
    [FormerlySerializedAs("onClick")]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new();

    public class ButtonUpEvent : UnityEvent { }

    [FormerlySerializedAs("onPointerDown")]
    [SerializeField]
    private ButtonClickedEvent m_OnPointerDown = new();

    [FormerlySerializedAs("onPointerUp")]
    [SerializeField]
    private ButtonClickedEvent m_OnPointerUp = new();


    [FormerlySerializedAs("onPointerExit")]
    [SerializeField]
    private ButtonClickedEvent m_OnPointerExit = new();
    protected ButtonPro(){ }


    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    private void Press()
    {
        if(!IsActive() || !IsInteractable())
            return;

        UISystemProfilerApi.AddMarker("Button.onClick", this);
        m_OnClick.Invoke();
    }


    [Serializable]
    /// <summary>
    /// Function definition for a button click event.
    /// </summary>
    public class ButtonLongPressEvent : UnityEvent { }

    [FormerlySerializedAs("onLongPress")]
    [SerializeField]
    private ButtonLongPressEvent m_onLongPress = new ButtonLongPressEvent();
    public ButtonLongPressEvent onLongPress
    {
        get { return m_onLongPress; }
        set { m_onLongPress = value; }
    }

    [FormerlySerializedAs("OnDoubleClick")]
    public ButtonClickedEvent m_onDoubleClick = new ButtonClickedEvent();
    public ButtonClickedEvent onDoubleClick
    {
        get { return m_onDoubleClick; }
        set { m_onDoubleClick = value; }
    }

    private bool _isStartPress = false;
    private float _curPointDownTime = 0f;
    private float _longPressTime = 0.6f;
    private bool _longPressTrigger = false;

    public enum ELongPressType 
    { 
        Single,
        Repect,
    }
    void Update()
    {
        CheckIsLongPress();
    }
    void CheckIsLongPress()
    {
        switch(longPressType)
        {
            case ELongPressType.Single:
                LongPressSingle();
                break;
            case ELongPressType.Repect:
                LongPressRepect();
                break;
        }

        
    }

    public void LongPressSingle()
    {
        if(_isStartPress && !_longPressTrigger)
        {
            if(Time.time > _curPointDownTime + _longPressTime)
            {
                _longPressTrigger = true;
                _isStartPress = false;
                if(m_onLongPress != null)
                {
                    m_onLongPress.Invoke();
                }
            }
        }
    }

    private float _LongPressRepectTimer = 0f;
    public void LongPressRepect()
    {
        if(_isStartPress)
        {
            if(Time.time > _curPointDownTime + _longPressTime)
            {
                _LongPressRepectTimer -= Time.deltaTime;
                if(_LongPressRepectTimer <= 0)
                {
                    _LongPressRepectTimer = RepeatDuration;
                    if(m_onLongPress != null)
                        m_onLongPress.Invoke();
                }
                
            }
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //(避免已經點擊進入長按后，擡起的情況)
        if(!_longPressTrigger)
        {
            // 双击
            if(eventData.clickCount == 2)
            {

                if(m_onDoubleClick != null)
                {
                    m_onDoubleClick.Invoke();
                }

            }// 单击
            else if(eventData.clickCount == 1)
            {
                onClick.Invoke();
            }
        }
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        Press();

        // if we get set disabled during the press
        // don't run the coroutine.
        if(!IsActive() || !IsInteractable())
            return;

        DoStateTransition(SelectionState.Pressed, false);
        StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while(elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
    }

    // 点下
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _curPointDownTime = Time.time;
        _isStartPress = true;
        _longPressTrigger = false;

        m_OnPointerDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        _isStartPress = false;
        m_OnPointerUp?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _isStartPress = false;
        m_OnPointerExit?.Invoke();
    }
}
