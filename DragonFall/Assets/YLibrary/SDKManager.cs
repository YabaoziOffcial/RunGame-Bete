using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YBZ.Core;

public class SDKManager : YBZ.Design.Singleton<SDKManager>
{
    bool is_no_ad = false;

    public bool IsNoAd
    {
        get => is_no_ad;
        set => is_no_ad = value;
    }

#if UNITY_EDITOR
    public void Init(Action<bool> action = null)
    {
        action?.Invoke(true);
        MoreMountains.NiceVibrations.MMVibrationManager.iOSInitializeHaptics();
        Debug.Log("SDKManager Init Complete.");
    }
#elif LauncherB
    public void Init(Action<bool> successCallBack, Dictionary<string, object> intParams = null)
    {
        NoviSDK.Scripts.NoviSDKMgr.Instance.Init((success, code, message) =>
        {
            successCallBack?.Invoke(success);
            if (success)
            {
                Debug.Log("SDK初始化成功");
            }
            else
            {
                Debug.Log($"SDK初始化失败：{code} - {message}");
            }
        }, intParams);
    }
#elif Launcher
    public void Init(Action<LauncherInitSuccess> action = null)
    {
        LauncherSDK.Instance.LS_InitializeSdk(action);
        Debug.Log("SDKManager Init Complete.");
    }
#else
    public void Init(Action<bool> action = null)
    {
        action?.Invoke(true);
        MoreMountains.NiceVibrations.MMVibrationManager.iOSInitializeHaptics();
        Debug.Log("SDKManager Init Complete.");
    }
#endif

    public void UnInit()
    {
        MoreMountains.NiceVibrations.MMVibrationManager.iOSReleaseHaptics();
    }

    // 隐私链接
    public void OpenPolicy()
    {
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.OpenPrivacyPolicy();
#elif Launcher
        LauncherSDK.Instance.LS_OpenPrivacy();
#else
        Application.OpenURL("https://abahjaya.com/privacy.html");
#endif
    }

    #region 震动
    public void SetVibrationValue(bool on)
    {
        GameConfig.SetConfig("Vibration", on ? "1" : "0");
    }

    public bool GetVibrationValue() => GameConfig.GetConfig("Vibration", "1") == "1";

    public void Vibration(int intensity, float type)
    {
        Y_Debug.Log("[SDKManager] Vibration intensity: " + intensity + " type: " + type);
        if (GetVibrationValue())
        {
#if LauncherB
            NoviSDK.Scripts.NoviSDKMgr.Instance.Vibrate(1);
#elif Launcher
            LauncherSDK.Instance.LS_Vibrate(intensity, (int)type);
#else
            MoreMountains.NiceVibrations.MMVibrationManager.Vibrate();
#endif
        }
    }
    #endregion

    #region  Banner Native 广告  
    public void ShowBanner(string _pos, RectTransform rect = null)
    {
        Debug.Log($"ShowBanner pos: {_pos}");
        if (is_no_ad) return;
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.ShowMiniNative(_pos, rect);
#elif Launcher
        LauncherSDK.Instance.LS_ShowBanner();
#endif
    }

    public void HideBanner()
    {
        Debug.Log("HideBanner");
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.CloseMiniNative();
#elif Launcher
        LauncherSDK.Instance.LS_HideBanner();
#endif
    }

    public void ShowNative(RectTransform rect, Camera camera, string pAdPos)
    {
        Debug.Log($"ShowNative pAdPos {pAdPos}");
        if (is_no_ad) return;
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.ShowNative(pAdPos, rect);
#elif Launcher
        LauncherSDK.Instance.LS_ShowNative(rect, camera, pAdPos);
#endif
    }

    public void HideNative()
    {
        Debug.Log("HideNative");
#if UNITY_EDITOR

#elif LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.CloseNative();
#elif Launcher
        LauncherSDK.Instance.LS_HideNative();
#endif
    }

    #region  远程参数
    public int GetRemoteConfigInt(string key, int defValue)
    {
#if LauncherB
        return NoviSDK.Scripts.NoviSDKMgr.Instance.GetInt(key, defValue);
#elif Launcher
        return LauncherSDK.Instance.LS_GetRemoteConfigInt(key, defValue);
#else
        return defValue;
#endif
    }


    public string GetRemoteConfigString(string key, string defValue)
    {
#if LauncherB
        return NoviSDK.Scripts.NoviSDKMgr.Instance.GetString(key, defValue);
#elif Launcher
        return LauncherSDK.Instance.LS_GetRemoteConfigStr(key, defValue);
#else
        return defValue;
#endif
    }
    #endregion


    // 插屏
    public void PlayInsertAD(string pos, string iv_type)
    {
        Debug.Log("PlayInsertAD  pos : " + pos + " iv_type : " + iv_type);
        if (is_no_ad) return;

#if LauncherB
        switch (iv_type)
        {
            case "IV1":
                NoviSDK.Scripts.NoviSDKMgr.Instance.ShowIv(pos, NoviSDK.Scripts.NoviIvType.IV1);
                break;
            case "IV2":
                NoviSDK.Scripts.NoviSDKMgr.Instance.ShowIv(pos, NoviSDK.Scripts.NoviIvType.IV2);
                break;
            case "IV3":
                NoviSDK.Scripts.NoviSDKMgr.Instance.ShowIv(pos, NoviSDK.Scripts.NoviIvType.IV3);
                break;
            case "IV4":
                NoviSDK.Scripts.NoviSDKMgr.Instance.ShowIv(pos, NoviSDK.Scripts.NoviIvType.IV4);
                break;
        }
#elif Launcher
        switch (iv_type)
        {
            case "IV1":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV1);
                break;
            case "IV2":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV2);
                break;
            case "IV3":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV3);
                break;
            case "IV4":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV4);
                break;
        }
#endif
    }

    public void PlayInsertAD(string pos, string iv_type, Action action)
    {
        Debug.Log("PlayInsertAD  pos : " + pos + " iv_type : " + iv_type);
        if (is_no_ad)
        {
            action?.Invoke();
            return;
        }
#if Launcher
        switch (iv_type)
        {
            case "IV1":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV1, (par) =>
                {
                    action?.Invoke();
                });
                break;
            case "IV2":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV2, (par) =>
                {
                    action?.Invoke();
                });
                break;
            case "IV3":
                LauncherSDK.Instance.LS_ShowInterstitial(pos, LauncherIVADType.IV3, (par) =>
                {
                    action?.Invoke();
                });
                break;
        }
#else
        action?.Invoke();
#endif
    }

    public void PlayRewardAD(string pos, Action<bool> successCallBack, Action<string> failCallBack = null)
    {
        Debug.Log("PlayRewardAD  pos : " + pos);
        if (is_no_ad)
        {
            successCallBack?.Invoke(true);
            return;
        }
#if UNITY_EDITOR
        successCallBack?.Invoke(true);
#elif LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.ShowRv(pos, closeCallback: (isRewarded, revenue) =>
        {
            successCallBack?.Invoke(isRewarded);
        });
#elif Launcher
        LauncherSDK.Instance.LS_ShowRewardedAd(pos, successCallBack, (faillresult) =>
        {
            Debug.Log("播放失败 : " + faillresult);
            // GameSet.instance.gameManager.ShowToast("ADisNotReady");
        });
#else
        successCallBack?.Invoke(true);
#endif
    }
    #endregion


    #region LogEvent

    public void LogEvent(string name)
    {
        Debug.Log("LogEvent name : " + name);
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.LogEvent(name);
#elif Launcher
        LauncherSDK.Instance.LS_LogEvent(name);
#endif
    }

    public void LogEvent(string name, string key, string value)
    {
        Debug.Log("LogEvent name : " + name + " key : " + key + " value : " + value);
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.LogEvent(name, key, value);
#elif Launcher
        LauncherSDK.Instance.LS_LogEvent(name, key, value);
#endif
    }

    public void LogEvent(string name, string key_01, string value_01, string key_02, string value_02)
    {
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.LogEvent(name, key_01, value_01, key_02, value_02);
#elif Launcher
        LauncherSDK.Instance.LS_LogEvent(name, key_01, value_01, key_02, value_02);
#endif
    }


    public void LevelStart(int level, Dictionary<string, object> extraInfo = null)
    {
        Debug.Log("levelStart : " + level);
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnter(level, extraInfo);
#elif Launcher
        LauncherSDK.Instance.LS_LevelEnter(level, extraInfo);
#endif
    }


    public void LevelStart(string level)
    {
        Debug.Log("LevelStart : " + level);
#if LauncherB
        NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnter(level);
#elif Launcher
        LauncherSDK.Instance.LS_LevelEnter(level);
#endif
    }

    public void LevelEnd(int level, string state)
    {
        Debug.Log("LevelEnd : " + level + "state : " + state);
#if LauncherB
        switch (state)
        {
            case "StageFail":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Fail);
                break;
            case "StageBack":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Back);
                break;
            case "StageRetry":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Retry);
                break;
            case "StageSucc":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Success);
                break;
            case "StageSkip":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Skip);
                break;
        }
#elif Launcher
        switch (state)
        {
            case "StageFail":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Fail);
                break;
            case "StageBack":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Back);
                break;
            case "StageRetry":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Retry);
                break;
            case "StageSucc":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Success);
                break;
            case "StageSkip":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Skip);
                break;
        }
#endif
    }

    public void LevelEnd(string level, string state)
    {
        Debug.Log("LevelEnd : " + level + "state : " + state);
#if LauncherB
        switch (state)
        {
            case "StageFail":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Fail);
                break;
            case "StageReturn":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Back);
                break;
            case "StageRetry":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Retry);
                break;
            case "StageSucc":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Success);
                break;
            case "StageSkip":
                NoviSDK.Scripts.NoviSDKMgr.Instance.LevelEnd(level, NoviSDK.Scripts.StageResult.Level_Skip);
                break;
        }
#elif Launcher
        switch (state)
        {
            case "StageFail":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Fail);
                break;
            case "StageBack":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Back);
                break;
            case "StageRetry":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Retry);
                break;
            case "StageSucc":
                LauncherSDK.Instance.LS_LevelEnd(level, LauncherStageResult.Level_Success);
                break;
            case "":
                break;
        }
#endif
    }
    #endregion

}
