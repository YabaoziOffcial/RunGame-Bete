using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using YBZ.Core;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] RectTransform handleRectTransform;    // 把手
    [SerializeField] float _duration = 0.5f;
    [SerializeField] Image _bgImage;
    [SerializeField] Sprite _onSprite;          
    [SerializeField] Sprite _offSprite;
    [SerializeField] Text on_text;
    [SerializeField] Text off_text;
    [SerializeField] Image m_HandlerImage;
    [SerializeField] Sprite m_HandlerOnSprite;
    [SerializeField] Sprite m_HandlerOffSprite;
    // 把手移动的位置
    Vector2 _handlePosition;
    [SerializeField] Toggle _toggle;
    public Action<bool> action;     // toogle 触发的效果

    void Awake()
    {
        _handlePosition = handleRectTransform.anchoredPosition; // 把手位置
        _toggle.onValueChanged.AddListener(OnSwitch);
        Y_Debug.LogBlue("SwitchToggle Awake");
    }

    /// <summary>
    /// 开关动画效果，调用改函数默认
    /// </summary>
    /// <param name="isON"></param>
    /// <param name="animaNone"></param>
    public virtual void OnSwitchNoneAnimation(bool isON)
    {
        handleRectTransform.anchoredPosition = isON ? _handlePosition : -_handlePosition;
        _bgImage.sprite = isON ? _onSprite : _offSprite;
        if (m_HandlerOnSprite != null && m_HandlerOffSprite != null)
            m_HandlerImage.sprite = isON ? m_HandlerOnSprite : m_HandlerOffSprite;

        on_text?.gameObject.SetActive(isON);
        off_text?.gameObject.SetActive(!isON);
        _toggle.isOn = isON;
    }

    // 
    public virtual void OnSwitch(bool isON)
    {
        Debug.Log("OnSwitch");
        AudioManager.Instance.PlaySoundTem("Common:BtnClick");
        handleRectTransform.DOAnchorPos(isON ? _handlePosition : - _handlePosition, _duration).SetEase(Ease.InOutBack);
        _bgImage.sprite = isON ? _onSprite : _offSprite;
        if (m_HandlerOnSprite != null && m_HandlerOffSprite != null)
            m_HandlerImage.sprite = isON ? m_HandlerOnSprite : m_HandlerOffSprite;

        on_text?.gameObject.SetActive(isON);
        off_text?.gameObject.SetActive(!isON);
        action?.Invoke(isON);
    }
}