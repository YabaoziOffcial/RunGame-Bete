using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class UIBtnAnima : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{
    public string soundKey = "UI:BtnClick";
    public bool isPlaySound = true;
    // 只能触发一次
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        transform.DOScale(Vector3.one * 1.2f, 0.2f);
        if (isPlaySound) AudioManager.Instance.PlaySoundTem(soundKey);
        SDKManager.Instance.Vibration(1, 1);
    }

    // 抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(Vector3.one, 0.2f);
    }
}
