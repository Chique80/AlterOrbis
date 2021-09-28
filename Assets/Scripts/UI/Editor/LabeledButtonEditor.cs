using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LabeledButton), true)]
    [CanEditMultipleObjects]
    public class LabeledButtonEditor : ButtonEditor
    {
        SerializedProperty m_TextProperty;
        GUIContent TextPropertyLabel;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_TextProperty = serializedObject.FindProperty("m_Text");
            TextPropertyLabel = new GUIContent("Text Component", "A Text component used as the label of this button");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_TextProperty, TextPropertyLabel);
            serializedObject.ApplyModifiedProperties();
        }
    }
}


