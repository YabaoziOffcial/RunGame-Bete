using System.Collections.Generic;
using System;
using YBZ.Design;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "SoundConfig", order = 0)]
public class SoundConfig : ScriptableObject
{
    public List<SoundTemplate> sound;
    
    public SoundTemplate Tem(string audioName)
    {
        foreach (var soundTemplate in sound)
        {
            if (soundTemplate.audioClip != null && soundTemplate.audioClip.name.Equals(audioName))
            {
                return soundTemplate;
            }
        }
        return null;
    }
}

[System.Serializable]
public class SoundTemplate
{
    #region Member
    public AudioClip audioClip;
    public int priority;    // 优先级， 数字越大，优先级越高，播放的时候，优先播放优先级高的音效
    public string mutexSoundID;         // 互斥音效         (遇到某个音效的时候，停止播放这个音效，先到先得)
    public string mutexAndStopSoundID;  // 互斥并且停止音效  （停止播放前面的互斥音效，播放这个音效）
    public float delay;     // 延迟多久播放,默认为0不延迟
    public float duration;  // 到多长时间停止播放，默认为0时，不暂停
    public bool isLoop;     // 是否循环
    public float volume;    // 播放的声音大小
    #endregion
}
