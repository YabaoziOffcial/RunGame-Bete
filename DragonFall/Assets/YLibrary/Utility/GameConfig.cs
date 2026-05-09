
using UnityEngine;

/// <summary>
/// 扩展：需要更具key的名字增加监听时间，在Set的时候触发
/// </summary>
// 0 false 1 true
// 0 关闭 1 开启
// 0 未读 1 已读
// 0 没有 1 存在

namespace YBZ.Core
{
    public class GameConfig
    {
        public static bool hasLog = true;  //  

        #region 设置游戏内常量
        public static void SetConfig(string nameKey, string value)
        {
            PlayerPrefs.SetString(nameKey, value);
            PlayerPrefs.Save();
            if (hasLog)
                Y_Debug.LogGreen("SetConfig : " + nameKey + " : " + value);
        }

        public static void SetConfig(string nameKey, int value)
        {
            PlayerPrefs.SetInt(nameKey, value);
            PlayerPrefs.Save();

            if (hasLog)
                Y_Debug.LogGreen("SetConfig : " + nameKey + " : " + value);
        }

        public static void SetConfig(string nameKey, float value)
        {
            PlayerPrefs.SetFloat(nameKey, value);
            PlayerPrefs.Save();
            if (hasLog)
                Y_Debug.LogGreen("SetConfig : " + nameKey + " : " + value);
        }

        public static string GetConfig(string nameKey, string defaultvalue = "0")
        {
            return PlayerPrefs.GetString(nameKey, defaultvalue);
        }

        public static int GetConfig_Int(string nameKey, int defaultvalue = 0)
        {
            return PlayerPrefs.GetInt(nameKey, defaultvalue);
        }

        public static float GetConfig_Float(string nameKey, float defaultValue = .0f)
        {
            return PlayerPrefs.GetFloat(nameKey, defaultValue);
        }

        public static bool HasKey(string nameKey)
        {
            return PlayerPrefs.HasKey(nameKey);
        }

        public static void DeleteKey(string nameKey)
        {
            if (HasKey(nameKey))
            {
                PlayerPrefs.DeleteKey(nameKey);
            }
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        #endregion

        public static void Init()
        {
            Debug.Log("GameConfig Init Complete.");
        }


        public static void UnInit()
        {

        }
    }
}
