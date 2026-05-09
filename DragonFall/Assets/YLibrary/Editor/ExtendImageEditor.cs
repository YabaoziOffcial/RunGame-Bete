using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;

using UnityEngine.UI;

namespace ExtendUI
{
    [CustomEditor(typeof(MBTExtendImage), true), CanEditMultipleObjects]
    public class ExtendImageEditor : ImageEditor
    {
        private SerializedProperty m_Sprite;
        private SerializedProperty m_Type;
        private SerializedProperty m_PreserveAspect;
        private SerializedProperty m_UseSpriteMesh;

        private AnimBool m_ShowImgType;
        private SerializedProperty m_FillMethod;
        private SerializedProperty m_SlicedClipMode;

        protected override void OnEnable()
        {
            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            m_FillMethod = serializedObject.FindProperty("m_FillMethod");
            m_SlicedClipMode = serializedObject.FindProperty("m_SlicedClipMode");
            m_ShowImgType = new AnimBool(m_Sprite.objectReferenceValue != null);
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();

            m_ShowImgType.target = m_Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowImgType.faded))
                TypeGUI();
            EditorGUILayout.EndFadeGroup();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                {
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);
                }
                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Filled)
                {
                    if ((Image.FillMethod)m_FillMethod.enumValueIndex == Image.FillMethod.Horizontal ||
                        (Image.FillMethod)m_FillMethod.enumValueIndex == Image.FillMethod.Vertical)
                        EditorGUILayout.PropertyField(m_SlicedClipMode);
                }

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();


            serializedObject.ApplyModifiedProperties();
        }

        private void SetShowNativeSize(bool instant)
        {
            var type = (Image.Type)m_Type.enumValueIndex;
            var showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }
    }
}