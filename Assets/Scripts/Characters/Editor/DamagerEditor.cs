using Characters.MonoBehaviours;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Characters.Editor
{
    [CustomEditor(typeof(Damager))]
    public class DamagerEditor : UnityEditor.Editor
    {
        private static readonly BoxBoundsHandle SBoxBoundsHandle = new BoxBoundsHandle();
        private static readonly Color SEnabledColor = Color.green + Color.grey;

        private SerializedProperty _mDamageProp;
        private SerializedProperty _mOffsetProp;
        private SerializedProperty _mSizeProp;
        private SerializedProperty _mOffsetBasedOnSpriteFacingProp;
        private SerializedProperty _mSpriteRendererProp;
        private SerializedProperty _mCanHitTriggersProp;
        private SerializedProperty _mForceRespawnProp;
        private SerializedProperty _mIgnoreInvincibilityProp;
        private SerializedProperty _mHittableLayersProp;
        private SerializedProperty _mOnDamageableHitProp;
        private SerializedProperty _mOnNonDamageableHitProp;

        private void OnEnable ()
        {
            _mDamageProp = serializedObject.FindProperty ("damage");
            _mOffsetProp = serializedObject.FindProperty("offset");
            _mSizeProp = serializedObject.FindProperty("size");
            _mOffsetBasedOnSpriteFacingProp = serializedObject.FindProperty("offsetBasedOnSpriteFacing");
            _mSpriteRendererProp = serializedObject.FindProperty("spriteRenderer");
            _mCanHitTriggersProp = serializedObject.FindProperty("canHitTriggers");
            _mForceRespawnProp = serializedObject.FindProperty("forceRespawn");
            _mIgnoreInvincibilityProp = serializedObject.FindProperty("ignoreInvincibility");
            _mHittableLayersProp = serializedObject.FindProperty("hittableLayers");
            _mOnDamageableHitProp = serializedObject.FindProperty("onDamageableHit");
            _mOnNonDamageableHitProp = serializedObject.FindProperty("onNonDamageableHit");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();

            EditorGUILayout.PropertyField(_mDamageProp);
            EditorGUILayout.PropertyField(_mOffsetProp);
            EditorGUILayout.PropertyField(_mSizeProp);
            EditorGUILayout.PropertyField(_mOffsetBasedOnSpriteFacingProp);
            if(_mOffsetBasedOnSpriteFacingProp.boolValue)
                EditorGUILayout.PropertyField(_mSpriteRendererProp);
            EditorGUILayout.PropertyField(_mCanHitTriggersProp);
            EditorGUILayout.PropertyField(_mForceRespawnProp);
            EditorGUILayout.PropertyField(_mIgnoreInvincibilityProp);
            EditorGUILayout.PropertyField(_mHittableLayersProp);
            EditorGUILayout.PropertyField(_mOnDamageableHitProp);
            EditorGUILayout.PropertyField(_mOnNonDamageableHitProp);

            serializedObject.ApplyModifiedProperties ();
        }

        private void OnSceneGUI ()
        {
            var damager = (Damager)target;

            if (!damager.enabled)
                return;

            var handleMatrix = damager.transform.localToWorldMatrix;
            handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
            handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
            handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, damager.transform.position.z));
            using (new Handles.DrawingScope(handleMatrix))
            {
                SBoxBoundsHandle.center = damager.offset;
                SBoxBoundsHandle.size = damager.size;

                SBoxBoundsHandle.SetColor(SEnabledColor);
                EditorGUI.BeginChangeCheck();
                SBoxBoundsHandle.DrawHandle();
                if (!EditorGUI.EndChangeCheck()) return;
                Undo.RecordObject(damager, "Modify Damager");

                damager.size = SBoxBoundsHandle.size;
                damager.offset = SBoxBoundsHandle.center;
            }
        }
    }
}