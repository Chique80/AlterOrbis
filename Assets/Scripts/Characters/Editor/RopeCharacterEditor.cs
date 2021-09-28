using Characters.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.Editor
{
    [CustomEditor(typeof(RopeCharacter))]
    public class RopeCharacterEditor : PlayerCharacterEditor
    {
        
        private SerializedProperty _ropeMaterialsProp;
        
        private bool _mRopeSettingsFoldout;
        
        
        private readonly GUIContent _mRopeSettingsContent = new GUIContent("Rope Settings");
        private readonly GUIContent _mRopeMaterialsContent = new GUIContent("Rope Materials");
        
        public new void OnEnable()
        {
            base.OnEnable();
            _ropeMaterialsProp = serializedObject.FindProperty("ropeSprites");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mRopeSettingsFoldout = EditorGUILayout.Foldout(_mRopeSettingsFoldout, _mRopeSettingsContent);

            if (_mRopeSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_ropeMaterialsProp, _mRopeMaterialsContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
   
    }
}
