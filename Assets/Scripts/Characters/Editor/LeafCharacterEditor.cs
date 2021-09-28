using Characters.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.Editor
{
    
    [CustomEditor(typeof(LeafCharacter))]
    public class LeafCharacterEditor : PlayerCharacterEditor
    {
        private SerializedProperty _leafCooldownProp;
        private SerializedProperty _facingLeftLeafSpawnPointProp;
        private SerializedProperty _facingRightLeafSpawnPointProp;
        private SerializedProperty _mLeafPoolProp;

        private bool _leafSettingsFoldout;

        private readonly GUIContent _leafCooldown = new GUIContent("Leaf cooldown");

        private readonly GUIContent _mFacingLeftLeafSpawnPointContent =
            new GUIContent("Facing Left Leaf Spawn Point");

        private readonly GUIContent _mFacingRightLeafSpawnPointContent =
            new GUIContent("Facing Right Leaf Spawn Point");

        private readonly GUIContent _leafPoolContent = new GUIContent("Leaf Pool");
        private readonly GUIContent _leafSettingsContent = new GUIContent("Leaf Settings");


        public new void OnEnable()
        {
            base.OnEnable();
            _leafCooldownProp = serializedObject.FindProperty("leafCooldown");
            _facingLeftLeafSpawnPointProp = serializedObject.FindProperty("facingLeftLeafSpawnPoint");
            _facingRightLeafSpawnPointProp = serializedObject.FindProperty("facingRightLeafSpawnPoint");
            _mLeafPoolProp = serializedObject.FindProperty("leafPool");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _leafSettingsFoldout = EditorGUILayout.Foldout(_leafSettingsFoldout, _leafSettingsContent);

            if (_leafSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_facingLeftLeafSpawnPointProp, _mFacingLeftLeafSpawnPointContent);
                EditorGUILayout.PropertyField(_facingRightLeafSpawnPointProp, _mFacingRightLeafSpawnPointContent);
                EditorGUILayout.PropertyField(_mLeafPoolProp, _leafPoolContent);
                EditorGUILayout.PropertyField(_leafCooldownProp, _leafCooldown);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
        
    }
}
