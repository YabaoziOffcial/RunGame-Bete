using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToastUI : YBZ.Design.O_MonoSingleton<ToastUI>
{
    public Transform allUI;
    public CanvasGroup bg;
    public Text msg;

    [SerializeField] Animator animator;
    void Start()
    {
        gameObject.SetActive(string.IsNullOrEmpty(msg.text));
    }

    public static void ShowToast(string mess = "")
    {
        if (Instance == null)
        {
            Instance = UIManager.Instance.transform.Find("ToastUI").GetComponent<ToastUI>();
        }
        if (mess != null || !mess.Equals("")) Debug.Log(mess);
        if (Instance.gameObject.activeSelf)
        {
            return;
        }
        Instance.gameObject.SetActive(true);
        Instance.animator.Play("Show");
        Instance.msg.text = mess;
        GameHelper.DelaySeconds(2.5f, () =>
        {
            Instance.animator.Play("Hide");
            GameHelper.DelaySeconds(0.7f, () =>
            {
                Instance.gameObject.SetActive(false);
            });
        });
    }
}
