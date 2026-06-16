using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI窗口，二级
/// </summary>
/// 
[RequireComponent(typeof(CanvasGroup))]
public class Y_PopupBase : ViewBase
{
    public RectTransform allUI;
    public CanvasGroup cgUI;
    public CanvasGroup cgBG;
    public Animator animator;

    // 手动挂在也行
    [SerializeField] Button m_CloseBtn, m_FinishBtn, m_CancelBtn; // 关闭，完成，取消
    public Action CloseCall, FinishCall, CancelCall;
    
    public Button CloseBtn { get => m_CloseBtn; set => m_CloseBtn = value; }
    public Button FinishBtn { get => m_FinishBtn; set => m_FinishBtn = value; }
    public Button CancelBtn { get => m_CancelBtn; set => m_CancelBtn = value; }

    protected override void Start()
    {
        base.Start();
        m_CloseBtn?.onClick.AddListener(OnCloseClick);
        m_FinishBtn?.onClick.AddListener(OnFinishClick);
        m_CancelBtn?.onClick.AddListener(OnCancelClick);
    }

    public override void Show()
     {
        Load();
        base.Show();
        ShowAnima();
     }

    public override void Close()
    { 
        UnLoad();
        base.Close();
        CloseAnima();
    }

    protected virtual void ShowAnima()
    {
        gameObject.SetActive(true);
        cgBG.alpha = 1;
        if (!animator)
        {
            allUI.transform.localScale = Vector3.zero;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(allUI.DOScale(Vector3.one * 1.15f, 0.15f));
            sequence.Append(allUI.DOScale(Vector3.one, 0.2f));
            sequence.SetUpdate(true);
            sequence.Play();
        }
    }
    
    protected virtual void CloseAnima()
    {
        if (animator)
        {
            animator.Play("Close");
        }else {
            allUI.transform.localScale = Vector3.one;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(allUI.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutSine));
            sequence.Append(allUI.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutSine));
            sequence.SetUpdate(true);
            sequence.Play();
        }

        if (cgBG != null)
        {
            cgBG.alpha = 1;
            cgBG.DOFade(0, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
            DG.Tweening.DOTween.Kill(this.transform);
        }
    }

    public virtual void OnCloseClick()
    {
        Close();
        CloseCall?.Invoke();
    }

    public virtual void OnFinishClick() { FinishCall?.Invoke(); }

    public virtual void OnCancelClick() { CancelCall?.Invoke(); }


    #region Editor
#pragma warning disable
    /// <summary>
    /// 检视
    /// </summary>
    protected void OnValidate()
    {
        if (transform.Find("AllUI") != null)
        {
            allUI ??= transform.Find("AllUI").GetComponent<RectTransform>();
        }
        else
        { 
            allUI ??= GetComponent<RectTransform>();
        }
        TryGetComponent<CanvasGroup>(out cgUI);
        m_CloseBtn ??= transform.Find("CloseBtn")?.GetComponent<Button>();
        m_FinishBtn ??= transform.Find("FinishBtn")?.GetComponent<Button>();
        m_CancelBtn ??= transform.Find("CancelBtn")?.GetComponent<Button>();
        
        transform.Find("BG").TryGetComponent<CanvasGroup>(out cgBG);
        cgBG ??=transform.GetComponent<CanvasGroup>();
        animator ??= GetComponent<Animator>();
    }
    #pragma warning restore

    [InspectorButton]
    public void EditInitPop()
    {
        // CanvasGroup bg = allUI.GetComponent<CanvasGroup>() == null ? allUI.gameObject.AddComponent<CanvasGroup>() : allUI.GetComponent<CanvasGroup>();
        m_CancelBtn = allUI.Find("CancelBtn")?.GetComponent<Button>();
        if(m_CancelBtn == null)
        {
            CreateButton("CancelBtn", allUI);
        }
        m_FinishBtn = allUI.Find("FinishBtn")?.GetComponent<Button>();
        if(m_FinishBtn == null)
        {
            CreateButton("FinishBtn", allUI);
        }
        m_CloseBtn = allUI.Find("CloseBtn")?.GetComponent<Button>();
        if(m_CloseBtn == null)
        {
            CreateButton("CloseBtn", allUI);
        }
    }

    public static void CreateButton(string name, Transform parent)
    {
        var btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent);
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localScale = Vector3.one;
        
    }
    #endregion
}
