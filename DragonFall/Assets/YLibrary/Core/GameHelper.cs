using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
/// <summary>
/// 等待的事件协程 计时器，计数器
/// </summary>
public class GameHelper : YBZ.Design.D_MonoSingleton<GameHelper>
{
    public void Init() { }
    protected override void Initialize(){ }

    public void Update() {
        UpdateTimer();
    }

    protected override void Dispose()
    {
        m_TimerDict.Clear();
        m_CounterDict.Clear();
    }

    #region WaitForSeconds 优化协程
    private static Dictionary<float, WaitForSeconds> m_WaitTimeDict = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitSecond(float timer)
    {
        if(!m_WaitTimeDict.TryGetValue(timer, out WaitForSeconds result))
        {
            m_WaitTimeDict.Add(timer, new WaitForSeconds(timer));
        }
        return m_WaitTimeDict[timer];
    }
    #endregion

    #region 计数器(Counter)
    private static Dictionary<string, int> m_CounterDict = new();
    /// <summary>
    /// 运行时计时器
    /// </summary>
    /// <param name="nameKey"></param>
    public static void CounterCC(string nameKey)
    {
        if(m_CounterDict.ContainsKey(nameKey))
        {
            m_CounterDict[nameKey]++;
        } else
        {
            m_CounterDict.Add(nameKey, 1);
        }
    }

    /// <summary>
    /// 持久化计时器
    /// </summary>
    /// <param name="nameKey"></param>
    public static void CounterPlayerPrefs(string nameKey)
    {
        if(PlayerPrefs.GetInt(nameKey, 0) == 0)
        {
            PlayerPrefs.SetInt(nameKey, 1);
        } else
        {
            int nameCount = PlayerPrefs.GetInt(nameKey);
            PlayerPrefs.SetInt(nameKey, nameCount + 1);
        }
    }
    #endregion

    #region Timer(计时器)

    public class TimerCounter
    {
        public bool IsValid { get; set; } = true;   // 默认启动
        public float timer;
        public Action doneCall;
    }

    private static Dictionary<string, TimerCounter> m_TimerDict = new();

    /// <summary>
    /// 添加计时器，成功返回
    /// </summary>
    /// <param name="nameKey"></param>
    /// <param name="timer"></param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public static bool AddTimer(string nameKey, float timer, Action doneCall = null, bool isCover = false)
    {
        var temp = new TimerCounter() { timer = timer, doneCall = doneCall };
        if (m_TimerDict.ContainsKey(nameKey))
        {
            if (isCover)    // 允许覆盖
            {
                m_TimerDict[nameKey] = temp;
                m_TimerDict[nameKey].IsValid = true;
                return true;
            } else {    
                if (!m_TimerDict[nameKey].IsValid)  // 如果无效了，允许重新开始
                {
                    m_TimerDict[nameKey] = temp;
                    m_TimerDict[nameKey].IsValid = true;
                    return true;
                } else  // 还有效
                {
                    return false;
                }
            }
        }
        else
        {
            m_TimerDict[nameKey] = temp;
            return true;
        }
    }
    
    public static TimerCounter GetTimer(string nameKey)
    {
        TimerCounter result = null;
        m_TimerDict.TryGetValue(nameKey, out result);
        return result;
    }
    
    /// <summary>
    /// 计时器更新
    /// </summary>
    public void UpdateTimer()
    {
        var list = m_TimerDict.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            if (m_TimerDict[list[i].Key].IsValid)
            {
                m_TimerDict[list[i].Key].timer -= Time.deltaTime;
                if (m_TimerDict[list[i].Key].timer < 0)
                {
                    m_TimerDict[list[i].Key].IsValid = false;
                    m_TimerDict[list[i].Key].doneCall?.Invoke();
                }
            }
        }
    }

    public void ClearTimer()
    {
        m_TimerDict.Clear();
    }
    #endregion

    /// <summary>
    /// Captures the screenshot2.
    /// </summary>
    /// <returns>The screenshot2.</returns>
    /// <param name="rect">Rect.截图的区域，左下角为o点</param>
    public static Texture2D CaptureScreenshot(RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        Vector3[] vector3s= new Vector3[4];
        rectTransform.GetWorldCorners(vector3s);
        Vector3 viewPos = Camera.main.WorldToViewportPoint(vector3s[0]);
        Debug.Log("viewPos: " + viewPos);

        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);//先创建一个的空纹理，大小可根据实现需要来设置
#pragma warning disable UNT0017 // SetPixels invocation is slow
        screenShot.ReadPixels(rect, (int)(viewPos.x * Screen.width), (int)(viewPos.y * Screen.height));//读取屏幕像素信息并存储为纹理数据，
#pragma warning restore UNT0017 // SetPixels invocation is slow
        screenShot.Apply();

#if UNITY_EDITOR
        byte[] bytes = screenShot.EncodeToPNG();//然后将这些纹理数据，成一个png图片文件
        string filename = Application.streamingAssetsPath + "/Screenshot.png";
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张图片: {0}", filename));
#endif
        return screenShot;
    }

    #region 强制重新构建UI布局并更新Canvas
    /// <summary>
    /// 强制重新构建UI布局并更新Canvas
    /// </summary>
    /// <param name="layoutRect"></param>
    public static void ForceRebuildLayoutAndUpdateCanvas(RectTransform layoutRect)
    {
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        Canvas.ForceUpdateCanvases();
    }

    #endregion

    public static void Invoke(Action action, float time)
    {
        
    }
    
    public static void CancelInvoke(Action action)
    {
        
    }


    // 延迟执行, DoTween版本
    public static void DelaySeconds(float delayTime, Action action)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delayTime);
        sequence.AppendCallback(() => { action?.Invoke(); });
        sequence.Play();
    }
    
    // 延迟执行，按照帧延迟
    public static void DelyaFrame(int delayFrame, Action action)
    {
        StartCoroutine(DelayFrameCoroutine(delayFrame, action));
    }

    private static IEnumerator DelayFrameCoroutine(int delayFrame, Action action)
    {
        for (int i = 0; i < delayFrame; i++)
        {
            yield return null;
        }
        action?.Invoke();
    }


    #region 协程上传到某个Momo运行(比如GameRoot)
    public static void StartCoroutine(IEnumerator routine, MonoBehaviour mono)
    {
        mono.StartCoroutine(routine);
    }

    public static void StartCoroutine(MonoBehaviour mono, IEnumerator routine)
    {
        mono.StartCoroutine(routine);
    }

    new public static void  StartCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine, YBZ.Core.GameRoot.Instance);
    }
    #endregion
}
