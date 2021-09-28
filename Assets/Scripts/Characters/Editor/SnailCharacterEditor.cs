using Characters.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.Editor
{
    [CustomEditor(typeof(SnailCharacter))]
    public class SnailCharacterEditor : PlayerCharacterEditor
    {
        private SerializedProperty _shellCooldownProp;
        private SerializedProperty _facingLeftShellSpawnPointProp;
        private SerializedProperty _facingRightShellSpawnPointProp;
        private SerializedProperty _mShellPoolProp;

        private bool _mShellSettingsFoldout;

        private readonly GUIContent _mShellCooldown = new GUIContent("Shell cooldown");

        private readonly GUIContent _mFacingLeftShellSpawnPointContent =
            new GUIContent("Facing Left Shell Spawn Point");

        private readonly GUIContent _mFacingRightShellSpawnPointContent =
            new GUIContent("Facing Right Shell Spawn Point");

        private readonly GUIContent _mShellPoolContent = new GUIContent("Shell Pool");
        private readonly GUIContent _mShellSettingsContent = new GUIContent("Shell Settings");


        public new void OnEnable()
        {
            base.OnEnable();
            _shellCooldownProp = serializedObject.FindProperty("shellCooldown");
            _facingLeftShellSpawnPointProp = serializedObject.FindProperty("facingLeftShellSpawnPoint");
            _facingRightShellSpawnPointProp = serializedObject.FindProperty("facingRightShellSpawnPoint");
            _mShellPoolProp = serializedObject.FindProperty("shellPool");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mShellSettingsFoldout = EditorGUILayout.Foldout(_mShellSettingsFoldout, _mShellSettingsContent);

            if (_mShellSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_facingLeftShellSpawnPointProp, _mFacingLeftShellSpawnPointContent);
                EditorGUILayout.PropertyField(_facingRightShellSpawnPointProp, _mFacingRightShellSpawnPointContent);
                EditorGUILayout.PropertyField(_mShellPoolProp, _mShellPoolContent);
                EditorGUILayout.PropertyField(_shellCooldownProp, _mShellCooldown);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
  
    }
}
