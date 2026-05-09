using UnityEngine;
using YBZ.Design;

/// <summary>
/// 鲁班配置表管理器
/// </summary>
public class LubanMgr : Singleton<LubanMgr>
{
    /// <summary>
    /// 表数据
    /// </summary>
    // public Luban.Tables tables;

    /// <summary>s
    /// 加载配置
    /// </summary>
    public void Init()
    {
        // var tablesCtor = typeof(Tables).GetConstructors()[0];
        // System.Delegate loader = new System.Func<string, ByteBuf>((path) =>
        // {
        //     Y_Debug.LogGreen("加载数据" + path);
        //     path = StringConst.GetDataPath(path);
        //     TextAsset text = ResourceManager.Instance.LoadRes<TextAsset>(path);
        //     byte[] ret = text.bytes;
        //     return new ByteBuf(ret);
        // });
        // // tables = (Tables)tablesCtor.Invoke(new object[] { loader }); // bin 方式加载
        // tables = new Tables(file => SimpleJSON.JSONNode.Parse(ResourceManager.Instance.LoadRes<TextAsset>(StringConst.GetDataPath(file)).text)); // json 方式加载
    }
}
