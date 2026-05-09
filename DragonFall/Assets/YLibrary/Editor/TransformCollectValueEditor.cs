#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform), true)]
public class TransformCollectValueEditor : Editor
{
    private const string ToolsFoldoutKey = "TransformCollectValueEditor.ToolsFoldout";
    private const string ClipboardIncludeInactiveKey = "TransformCollectValueEditor.IncludeInactive";
    private const string ClipboardIncludeSelfKey = "TransformCollectValueEditor.IncludeSelf";
    private const string GeneratedRegionStart = "   #region Values";
    private const string GeneratedRegionEnd = "     #endregion";

    private static readonly Regex CollectPattern = new Regex(
        @"^\[(?<component>[^\]]+)\](?<name>.+)$",
        RegexOptions.Compiled);
    private static readonly Regex PrefixRegex = new Regex(@"^\[[^\]]+\]", RegexOptions.Compiled);

    private class CollectedMember
    {
        public string ComponentType;
        public string FieldName;
        public string PropertyName;
    }


    private Editor m_DefaultEditor;

    private void OnEnable()
    {
        string inspectorTypeName = target is RectTransform
            ? "UnityEditor.RectTransformEditor, UnityEditor"
            : "UnityEditor.TransformInspector, UnityEditor";
        var inspectorType = System.Type.GetType(inspectorTypeName);
        if (inspectorType != null)
            m_DefaultEditor = CreateEditor(targets, inspectorType);
    }

    private void OnDisable()
    {
        if (m_DefaultEditor != null)
            DestroyImmediate(m_DefaultEditor);
    }

    public override void OnInspectorGUI()
    {
        if (m_DefaultEditor != null)
            m_DefaultEditor.OnInspectorGUI();
        else
            DrawDefaultInspector();

        bool toolsExpanded = SessionState.GetBool(ToolsFoldoutKey, true);
        bool nextExpanded = EditorGUILayout.Foldout(toolsExpanded, "Transform Tools", true);
        if (nextExpanded != toolsExpanded)
            SessionState.SetBool(ToolsFoldoutKey, nextExpanded);

        if (!nextExpanded) return;

        EditorGUILayout.Space(2f);
        EditorGUILayout.LabelField("Copy", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Position"))
                CopyLocalPosition();

            if (GUILayout.Button("Rotation"))
                CopyLocalRotation();

            if (GUILayout.Button("Scale"))
                CopyLocalScale();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("World Transform"))
                CopyWorldTransform();

            if (GUILayout.Button("Component"))
                CopyComponentJson();
        }

        EditorGUILayout.Space(2f);
        DrawSeparatorLine();
        EditorGUILayout.LabelField("Collect Values", EditorStyles.boldLabel);
        EditorGUILayout.Space(2f);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Transform命名"))
            {
                RenameAsTransform();
            }

            if (GUILayout.Button("取消命名"))
            {
                RemoveCollectValueName();
            }

            if (GUILayout.Button("按组件自动命名"))
            {
                RenameByPreferredComponent();
            }

            if (GUILayout.Button("选择组件类型命名"))
            {
                ShowComponentSelectionMenu();
            }
        }

        bool includeInactive = SessionState.GetBool(ClipboardIncludeInactiveKey, true);
        bool includeSelf = SessionState.GetBool(ClipboardIncludeSelfKey, true);
        bool nextIncludeInactive = includeInactive;
        bool nextIncludeSelf = includeSelf;
        using (new EditorGUILayout.HorizontalScope())
        {
            nextIncludeInactive = EditorGUILayout.ToggleLeft("包含未激活子物体", includeInactive, GUILayout.Width(140f));
            if (nextIncludeInactive != includeInactive)
                SessionState.SetBool(ClipboardIncludeInactiveKey, nextIncludeInactive);

            nextIncludeSelf = EditorGUILayout.ToggleLeft("包含自己", includeSelf, GUILayout.Width(80f));
            if (nextIncludeSelf != includeSelf)
                SessionState.SetBool(ClipboardIncludeSelfKey, nextIncludeSelf);

            if (GUILayout.Button("复制 CollectValues 变量代码"))
            {
                CopyCollectedValueCode(nextIncludeInactive, nextIncludeSelf);
            }
        }


    }

    private void RenameAsTransform()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var transform = targets[i] as Transform;
            if (transform == null) continue;

            Component component = transform is RectTransform
                ? transform.GetComponent<RectTransform>()
                : transform;
            ApplyCollectValueName(component);
        }
    }

    private void RenameByPreferredComponent()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var transform = targets[i] as Transform;
            if (transform == null) continue;

            var component = FindPreferredComponent(transform);
            if (component == null) continue;

            ApplyCollectValueName(component);
        }
    }

    private void RemoveCollectValueName()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var transform = targets[i] as Transform;
            if (transform == null) continue;

            Component component = transform is RectTransform
                ? transform.GetComponent<RectTransform>()
                : transform;
            RemoveCollectedValuePrefix(component);
        }
    }

    private void ShowComponentSelectionMenu()
    {
        var selectedTransform = target as Transform;
        if (selectedTransform == null) return;

        var components = selectedTransform.GetComponents<Component>();
        var menu = new GenericMenu();
        bool hasValidItem = false;

        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            if (component == null || component is Transform) continue;

            string typeName = GetCollectTypeName(component);
            if (string.IsNullOrWhiteSpace(typeName)) continue;

            hasValidItem = true;
            var capturedComponent = component;
            menu.AddItem(new GUIContent(typeName), false, () =>
            {
                for (int j = 0; j < targets.Length; j++)
                {
                    var currentTransform = targets[j] as Transform;
                    if (currentTransform == null) continue;

                    var matchedComponent = currentTransform.GetComponent(capturedComponent.GetType());
                    if (matchedComponent != null)
                        ApplyCollectValueName(matchedComponent);
                }
            });
        }

        if (!hasValidItem)
            menu.AddDisabledItem(new GUIContent("没有可用组件"));

        menu.ShowAsContext();
    }

    private Component FindPreferredComponent(Transform transform)
    {
        var components = transform.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            if (component == null || component is Transform) continue;

            string typeName = GetCollectTypeName(component);
            if (!string.IsNullOrWhiteSpace(typeName))
                return component;
        }

        return null;
    }

    /// <summary>
    /// 按组件类型为物体补上 CollectValues 前缀。
    /// </summary>
    private static bool ApplyCollectValueName(Component component)
    {
        if (component == null || component.gameObject == null) return false;

        var gameObject = component.gameObject;
        string typeName = GetCollectTypeName(component);
        if (string.IsNullOrWhiteSpace(typeName)) return false;

        string rawName = PrefixRegex.Replace(gameObject.name, string.Empty).TrimStart();
        if (string.IsNullOrWhiteSpace(rawName))
            rawName = gameObject.name;

        string targetName = $"[{typeName}]{rawName}";
        if (gameObject.name == targetName) return false;

        Undo.RecordObject(gameObject, "Collect Value Name");
        gameObject.name = targetName;
        EditorUtility.SetDirty(gameObject);
        return true;
    }

    /// <summary>
    /// 移除物体名称上的 CollectValues 前缀。
    /// </summary>
    private static bool RemoveCollectedValuePrefix(Component component)
    {
        if (component == null || component.gameObject == null) return false;

        var gameObject = component.gameObject;
        string rawName = PrefixRegex.Replace(gameObject.name, string.Empty).TrimStart();
        if (string.IsNullOrWhiteSpace(rawName) || gameObject.name == rawName) return false;

        Undo.RecordObject(gameObject, "Remove Collect Value Name");
        gameObject.name = rawName;
        EditorUtility.SetDirty(gameObject);
        return true;
    }

    /// <summary>
    /// 获取 CollectValues 约定使用的组件类型名。
    /// </summary>
    private static string GetCollectTypeName(Component component)
    {
        switch (component)
        {
            case RectTransform _:
                return "RTF";
            case Transform _:
                return "TF";
            case Button _:
                return "Button";
            case Image _:
                return "Image";
            case Text _:
                return "Text";
            case Toggle _:
                return "Toggle";
            case Slider _:
                return "Slider";
            case InputField _:
                return "InputField";
            case Dropdown _:
                return "Dropdown";
            case Scrollbar _:
                return "Scrollbar";
            case ScrollRect _:
                return "ScrollRect";
            case RawImage _:
                return "RawImage";
            default:
                return component.GetType().Name.Replace(" (Legacy)", string.Empty);
        }
    }

    private void CopyCollectedValueCode(bool includeInactive, bool includeSelf)
    {
        var root = target as Transform;
        if (root == null) return;

        var members = CollectMembers(root, includeInactive, includeSelf);
        if (members.Count == 0)
        {
            Debug.LogWarning("[TransformCollectValueEditor] 未找到符合 [组件]变量名 格式的自身或子物体。", root);
            return;
        }

        string generatedCode = BuildGeneratedBlock(members);
        EditorGUIUtility.systemCopyBuffer = generatedCode;
        Debug.Log(
            $"[TransformCollectValueEditor] 已复制 {members.Count} 个变量定义到剪贴板：\n{generatedCode}",
            root);
    }

    private List<CollectedMember> CollectMembers(Transform root, bool includeInactive, bool includeSelf)
    {
        var members = new List<CollectedMember>();
        var uniqueNames = new HashSet<string>();
        var transforms = root.GetComponentsInChildren<Transform>(includeInactive);

        for (int i = 0; i < transforms.Length; i++)
        {
            var child = transforms[i];
            if (!includeSelf && child == root) continue;

            var match = CollectPattern.Match(child.name);
            if (!match.Success) continue;

            string componentType = NormalizeComponentType(match.Groups["component"].Value.Trim());
            string propertyName = SanitizeIdentifier(match.Groups["name"].Value.Trim());
            if (string.IsNullOrWhiteSpace(componentType) || string.IsNullOrWhiteSpace(propertyName)) continue;
            if (!uniqueNames.Add(propertyName)) continue;

            members.Add(new CollectedMember
            {
                ComponentType = componentType,
                FieldName = "m_" + propertyName,
                PropertyName = propertyName,
            });
        }

        return members;
    }

    private string BuildGeneratedBlock(List<CollectedMember> members)
    {
        var builder = new StringBuilder();
        builder.AppendLine(GeneratedRegionStart);
        builder.AppendLine("    [Header(\"Auto Collected Values\")]");

        for (int i = 0; i < members.Count; i++)
        {
            var member = members[i];
            builder.Append("    [SerializeField] ")
                .Append(member.ComponentType)
                .Append(' ')
                .Append(member.FieldName)
                .AppendLine(";");
            builder.Append("    public ")
                .Append(member.ComponentType)
                .Append(' ')
                .Append(member.PropertyName)
                .Append(" { get => ")
                .Append(member.FieldName)
                .Append("; private set => ")
                .Append(member.FieldName)
                .AppendLine(" = value; }");

            if (i < members.Count - 1)
                builder.AppendLine();
        }

        builder.AppendLine(GeneratedRegionEnd);
        return builder.ToString();
    }

    private string NormalizeComponentType(string componentToken)
    {
        switch (componentToken)
        {
            case "TF":
                return "Transform";
            case "RTF":
                return "RectTransform";
            case "Button":
            case "Image":
            case "Text":
            case "Toggle":
            case "Slider":
            case "InputField":
            case "Dropdown":
            case "Scrollbar":
            case "ScrollRect":
            case "RawImage":
            case "TMP_Text":
            case "TextMeshProUGUI":
            case "TMP_InputField":
                return componentToken;
            default:
                return SanitizeIdentifier(componentToken);
        }
    }

    private string SanitizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (char.IsLetterOrDigit(current) || current == '_')
                builder.Append(current);
        }

        if (builder.Length == 0) return string.Empty;
        if (char.IsDigit(builder[0])) builder.Insert(0, '_');
        return builder.ToString();
    }

    private void CopyLocalPosition()
    {
        var transform = target as Transform;
        if (transform == null) return;

        string content = $"Position: {FormatVector3(transform.localPosition)}";
        CopyToClipboardAndLog(content, transform);
    }

    private void CopyLocalRotation()
    {
        var transform = target as Transform;
        if (transform == null) return;

        string content = $"Rotation: {FormatVector3(transform.localEulerAngles)}";
        CopyToClipboardAndLog(content, transform);
    }

    private void CopyLocalScale()
    {
        var transform = target as Transform;
        if (transform == null) return;

        string content = $"Scale: {FormatVector3(transform.localScale)}";
        CopyToClipboardAndLog(content, transform);
    }

    private void CopyWorldTransform()
    {
        var transform = target as Transform;
        if (transform == null) return;

        var builder = new StringBuilder();
        builder.AppendLine($"Position: {FormatVector3(transform.position)}");
        builder.AppendLine($"Rotation: {FormatVector3(transform.eulerAngles)}");
        builder.Append($"Scale: {FormatVector3(transform.lossyScale)}");
        CopyToClipboardAndLog(builder.ToString(), transform);
    }

    private void CopyComponentJson()
    {
        var transform = target as Transform;
        if (transform == null) return;

        string content = EditorJsonUtility.ToJson(transform, true);
        CopyToClipboardAndLog(content, transform);
    }

    private void CopyToClipboardAndLog(string content, Object context)
    {
        EditorGUIUtility.systemCopyBuffer = content;
        Debug.Log($"[TransformCollectValueEditor] 已复制到剪贴板：\n{content}", context);
    }

    private string FormatVector3(Vector3 value)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "({0:0.###}, {1:0.###}, {2:0.###})",
            value.x,
            value.y,
            value.z);
    }

    private void DrawSeparatorLine()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 4f);
        rect.height = 2f;
        EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.1f));
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(RectTransform), true)]
public class RectTransformCollectValueEditor : TransformCollectValueEditor
{
}
#endif
