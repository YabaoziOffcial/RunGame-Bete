using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YBZ.Core;

/// <summary>
/// 音频管理器
/// </summary>
public class AudioManager : YBZ.Design.O_MonoSingleton<AudioManager>
{

    private static string audioPath = "Audio/"; // 音效配置文件路径
    public enum EAudioType
    {
        BGM = 0,      // BGM
        Other,
    }

    public void SetMusicValue(bool on)
    {
        var audioSource = GetAvailableAudioSource("BGM");
        if (on)
        {
            SoundTemplate soundTemplate = soundConfig.Tem("MainBGM");
            if (soundTemplate != null)
            {
                audioSource.volume = soundTemplate.volume;
            }
            else
            {
                audioSource.volume = 0;
            }
            if (!audioSource.isPlaying)
            {
                PlaySoundTem("BGM:MainBGM");
            }
        }
        else
        {
            audioSource.volume = 0;
        }
        GameConfig.SetConfig("Music", on ? "1" : "0");
    }

    public void SetSoundValue(bool on)
    {
        // var audioSource = GetAvailableAudioSource("Common:MainBGM");
        // if (on)
        // {
        //     audioSource.volume = 1;
        // }
        // else
        // {
        //     audioSource.volume = 0;
        // }
        GameConfig.SetConfig("Sound", on ? "1" : "0");
    }

    public bool GetMusicValue() => GameConfig.GetConfig("Music", "1") == "1";

    public bool GetSoundValue() => GameConfig.GetConfig("Sound", "1") == "1";

    [SerializeField] Transform m_AudioManager;
    Dictionary<string, AudioSource> m_AudioSourceDict = new Dictionary<string, AudioSource>();
    /// <summary>各类型通道最近一次使用的模板音量（用于滑块变更时重算）</summary>
    readonly Dictionary<string, float> m_LastTemplateVolumeByType = new Dictionary<string, float>();

    float _masterVolumeScale = 1f;
    float _musicVolumeScale = 1f;
    float _sfxVolumeScale = 1f;

    private readonly string soundConfigPath = "SoundConfig"; // 音效配置文件路径
    private SoundConfig soundConfig; // 音效配置文件

    protected override void Initialize()
    {
        base.Initialize();
        soundConfig = ResourceManager.Instance.LoadRes<SoundConfig>(soundConfigPath);
        LoadVolumeSettingsFromPrefs();
        Y_Debug.Log("AudioManager Init Complete !");
    }

    /// <summary>
    /// 设置总音量 / 音乐 / 音效（0~1）。总音量作用于 AudioListener，音乐/音效分别作用于 BGM 与其它通道。
    /// </summary>
    public void SetVolumeLevels(float master, float music, float sfx, bool persist = true)
    {
        _masterVolumeScale = Mathf.Clamp01(master);
        _musicVolumeScale = Mathf.Clamp01(music);
        _sfxVolumeScale = Mathf.Clamp01(sfx);
        AudioListener.volume = _masterVolumeScale;
        if (persist)
        {
            GameConfig.SetConfig("Volume_Master", _masterVolumeScale);
            GameConfig.SetConfig("Volume_Music", _musicVolumeScale);
            GameConfig.SetConfig("Volume_SFX", _sfxVolumeScale);
        }
        ApplyVolumeToExistingSources();
    }

    void LoadVolumeSettingsFromPrefs()
    {
        float master = GameConfig.GetConfig_Float("Volume_Master", 1f);
        float music = GameConfig.GetConfig_Float("Volume_Music", 1f);
        float sfx = GameConfig.GetConfig_Float("Volume_SFX", 1f);
        SetVolumeLevels(master, music, sfx, persist: false);
    }

    void ApplyVolumeToExistingSources()
    {
        foreach (var kv in m_AudioSourceDict)
        {
            string type = kv.Key;
            var src = kv.Value;
            if (!m_LastTemplateVolumeByType.TryGetValue(type, out float baseVol)) continue;
            float bus = type == EAudioType.BGM.ToString() ? _musicVolumeScale : _sfxVolumeScale;
            src.volume = baseVol * bus;
        }
    }

    public void PlayClip(string type, AudioClip clip)
    {
        GetAvailableAudioSource(type).clip = clip;
        GetAvailableAudioSource(type).Play();
    }

    /// <summary>
    /// 按照key 播放音乐，key 格式为 类型:名称
    /// </summary>
    /// <param name="key"></param>
    /// <param name="same_cover">相同覆盖，即 允许相同的播放</param>
    /// 
    public void PlaySoundTem(string key, bool same_cover = true)
    {
        string type = key.Split(":")[0];
        string clipName = key.Split(":")[1];
        var soundInfo = soundConfig.Tem(clipName);
        if (soundInfo != null)
        {
            // 条件过滤
            if ((type == EAudioType.BGM.ToString() && !GetMusicValue()) || (type != EAudioType.BGM.ToString() && !GetSoundValue()))
            {
                return;
            }
            StopMutexSounds(soundInfo.mutexSoundID, soundInfo.mutexAndStopSoundID); // 停止互斥音效
            var audioSource = GetAvailableAudioSource(type); // 找到可用的 AudioSource
            if (audioSource != null)
            {
                var clip = soundInfo.audioClip;
                // 如果不允许相同覆盖，当遇到同样的音效时 如果没有在播放就播放
                if (!same_cover && audioSource.clip != null && audioSource.clip.name == clip.name && audioSource.isPlaying) return;
                audioSource.clip = clip;
                m_LastTemplateVolumeByType[type] = soundInfo.volume;
                float bus = type == EAudioType.BGM.ToString() ? _musicVolumeScale : _sfxVolumeScale;
                audioSource.volume = soundInfo.volume * bus;
                audioSource.loop = soundInfo.isLoop;
                audioSource.priority = soundInfo.priority;
                // 延迟播放
                if (soundInfo.delay > 0)
                {
                    StartCoroutine(DelayedPlay(audioSource, soundInfo.delay));
                }
                else
                {
                    audioSource.Play();
                }
            }
        }
        else
        {
            Debug.LogWarning($"SoundTemplate with key {clipName} not found.");
        }
    }

    /// 得到AudioSource
    public AudioSource GetAudioSource(string type)
    {
        return GetAvailableAudioSource(type);
    }

    // 得到对应类型的播放器
    private AudioSource GetAvailableAudioSource(string type)
    {
        if (!m_AudioSourceDict.ContainsKey(type))
        {
            AudioSource audioSource = m_AudioManager.Find(type)?.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = type;
                audioSource = gameObject.AddComponent<AudioSource>();
                gameObject.transform.SetParent(m_AudioManager);
                gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                gameObject.transform.localScale = Vector3.one;
            }
            m_AudioSourceDict[type] = audioSource;
        }
        return m_AudioSourceDict[type];
    }

    // 延迟播放
    private IEnumerator DelayedPlay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
    }

    // 停止互斥的音效
    private void StopMutexSounds(string mutexSoundID, string mutexAndStopSoundID)
    {
        if (!string.IsNullOrEmpty(mutexSoundID))
        {
            foreach (var soundKey in mutexSoundID.Split(','))
            {
                StopClip(soundKey);
            }
        }
        if (!string.IsNullOrEmpty(mutexAndStopSoundID))
        {
            foreach (var soundKey in mutexAndStopSoundID.Split(','))
            {
                StopClip(soundKey);
            }
        }
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    /// <param name="key"></param>
    public void StopClip(string key)
    {
        GetAvailableAudioSource(key.Split(":")[0]).Stop();
    }

    // 停止某个类型的音频播放器
    public void StopAudioSource(string audioSourceKey)
    {
        GetAvailableAudioSource(audioSourceKey).Stop();
    }

    public static void LoadAudioClip(string name, Action<AudioClip> doneCall)
    {
        var path = GetAudioPath(name);
        var clip = ResourceManager.Instance.LoadRes<AudioClip>(path);
        if (clip == null)
            Debug.Log($"{name} Is Not Load");
        else
        {
            doneCall?.Invoke(clip);
        }
    }

    public static string GetAudioPath(string name)
    {
        return audioPath + name;
    }
}


// 音频的信息，是否循环，结局如何，是否冲突，全部都有音频自己控制
[Serializable]
public class SoundBaseInfo
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key;

    /// <summary>
    /// 音频类型
    /// </summary>
    public string Category;

    /// <summary>
    /// 音频的路径及名称(多个随机用冒号分隔,Unity不带扩展名)
    /// </summary>
    public string Name;

    /// <summary>
    /// 播放模式
    /// </summary>
    public string PlayMode;

    /// <summary>
    /// 音频长度（秒）
    /// </summary>
    public float Duration;

    /// <summary>
    /// 延时播放（秒）
    /// </summary>
    public float Delay;

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public bool IsLoop;

    /// <summary>
    /// 互斥(多个用逗号分隔)
    /// </summary>
    public string MutexSoundID;

    /// <summary>
    /// 打断
    /// </summary>
    public string MutexAndStopSoundID;

    /// <summary>
    /// 音量大小（0 ~ 1）
    /// </summary>
    public float Volume;

    /// <summary>
    /// 渐入曲线
    /// </summary>
    public int FadeInCurveId;

    /// <summary>
    /// 渐出曲线
    /// </summary>
    public int FadeOutCurveId;

    /// <summary>
    /// 渐入时间
    /// </summary>
    public float FadeInTime;

    /// <summary>
    /// 渐出时间
    /// </summary>
    public float FadeOutTime;

    /// <summary>
    /// 音调最小
    /// </summary>
    public float PitchMin;

    /// <summary>
    /// 音调最大
    /// </summary>
    public float PitchMax;
}