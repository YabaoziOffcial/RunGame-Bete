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
}
