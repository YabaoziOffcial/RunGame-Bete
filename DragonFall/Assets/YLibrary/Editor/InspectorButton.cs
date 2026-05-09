using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

/// <summary>
/// ���ӻ���ť,
/// </summary>
[CustomEditor(typeof(UnityEngine.Object), true)]
[CanEditMultipleObjects]
public class InspectorButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 获取 MonoBehaviour 组件
        if(target is MonoBehaviour mono)
        {
            DrawInspectorButtons(mono);
        }
        // 获取 ScriptableObject 组件
        if(target is ScriptableObject scriptableObject)
        {
            DrawInspectorButtons(scriptableObject);
        }
    }

    private void DrawInspectorButtons(MonoBehaviour mono)
    {
        // �õ��ű�������
        var methods = mono.GetType()
            .GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static
            ).Where(method =>
                Attribute.IsDefined(method, typeof(InspectorButtonAttribute))
            ).ToArray();

        foreach(var method in methods)
        {
            var attr = method.GetCustomAttribute<InspectorButtonAttribute>();
            DrawButton(method, attr?.Name ?? method.Name);
        }
    }

    private void DrawInspectorButtons(ScriptableObject scriptableObject)
    {
        // �õ��ű�������
        var methods = scriptableObject.GetType()
            .GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static
            ).Where(method =>
                Attribute.IsDefined(method, typeof(InspectorButtonAttribute))
            ).ToArray();

        foreach(var method in methods)
        {
            var attr = method.GetCustomAttribute<InspectorButtonAttribute>();
            DrawButton(method, attr?.Name ?? method.Name);
        }
    }

    /// <summary>
    /// ���Ƴ���ť
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="methodName"></param>
    private void DrawButton(MethodInfo methodInfo, string methodName)
    {
        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button(methodName, GUILayout.ExpandWidth(true)))
        {
            foreach(var targetObj in targets)
            {
                if(targetObj is MonoBehaviour mono)
                {
                    CallMethod(mono, methodInfo);
                } else if(targetObj is ScriptableObject scriptableObject)
                {
                    CallMethod(scriptableObject, methodInfo);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void CallMethod(MonoBehaviour mono, MethodInfo methodInfo)
    {
        var val = methodInfo.Invoke(mono, new object[] { });
        if(val is IEnumerator coroutine)
            mono.StartCoroutine(coroutine);
        else if(val != null)
            Debug.Log($"{methodInfo.Name} ���ý��: {val}");
    }

    private void CallMethod(ScriptableObject scriptableObject, MethodInfo methodInfo)
    {
        var val = methodInfo.Invoke(scriptableObject, new object[] { });
        if(val != null)
            Debug.Log($"{methodInfo.Name} ���ý��: {val}");
    }
}
