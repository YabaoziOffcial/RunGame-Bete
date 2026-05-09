using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public class HierarchyExpand
{
    [MenuItem("YBZ/Hierarchy/AddObjectType _v")]
    public static void ChangeGoName()
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go.TryGetComponent<Button>(out var btn))
            {
                if (go.name.StartsWith("[Button]")) continue;
                go.name = "[Button]" + go.name;
            }
            else if (go.TryGetComponent<Image>(out var img))
            {
                if (go.name.StartsWith("[Image]")) continue;
                go.name = "[Image]" + go.name;
            }
            else if (go.TryGetComponent<Text>(out var txt))
            {
                if (go.name.StartsWith("[Text]")) continue;
                go.name = go.name.Replace(" (Legacy)","");
                go.name = "[Text]" + go.name;
            }
            else continue;
            Undo.RecordObject(go, "Change GameObject Name");
        }
    }

    [MenuItem("YBZ/Hierarchy/UndoAddObjectType _c")]
    public static void UndoChangeGoName()
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go.name.StartsWith("[Button]"))
            {
                go.name = go.name.Replace("[Button]", "");
            }
            else if (go.name.StartsWith("[Image]"))
            {
                go.name = go.name.Replace("[Image]", "");
            }
            else if (go.name.StartsWith("[Text]"))
            {
                go.name = go.name.Replace("[Text]", "");
            }
            else continue;
            Undo.RecordObject(go, "Change GameObject Name");
        }
    }
}
