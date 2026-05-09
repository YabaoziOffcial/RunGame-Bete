using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class ReferenceFinder
{
    static string[] assetGUIDs;
    static string[] assetPaths;
    static string[] allAssetPaths;
    static Thread thread;

    [MenuItem("Assets/Find References In Project(YBZ )", false)]
    [MenuItem("Assets/Find References In Project(YBZ )", false, 1000)]
    static void FindreAssetFerencesMenu()
    {
        Debug.LogError("查找资源引用");

        if(Selection.assetGUIDs.Length == 0)
        {
            Debug.LogError("请先选择任意一个组件，再右键点击此菜单");
            return;
        }

        assetGUIDs = Selection.assetGUIDs;

        assetPaths = new string[assetGUIDs.Length];

        for(int i = 0; i < assetGUIDs.Length; i++)
        {
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
        }

        allAssetPaths = AssetDatabase.GetAllAssetPaths();

        thread = new Thread(new ThreadStart(FindreAssetFerences));
        thread.Start();
    }


    static void FindreAssetFerences()
    {
        List<string> logInfo = new List<string>();
        string path;
        string log;
        for(int i = 0; i < allAssetPaths.Length; i++)
        {
            path = allAssetPaths[i];

            // 主要针对预制体和场景对资源的引用
            if(path.EndsWith(".prefab") || path.EndsWith(".unity"))
            {
                string content = File.ReadAllText(path);
                if(content == null)
                {
                    continue;
                }

                for(int j = 0; j < assetGUIDs.Length; j++)
                {
                    if(content.IndexOf(assetGUIDs[j]) > 0)
                    {
                        log = string.Format("{0} 引用了 {1}", path, assetPaths[j]);
                        logInfo.Add(log);
                    }
                }
            }
        }

        for(int i = 0; i < logInfo.Count; i++)
        {
            Debug.LogError(logInfo[i]);
        }

        Debug.LogError("选择对象引用数量：" + logInfo.Count);

        Debug.LogError("查找完成");
    }


    [MenuItem("Assets/Find References In Scene(YBZ)", false)]
    [MenuItem("Assets/Find References In Scene(YBZ )", false, 1000)]
    static void FinderAssetFerencesInScene()
    {
        Debug.Log("查找资源在场景中的引用");
        if(Selection.assetGUIDs.Length == 0)
        {
            Debug.Log("请先选择任意一个组件，再右键点击此菜单");
        }

        assetGUIDs = Selection.assetGUIDs;
        assetPaths = new string[assetGUIDs.Length];

        for(int i = 0; i < assetGUIDs.Length; i++)
        {
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
        }

        allAssetPaths = AssetDatabase.GetAllAssetPaths();

        thread = new Thread(new ThreadStart(FindreAssetFerencesInScene));
        thread.Start();
    }
    
    static void FindreAssetFerencesInScene()
    {
        List<string> logInfo = new List<string>();
        string path;
        // string log;

        for(int i = 0; i < allAssetPaths.Length; i++)
        {
            path = allAssetPaths[i];
            var asset = Resources.Load(path);
            var type = asset.GetType();
            // 得到当前中所有的内容，并将物体的名字和组件的名字都加入到一个列表中
            // GameObject[] allObjects = Object.FindObjectsOfType(type);
        }
    }
}