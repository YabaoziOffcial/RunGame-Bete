using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClearAllKey { 


    [InspectorButton]
    [MenuItem("Tools/ClearAllKeys")]
    public static void ClearAllKeys()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("ClearAllKeys Complete");
    }

    [MenuItem("Tools/PrintSelectedPath")]
    public static void PrintSelectedPath()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("No scene GameObject selected.");
            return;
        }

        string path = GetHierarchyPath(selected.transform);
        Debug.Log($"Selected Path: {path}", selected);
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        while (parent != null)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }

        return path;
    }


}
