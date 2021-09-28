using Characters.MonoBehaviours;
using UnityEditor;
using UnityEngine;


namespace Characters.Editor
{
    [CustomEditor(typeof(StasisCharacter))]
    public class StasisCharacterEditor : PlayerCharacterEditor
    {
        private SerializedProperty _mShotsPerSecondProp;
        private SerializedProperty _mBulletSpeedProp;
        private SerializedProperty _mFacingLeftBulletSpawnPointProp;
        private SerializedProperty _mFacingRightBulletSpawnPointProp;
        private SerializedProperty _mBulletPoolProp;

        private bool _mRangedSettingsFoldout;

        private readonly GUIContent _mShotsPerSecondContent = new GUIContent("Shots Per Second");
        private readonly GUIContent _mBulletSpeedContent = new GUIContent("Bullet Speed");

        private readonly GUIContent _mFacingLeftBulletSpawnPointContent =
            new GUIContent("Facing Left Bullet Spawn Point");

        private readonly GUIContent _mFacingRightBulletSpawnPointContent =
            new GUIContent("Facing Right Bullet Spawn Point");

        private readonly GUIContent _mBulletPoolContent = new GUIContent("Bullet Pool");
        private readonly GUIContent _mRangedSettingsContent = new GUIContent("Ranged Settings");


        public new void OnEnable()
        {
            base.OnEnable();
            _mShotsPerSecondProp = serializedObject.FindProperty("shotsPerSecond");
            _mBulletSpeedProp = serializedObject.FindProperty("bulletSpeed");
            _mFacingLeftBulletSpawnPointProp = serializedObject.FindProperty("facingLeftBulletSpawnPoint");
            _mFacingRightBulletSpawnPointProp = serializedObject.FindProperty("facingRightBulletSpawnPoint");
            _mBulletPoolProp = serializedObject.FindProperty("bulletPool");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mRangedSettingsFoldout = EditorGUILayout.Foldout(_mRangedSettingsFoldout, _mRangedSettingsContent);

            if (_mRangedSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mFacingLeftBulletSpawnPointProp, _mFacingLeftBulletSpawnPointContent);
                EditorGUILayout.PropertyField(_mFacingRightBulletSpawnPointProp, _mFacingRightBulletSpawnPointContent);
                EditorGUILayout.PropertyField(_mBulletPoolProp, _mBulletPoolContent);
                EditorGUILayout.PropertyField(_mShotsPerSecondProp, _mShotsPerSecondContent);
                EditorGUILayout.PropertyField(_mBulletSpeedProp, _mBulletSpeedContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}