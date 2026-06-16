#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MapConfig))]
public class MapConfigInspector : Editor
{
    private List<bool> m_Foldouts = new List<bool>();

    public override void OnInspectorGUI()
    {
        MapConfig config = (MapConfig)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnNodeIndex"));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"节点: {config.nodes.Count}", EditorStyles.boldLabel);

        EnsureFoldoutCount(config.nodes.Count);

        for (int i = 0; i < config.nodes.Count; i++)
        {
            DrawNode(config, i);
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加节点", GUILayout.Height(30)))
        {
            Undo.RecordObject(config, "Add ChunkNode");
            config.nodes.Add(new ChunkNode());
            m_Foldouts.Add(true);
            EditorUtility.SetDirty(config);
        }
        if (config.nodes.Count > 0 && GUILayout.Button("删除最后一个", GUILayout.Height(30)))
        {
            Undo.RecordObject(config, "Remove ChunkNode");
            config.nodes.RemoveAt(config.nodes.Count - 1);
            if (m_Foldouts.Count > config.nodes.Count) m_Foldouts.RemoveAt(m_Foldouts.Count - 1);
            EditorUtility.SetDirty(config);
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawNode(MapConfig config, int index)
    {
        ChunkNode node = config.nodes[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        m_Foldouts[index] = EditorGUILayout.Foldout(m_Foldouts[index], $"#{index} {(node.prefab != null ? node.prefab.name : "空")}", true);

        if (!m_Foldouts[index]) { EditorGUILayout.EndVertical(); return; }

        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", node.prefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(config, "Change ChunkNode Prefab");
            node.prefab = newPrefab;
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.Space();

        DrawConnectionList(config, node, "上连接", node.upConnections, index);
        DrawConnectionList(config, node, "下连接", node.downConnections, index);
        DrawConnectionList(config, node, "左连接", node.leftConnections, index);
        DrawConnectionList(config, node, "右连接", node.rightConnections, index);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawConnectionList(MapConfig config, ChunkNode node, string label, List<int> connections, int ownIndex)
    {
        EditorGUILayout.LabelField(label);

        EditorGUI.indentLevel++;

        int removeIdx = -1;
        for (int i = 0; i < connections.Count; i++)
        {
            int target = connections[i];
            string targetName = (target >= 0 && target < config.nodes.Count && config.nodes[target].prefab != null)
                ? config.nodes[target].prefab.name
                : $"节点 {target}";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"  → {targetName}", GUILayout.Width(200));
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                removeIdx = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (removeIdx >= 0)
        {
            Undo.RecordObject(config, "Remove Connection");
            connections.RemoveAt(removeIdx);
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("  添加 →", GUILayout.Width(200));

        int addIdx = EditorGUILayout.IntField(ownIndex, GUILayout.Width(50));
        if (GUILayout.Button("+", GUILayout.Width(25)) && addIdx != ownIndex && addIdx >= 0 && addIdx < config.nodes.Count)
        {
            Undo.RecordObject(config, "Add Connection");
            if (!connections.Contains(addIdx))
            {
                connections.Add(addIdx);
            }
            EditorUtility.SetDirty(config);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
    }

    private void EnsureFoldoutCount(int count)
    {
        while (m_Foldouts.Count < count) m_Foldouts.Add(true);
    }
}
#endif
