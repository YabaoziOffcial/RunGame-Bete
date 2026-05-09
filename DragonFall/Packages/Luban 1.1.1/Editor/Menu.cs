using System.IO;
using UnityEditor;
using UnityEngine;

namespace Luban.Editor
{
    public static class Menu
    {

        [MenuItem("YBZ/Luban/About", priority = 0)]
        public static void OpenAbout() => Application.OpenURL("https://www.datable.cn/docs/intro");


        [MenuItem("YBZ/Luban/Quick Start")]
        public static void OpenQuickStart() => Application.OpenURL("https://www.datable.cn/docs/beginner/quickstart");

        [MenuItem("YBZ/Luban/Open Data Path")]
        public static void OpenDataPath()
        {
            string path = new System.IO.DirectoryInfo(Application.dataPath).Parent.FullName;
            path = Path.Combine(path, "Luban");
            YBZ.OSFileBrowser.Open(path);
        } 
    }
}