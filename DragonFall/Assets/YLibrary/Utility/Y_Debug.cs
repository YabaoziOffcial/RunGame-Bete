using System;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public static class Y_Debug
{
#if UNITY_EDITOR || DEBUG
    private static bool is_log_print = true;
#else
    private static bool is_log_print = false;
#endif

    public static string AddColor(this string text, Color color)
    {
        if (!is_log_print) return null;
        string colorText = ColorUtility.ToHtmlStringRGB(color);
        text = ($"<color=#{colorText}>{text}</color>");
        return text;
    }


#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void Log(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(args);
    }

#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void LogRed(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.red, args));
    }

#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void LogGreen(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.green, args));
    }

#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void LogBlue(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.blue, args));
    }
#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void LogYellow(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.yellow, args));
    }
#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void LogCyan(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.cyan, args));
    }
#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void Warning(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.red, args));
    }
#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void Error(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        obj.Print(PrependColorArgs(Color.red, args));
    }

#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static bool IsNotNull(this object obj, params object[] args)
    {
        if (!is_log_print) return false;
        obj.Print(PrependColorArgs(Color.red, args));
        return false;
    }

    private static object[] PrependColorArgs(Color color, object[] args)
    {
        args ??= Array.Empty<object>();
        var mergedArgs = new object[args.Length + 1];
        mergedArgs[0] = color;
        Array.Copy(args, 0, mergedArgs, 1, args.Length);
        return mergedArgs;
    }

    // 统一处理参数，返回一个字符串，并添加颜色
    public static string UniformArgs(params object[] args)
    {
        if (!is_log_print) return null;
        if (args == null) return null;
        // 处理颜色
        Color color = Color.white;
        if (args.Any(arg => arg is Color))
        {
            color = (args.First(arg => arg is Color) as Color?) ?? Color.white; // Color? 为可控类型
        }
        string[] stringArgs = args.Where(arg => arg is not Color).Select(arg => arg.ToString()).ToArray();

        // 处理其他参数
        // ......
        // ......
        // ......
        // 优化内容，使用StringBuilder来拼接
        return " " + string.Join(" ", stringArgs).AddColor(color);
    }

    /// <summary>
    /// 打印日志：会自动在消息尾部追加真实调用位置
    /// 这样 Console 里点击能直接跳到业务代码，而不是 Y_Debug.cs
    /// </summary>
#if UNITY_2022_2_OR_NEWER
    [UnityEngine.HideInCallstack]
#endif
    public static void Print(this object obj, params object[] args)
    {
        if (!is_log_print) return;
        args ??= Array.Empty<object>();

        // 处理颜色
        Color color = Color.white;
        if (args.Any(arg => arg is Color))
        {
            color = (args.First(arg => arg is Color) as Color?) ?? Color.white; // Color? 为可控类型
        }
        var logText = obj.ToString().AddColor(color) + " " + UniformArgs(args);
        UnityEngine.Debug.Log(logText.Replace("System.Object[]", ""));

    }
}